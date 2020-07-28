#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Persistence
{
	internal sealed class StateMachinePersistedContext : StateMachineContext, IPersistenceContext
	{
		private readonly OrderedSetPersistingController<StateEntityNode> _configurationController;
		private readonly DataModelListPersistingController               _dataModelPersistingController;
		private readonly DataModelReferenceTracker                       _dataModelReferenceTracker;
		private readonly KeyListPersistingController<StateEntityNode>    _historyValuePersistingController;
		private readonly EntityQueuePersistingController<IEvent>         _internalQueuePersistingController;
		private readonly Bucket                                          _state;
		private readonly OrderedSetPersistingController<StateEntityNode> _statesToInvokeController;
		private readonly ITransactionalStorage                           _storage;

		private bool _disposed;

		public StateMachinePersistedContext(string? stateMachineName, SessionId sessionId, IDataModelValueProvider dataModelValueProvider, ITransactionalStorage storage,
											ImmutableDictionary<int, IEntity> entityMap, ILogger logger, IExternalCommunication externalCommunication,
											ImmutableDictionary<object, object> contextRuntimeItems)
				: base(stateMachineName, sessionId, dataModelValueProvider, logger, externalCommunication, contextRuntimeItems)
		{
			_storage = storage;
			var bucket = new Bucket(storage);

			_configurationController = new OrderedSetPersistingController<StateEntityNode>(bucket.Nested(StorageSection.Configuration), Configuration, entityMap);
			_statesToInvokeController = new OrderedSetPersistingController<StateEntityNode>(bucket.Nested(StorageSection.StatesToInvoke), StatesToInvoke, entityMap);
			_dataModelReferenceTracker = new DataModelReferenceTracker(bucket.Nested(StorageSection.DataModelReferences));
			_dataModelPersistingController = new DataModelListPersistingController(bucket.Nested(StorageSection.DataModel), _dataModelReferenceTracker, DataModel);
			_historyValuePersistingController = new KeyListPersistingController<StateEntityNode>(bucket.Nested(StorageSection.HistoryValue), HistoryValue, entityMap);
			_internalQueuePersistingController = new EntityQueuePersistingController<IEvent>(bucket.Nested(StorageSection.InternalQueue), InternalQueue, EventCreator);
			_state = bucket.Nested(StorageSection.StateBag);
		}

		public override IPersistenceContext PersistenceContext => this;

	#region Interface IPersistenceContext

		public void ClearState(int key) => _state.RemoveSubtree(key);

		public int GetState(int key) => _state.TryGet(key, out int value) ? value : 0;

		public int GetState(int key, int subKey) => _state.Nested(key).TryGet(subKey, out int value) ? value : 0;

		public void SetState(int key, int value) => _state.Add(key, value);

		public void SetState(int key, int subKey, int value) => _state.Nested(key).Add(subKey, value);

		public ValueTask CheckPoint(int level, CancellationToken token) => _storage.CheckPoint(level, token);

		public ValueTask Shrink(CancellationToken token) => _storage.Shrink(token);

	#endregion

		private static IEvent EventCreator(Bucket bucket) => new EventObject(bucket);

		public override async ValueTask DisposeAsync()
		{
			if (_disposed)
			{
				return;
			}

			await _storage.DisposeAsync().ConfigureAwait(false);

			_internalQueuePersistingController.Dispose();
			_historyValuePersistingController.Dispose();
			_dataModelPersistingController.Dispose();
			_dataModelReferenceTracker.Dispose();
			_statesToInvokeController.Dispose();
			_configurationController.Dispose();

			_disposed = true;

			await base.DisposeAsync().ConfigureAwait(false);
		}

		private enum StorageSection
		{
			Configuration,
			StatesToInvoke,
			DataModel,
			DataModelReferences,
			InternalQueue,
			HistoryValue,
			StateBag
		}
	}
}