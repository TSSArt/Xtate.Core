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

using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public class InterpreterModelBuilder : StateMachineVisitor
{
	private readonly Dictionary<IIdentifier, StateEntityNode>           _idMap     = new(Identifier.EqualityComparer);
	private readonly List<TransitionNode>                               _targetMap = [];
	private          int                                                _counter;
	private          int                                                _deepLevel;
	private          LinkedList<int>?                                   _documentIdList;
	private          List<IEntity>?                                     _entities;
	private          List<(Uri Uri, IExternalScriptConsumer Consumer)>? _externalScriptList;
	private          bool                                               _inParallel;

	public required IStateMachine                                   StateMachine          { private get; [UsedImplicitly] init; }
	public required IStateMachineLocation?                          StateMachineLocation  { private get; [UsedImplicitly] init; }
	public required IDataModelHandler                               DataModelHandler      { private get; [UsedImplicitly] init; }
	public required IErrorProcessorService<InterpreterModelBuilder> ErrorProcessorService { private get; [UsedImplicitly] init; }
	public required IResourceLoader                                 ResourceLoader        { private get; [UsedImplicitly] init; }

	public required Func<DocumentIdNode, TransitionNode, EmptyInitialNode>                     EmptyInitialNodeFactory             { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, ImmutableArray<StateEntityNode>, EmptyTransitionNode> EmptyTransitionNodeFactory          { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IInitial, InitialNode>                                InitialNodeFactory                  { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, ITransition, TransitionNode>                          TransitionNodeFactory               { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IDoneData, DoneDataNode>                              DoneDataNodeFactory                 { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IState, StateNode>                                    StateNodeFactory                    { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IParallel, ParallelNode>                              ParallelNodeFactory                 { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IState, CompoundNode>                                 CompoundNodeFactory                 { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IStateMachine, StateMachineNode>                      StateMachineNodeFactory             { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IFinal, FinalNode>                                    FinalNodeFactory                    { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IHistory, HistoryNode>                                HistoryNodeFactory                  { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IDataModel, DataModelNode>                            DataModelNodeFactory                { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IOnEntry, OnEntryNode>                                OnEntryNodeFactory                  { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IOnExit, OnExitNode>                                  OnExitNodeFactory                   { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IData, DataNode>                                      DataNodeFactory                     { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IInvoke, InvokeNode>                                  InvokeNodeFactory                   { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, ICancel, CancelNode>                                  CancelNodeFactory                   { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IAssign, AssignNode>                                  AssignNodeFactory                   { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IForEach, ForEachNode>                                ForEachNodeFactory                  { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IIf, IfNode>                                          IfNodeFactory                       { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IElseIf, ElseIfNode>                                  ElseIfNodeFactory                   { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IElse, ElseNode>                                      ElseNodeFactory                     { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, ILog, LogNode>                                        LogNodeFactory                      { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IRaise, RaiseNode>                                    RaiseNodeFactory                    { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, ISend, SendNode>                                      SendNodeFactory                     { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IScript, ScriptNode>                                  ScriptNodeFactory                   { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IExecutableEntity, RuntimeExecNode>                   RuntimeExecNodeFactory              { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, ICustomAction, CustomActionNode>                      CustomActionNodeFactory             { private get; [UsedImplicitly] init; }
	public required Func<DocumentIdNode, IParam, ParamNode>                                    ParamNodeFactory                    { private get; [UsedImplicitly] init; }
	public required Func<IScriptExpression, ScriptExpressionNode>                              ScriptExpressionNodeFactory         { private get; [UsedImplicitly] init; }
	public required Func<IExternalScriptExpression, ExternalScriptExpressionNode>              ExternalScriptExpressionNodeFactory { private get; [UsedImplicitly] init; }
	public required Func<IExternalDataExpression, ExternalDataExpressionNode>                  ExternalDataExpressionNodeFactory   { private get; [UsedImplicitly] init; }
	public required Func<IValueExpression, ValueExpressionNode>                                ValueExpressionNodeFactory          { private get; [UsedImplicitly] init; }
	public required Func<ILocationExpression, LocationExpressionNode>                          LocationExpressionNodeFactory       { private get; [UsedImplicitly] init; }
	public required Func<IConditionExpression, ConditionExpressionNode>                        ConditionExpressionNodeFactory      { private get; [UsedImplicitly] init; }
	public required Func<IContent, ContentNode>                                                ContentNodeFactory                  { private get; [UsedImplicitly] init; }
	public required Func<IFinalize, FinalizeNode>                                              FinalizeNodeFactory                 { private get; [UsedImplicitly] init; }
	public required Func<IIdentifier, IdentifierNode>                                          IdentifierNodeFactory               { private get; [UsedImplicitly] init; }
	public required Func<IOutgoingEvent, EventNode>                                            EventNodeFactory                    { private get; [UsedImplicitly] init; }
	public required Func<IEventDescriptor, EventDescriptorNode>                                EventDescriptorNodeFactory          { private get; [UsedImplicitly] init; }

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

	public async ValueTask<IInterpreterModel> BuildModel(bool withEntityMap = false)
	{
		_idMap.Clear();
		_targetMap.Clear();
		_entities = withEntityMap ? [] : default;
		_documentIdList = withEntityMap ? [] : default;
		_externalScriptList = default;
		_inParallel = false;
		_deepLevel = 0;
		_counter = 0;

		var stateMachine = StateMachine;

		Visit(ref stateMachine);

		foreach (var transition in _targetMap)
		{
			if (!transition.TryMapTarget(_idMap))
			{
				ErrorProcessorService.AddError(entity: null, Resources.ErrorMessage_TargetIdDoesNotExists);
			}
		}

		var id = 0;

		for (var node = _documentIdList?.First; node is not null; node = node.Next)
		{
			node.Value = id ++;
		}

		_documentIdList = default;

		var entityMap = _entities is { } entities ? GetEntityMap(entities, id) : default;

		if (_externalScriptList is not null)
		{
			await SetExternalResources(_externalScriptList).ConfigureAwait(false);
		}

		return new InterpreterModel(stateMachine.As<StateMachineNode>(), entityMap);
	}

	private static IEntityMap GetEntityMap(List<IEntity> entities, int maxId)
	{
		var map = maxId > 0 ? new IEntity?[maxId] : [];

		foreach (var entity in entities)
		{
			if (entity.Is<IDocumentId>(out var docId))
			{
				map[docId.DocumentId] = entity;
			}
		}

		return new EntityMap(map);
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
		var baseUri = StateMachineLocation?.Location;
		var resource = await ResourceLoader.Request(baseUri.CombineWith(uri)).ConfigureAwait(false);
		await using (resource.ConfigureAwait(false))
		{
			consumer.SetContent(await resource.GetContent().ConfigureAwait(false));
		}
	}

	private void RegisterEntity(IEntity entity) => _entities?.Add(entity);

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

		dataModel = DataModelNodeFactory(documentId, dataModel);
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
		base.Visit(ref scriptExpression);

		scriptExpression = ScriptExpressionNodeFactory(scriptExpression);
		RegisterEntity(scriptExpression);
	}

	protected override void Visit(ref IExternalScriptExpression externalScriptExpression)
	{
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
				_externalScriptList ??= [];
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
		DataModelHandler.Process(ref externalDataExpression);

		base.Visit(ref externalDataExpression);

		externalDataExpression = ExternalDataExpressionNodeFactory(externalDataExpression);
		RegisterEntity(externalDataExpression);
	}

	protected override void Visit(ref IValueExpression valueExpression)
	{
		DataModelHandler.Process(ref valueExpression);

		base.Visit(ref valueExpression);

		valueExpression = ValueExpressionNodeFactory(valueExpression);
		RegisterEntity(valueExpression);
	}

	protected override void Visit(ref ILocationExpression locationExpression)
	{
		DataModelHandler.Process(ref locationExpression);

		base.Visit(ref locationExpression);

		locationExpression = LocationExpressionNodeFactory(locationExpression);
		RegisterEntity(locationExpression);
	}

	protected override void Visit(ref IConditionExpression conditionExpression)
	{
		DataModelHandler.Process(ref conditionExpression);

		base.Visit(ref conditionExpression);

		conditionExpression = ConditionExpressionNodeFactory(conditionExpression);
		RegisterEntity(conditionExpression);
	}

	protected override void Visit(ref IInlineContent inlineContent)
	{
		DataModelHandler.Process(ref inlineContent);

		base.Visit(ref inlineContent);
	}

	protected override void Visit(ref IContentBody contentBody)
	{
		DataModelHandler.Process(ref contentBody);

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
		base.Visit(ref content);

		content = ContentNodeFactory(content);
		RegisterEntity(content);
	}

	protected override void Visit(ref IFinalize finalize)
	{
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
		base.Visit(ref entity);

		entity = IdentifierNodeFactory(entity);
		RegisterEntity(entity);
	}

	protected override void Visit(ref IOutgoingEvent entity)
	{
		base.Visit(ref entity);

		entity = EventNodeFactory(entity);
		RegisterEntity(entity);
	}

	protected override void Visit(ref IEventDescriptor entity)
	{
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
			DataModelHandler.Process(ref executableEntity);
		}

		_deepLevel ++;
		base.Visit(ref executableEntity);
		_deepLevel --;
	}

	private class EntityMap(IEntity?[] map) : IEntityMap
	{
	#region Interface IEntityMap

		public bool TryGetEntityByDocumentId(int id, [MaybeNullWhen(false)] out IEntity entity)
		{
			entity = id < map.Length ? map[id] : default;

			return entity is not null;
		}

	#endregion
	}
}