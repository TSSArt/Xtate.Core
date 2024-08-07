﻿// Copyright © 2019-2024 Sergii Artemenko
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

public class EventController : IEventController
{
	private const int SendEventId   = 1;
	private const int CancelEventId = 2;

	private static readonly Uri InternalTarget = new(uriString: "_internal", UriKind.Relative);

	public required IExternalCommunication?   ExternalCommunication { private get; [UsedImplicitly] init; }
	public required ILogger<IEventController> Logger                { private get; [UsedImplicitly] init; }

	public required IStateMachineContext StateMachineContext { private get; [UsedImplicitly] init; }

#region Interface IEventController

	public virtual async ValueTask Cancel(SendId sendId)
	{
		await Logger.Write(Level.Trace, CancelEventId, $@"Cancel Event '{sendId}'", sendId).ConfigureAwait(false);

		if (ExternalCommunication is not null)
		{
			await ExternalCommunication.CancelEvent(sendId).ConfigureAwait(false);
		}
	}

	public virtual async ValueTask Send(IOutgoingEvent outgoingEvent)
	{
		await Logger.Write(Level.Trace, SendEventId, $@"Send event: '{EventName.ToName(outgoingEvent.NameParts)}'", outgoingEvent).ConfigureAwait(false);

		if (IsInternalEvent(outgoingEvent))
		{
			StateMachineContext.InternalQueue.Enqueue(new EventObject(outgoingEvent) { Type = EventType.Internal });

			return;
		}

		if (ExternalCommunication is not null)
		{
			if (await ExternalCommunication.TrySendEvent(outgoingEvent).ConfigureAwait(false) == SendStatus.ToInternalQueue)
			{
				StateMachineContext.InternalQueue.Enqueue(new EventObject(outgoingEvent) { Type = EventType.Internal });
			}
		}
	}

#endregion

	private static bool IsInternalEvent(IOutgoingEvent outgoingEvent)
	{
		if (outgoingEvent.Target != InternalTarget || outgoingEvent.Type is not null)
		{
			return false;
		}

		if (outgoingEvent.DelayMs != 0)
		{
			throw new ExecutionException(Resources.Exception_InternalEventsCantBeDelayed);
		}

		return true;
	}
}

public interface IXDataModelProperty
{
	string         Name  { get; }
	DataModelValue Value { get; }
}

public class InterpreterXDataModelProperty : IXDataModelProperty
{
	public required IDataModelHandler             DataModelHandler        { private get; [UsedImplicitly] init; }
	public required IStateMachineInterpreter      StateMachineInterpreter { private get; [UsedImplicitly] init; }
	public required Func<Type, IAssemblyTypeInfo> TypeInfoFactory         { private get; [UsedImplicitly] init; }

#region Interface IXDataModelProperty

	public string Name => @"interpreter";

	public virtual DataModelValue Value => LazyValue.Create(this, static p => p.Factory());

#endregion

	private DataModelValue Factory()
	{
		var typeInfo = TypeInfoFactory(StateMachineInterpreter.GetType());

		var interpreterList = new DataModelList(DataModelHandler.CaseInsensitive)
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
	public required IDataModelHandler             DataModelHandler { private get; [UsedImplicitly] init; }
	public required Func<Type, IAssemblyTypeInfo> TypeInfoFactory  { private get; [UsedImplicitly] init; }

#region Interface IXDataModelProperty

	public string Name => @"datamodel";

	public DataModelValue Value => LazyValue.Create(this, static p => p.Factory());

#endregion

	private DataModelValue Factory()
	{
		var typeInfo = TypeInfoFactory(DataModelHandler.GetType());

		var dataModelHandlerList = new DataModelList(DataModelHandler.CaseInsensitive)
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
	public required IStateMachineStartOptions? StartOptions { private get; [UsedImplicitly] init; }

#region Interface IXDataModelProperty

	public string Name => @"args";

	public DataModelValue Value => StartOptions?.Parameters ?? default;

#endregion
}

public class StateMachineContext : IStateMachineContext, IAsyncInitialization //TODO, IExecutionContext
{
	//public required IStateMachineInterpreter       StateMachineInterpreter                 { private get; [UsedImplicitly] init; }
	//public required Func<Type, ITypeInfo>          TypeInfoFactory { private get; [UsedImplicitly] init; }

	private readonly AsyncInit<ImmutableArray<IIoProcessor>>        _ioProcessorsAsyncInit;
	private readonly AsyncInit<ImmutableArray<IXDataModelProperty>> _ixDataModelPropertyAsyncInit;

	//private readonly IStateMachineContextOptions _options;

	//private readonly ILoggerOld                 _logger;
	//private readonly ILoggerContext          _loggerContext;
	//private readonly IExternalCommunication? _externalCommunication;

	//private readonly Parameters                 _parameters;
	private DataModelList?            _dataModel;
	private KeyList<StateEntityNode>? _historyValue;

	public StateMachineContext()
	{
		_ioProcessorsAsyncInit = AsyncInit.Run(this, ctx => ctx.IoProcessors.ToImmutableArrayAsync());
		_ixDataModelPropertyAsyncInit = AsyncInit.Run(this, ctx => ctx.XDataModelProperties.ToImmutableArrayAsync());
	}

	public required IDataModelHandler                     DataModelHandler      { private get; [UsedImplicitly] init; }
	public required IStateMachine                         StateMachine          { private get; [UsedImplicitly] init; }
	public required IAsyncEnumerable<IIoProcessor>        IoProcessors          { private get; [UsedImplicitly] init; }
	public required IAsyncEnumerable<IXDataModelProperty> XDataModelProperties  { private get; [UsedImplicitly] init; }
	public required IStateMachineSessionId                StateMachineSessionId { private get; [UsedImplicitly] init; }

#region Interface IAsyncInitialization

	public Task Initialization => Task.WhenAll(_ioProcessorsAsyncInit.Task, _ixDataModelPropertyAsyncInit.Task);

#endregion

#region Interface IStateMachineContext

	public DataModelList DataModel => _dataModel ??= CreateDataModel();

	public OrderedSet<StateEntityNode> Configuration { get; } = [];

	//public IExecutionContext ExecutionContext => this;

	public KeyList<StateEntityNode> HistoryValue => _historyValue ??= new KeyList<StateEntityNode>();

	public EntityQueue<IEvent> InternalQueue { get; } = new();

	public OrderedSet<StateEntityNode> StatesToInvoke { get; } = [];

	public ServiceIdSet ActiveInvokes { get; } = [];

	public DataModelValue DoneData { get; set; }

#endregion

	private DataModelList CreateDataModel()
	{
		var dataModel = new DataModelList(DataModelHandler.CaseInsensitive);

		dataModel.AddInternal(key: @"_name", StateMachine.Name, DataModelAccess.ReadOnly);
		dataModel.AddInternal(key: @"_sessionid", StateMachineSessionId.SessionId, DataModelAccess.Constant);
		dataModel.AddInternal(key: @"_event", value: default, DataModelAccess.ReadOnly);
		dataModel.AddInternal(key: @"_ioprocessors", LazyValue.Create(this, ctx => ctx.GetIoProcessors()), DataModelAccess.Constant);
		dataModel.AddInternal(key: @"_x", LazyValue.Create(this, ctx => ctx.GetPlatform()), DataModelAccess.Constant);

		return dataModel;
	}

	private DataModelValue GetPlatform()
	{
		var list = new DataModelList(DataModelAccess.ReadOnly, DataModelHandler.CaseInsensitive);

		foreach (var property in _ixDataModelPropertyAsyncInit.Value)
		{
			list.AddInternal(property.Name, property.Value, DataModelAccess.Constant);
		}

		return list;
	}

	private DataModelValue GetIoProcessors()
	{
		if (_ioProcessorsAsyncInit.Value.IsEmpty)
		{
			return DataModelList.Empty;
		}

		var list = new DataModelList(DataModelHandler.CaseInsensitive);

		foreach (var ioProcessor in _ioProcessorsAsyncInit.Value)
		{
			list.Add(ioProcessor.Id.ToString(), new DataModelList(DataModelHandler.CaseInsensitive) { { @"location", ioProcessor.GetTarget(StateMachineSessionId.SessionId)?.ToString() } });
		}

		list.MakeDeepConstant();

		return list;
	}

	/*public ILogger?                             Logger                   { get; init; }
		public IInterpreterLoggerContext?           LoggerContext            { get; init; }
		public IExternalCommunication?              ExternalCommunication    { get; init; }
		public ISecurityContext?                    SecurityContext          { get; init; }*/

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
		public ISecurityContext?                    SecurityContext          { get; init; }
		public ImmutableDictionary<object, object>? ContextRuntimeItems      { get; init; }
	}*/
}