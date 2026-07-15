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

using Xtate.DataModel;
using Xtate.Interpreter;
using Xtate.Interpreter.Services;
using Xtate.Persistence;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class PersistedInterpreterModelGetterCoverageTest
{
	[TestMethod]
	public async Task UnsupportedPersistedVersionIsRejected()
	{
		using var storage = new TestTransactionalStorage();
		new Bucket(storage).Add(Key.Version, 2);
		var getter = CreateGetter(storage, SessionId.FromString("session"));

		await Assert.ThrowsExactlyAsync<PersistenceException>([ExcludeFromCodeCoverage] async () => await getter.GetInterpreterModel());
	}

	[TestMethod]
	public async Task MismatchedPersistedSessionIsRejected()
	{
		using var storage = new TestTransactionalStorage();
		var bucket = new Bucket(storage);
		bucket.Add(Key.Version, 1);
		bucket.AddId(Key.SessionId, SessionId.FromString("stored-session"));
		var getter = CreateGetter(storage, SessionId.FromString("provided-session"));

		await Assert.ThrowsExactlyAsync<PersistenceException>([ExcludeFromCodeCoverage] async () => await getter.GetInterpreterModel());
	}

	[TestMethod]
	public async Task MissingStoredDefinitionRequiresProvidedStateMachine()
	{
		using var storage = new TestTransactionalStorage();
		var getter = CreateGetter(storage, SessionId.FromString("session"));

		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await getter.GetInterpreterModel());
	}

	private static PersistedInterpreterModelGetter CreateGetter(ITransactionalStorage storage, SessionId sessionId) =>
		new()
		{
			InterpreterModelBuilderFactory = static (_, _) => throw new InvalidOperationException("The model-builder factory is not used by these early validation tests."),
			InterpreterModelBuilder = null!,
			DataModelHandlerService = Mock.Of<IDataModelHandlerService>(),
			StateMachineSessionId = Mock.Of<IStateMachineSessionId>(value => value.SessionId == sessionId),
			StateMachine = null,
			ErrorProcessor = Mock.Of<IErrorProcessor>(),
			TransactionalStorage = storage,
			InMemoryStorageFactory = static memory => new InMemoryStorage(memory.Span)
		};

	private sealed class TestTransactionalStorage : ITransactionalStorage
	{
		private readonly InMemoryStorage _storage = new(writeOnly: false);

		public ReadOnlyMemory<byte> Get(ReadOnlySpan<byte> key) => _storage.Get(key);

		public void Set(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) => _storage.Set(key, value);

		public void Remove(ReadOnlySpan<byte> key) => _storage.Remove(key);

		public void RemoveAll(ReadOnlySpan<byte> prefix) => _storage.RemoveAll(prefix);

		public ValueTask CheckPoint(int level) => ValueTask.CompletedTask;

		public ValueTask Shrink() => ValueTask.CompletedTask;

		public void Dispose() => _storage.Dispose();

		public ValueTask DisposeAsync()
		{
			Dispose();

			return ValueTask.CompletedTask;
		}
	}
}
