#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

namespace Xtate.Persistence;

public interface IStateMachinePersistedContextOptions : IStateMachineContextOptions
{
	ImmutableDictionary<int, IEntity> EntityMap { get; }
}
/*
public class StateMachinePersistedContextOptions : StateMachineContextOptions, IStateMachinePersistedContextOptions
{
	protected StateMachinePersistedContextOptions(IStateMachineInterpreterOptions stateMachineInterpreterOptions,
												  IDataModelHandler dataModelHandler,
												  IAsyncEnumerable<IIoProcessor> ioProcessors,
												  ImmutableDictionary<int, IEntity> entityMap) :
		base(stateMachineInterpreterOptions, dataModelHandler, ioProcessors) =>
		EntityMap = entityMap;

#region Interface IStateMachinePersistedContextOptions

	public ImmutableDictionary<int, IEntity> EntityMap { get; }

#endregion
}*/

public class StateMachinePersistedContext : StateMachineContext, IPersistenceContext, IAsyncDisposable
{
	private readonly ServiceIdSetPersistingController                _activeInvokesController;
	private readonly OrderedSetPersistingController<StateEntityNode> _configurationController;
	private readonly DataModelListPersistingController               _dataModelPersistingController;
	private readonly DataModelReferenceTracker                       _dataModelReferenceTracker;
	private readonly KeyListPersistingController<StateEntityNode>    _historyValuePersistingController;
	private readonly EntityQueuePersistingController<IEvent>         _internalQueuePersistingController;
	private readonly Bucket                                          _state;
	private readonly OrderedSetPersistingController<StateEntityNode> _statesToInvokeController;
	private readonly ITransactionalStorage                           _storage;

	public StateMachinePersistedContext(IStateMachinePersistedContextOptions options,
										ITransactionalStorage storage,
										//ILoggerOld logger,
										ILoggerContext loggerContext,
										IExternalCommunication? externalCommunication)
	{
		_storage = storage;
		var bucket = new Bucket(storage);

		_configurationController = new OrderedSetPersistingController<StateEntityNode>(bucket.Nested(StorageSection.Configuration), Configuration, options.EntityMap);
		_statesToInvokeController = new OrderedSetPersistingController<StateEntityNode>(bucket.Nested(StorageSection.StatesToInvoke), StatesToInvoke, options.EntityMap);
		_activeInvokesController = new ServiceIdSetPersistingController(bucket.Nested(StorageSection.ActiveInvokes), ActiveInvokes);
		_dataModelReferenceTracker = new DataModelReferenceTracker(bucket.Nested(StorageSection.DataModelReferences));
		_dataModelPersistingController = new DataModelListPersistingController(bucket.Nested(StorageSection.DataModel), _dataModelReferenceTracker, DataModel);
		_historyValuePersistingController = new KeyListPersistingController<StateEntityNode>(bucket.Nested(StorageSection.HistoryValue), HistoryValue, options.EntityMap);
		_internalQueuePersistingController = new EntityQueuePersistingController<IEvent>(bucket.Nested(StorageSection.InternalQueue), InternalQueue, EventCreator);

		_state = bucket.Nested(StorageSection.StateBag);
	}

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IPersistenceContext

	public void ClearState(int key) => _state.RemoveSubtree(key);

	public int GetState(int key) => _state.TryGet(key, out int value) ? value : 0;

	public int GetState(int key, int subKey) => _state.Nested(key).TryGet(subKey, out int value) ? value : 0;

	public void SetState(int key, int value) => _state.Add(key, value);

	public void SetState(int key, int subKey, int value) => _state.Nested(key).Add(subKey, value);

	public ValueTask CheckPoint(int level) => _storage.CheckPoint(level);

	public ValueTask Shrink() => _storage.Shrink();

#endregion

	private static IEvent EventCreator(Bucket bucket) => new EventObject(bucket);

	protected virtual async ValueTask DisposeAsyncCore()
	{
		await _storage.DisposeAsync().ConfigureAwait(false);
		DisposeControllers();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_storage.Dispose();
			DisposeControllers();
		}
	}

	private void DisposeControllers()
	{
		_internalQueuePersistingController.Dispose();
		_historyValuePersistingController.Dispose();
		_dataModelPersistingController.Dispose();
		_dataModelReferenceTracker.Dispose();
		_statesToInvokeController.Dispose();
		_activeInvokesController.Dispose();
		_configurationController.Dispose();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private enum StorageSection
	{
		Configuration,
		StatesToInvoke,
		ActiveInvokes,
		DataModel,
		DataModelReferences,
		InternalQueue,
		HistoryValue,
		StateBag
	}

	/*
	public StateMachinePersistedContext(ITransactionalStorage storage, ImmutableDictionary<int, IEntity> entityMap, Parameters parameters) : base(parameters)
	{
		_storage = storage;
		var bucket = new Bucket(storage);

		_configurationController = new OrderedSetPersistingController<StateEntityNode>(bucket.Nested(StorageSection.Configuration), Configuration, entityMap);
		_statesToInvokeController = new OrderedSetPersistingController<StateEntityNode>(bucket.Nested(StorageSection.StatesToInvoke), StatesToInvoke, entityMap);
		_activeInvokesController = new ServiceIdSetPersistingController(bucket.Nested(StorageSection.ActiveInvokes), ActiveInvokes);
		_dataModelReferenceTracker = new DataModelReferenceTracker(bucket.Nested(StorageSection.DataModelReferences));
		_dataModelPersistingController = new DataModelListPersistingController(bucket.Nested(StorageSection.DataModel), _dataModelReferenceTracker, DataModel);
		_historyValuePersistingController = new KeyListPersistingController<StateEntityNode>(bucket.Nested(StorageSection.HistoryValue), HistoryValue, entityMap);
		_internalQueuePersistingController = new EntityQueuePersistingController<IEvent>(bucket.Nested(StorageSection.InternalQueue), InternalQueue, EventCreator);
		_state = bucket.Nested(StorageSection.StateBag);
	}*/
}