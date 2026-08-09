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

// ReSharper disable UseAwaitUsing

using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Persistence;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Core.Test.UnitTests.Persistence;

[TestClass]
public class PersistedInterpreterModelGetterCoverageTest
{
	[TestMethod]
	public void UnsupportedPersistedVersionIsRejected()
	{
		using var storage = new TestTransactionalStorage();
		new Bucket(storage).Add(Key.Version, value: 2);

		// ReSharper disable once AccessToDisposedClosure
		Assert.ThrowsExactly<PersistenceException>([ExcludeFromCodeCoverage]() => CreateGetter(storage, SessionId.FromString("session")));
	}

	[TestMethod]
	public void MismatchedPersistedSessionIsRejected()
	{
		using var storage = new TestTransactionalStorage();
		var bucket = new Bucket(storage);
		bucket.Add(Key.Version, value: 1);
		bucket.AddId(Key.SessionId, SessionId.FromString("stored-session"));

		// ReSharper disable once AccessToDisposedClosure
		Assert.ThrowsExactly<PersistenceException>([ExcludeFromCodeCoverage]() => CreateGetter(storage, SessionId.FromString("provided-session")));
	}

	[TestMethod]
	public void LocationAndArgumentsAreRestoredWithoutBuildingTheStateMachine()
	{
		using var storage = new TestTransactionalStorage();
		var location = new Uri("https://example.test/state-machines/persisted.scxml");
		DataModelValue arguments = "persisted arguments";
		var bucket = new Bucket(storage);
		bucket.Add(Key.Location, location);
		bucket.AddDataModelValue(Key.Arguments, arguments);
		var getter = CreateGetter(storage, SessionId.FromString("session"));

		Assert.AreEqual(location, getter.GetStateMachineLocation().Location);
		Assert.AreEqual(arguments, getter.GetStateMachineArguments().Arguments);
	}

	[TestMethod]
	public void MissingStateMachineDefinitionIsRejectedWhenRequested()
	{
		using var storage = new TestTransactionalStorage();
		var getter = CreateGetter(storage, SessionId.FromString("session"));

		Assert.ThrowsExactly<PersistenceException>(getter.GetStateMachine);
	}

	private static ResumedStateMachineGetter CreateGetter(IStorage storage, SessionId sessionId) =>
		new(Mock.Of<IStateMachineSessionId>(value => value.SessionId == sessionId), storage, static memory => new InMemoryStorage(memory.Span));

	private sealed class TestTransactionalStorage : ITransactionalStorage
	{
		private readonly InMemoryStorage _storage = new(writeOnly: false);

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Dispose();

			return ValueTask.CompletedTask;
		}

	#endregion

	#region Interface IDisposable

		public void Dispose() => _storage.Dispose();

	#endregion

	#region Interface IStorage

		public ReadOnlyMemory<byte> Get(ReadOnlySpan<byte> key) => _storage.Get(key);

		public void Set(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) => _storage.Set(key, value);

		public void Remove(ReadOnlySpan<byte> key) => _storage.Remove(key);

		public void RemoveAll(ReadOnlySpan<byte> prefix) => _storage.RemoveAll(prefix);

	#endregion

	#region Interface ITransactionalStorage

		public ValueTask CheckPoint(int level) => ValueTask.CompletedTask;

		public ValueTask Shrink() => ValueTask.CompletedTask;

	#endregion
	}
}
