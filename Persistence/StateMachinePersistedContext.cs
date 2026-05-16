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

using Xtate.IoC;

namespace Xtate.Persistence;

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

[InstantiatedByIoC]
public class StateMachinePersistedContext : StateMachineContext, IStateMachinePersistenceContext, IAsyncInitialization, IAsyncDisposable
{
	private readonly AsyncInit<StateMachinePersistedContext> _init = new(context => context.Init());

	private InvokeIdSetPersistingController? _activeInvokesController;

	private OrderedSetPersistingController<StateEntityNode>? _configurationController;

	private DataModelListPersistingController? _dataModelPersistingController;

	private DataModelReferenceTracker? _dataModelReferenceTracker;

	private KeyListPersistingController<StateEntityNode>? _historyValuePersistingController;

	private EntityQueuePersistingController<IIncomingEvent>? _internalQueuePersistingController;

	private Bucket _stateBucket;

	private OrderedSetPersistingController<StateEntityNode>? _statesToInvokeController;

	//ILoggerOld logger,
	//ILoggerContext loggerContext,
	//public required IExternalServiceManager? externalCommunication { private get; init; }

	public required IInterpreterModel InterpreterModel { private get; [SetByIoC] init; }

	public required ITransactionalStorage Storage { private get; [SetByIoC] init; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IAsyncInitialization

	public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

#endregion

#region Interface IStateMachinePersistenceContext

	public Bucket GetStateBucket() => _stateBucket;

	public ValueTask CheckPoint(int level) => Storage.CheckPoint(level);

	public ValueTask Shrink() => Storage.Shrink();

#endregion

	private ValueTask Init()
	{
		var bucket = new Bucket(Storage);

		_stateBucket = bucket.Nested(StorageSection.StateBag);

		var entityMap = InterpreterModel.EntityMap;
		Infra.NotNull(entityMap);

		_configurationController = new OrderedSetPersistingController<StateEntityNode>(bucket.Nested(StorageSection.Configuration), Configuration, entityMap);
		_statesToInvokeController = new OrderedSetPersistingController<StateEntityNode>(bucket.Nested(StorageSection.StatesToInvoke), StatesToInvoke, entityMap);
		_activeInvokesController = new InvokeIdSetPersistingController(bucket.Nested(StorageSection.ActiveInvokes), ActiveInvokes);
		_dataModelReferenceTracker = new DataModelReferenceTracker(bucket.Nested(StorageSection.DataModelReferences));
		_dataModelPersistingController = new DataModelListPersistingController(bucket.Nested(StorageSection.DataModel), _dataModelReferenceTracker, DataModel);
		_historyValuePersistingController = new KeyListPersistingController<StateEntityNode>(bucket.Nested(StorageSection.HistoryValue), HistoryValue, entityMap);
		_internalQueuePersistingController = new EntityQueuePersistingController<IIncomingEvent>(bucket.Nested(StorageSection.InternalQueue), InternalQueue, EventCreator);

		return ValueTask.CompletedTask;
	}

	private static IIncomingEvent EventCreator(Bucket bucket) => new IncomingEvent(bucket);

	protected virtual async ValueTask DisposeAsyncCore()
	{
		await Storage.DisposeAsync().ConfigureAwait(false);
		DisposeControllers();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Storage.Dispose();
			DisposeControllers();
		}
	}

	private void DisposeControllers()
	{
		_internalQueuePersistingController?.Dispose();
		_historyValuePersistingController?.Dispose();
		_dataModelPersistingController?.Dispose();
		_dataModelReferenceTracker?.Dispose();
		_statesToInvokeController?.Dispose();
		_activeInvokesController?.Dispose();
		_configurationController?.Dispose();
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
		_activeInvokesController = new InvokeIdSetPersistingController(bucket.Nested(StorageSection.ActiveInvokes), ActiveInvokes);
		_dataModelReferenceTracker = new DataModelReferenceTracker(bucket.Nested(StorageSection.DataModelReferences));
		_dataModelPersistingController = new DataModelListPersistingController(bucket.Nested(StorageSection.DataModel), _dataModelReferenceTracker, DataModel);
		_historyValuePersistingController = new KeyListPersistingController<StateEntityNode>(bucket.Nested(StorageSection.HistoryValue), HistoryValue, entityMap);
		_internalQueuePersistingController = new EntityQueuePersistingController<IIncomingEvent>(bucket.Nested(StorageSection.InternalQueue), InternalQueue, EventCreator);
		_state = bucket.Nested(StorageSection.StateBag);
	}*/
}