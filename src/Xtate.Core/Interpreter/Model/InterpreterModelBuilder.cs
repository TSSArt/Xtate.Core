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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.CustomAction;
using Xtate.DataModel;
using Xtate.IoC;
using Xtate.Persistence;
using IServiceProvider = Xtate.IoC.IServiceProvider;

namespace Xtate.Core;

public class InterpreterModelGetter : IAsyncInitialization
{
	private readonly AsyncInit<IInterpreterModel> _interpreterModelAsyncInit;
	public required  IServiceProvider             ss;//TODO:delete

	public InterpreterModelGetter(InterpreterModelBuilder interpreterModelBuilder, IStateMachine stateMachine) =>
		_interpreterModelAsyncInit = AsyncInit.RunNow(interpreterModelBuilder, builder => builder.Build3(stateMachine));

#region Interface IAsyncInitialization

	public Task Initialization => _interpreterModelAsyncInit.Task;

#endregion

	public IInterpreterModel GetInterpreterModel() => _interpreterModelAsyncInit.Value;
}

public class InterpreterModelNew : IInterpreterModel
{
#region Interface IInterpreterModel

	public StateMachineNode                  Root      { get; set; } //TODO: remove set

	public ImmutableDictionary<int, IEntity> EntityMap { get; set; } 

#endregion
}

public class InterpreterModelBuilder : StateMachineVisitor
{
	private readonly Uri?                                               _BaseUri;
	private readonly IDataModelHandler                                  _DataModelHandler;
	private readonly LinkedList<int>                                    _documentIdList = new();
	private readonly List<IEntity>                                      _entities       = new();
	private readonly IErrorProcessorService<InterpreterModelBuilder>    _errorProcessorService;
	private readonly Dictionary<IIdentifier, StateEntityNode>           _idMap = new(Identifier.EqualityComparer);
	private readonly IPreDataModelProcessor                             _preDataModelProcessor;
	private readonly IResourceLoader                                    _resourceLoaderService;
	private readonly IStateMachineStartOptions                          _stateMachineStartOptions;
	private readonly List<TransitionNode>                               _targetMap = new();
	private          int                                                _counter;
	private          ImmutableArray<DataModelNode>.Builder?             _dataModelNodeArray;
	private          int                                                _deepLevel;
	private          List<(Uri Uri, IExternalScriptConsumer Consumer)>? _externalScriptList;

	private bool _inParallel;

	//private IStateMachine  _StateMachine;

	[Obsolete]
	public InterpreterModelBuilder(Parameters parameters)
	{
		_BaseUri = parameters.BaseUri;
		_DataModelHandler = parameters.DataModelHandler;

		//_StateMachine = parameters.StateMachine;
		_preDataModelProcessor = new PreDataModelProcessor(parameters);
		_resourceLoaderService = parameters.ServiceLocator.GetService<IResourceLoader>();
	}

	public InterpreterModelBuilder(IDataModelHandler dataModelHandler, /*IStateMachine stateMachine, IPreDataModelProcessor preDataModelProcessor, */
								   IResourceLoader resourceLoaderService,
								   IErrorProcessorService<InterpreterModelBuilder> errorProcessorService)
	{
		//_preDataModelProcessor = preDataModelProcessor; 
		_resourceLoaderService = resourceLoaderService;
		_errorProcessorService = errorProcessorService;

		//_StateMachine = stateMachine;
		_DataModelHandler = dataModelHandler;
	}

	public required Func<DocumentIdNode, TransitionNode, EmptyInitialNode>                     EmptyInitialNodeFactory             { private get; init; }
	public required Func<DocumentIdNode, ImmutableArray<StateEntityNode>, EmptyTransitionNode> EmptyTransitionNodeFactory          { private get; init; }
	public required Func<DocumentIdNode, IInitial, InitialNode>                                InitialNodeFactory                  { private get; init; }
	public required Func<DocumentIdNode, ITransition, TransitionNode>                          TransitionNodeFactory               { private get; init; }
	public required Func<DocumentIdNode, IDoneData, DoneDataNode>                              DoneDataNodeFactory                 { private get; init; }
	public required Func<DocumentIdNode, IState, StateNode>                                    StateNodeFactory                    { private get; init; }
	public required Func<DocumentIdNode, IParallel, ParallelNode>                              ParallelNodeFactory                 { private get; init; }
	public required Func<DocumentIdNode, IState, CompoundNode>                                 CompoundNodeFactory                 { private get; init; }
	public required Func<DocumentIdNode, IStateMachine, StateMachineNode>                      StateMachineNodeFactory             { private get; init; }
	public required Func<DocumentIdNode, IFinal, FinalNode>                                    FinalNodeFactory                    { private get; init; }
	public required Func<DocumentIdNode, IHistory, HistoryNode>                                HistoryNodeFactory                  { private get; init; }
	public required Func<DocumentIdNode, IDataModel, DataModelNode>                            DataModelNodeFactory                { private get; init; }
	public required Func<DocumentIdNode, IOnEntry, OnEntryNode>                                OnEntryNodeFactory                  { private get; init; }
	public required Func<DocumentIdNode, IOnExit, OnExitNode>                                  OnExitNodeFactory                   { private get; init; }
	public required Func<DocumentIdNode, IData, DataNode>                                      DataNodeFactory                     { private get; init; }
	public required Func<DocumentIdNode, IInvoke, InvokeNode>                                  InvokeNodeFactory                   { private get; init; }
	public required Func<DocumentIdNode, ICancel, CancelNode>                                  CancelNodeFactory                   { private get; init; }
	public required Func<DocumentIdNode, IAssign, AssignNode>                                  AssignNodeFactory                   { private get; init; }
	public required Func<DocumentIdNode, IForEach, ForEachNode>                                ForEachNodeFactory                  { private get; init; }
	public required Func<DocumentIdNode, IIf, IfNode>                                          IfNodeFactory                       { private get; init; }
	public required Func<DocumentIdNode, IElseIf, ElseIfNode>                                  ElseIfNodeFactory                   { private get; init; }
	public required Func<DocumentIdNode, IElse, ElseNode>                                      ElseNodeFactory                     { private get; init; }
	public required Func<DocumentIdNode, ILog, LogNode>                                        LogNodeFactory                      { private get; init; }
	public required Func<DocumentIdNode, IRaise, RaiseNode>                                    RaiseNodeFactory                    { private get; init; }
	public required Func<DocumentIdNode, ISend, SendNode>                                      SendNodeFactory                     { private get; init; }
	public required Func<DocumentIdNode, IScript, ScriptNode>                                  ScriptNodeFactory                   { private get; init; }
	public required Func<DocumentIdNode, IExecutableEntity, RuntimeExecNode>                   RuntimeExecNodeFactory              { private get; init; }
	public required Func<DocumentIdNode, ICustomAction, CustomActionNode>                      CustomActionNodeFactory             { private get; init; }
	public required Func<DocumentIdNode, IParam, ParamNode>                                    ParamNodeFactory                    { private get; init; }
	public required Func<IScriptExpression, ScriptExpressionNode>                              ScriptExpressionNodeFactory         { private get; init; }
	public required Func<IExternalScriptExpression, ExternalScriptExpressionNode>              ExternalScriptExpressionNodeFactory { private get; init; }
	public required Func<IExternalDataExpression, ExternalDataExpressionNode>                  ExternalDataExpressionNodeFactory   { private get; init; }
	public required Func<IValueExpression, ValueExpressionNode>                                ValueExpressionNodeFactory          { private get; init; }
	public required Func<ILocationExpression, LocationExpressionNode>                          LocationExpressionNodeFactory       { private get; init; }
	public required Func<IConditionExpression, ConditionExpressionNode>                        ConditionExpressionNodeFactory      { private get; init; }
	public required Func<IContent, ContentNode>                                                ContentNodeFactory                  { private get; init; }
	public required Func<IFinalize, FinalizeNode>                                              FinalizeNodeFactory                 { private get; init; }
	public required Func<IIdentifier, IdentifierNode>                                          IdentifierNodeFactory               { private get; init; }
	public required Func<IOutgoingEvent, EventNode>                                            EventNodeFactory                    { private get; init; }
	public required Func<IEventDescriptor, EventDescriptorNode>                                EventDescriptorNodeFactory          { private get; init; }

	private void CounterBefore(bool inParallel, out (int counter, bool inParallel) saved)
	{
		saved.counter = _counter;
		saved.inParallel = _inParallel;

		_counter = 0;
		_inParallel = inParallel;
	}

	private void CounterAfter((int counter, bool inParallel) saved)
	{
		_inParallel = saved.inParallel;
		_counter ++;
		_counter = _inParallel ? _counter + saved.counter : _counter > saved.counter ? _counter : saved.counter;
	}

	//TODO:delete
	[Obsolete]
	public async ValueTask<InterpreterModel> Build(CancellationToken token)
	{
		_idMap.Clear();
		_entities.Clear();
		_targetMap.Clear();
		_documentIdList.Clear();
		_dataModelNodeArray = default;
		_externalScriptList = default;
		_inParallel = false;
		_deepLevel = 0;
		_counter = 0;

		var stateMachine = (IStateMachine) null; //_StateMachine;

		//await _preDataModelProcessor.PreProcessStateMachine(stateMachine, token).ConfigureAwait(false);

		Visit(ref stateMachine);

		foreach (var transition in _targetMap)
		{
			if (!transition.TryMapTarget(_idMap))
			{
				_errorProcessorService.AddError(entity: null, Resources.ErrorMessage_TargetIdDoesNotExists);
			}
		}

		var entityMap = CreateEntityMap();

		var model = new InterpreterModel(stateMachine.As<StateMachineNode>(), _counter, entityMap, _dataModelNodeArray?.ToImmutable() ?? default);

		if (_externalScriptList is not null)
		{
			await SetExternalResources(_externalScriptList).ConfigureAwait(false);
		}

		return model;
	}


	public async ValueTask<IInterpreterModel> Build3(IStateMachine stateMachine)
	{
		_idMap.Clear();
		_entities.Clear();
		_targetMap.Clear();
		_dataModelNodeArray = default;
		_externalScriptList = default;
		_inParallel = false;
		_deepLevel = 0;
		_counter = 0;

		Visit(ref stateMachine);

		foreach (var transition in _targetMap)
		{
			if (!transition.TryMapTarget(_idMap))
			{
				_errorProcessorService.AddError(entity: null, Resources.ErrorMessage_TargetIdDoesNotExists);
			}
		}

		var entityMap = CreateEntityMap();

		if (_externalScriptList is not null)
		{
			await SetExternalResources(_externalScriptList).ConfigureAwait(false);
		}

		var model = new InterpreterModelNew { Root = stateMachine.As<StateMachineNode>(), EntityMap = entityMap };

		return model;
	}

	[Obsolete]
	public async ValueTask<StateMachineNode> Build2(IStateMachine stateMachine)
	{
		return (await Build3(stateMachine).ConfigureAwait(false)).Root;
	}

	private ImmutableDictionary<int, IEntity> CreateEntityMap()
	{
		var id = 0;

		for (var node = _documentIdList.First; node is not null; node = node.Next)
		{
			node.Value = id ++;
		}

		_documentIdList.Clear();

		var entityMap = ImmutableDictionary.CreateBuilder<int, IEntity>();
		foreach (var entity in _entities)
		{
			if (entity.Is<IDocumentId>(out var autoDocId))
			{
				entityMap.Add(autoDocId.DocumentId, entity);
			}
		}

		return entityMap.ToImmutable();
	}

	private async ValueTask SetExternalResources(List<(Uri Uri, IExternalScriptConsumer Consumer)> externalScriptList)
	{
		foreach (var (uri, consumer) in externalScriptList)
		{
			await LoadAndSetContent(uri, consumer).ConfigureAwait(false);
		}
	}

	private async ValueTask LoadAndSetContent(Uri uri, IExternalScriptConsumer consumer)
	{
		uri = _BaseUri.CombineWith(uri);
		var resource = await _resourceLoaderService.Request(uri).ConfigureAwait(false);
		await using (resource.ConfigureAwait(false))
		{
			consumer.SetContent(await resource.GetContent().ConfigureAwait(false));
		}
	}

	private void RegisterEntity(IEntity entity) => _entities.Add(entity);

	private DocumentIdNode CreateDocumentId() => new(_documentIdList);

	protected override void Build(ref StateMachineEntity stateMachineProperties)
	{
		var initialNodeDocumentId = CreateDocumentId();
		var transitionNodeDocumentId = CreateDocumentId();

		base.Build(ref stateMachineProperties);

		if (!stateMachineProperties.States.IsDefaultOrEmpty && stateMachineProperties.Initial is null)
		{
			var target = ImmutableArray.Create(stateMachineProperties.States[index: 0].As<StateEntityNode>());
			var transition = EmptyTransitionNodeFactory(transitionNodeDocumentId, target);
			stateMachineProperties.Initial = EmptyInitialNodeFactory(initialNodeDocumentId, transition);
		}
		else
		{
			initialNodeDocumentId.Discard();
			transitionNodeDocumentId.Discard();
		}
	}

	protected override void Visit(ref IStateMachine stateMachine)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref stateMachine);

		var newStateMachine = StateMachineNodeFactory(documentId, stateMachine);

		if (_dataModelNodeArray is not null && newStateMachine.DataModel is { } dataModelNode)
		{
			_dataModelNodeArray.Remove(dataModelNode);
		}

		stateMachine = newStateMachine;
		RegisterEntity(stateMachine);
	}

	protected override void Build(ref StateEntity stateProperties)
	{
		var initialNodeDocumentId = CreateDocumentId();
		var transitionNodeDocumentId = CreateDocumentId();

		base.Build(ref stateProperties);

		if (!stateProperties.States.IsDefaultOrEmpty && stateProperties.Initial is null)
		{
			var target = ImmutableArray.Create(stateProperties.States[index: 0].As<StateEntityNode>());
			var transition = EmptyTransitionNodeFactory(transitionNodeDocumentId, target);
			stateProperties.Initial = EmptyInitialNodeFactory(initialNodeDocumentId, transition);
		}
		else
		{
			initialNodeDocumentId.Discard();
			transitionNodeDocumentId.Discard();
		}
	}

	protected override void Visit(ref IState state)
	{
		var documentId = CreateDocumentId();

		CounterBefore(inParallel: false, out var saved);
		base.Visit(ref state);
		CounterAfter(saved);

		var newState = !state.States.IsDefaultOrEmpty
			? CompoundNodeFactory(documentId, state)
			: StateNodeFactory(documentId, state);

		_idMap.Add(newState.Id, newState);

		state = newState;
		RegisterEntity(state);
	}

	protected override void Visit(ref IParallel parallel)
	{
		var documentId = CreateDocumentId();

		CounterBefore(inParallel: false, out var saved);
		base.Visit(ref parallel);
		CounterAfter(saved);

		var newParallel = ParallelNodeFactory(documentId, parallel);
		_idMap.Add(newParallel.Id, newParallel);

		parallel = newParallel;
		RegisterEntity(parallel);
	}

	protected override void Visit(ref IFinal final)
	{
		var documentId = CreateDocumentId();

		CounterBefore(inParallel: false, out var saved);
		base.Visit(ref final);
		CounterAfter(saved);

		var newFinal = FinalNodeFactory(documentId, final);
		_idMap.Add(newFinal.Id, newFinal);

		final = newFinal;
		RegisterEntity(final);
	}

	protected override void Visit(ref IHistory history)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref history);

		var newHistory = HistoryNodeFactory(documentId, history);
		_idMap.Add(newHistory.Id, newHistory);

		history = newHistory;
		RegisterEntity(history);
	}

	protected override void Visit(ref IInitial initial)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref initial);

		initial = InitialNodeFactory(documentId, initial);
		RegisterEntity(initial);
	}

	protected override void Visit(ref ITransition transition)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref transition);

		var newTransition = TransitionNodeFactory(documentId, transition);

		if (!transition.Target.IsDefaultOrEmpty)
		{
			_targetMap.Add(newTransition);
		}

		transition = newTransition;
		RegisterEntity(transition);
	}

	protected override void Visit(ref IOnEntry onEntry)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref onEntry);

		onEntry = OnEntryNodeFactory(documentId, onEntry);
		RegisterEntity(onEntry);
	}

	protected override void Visit(ref IOnExit onExit)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref onExit);

		onExit = OnExitNodeFactory(documentId, onExit);
		RegisterEntity(onExit);
	}

	protected override void Visit(ref IDataModel dataModel)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref dataModel);

		var dataModelNode = DataModelNodeFactory(documentId, dataModel);

		(_dataModelNodeArray ??= ImmutableArray.CreateBuilder<DataModelNode>()).Add(dataModelNode);

		dataModel = dataModelNode;
		RegisterEntity(dataModel);
	}

	protected override void Visit(ref IData data)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref data);

		data = DataNodeFactory(documentId, data);
		RegisterEntity(data);
	}

	protected override void Visit(ref IInvoke invoke)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref invoke);

		invoke = InvokeNodeFactory(documentId, invoke);
		RegisterEntity(invoke);
	}

	protected override void Visit(ref IScriptExpression scriptExpression)
	{
		CreateDocumentId();

		base.Visit(ref scriptExpression);

		scriptExpression = ScriptExpressionNodeFactory(scriptExpression);
		RegisterEntity(scriptExpression);
	}

	protected override void Visit(ref IExternalScriptExpression externalScriptExpression)
	{
		CreateDocumentId();

		base.Visit(ref externalScriptExpression);

		var externalScriptExpressionNode = ExternalScriptExpressionNodeFactory(externalScriptExpression);
		externalScriptExpression = externalScriptExpressionNode;

		RegisterEntity(externalScriptExpression);

		if (externalScriptExpression.Is<IExternalScriptConsumer>(out var consumer))
		{
			if (externalScriptExpression.Is<IExternalScriptProvider>(out var provider))
			{
				consumer.SetContent(provider.Content);
			}
			else
			{
				_externalScriptList ??= new List<(Uri Uri, IExternalScriptConsumer Consumer)>();

				_externalScriptList.Add((externalScriptExpressionNode.Uri, consumer));
			}
		}
	}

	protected override void Visit(ref ICustomAction customAction)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref customAction);

		customAction = CustomActionNodeFactory(documentId, customAction);
		RegisterEntity(customAction);
	}

	protected override void Visit(ref IExternalDataExpression externalDataExpression)
	{
		_DataModelHandler.Process(ref externalDataExpression);

		CreateDocumentId();

		base.Visit(ref externalDataExpression);

		externalDataExpression = ExternalDataExpressionNodeFactory(externalDataExpression);
		RegisterEntity(externalDataExpression);
	}

	protected override void Visit(ref IValueExpression valueExpression)
	{
		_DataModelHandler.Process(ref valueExpression);

		CreateDocumentId();

		base.Visit(ref valueExpression);

		valueExpression = ValueExpressionNodeFactory(valueExpression);
		RegisterEntity(valueExpression);
	}

	protected override void Visit(ref ILocationExpression locationExpression)
	{
		_DataModelHandler.Process(ref locationExpression);

		CreateDocumentId();

		base.Visit(ref locationExpression);

		locationExpression = LocationExpressionNodeFactory(locationExpression);
		RegisterEntity(locationExpression);
	}

	protected override void Visit(ref IConditionExpression conditionExpression)
	{
		_DataModelHandler.Process(ref conditionExpression);

		CreateDocumentId();

		base.Visit(ref conditionExpression);

		conditionExpression = ConditionExpressionNodeFactory(conditionExpression);
		RegisterEntity(conditionExpression);
	}

	protected override void Visit(ref IInlineContent inlineContent)
	{
		_DataModelHandler.Process(ref inlineContent);

		base.Visit(ref inlineContent);
	}

	protected override void Visit(ref IContentBody contentBody)
	{
		_DataModelHandler.Process(ref contentBody);

		base.Visit(ref contentBody);
	}

	protected override void Visit(ref IDoneData doneData)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref doneData);

		doneData = DoneDataNodeFactory(documentId, doneData);
		RegisterEntity(doneData);
	}

	protected override void Visit(ref IContent content)
	{
		CreateDocumentId();

		base.Visit(ref content);

		content = ContentNodeFactory(content);
		RegisterEntity(content);
	}

	protected override void Visit(ref IFinalize finalize)
	{
		CreateDocumentId();

		base.Visit(ref finalize);

		finalize = FinalizeNodeFactory(finalize);
		RegisterEntity(finalize);
	}

	protected override void Visit(ref IParam param)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref param);

		param = ParamNodeFactory(documentId, param);
		RegisterEntity(param);
	}

	protected override void Visit(ref IIdentifier entity)
	{
		CreateDocumentId();

		base.Visit(ref entity);

		entity = IdentifierNodeFactory(entity);
		RegisterEntity(entity);
	}

	protected override void Visit(ref IOutgoingEvent entity)
	{
		CreateDocumentId();

		base.Visit(ref entity);

		entity = EventNodeFactory(entity);
		RegisterEntity(entity);
	}

	protected override void Visit(ref IEventDescriptor entity)
	{
		CreateDocumentId();

		base.Visit(ref entity);

		entity = EventDescriptorNodeFactory(entity);
		RegisterEntity(entity);
	}

	protected override void Visit(ref ICancel cancel)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref cancel);

		cancel = CancelNodeFactory(documentId, cancel);
		RegisterEntity(cancel);
	}

	protected override void Visit(ref IAssign assign)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref assign);

		assign = AssignNodeFactory(documentId, assign);
		RegisterEntity(assign);
	}

	protected override void Visit(ref IForEach forEach)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref forEach);

		forEach = ForEachNodeFactory(documentId, forEach);
		RegisterEntity(forEach);
	}

	protected override void Visit(ref IIf @if)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref @if);

		@if = IfNodeFactory(documentId, @if);
		RegisterEntity(@if);
	}

	protected override void Visit(ref IElseIf elseIf)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref elseIf);

		elseIf = ElseIfNodeFactory(documentId, elseIf);
		RegisterEntity(elseIf);
	}

	protected override void Visit(ref IElse @else)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref @else);

		@else = ElseNodeFactory(documentId, @else);
		RegisterEntity(@else);
	}

	protected override void Visit(ref ILog log)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref log);

		log = LogNodeFactory(documentId, log);
		RegisterEntity(log);
	}

	protected override void Visit(ref IRaise raise)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref raise);

		raise = RaiseNodeFactory(documentId, raise);
		RegisterEntity(raise);
	}

	protected override void Visit(ref ISend send)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref send);

		send = SendNodeFactory(documentId, send);
		RegisterEntity(send);
	}

	protected override void Visit(ref IScript script)
	{
		var documentId = CreateDocumentId();

		base.Visit(ref script);

		script = ScriptNodeFactory(documentId, script);
		RegisterEntity(script);
	}

	protected override void VisitUnknown(ref IExecutableEntity entity)
	{
		var documentId = CreateDocumentId();

		base.VisitUnknown(ref entity);

		if (!entity.Is<IStoreSupport>())
		{
			entity = RuntimeExecNodeFactory(documentId, entity);
			RegisterEntity(entity);
		}
	}

	protected override void Visit(ref IExecutableEntity executableEntity)
	{
		if (_deepLevel == 0)
		{
			_DataModelHandler.Process(ref executableEntity);
		}

		_deepLevel ++;
		base.Visit(ref executableEntity);
		_deepLevel --;
	}

	[PublicAPI]
	public record Parameters
	{
		public Parameters(ServiceLocator serviceLocator, IStateMachine stateMachine, IDataModelHandler dataModelHandler)
		{
			ServiceLocator = serviceLocator;
			StateMachine = stateMachine;
			DataModelHandler = dataModelHandler;
		}

		public Uri?                                 BaseUri               { get; init; }
		public ImmutableArray<ICustomActionFactory> CustomActionProviders { get; init; }
		public IDataModelHandler                    DataModelHandler      { get; init; }
		public IErrorProcessor?                     ErrorProcessor        { get; init; }
		public ILoggerOld?                          Logger                { get; init; }
		public ILoggerContext?                      LoggerContext         { get; init; }
		public ISecurityContext?                    SecurityContext       { get; init; }

		public ImmutableArray<IResourceLoaderFactory> ResourceLoaderFactories { get; init; }

		//TODO:
		public ServiceLocator ServiceLocator { get; }
		public IStateMachine  StateMachine   { get; init; }
	}
}