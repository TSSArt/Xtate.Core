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

using System.Reflection;
using Xtate.DataModel;
using Xtate.Interpreter;
using Xtate.Persistence;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;
using TypeInfo = Xtate.Persistence.Internal.TypeInfo;

namespace Xtate.Test.UnitTests.Persistence;

[TestClass]
public class StateMachinePersistedContextCoverageTest
{
	[TestMethod]
	public async Task InitializationCreatesControllersAndDelegatesStateCheckpointAndShrink()
	{
		var storage = new RecordingTransactionalStorage();
		var context = CreateContext(storage);

		await context.InitializeAsync();
		await context.InitializeAsync();
		var stateBucket = context.GetStateBucket();
		stateBucket.Add(key: "state-value", value: 42);
		context.DataModel["persisted"] = "value";
		context.ActiveInvokes.Add(InvokeId.FromString("active-invoke"));
		await context.CheckPoint(level: 3);
		await context.Shrink();

		Assert.AreEqual(expected: 42, stateBucket.GetInt32("state-value"));
		Assert.AreEqual(expected: "value", context.DataModel["persisted"].AsString());
		Assert.AreEqual(expected: 3, storage.CheckPointLevels.Single());
		Assert.AreEqual(expected: 1, storage.ShrinkCount);

		context.Dispose();
		Assert.AreEqual(expected: 1, storage.DisposeCount);
		Assert.AreEqual(expected: 0, storage.DisposeAsyncCount);
	}

	[TestMethod]
	public async Task AsynchronousDisposalUsesStorageAsyncPathAfterInitialization()
	{
		var storage = new RecordingTransactionalStorage();
		var context = CreateContext(storage);
		await context.InitializeAsync();

		await context.DisposeAsync();

		Assert.AreEqual(expected: 0, storage.DisposeCount);
		Assert.AreEqual(expected: 1, storage.DisposeAsyncCount);
	}

	[TestMethod]
	public void EventCreatorRestoresPersistedIncomingEvent()
	{
		var bucket = new Bucket(new InMemoryStorage(writeOnly: false));
		bucket.Add(Key.TypeInfo, TypeInfo.EventObject);
		bucket.AddEventName(Key.Name, EventName.FromString("persisted.event"));
		bucket.Add(Key.Type, EventType.External);

		var restored = typeof(StateMachinePersistedContext)
					   .GetMethod(name: "EventCreator", BindingFlags.Static | BindingFlags.NonPublic)!
					   .Invoke(obj: null, [bucket]);

		var incomingEvent = Assert.IsInstanceOfType<PersistedIncomingEvent>(restored);
		Assert.AreEqual(expected: "persisted.event", incomingEvent.Name.ToString());
		Assert.AreEqual(EventType.External, incomingEvent.Type);
	}

	private static StateMachinePersistedContext CreateContext(ITransactionalStorage storage)
	{
		var entityMap = new EmptyEntityMap();

		return new StateMachinePersistedContext
			   {
				   CaseSensitivity = Mock.Of<ICaseSensitivity>(static value => value.CaseInsensitive),
				   StateMachine = Mock.Of<IStateMachine>(static value => value.Name == "persisted-machine"),
				   IoProcessors = Array.Empty<IIoProcessor>(),
				   XDataModelProperties = Array.Empty<IXDataModelProperty>(),
				   StateMachineSessionId = Mock.Of<IStateMachineSessionId>(static value => value.SessionId == SessionId.FromString("persisted-session")),
				   InterpreterModel = Mock.Of<IInterpreterModel>(value => value.EntityMap == entityMap),
				   Storage = storage
			   };
	}

	private sealed class EmptyEntityMap : IEntityMap
	{
	#region Interface IEntityMap

		public bool TryGetEntityByDocumentId(int id, [MaybeNullWhen(false)] out IEntity entity)
		{
			entity = null;

			return false;
		}

	#endregion
	}

	private sealed class RecordingTransactionalStorage : ITransactionalStorage
	{
		private readonly InMemoryStorage _storage = new(writeOnly: false);

		public List<int> CheckPointLevels { get; } = [];

		public int ShrinkCount { get; private set; }

		public int DisposeCount { get; private set; }

		public int DisposeAsyncCount { get; private set; }

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			DisposeAsyncCount ++;
			_storage.Dispose();

			return ValueTask.CompletedTask;
		}

	#endregion

	#region Interface IDisposable

		public void Dispose()
		{
			DisposeCount ++;
			_storage.Dispose();
		}

	#endregion

	#region Interface IStorage

		public ReadOnlyMemory<byte> Get(ReadOnlySpan<byte> key) => _storage.Get(key);

		public void Set(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) => _storage.Set(key, value);

		public void Remove(ReadOnlySpan<byte> key) => _storage.Remove(key);

		public void RemoveAll(ReadOnlySpan<byte> prefix) => _storage.RemoveAll(prefix);

	#endregion

	#region Interface ITransactionalStorage

		public ValueTask CheckPoint(int level)
		{
			CheckPointLevels.Add(level);

			return ValueTask.CompletedTask;
		}

		public ValueTask Shrink()
		{
			ShrinkCount ++;

			return ValueTask.CompletedTask;
		}

	#endregion
	}
}