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

using System.Collections.Concurrent;
using Xtate.DataModel;
using Xtate.IoC;
using Xtate.IoProcessor;

namespace Xtate.Core;

/*
[Obsolete]
public class StateMachineContextOptions : IStateMachineContextOptions, IAsyncInitialization
{
	private readonly IDataModelHandler               _dataModelHandler;
	private readonly IAsyncEnumerable<IIoProcessor>  _ioProcessors;
	private readonly IStateMachineInterpreterOptions _stateMachineInterpreterOptions;
	private          ImmutableArray<IIoProcessor>    _ioProcessorArray;

	public StateMachineContextOptions(IStateMachineInterpreterOptions stateMachineInterpreterOptions, IDataModelHandler dataModelHandler, IAsyncEnumerable<IIoProcessor> ioProcessors)
	{
		_stateMachineInterpreterOptions = stateMachineInterpreterOptions ?? throw new ArgumentNullException(nameof(stateMachineInterpreterOptions));
		_dataModelHandler = dataModelHandler ?? throw new ArgumentNullException(nameof(dataModelHandler));
		_ioProcessors = ioProcessors;

		//DataModelInterpreter = new LazyValue<StateMachineContextOptions>(CreateInterpreterList, this);
		//DataModelHandlerData = new LazyValue<StateMachineContextOptions>(CreateDataModelHandlerList, this);
		//IoProcessors = new LazyValue<StateMachineContextOptions>(GetIoProcessors, this);

		Initialization = Initialize();
	}

#region Interface IAsyncInitialization

	public virtual Task Initialization { get; }

#endregion

#region Interface IStateMachineContextOptions

	public SessionId SessionId => _stateMachineInterpreterOptions.SessionId;

	//public string?                              StateMachineName         => _stateMachineInterpreterOptions.model.Root.Name;
	public string?        StateMachineName         => throw new NotImplementedException(); //TODO:
	public DataModelValue IoProcessors             { get; }
	public bool           DataModelCaseInsensitive => _dataModelHandler.CaseInsensitive;
	public DataModelValue DataModelArguments       => _stateMachineInterpreterOptions.options.Arguments;
	public DataModelValue DataModelInterpreter     { get; }
	public DataModelValue DataModelConfiguration   => _stateMachineInterpreterOptions.options.Configuration;
	public DataModelValue DataModelHost            => _stateMachineInterpreterOptions.options.Host;
	public DataModelValue DataModelHandlerData     { get; }

#endregion

	private async Task Initialize()
	{
		_ioProcessorArray = await _ioProcessors.ToImmutableArrayAsync().ConfigureAwait(false);
	}

	private static DataModelValue CreateInterpreterList(StateMachineContextOptions options)
	{
		var typeInfo = TypeInfo<StateMachineInterpreter>.Instance;

		var interpreterList = new DataModelList(options._dataModelHandler.CaseInsensitive)
							  {
								  { @"name", typeInfo.FullTypeName },
								  { @"version", typeInfo.AssemblyVersion }
							  };

		interpreterList.MakeDeepConstant();

		return new DataModelValue(interpreterList);
	}

	private static DataModelValue CreateDataModelHandlerList(StateMachineContextOptions options)
	{
		var typeInfo = TypeInfo<int>.Instance; // options._dataModelHandler.TypeInfo;

		var dataModelHandlerList = new DataModelList(options._dataModelHandler.CaseInsensitive)
								   {
									   { @"name", typeInfo.FullTypeName },
									   { @"assembly", typeInfo.AssemblyName },
									   { @"version", typeInfo.AssemblyVersion },
									   { @"vars", DataModelValue.FromObject(options._dataModelHandler.DataModelVars) }
								   };

		dataModelHandlerList.MakeDeepConstant();

		return new DataModelValue(dataModelHandlerList);
	}

	private static DataModelValue GetIoProcessors(StateMachineContextOptions options)
	{
		Infra.Assert(!options._ioProcessorArray.IsDefault);

		if (options._ioProcessorArray.IsEmpty)
		{
			return DataModelList.Empty;
		}

		var list = new DataModelList(options._dataModelHandler.CaseInsensitive);

		foreach (var ioProcessor in options._ioProcessorArray)
		{
			//var locationLazy = new LazyValue<IIoProcessor, SessionId>(GetLocation, ioProcessor, options._stateMachineInterpreterOptions.SessionId);

			var entry = new DataModelList(options._dataModelHandler.CaseInsensitive)
						{
							//{ @"location", locationLazy }
						};

			list.Add(ioProcessor.Id.ToString(), entry);
		}

		list.MakeDeepConstant();

		return list;

		static DataModelValue GetLocation(IIoProcessor ioProcessor, SessionId sessionId) => new(ioProcessor.GetTarget(sessionId)?.ToString());
	}
}
*/
public class InStateController : IInStateController
{
	public required IStateMachineContext StateMachineContext { private get; [UsedImplicitly] init; }

#region Interface IInStateController

	public virtual bool InState(IIdentifier id)
	{
		foreach (var state in StateMachineContext.Configuration)
		{
			if (Identifier.EqualityComparer.Equals(id, state.Id))
			{
				return true;
			}
		}

		return false;
	}

#endregion
}

public class DataModelController : IDataModelController
{
	public required IStateMachineContext StateMachineContext { private get; [UsedImplicitly] init; }

#region Interface IDataModelController

	public virtual DataModelList DataModel => StateMachineContext.DataModel;

#endregion
}

public class LogController : ILogController
{
	private const int EventId = 1;

	public required ILogger<ILogController> Logger { private get; [UsedImplicitly] init; }

	public bool IsEnabled => Logger.IsEnabled(Level.Info);

	public ValueTask Log(string? message = default, DataModelValue arguments = default) => Logger.Write(Level.Info, EventId, message, arguments);
}

public class InvokeController : IInvokeController
{
	private const int StartInvokeEventId  = 1;
	private const int CancelInvokeEventId = 2;
	private const int EventForwardEventId = 3;

	public required IExternalCommunication     ExternalCommunication { private get; [UsedImplicitly] init; }
	public required ILogger<IInvokeController> Logger                { private get; [UsedImplicitly] init; }
	public required StateMachineRuntimeError               StateMachineRuntimeError          { private get; [UsedImplicitly] init; }

	public async ValueTask Start(InvokeData invokeData)
	{
		await Logger.Write(Level.Trace, StartInvokeEventId, $@"Start invoke. InvokeId: [{invokeData.InvokeId}]", invokeData).ConfigureAwait(false);

		try
		{
			await ExternalCommunication.StartInvoke(invokeData).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex);
		}
	}

	public async ValueTask Cancel(InvokeId invokeId)
	{
		await Logger.Write(Level.Trace, CancelInvokeEventId, $@"Cancel invoke. InvokeId: [{invokeId}]", invokeId).ConfigureAwait(false);

		try
		{
			await ExternalCommunication.CancelInvoke(invokeId).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex);
		}
	}

	public async ValueTask Forward(InvokeId invokeId, IEvent evt)
	{
		await Logger.Write(Level.Trace, EventForwardEventId, $@"Forward event: '{EventName.ToName(evt.NameParts)}'", evt).ConfigureAwait(false);

		try
		{
			await ExternalCommunication.ForwardEvent(invokeId, evt).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex);
		}
	}
}

public class NoExternalCommunication : IExternalCommunication
{
	public required StateMachineRuntimeError StateMachineRuntimeError { private get; [UsedImplicitly] init; }

	public ValueTask StartInvoke(InvokeData invokeData) => throw StateMachineRuntimeError.NoExternalCommunication();

	public ValueTask CancelInvoke(InvokeId invokeId) => throw StateMachineRuntimeError.NoExternalCommunication();

	public ValueTask<SendStatus> TrySendEvent(IOutgoingEvent outgoingEvent) => throw StateMachineRuntimeError.NoExternalCommunication();

	public ValueTask ForwardEvent(InvokeId invokeId, IEvent evt) => throw StateMachineRuntimeError.NoExternalCommunication();

	public ValueTask CancelEvent(SendId sendId) => throw StateMachineRuntimeError.NoExternalCommunication();
}

public class EventController : IEventController
{
	private const int SendEventId   = 1;
	private const int CancelEventId = 2;

	private static readonly Uri InternalTarget = new(uriString: "_internal", UriKind.Relative);

	public required IExternalCommunication    ExternalCommunication { private get; [UsedImplicitly] init; }
	public required ILogger<IEventController> Logger                { private get; [UsedImplicitly] init; }
	public required StateMachineRuntimeError              StateMachineRuntimeError          { private get; [UsedImplicitly] init; }

	public required IStateMachineContext StateMachineContext { private get; [UsedImplicitly] init; }

#region Interface IEventController

	public virtual async ValueTask Cancel(SendId sendId)
	{
		await Logger.Write(Level.Trace, CancelEventId, $@"Cancel Event '{sendId}'", sendId).ConfigureAwait(false);

		try
		{
			await ExternalCommunication.CancelEvent(sendId).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex, sendId);
		}
	}

	public virtual async ValueTask Send(IOutgoingEvent outgoingEvent)
	{
		await Logger.Write(Level.Trace, SendEventId, $@"Send event: '{EventName.ToName(outgoingEvent.NameParts)}'", outgoingEvent).ConfigureAwait(false);

		if (await TrySendEvent(outgoingEvent).ConfigureAwait(false) == SendStatus.ToInternalQueue)
		{
			StateMachineContext.InternalQueue.Enqueue(new EventObject(outgoingEvent) { Type = EventType.Internal });
		}
	}

#endregion

	private async ValueTask<SendStatus> TrySendEvent(IOutgoingEvent outgoingEvent)
	{
		await Logger.Write(Level.Trace, SendEventId, $@"Send event: '{EventName.ToName(outgoingEvent.NameParts)}'", outgoingEvent).ConfigureAwait(false);

		if (IsInternalEvent(outgoingEvent))
		{
			return SendStatus.ToInternalQueue;
		}

		try
		{
			return await ExternalCommunication.TrySendEvent(outgoingEvent).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw StateMachineRuntimeError.CommunicationError(ex, outgoingEvent.SendId);
		}
	}

	private static bool IsInternalEvent(IOutgoingEvent outgoingEvent)
	{
		if (outgoingEvent.Target == InternalTarget && outgoingEvent.Type is null)
		{
			return true;
		}

		if (outgoingEvent.DelayMs != 0)
		{
			throw new ExecutionException(Resources.Exception_InternalEventsCantBeDelayed);
		}

		return false;
	}
}

public interface IXDataModelProperty
{
	string Name { get; }

	DataModelValue Value { get; }
}

public class AncestorModule : Module
{
	protected override void AddServices()
	{
		Services.AddFactory<AncestorFactory<Any>>().For<Ancestor<Any>>();
		Services.AddSharedImplementationSync<AncestorTracker>(SharedWithin.Scope).For<AncestorTracker>().For<IServiceProviderActions>();

		Services.AddFactorySync<DeferredFactory<Any>>().For<Deferred<Any>>();

		Services.AddType<ServiceList<Any>>();
		Services.AddType<ServiceSyncList<Any>>();
	}
}

public delegate ValueTask<T> Deferred<T>();

public class DeferredFactory<T>
{
	private ValueTask<T>? _valueTask;

	public required Func<ValueTask<T>> Factory { private get; [UsedImplicitly] init; }

	private ValueTask<T> GetValue() => _valueTask ??= Factory().Preserve();

	[UsedImplicitly]
	public Deferred<T> GetValueFunc() => GetValue;
}

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class ServiceList<T> : IReadOnlyList<T>, IAsyncInitialization
{
	private readonly Task _initTask;

	private ImmutableArray<T> _array;

	public ServiceList(IAsyncEnumerable<T> asyncEnumerable) => _initTask = Initialize(asyncEnumerable);

#region Interface IAsyncInitialization

	Task IAsyncInitialization.Initialization => _initTask;

#endregion

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _array).GetEnumerator();

#endregion

#region Interface IEnumerable<T>

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>) _array).GetEnumerator();

#endregion

#region Interface IReadOnlyCollection<T>

	public int Count => _array.Length;

#endregion

#region Interface IReadOnlyList<T>

	public T this[int index] => _array[index];

#endregion

	public ImmutableArray<T>.Enumerator GetEnumerator() => _array.GetEnumerator();

	private async Task Initialize(IAsyncEnumerable<T> asyncEnumerable) => _array = await asyncEnumerable.ToImmutableArrayAsync().ConfigureAwait(false);
}

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class ServiceSyncList<T>(IEnumerable<T> asyncEnumerable) : IReadOnlyList<T>
{
	private readonly ImmutableArray<T> _array = [..asyncEnumerable];

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _array).GetEnumerator();

#endregion

#region Interface IEnumerable<T>

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>) _array).GetEnumerator();

#endregion

#region Interface IReadOnlyCollection<T>

	public int Count => _array.Length;

#endregion

#region Interface IReadOnlyList<T>

	public T this[int index] => _array[index];

#endregion

	public ImmutableArray<T>.Enumerator GetEnumerator() => _array.GetEnumerator();
}

public delegate T Ancestor<out T>();

public class AncestorFactory<T> : IAsyncInitialization
{
	private ValueTask<T> _task;

	public AncestorFactory(AncestorTracker tracker, Func<ValueTask<T>> factory) => _task = tracker.TryCaptureAncestor(typeof(T), this) ? default : factory().Preserve();

#region Interface IAsyncInitialization

	Task IAsyncInitialization.Initialization => _task.IsCompletedSuccessfully ? Task.CompletedTask : _task.AsTask();

#endregion

	[UsedImplicitly]
	public Ancestor<T> GetValueFunc() => GetValue;

	private T GetValue() => _task.Result ?? throw ImplementationEntry.MissedServiceException<T, ValueTuple>();

	internal void SetValue(T? instance)
	{
		if (instance is null)
		{
			throw ImplementationEntry.MissedServiceException<T, ValueTuple>();
		}

		_task = new ValueTask<T>(instance);
	}
}

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class AncestorTracker : IServiceProviderActions, IServiceProviderDataActions
{
	private static readonly ConcurrentBag<Container> ContainerPool = [];

	private readonly AsyncLocal<Container?> _local = new();

#region Interface IServiceProviderActions

	public IServiceProviderDataActions? RegisterServices() => default;

	public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey) => default;

	public IServiceProviderDataActions? ServiceRequested(TypeKey typeKey) => default;

	public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey) => typeKey.IsEmptyArg ? this : default;

	public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey) => typeKey.IsEmptyArg ? this : default;

#endregion

#region Interface IServiceProviderDataActions

	[ExcludeFromCodeCoverage]
	public void RegisterService(ServiceEntry serviceEntry) { }

	[ExcludeFromCodeCoverage]
	public void ServiceRequesting<T, TArg>(TArg argument) { }

	[ExcludeFromCodeCoverage]
	public void ServiceRequested<T, TArg>(T? instance) { }

	public void FactoryCalling<T, TArg>(TArg argument) => CurrentContainer().Add((typeof(T), default));

	public void FactoryCalled<T, TArg>(T? instance)
	{
		var container = CurrentContainer();

		for (var i = 0; i < container.Count; i ++)
		{
			var (type, ancestor) = container[i];

			if (type == typeof(T))
			{
				container[i] = default;

				if (ancestor is AncestorFactory<T> ancestorFactory)
				{
					ancestorFactory.SetValue(instance);
				}
			}
		}

		container.RemoveAll(static p => p.Type is null);

		if (container.Count == 0)
		{
			_local.Value = default!;

			ContainerPool.Add(container);
		}
	}

#endregion

	private Container CurrentContainer()
	{
		if (_local.Value is { } container)
		{
			return container;
		}

		if (!ContainerPool.TryTake(out container))
		{
			container = [];
		}

		return _local.Value = container;
	}

	public bool TryCaptureAncestor(Type ancestorType, object ancestorFactory)
	{
		var container = CurrentContainer();

		for (var i = 0; i < container.Count; i ++)
		{
			var (type, ancestor) = container[i];

			if (type == ancestorType)
			{
				if (ancestor is null)
				{
					container[i] = (type, ancestorFactory);
				}
				else
				{
					container.Add((type, ancestorFactory));
				}

				return true;
			}
		}

		return false;
	}

	private class Container : List<(Type Type, object? Ancestor)>;
}

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class InterpreterXDataModelProperty : IXDataModelProperty
{
	public required ICaseSensitivity                    CaseSensitivity         { private get; [UsedImplicitly] init; }
	public required Ancestor<IStateMachineInterpreter> StateMachineInterpreter { private get; [UsedImplicitly] init; }
	public required Func<Type, IAssemblyTypeInfo>       TypeInfoFactory         { private get; [UsedImplicitly] init; }

#region Interface IXDataModelProperty

	public string Name => @"interpreter";

	public virtual DataModelValue Value => LazyValue.Create(this, static p => p.Factory());

#endregion

	private DataModelValue Factory()
	{
		var typeInfo = TypeInfoFactory(StateMachineInterpreter().GetType());

		var interpreterList = new DataModelList(CaseSensitivity.CaseInsensitive)
							  {
								  { @"name", typeInfo.FullTypeName },
								  { @"assembly", typeInfo.AssemblyName },
								  { @"version", typeInfo.AssemblyVersion }
							  };

		interpreterList.MakeDeepConstant();

		return interpreterList;
	}
}

public class DataModelXDataModelProperty : IXDataModelProperty
{
	public required ICaseSensitivity CaseSensitivity { private get; [UsedImplicitly] init; }

	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	public required Func<Type, IAssemblyTypeInfo> TypeInfoFactory { private get; [UsedImplicitly] init; }

#region Interface IXDataModelProperty

	public string Name => @"datamodel";

	public DataModelValue Value => LazyValue.Create(this, static p => p.Factory());

#endregion

	private DataModelValue Factory()
	{
		var typeInfo = TypeInfoFactory(DataModelHandler.GetType());

		var dataModelHandlerList = new DataModelList(CaseSensitivity.CaseInsensitive)
								   {
									   { @"name", typeInfo.FullTypeName },
									   { @"assembly", typeInfo.AssemblyName },
									   { @"version", typeInfo.AssemblyVersion },
									   { @"vars", DataModelValue.FromObject(DataModelHandler.DataModelVars) }
								   };

		dataModelHandlerList.MakeDeepConstant();

		return dataModelHandlerList;
	}
}

public class ConfigurationXDataModelProperty : IXDataModelProperty
{
#region Interface IXDataModelProperty

	public string Name => @"configuration";

	public DataModelValue Value => default;

#endregion
}

public class HostXDataModelProperty : IXDataModelProperty
{
#region Interface IXDataModelProperty

	public string Name => @"host";

	public DataModelValue Value => default;

#endregion
}

public class ArgsXDataModelProperty : IXDataModelProperty
{
	public required IStateMachineArguments? StateMachineArguments { private get; [UsedImplicitly] init; }

#region Interface IXDataModelProperty

	public string Name => @"args";

	public DataModelValue Value => StateMachineArguments?.Arguments ?? default;

#endregion
}

public class StateMachineContext : IStateMachineContext //, IAsyncInitialization //TODO, IExecutionContext
{
	//public required IStateMachineInterpreter       StateMachineInterpreter                 { private get; [UsedImplicitly] init; }
	//public required Func<Type, ITypeInfo>          TypeInfoFactory { private get; [UsedImplicitly] init; }

	//private readonly AsyncInit<ImmutableArray<IIoProcessor>>        _ioProcessorsAsyncInit;
	//private readonly AsyncInit<ImmutableArray<IXDataModelProperty>> _ixDataModelPropertyAsyncInit;

	//private readonly IStateMachineContextOptions _options;

	//private readonly ILoggerOld                 _logger;
	//private readonly ILoggerContext          _loggerContext;
	//private readonly IExternalCommunication? _externalCommunication;

	//private readonly Parameters                 _parameters;
	private DataModelList? _dataModel;

	//public StateMachineContext()
	//{
	//	_ioProcessorsAsyncInit = AsyncInit.Run(this, ctx => ctx.IoProcessors.ToImmutableArrayAsync());
	//	_ixDataModelPropertyAsyncInit = AsyncInit.Run(this, ctx => ctx.XDataModelProperties.ToImmutableArrayAsync());
	//}

	public required ICaseSensitivity                 CaseSensitivity       { private get; [UsedImplicitly] init; }
	public required IStateMachine                    StateMachine          { private get; [UsedImplicitly] init; }
	public required ServiceList<IIoProcessor>        IoProcessors          { private get; [UsedImplicitly] init; }
	public required ServiceList<IXDataModelProperty> XDataModelProperties  { private get; [UsedImplicitly] init; }
	public required IStateMachineSessionId           StateMachineSessionId { private get; [UsedImplicitly] init; }

#region Interface IAsyncInitialization

	//public Task Initialization => _ioProcessorsAsyncInit.Then(_ixDataModelPropertyAsyncInit).Task;

#endregion

#region Interface IStateMachineContext

	public DataModelList DataModel => _dataModel ??= CreateDataModel();

	public OrderedSet<StateEntityNode> Configuration { get; } = [];

	public KeyList<StateEntityNode> HistoryValue => new();

	public EntityQueue<IEvent> InternalQueue { get; } = new();

	public OrderedSet<StateEntityNode> StatesToInvoke { get; } = [];

	public ServiceIdSet ActiveInvokes { get; } = [];

	public DataModelValue DoneData { get; set; }

#endregion

	private DataModelList CreateDataModel()
	{
		var dataModel = new DataModelList(CaseSensitivity.CaseInsensitive);

		dataModel.AddInternal(key: @"_name", StateMachine.Name, DataModelAccess.ReadOnly);
		dataModel.AddInternal(key: @"_sessionid", StateMachineSessionId.SessionId, DataModelAccess.Constant);
		dataModel.AddInternal(key: @"_event", value: default, DataModelAccess.ReadOnly);
		dataModel.AddInternal(key: @"_ioprocessors", LazyValue.Create(this, ctx => ctx.GetIoProcessors()), DataModelAccess.Constant);
		dataModel.AddInternal(key: @"_x", LazyValue.Create(this, ctx => ctx.GetPlatform()), DataModelAccess.Constant);

		return dataModel;
	}

	private DataModelValue GetPlatform()
	{
		if (XDataModelProperties.Count == 0)
		{
			return DataModelList.Empty;
		}

		var list = new DataModelList(DataModelAccess.ReadOnly, CaseSensitivity.CaseInsensitive);

		foreach (var property in XDataModelProperties)
		{
			list.AddInternal(property.Name, property.Value, DataModelAccess.Constant);
		}

		return list;
	}

	private DataModelValue GetIoProcessors()
	{
		if (IoProcessors.Count == 0)
		{
			return DataModelList.Empty;
		}

		var caseInsensitive = CaseSensitivity.CaseInsensitive;

		var list = new DataModelList(DataModelAccess.ReadOnly, caseInsensitive);

		foreach (var ioProcessor in IoProcessors)
		{
			var value = new DataModelList(DataModelAccess.ReadOnly, caseInsensitive);
			value.AddInternal(key: @"location", ioProcessor.GetTarget(StateMachineSessionId.SessionId)?.ToString(), DataModelAccess.Constant);

			list.AddInternal(ioProcessor.Id.ToString(), value, DataModelAccess.Constant);
		}

		return list;
	}

	/*public ILogger?                             Logger                   { get; init; }
		public IInterpreterLoggerContext?           LoggerContext            { get; init; }
		public IExternalCommunication?              ExternalCommunication    { get; init; }
		public SecurityContext?                    SecurityContext          { get; init; }*/

	/*public StateMachineContext(
		/*IStateMachineContextOptions options, ILogger logger,
		/*IExternalCommunication? externalCommunication)
	{
	//	_options = options;
		//_logger = logger;
		//_loggerContext = loggerContext;
		//_externalCommunication = externalCommunication;
	}*/

	//TODO: delete
	//private StateMachineContext(Parameters parameters) { } //_parameters = parameters; }

	//public virtual IPersistenceContext PersistenceContext => throw new NotSupportedException();

	/*
	public record Parameters
	{
		public Parameters(SessionId sessionId) => SessionId = sessionId;

		public SessionId                            SessionId                { get; init; }
		public string?                              StateMachineName         { get; init; }
		public DataModelValue                       DataModelArguments       { get; init; }
		public DataModelValue                       DataModelInterpreter     { get; init; }
		public DataModelValue                       DataModelConfiguration   { get; init; }
		public DataModelValue                       DataModelHost            { get; init; }
		public DataModelValue                       DataModelHandlerData     { get; init; }
		public bool                                 DataModelCaseInsensitive { get; init; }
		public ILoggerOld?                             Logger                   { get; init; }
		public IInterpreterLoggerContext?           LoggerContext            { get; init; }
		public IExternalCommunication?              ExternalCommunication    { get; init; }
		public SecurityContext?                    SecurityContext          { get; init; }
		public ImmutableDictionary<object, object>? ContextRuntimeItems      { get; init; }
	}*/
}