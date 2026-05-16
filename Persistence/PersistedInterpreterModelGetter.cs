// Copyright © 2019-2026 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Buffers;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public class PersistedInterpreterModelGetter
{
	public required Func<IStateMachine, IDataModelHandler, ValueTask<InterpreterModelBuilder>> InterpreterModelBuilderFactory { private get; [UsedImplicitly] init; }

	public required InterpreterModelBuilder InterpreterModelBuilder { private get; [UsedImplicitly] init; }

	public required IDataModelHandlerService DataModelHandlerService { private get; [UsedImplicitly] init; }

	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

	public required IStateMachine? StateMachine { private get; [UsedImplicitly] init; }

	public required IErrorProcessor ErrorProcessor { private get; [UsedImplicitly] init; }

	public required ITransactionalStorage TransactionalStorage { private get; [UsedImplicitly] init; }

	public required Func<ReadOnlyMemory<byte>, InMemoryStorage> InMemoryStorageFactory { private get; [UsedImplicitly] init; }

	[CalledByIoC]
	public async ValueTask<IInterpreterModel> GetInterpreterModel()
	{
		if (await TryRestoreInterpreterModel().ConfigureAwait(false) is { } interpreterModel)
		{
			return interpreterModel;
		}

		Infra.NotNull(StateMachine);

		try
		{
			interpreterModel = await InterpreterModelBuilder.BuildModel(true).ConfigureAwait(false);
		}
		finally
		{
			ErrorProcessor.ThrowIfErrors();
		}

		await SaveInterpreterModel(interpreterModel).ConfigureAwait(false);

		await Disposer.DisposeAsync(TransactionalStorage).ConfigureAwait(false);

		return interpreterModel;
	}

	private async ValueTask<IInterpreterModel?> TryRestoreInterpreterModel()
	{
		var bucket = new Bucket(TransactionalStorage);

		if (bucket.TryGet(Key.Version, out int version) && version != 1)
		{
			throw new PersistenceException(Resources.Exception_PersistedStateCantBeReadUnsupportedVersion);
		}

		var storedSessionId = bucket.GetSessionId(Key.SessionId);

		if (storedSessionId is not null && storedSessionId != StateMachineSessionId.SessionId)
		{
			throw new PersistenceException(Resources.Exception_PersistedStateCantBeReadStoredAndProvidedSessionIdsDoesNotMatch);
		}

		if (!bucket.TryGet(Key.StateMachineDefinition, out var memory))
		{
			return null;
		}

		var smdBucket = new Bucket(InMemoryStorageFactory(memory));
		var dataModelType = smdBucket.GetString(Key.DataModelType);
		var dataModelHandler = await DataModelHandlerService.GetDataModelHandler(dataModelType).ConfigureAwait(false);

		IEntityMap? entityMap = null;

		if (StateMachine is not null)
		{
			var interpreterModelBuilder = await InterpreterModelBuilderFactory(StateMachine, dataModelHandler).ConfigureAwait(false);
			entityMap = (await interpreterModelBuilder.BuildModel(true).ConfigureAwait(false)).EntityMap;
		}

		var restoredStateMachine = new StateMachineReader().Build(smdBucket, entityMap);

		if (StateMachine is not null)
		{
			//TODO: Validate stateMachine vs restoredStateMachine (number of elements should be the same and documentId should point to the same entity type)
		}

		try
		{
			var interpreterModelBuilder = await InterpreterModelBuilderFactory(restoredStateMachine, dataModelHandler).ConfigureAwait(false);

			return await interpreterModelBuilder.BuildModel(true).ConfigureAwait(false);
		}
		finally
		{
			ErrorProcessor.ThrowIfErrors();
		}
	}

	private async ValueTask SaveInterpreterModel(IInterpreterModel interpreterModel)
	{
		SaveToStorage(interpreterModel.Root, new Bucket(TransactionalStorage));

		await TransactionalStorage.CheckPoint(0).ConfigureAwait(false);
	}

	private void SaveToStorage(IStoreSupport root, in Bucket bucket)
	{
		var memoryStorage = new InMemoryStorage();
		root.Store(new Bucket(memoryStorage));

		var transactionLogSize = memoryStorage.GetTransactionLogSize();
		var buffer = ArrayPool<byte>.Shared.Rent(transactionLogSize);

		try
		{
			var span = buffer.AsSpan(start: 0, transactionLogSize);

			memoryStorage.WriteTransactionLogToSpan(span);

			bucket.Add(Key.Version, value: 1);
			bucket.AddId(Key.SessionId, StateMachineSessionId.SessionId);
			bucket.Add(Key.StateMachineDefinition, span);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}