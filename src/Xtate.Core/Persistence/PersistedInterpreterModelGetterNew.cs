// Copyright © 2019-2025 Sergii Artemenko
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

using Xtate.Persistence;

namespace Xtate.Core;

public class PersistedInterpreterModelGetterNew
{
	public required IStorageProvider StorageProvider { private get; [UsedImplicitly] init; }
	
	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

	public required InterpreterModelBuilder InterpreterModelBuilder { private get; [UsedImplicitly] init; }

	public required IErrorProcessor ErrorProcessor { private get; [UsedImplicitly] init; }

	[UsedImplicitly]
	public async ValueTask<IInterpreterModel> GetInterpreterModel()
	{
		try
		{
			return await InterpreterModelBuilder.BuildModel(true).ConfigureAwait(false);
		}
		finally
		{
			ErrorProcessor.ThrowIfErrors();
		}
	}

	private async ValueTask SaveInterpreterModel(IInterpreterModel interpreterModel)
	{
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

		using var ss = new StackSpan<byte>(memoryStorage.GetTransactionLogSize());
		var span = ss ? ss : stackalloc byte[ss];

		memoryStorage.WriteTransactionLogToSpan(span);

		bucket.Add(Key.Version, value: 1);
		bucket.AddId(Key.SessionId, StateMachineSessionId.SessionId);
		bucket.Add(Key.StateMachineDefinition, span);
	}
}