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
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Interpreter.Services;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.StateMachine.Validator;

namespace Xtate.Persistence.Services;

public class PersistedInterpreterModelGetter
{
	public required IStateMachineSessionId StateMachineSessionId { private get; [SetByIoC] init; }

	public required IStateMachineLocation? StateMachineLocation { private get; [SetByIoC] init; }

	public required IStateMachineArguments? StateMachineArguments { private get; [SetByIoC] init; }

	public required ITransactionalStorage TransactionalStorage { private get; [SetByIoC] init; }

	public required InterpreterModelBuilder InterpreterModelBuilder { private get; [SetByIoC] init; }

	public required IErrorProcessor ErrorProcessor { private get; [SetByIoC] init; }

	[CalledByIoC]
	public async ValueTask<IInterpreterModel> GetInterpreterModel()
	{
		try
		{
			IInterpreterModel interpreterModel;

			try
			{
				interpreterModel = await InterpreterModelBuilder.BuildModel(true).ConfigureAwait(false);
			}
			finally
			{
				ErrorProcessor.ThrowIfErrors();
			}

			await SaveInterpreterModel(interpreterModel).ConfigureAwait(false);

			return interpreterModel;
		}
		finally
		{
			await Disposer.DisposeAsync(TransactionalStorage).ConfigureAwait(false);
		}
	}

	private async ValueTask SaveInterpreterModel(IInterpreterModel interpreterModel)
	{
		var bucket = new Bucket(TransactionalStorage);

		if (bucket.TryGet(Key.Version, out int version))
		{
			if (version == 1)
			{
				return;
			}

			bucket.RemoveSubtree(Bucket.RootKey);
		}

		bucket.Add(Key.Version, value: 1);

		SaveToStorage((IStoreSupport)interpreterModel.Root, bucket);

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

			bucket.AddId(Key.SessionId, StateMachineSessionId.SessionId);
			bucket.Add(Key.Location, StateMachineLocation?.Location);
			bucket.AddDataModelValue(Key.Arguments, StateMachineArguments?.Arguments ?? DataModelValue.Undefined);
			bucket.Add(Key.StateMachineDefinition, span);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}