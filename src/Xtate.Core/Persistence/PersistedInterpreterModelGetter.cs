// Copyright © 2019-2024 Sergii Artemenko
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

	public required IStorageProvider StorageProvider { private get; [UsedImplicitly] init; }

	public required Func<ReadOnlyMemory<byte>, InMemoryStorage> InMemoryStorageFactory { private get; [UsedImplicitly] init; }

	[UsedImplicitly]
	public async ValueTask<IInterpreterModel> GetInterpreterModel()
	{
		if (await TryRestoreInterpreterModel().ConfigureAwait(false) is { } interpreterModel)
		{
			return interpreterModel;
		}

		Infra.NotNull(StateMachine);

		try
		{
			//var dataModelHandler = await DataModelHandlerService.GetDataModelHandler(StateMachine.DataModelType).ConfigureAwait(false);
			//var interpreterModelBuilder = await InterpreterModelBuilderFactory(StateMachine, dataModelHandler).ConfigureAwait(false);
			//interpreterModel = await interpreterModelBuilder.BuildModel(true).ConfigureAwait(false);
			//var interpreterModelBuilder = await InterpreterModelBuilder().ConfigureAwait(false);
			interpreterModel = await InterpreterModelBuilder.BuildModel(true).ConfigureAwait(false);
		}
		finally
		{
			ErrorProcessor.ThrowIfErrors();
		}

		await SaveInterpreterModel(interpreterModel).ConfigureAwait(false);

		return interpreterModel;
	}

	private async ValueTask<IInterpreterModel?> TryRestoreInterpreterModel()
	{
		//var storage = await StorageProvider.GetTransactionalStorage(partition: default, StateMachineDefinitionStorageKey).ConfigureAwait(false);
		var storage = await StorageProvider.GetTransactionalStorage(partition: default, key: @"StateMachineDefinitionStorageKey").ConfigureAwait(false); //TODO:

		await using (storage.ConfigureAwait(false))
		{
			var bucket = new Bucket(storage);

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
			var dataModelHandler = await DataModelHandlerService.GetDataModelHandler(dataModelType).ConfigureAwait(false); // TODO: uncomment

			IEntityMap? entityMap = default;

			if (StateMachine is not null)
			{
				//var parameters = CreateInterpreterModelBuilderParameters();
				//var temporaryModelBuilder = new InterpreterModelBuilder(parameters);
				var interpreterModelBuilder = await InterpreterModelBuilderFactory(StateMachine, dataModelHandler).ConfigureAwait(false);
				entityMap = (await interpreterModelBuilder.BuildModel().ConfigureAwait(false)).EntityMap;

				//var model = await temporaryModelBuilder.Build(CancellationToken.None).ConfigureAwait(false);
				//entityMap = model.EntityMap;
			}

			var restoredStateMachine = new StateMachineReader().Build(smdBucket, entityMap);

			if (StateMachine is not null)
			{
				//TODO: Validate stateMachine vs restoredStateMachine (number of elements should be the same and documentId should point to the same entity type)
			}

			//_stateMachine = restoredStateMachine;//TODO:uncomment

			try
			{
				//var parameters = CreateInterpreterModelBuilderParameters();
				//var interpreterModelBuilder = new InterpreterModelBuilder(parameters);
				var interpreterModelBuilder = await InterpreterModelBuilderFactory(restoredStateMachine, dataModelHandler).ConfigureAwait(false);

				return await interpreterModelBuilder.BuildModel().ConfigureAwait(false);
			}

			// ReSharper disable once RedundantEmptyFinallyBlock
			finally
			{
				//_options.ErrorProcessor?.ThrowIfErrors(); //TODO:uncomment
			}
		}
	}

	private async ValueTask SaveInterpreterModel(IInterpreterModel interpreterModel)
	{
		//var storage = await StorageProvider.GetTransactionalStorage(partition: default, StateMachineDefinitionStorageKey).ConfigureAwait(false);
		var storage = await StorageProvider.GetTransactionalStorage(partition: default, key: @"StateMachineDefinitionStorageKey").ConfigureAwait(false); //TODO:

		await using (storage.ConfigureAwait(false))
		{
			SaveToStorage(interpreterModel.Root, new Bucket(storage));

			await storage.CheckPoint(0).ConfigureAwait(false);
		}
	}

    private void SaveToStorage(IStoreSupport root, in Bucket bucket)
    {
        var memoryStorage = new InMemoryStorage();
        root.Store(new Bucket(memoryStorage));

        var transactionLogSize = memoryStorage.GetTransactionLogSize();
        var buffer = ArrayPool<byte>.Shared.Rent(transactionLogSize);

        try
        {
            var span = buffer.AsSpan(0, transactionLogSize);
            
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