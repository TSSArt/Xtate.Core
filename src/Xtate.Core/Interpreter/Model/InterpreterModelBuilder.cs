#region Copyright © 2019-2021 Sergii Artemenko

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
using Xtate.Persistence;

namespace Xtate.Core
{
	internal sealed class InterpreterModelBuilder : StateMachineVisitor
	{
		private readonly Uri?                                               _baseUri;
		private readonly ImmutableArray<ICustomActionFactory>               _customActionProviders;
		private readonly IDataModelHandler                                  _dataModelHandler;
		private readonly LinkedList<int>                                    _documentIdList;
		private readonly List<IEntity>                                      _entities;
		private readonly IErrorProcessor                                    _errorProcessor;
		private readonly Dictionary<IIdentifier, StateEntityNode>           _idMap;
		private readonly PreDataModelProcessor                              _preDataModelProcessor;
		private readonly ImmutableArray<IResourceLoaderFactory>             _resourceLoaderFactories;
		private readonly ISecurityContext                                    _securityContext;
		private readonly IStateMachine                                      _stateMachine;
		private readonly List<TransitionNode>                               _targetMap;
		private          int                                                _counter;
		private          ImmutableArray<DataModelNode>.Builder?             _dataModelNodeArray;
		private          int                                                _deepLevel;
		private          List<(Uri Uri, IExternalScriptConsumer Consumer)>? _externalScriptList;
		private          bool                                               _inParallel;

		public InterpreterModelBuilder(IStateMachine stateMachine, IDataModelHandler dataModelHandler, ImmutableArray<ICustomActionFactory> customActionProviders,
									   ImmutableArray<IResourceLoaderFactory> resourceLoaderFactories, ISecurityContext securityContext, IErrorProcessor errorProcessor, Uri? baseUri)
		{
			_stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
			_dataModelHandler = dataModelHandler ?? throw new ArgumentNullException(nameof(dataModelHandler));
			_customActionProviders = customActionProviders;
			_resourceLoaderFactories = resourceLoaderFactories;
			_securityContext = securityContext;
			_errorProcessor = errorProcessor;
			_baseUri = baseUri;
			_preDataModelProcessor = new PreDataModelProcessor(errorProcessor, resourceLoaderFactories, securityContext);
			_idMap = new Dictionary<IIdentifier, StateEntityNode>(IdentifierEqualityComparer.Instance);
			_entities = new List<IEntity>();
			_targetMap = new List<TransitionNode>();
			_documentIdList = new LinkedList<int>();
		}

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

		public async ValueTask<InterpreterModel> Build(CancellationToken token)
		{
			_idMap.Clear();
			_entities.Clear();
			_targetMap.Clear();
			_documentIdList.Clear();
			_dataModelNodeArray = null;
			_externalScriptList = null;
			_inParallel = false;
			_deepLevel = 0;
			_counter = 0;

			var stateMachine = _stateMachine;

			await _preDataModelProcessor.PreProcessStateMachine(stateMachine, _customActionProviders, token).ConfigureAwait(false);

			Visit(ref stateMachine);

			foreach (var transition in _targetMap)
			{
				if (!transition.TryMapTarget(_idMap))
				{
					_errorProcessor.AddError<InterpreterModelBuilder>(entity: null, Resources.ErrorMessage_Target_Id_does_not_exists);
				}
			}

			var entityMap = CreateEntityMap();

			_documentIdList.Clear();

			var model = new InterpreterModel(stateMachine.As<StateMachineNode>(), _counter, entityMap, _dataModelNodeArray?.ToImmutable() ?? default);

			if (_externalScriptList is not null)
			{
				await SetExternalResources(_externalScriptList, token).ConfigureAwait(false);
			}

			return model;
		}

		private ImmutableDictionary<int, IEntity> CreateEntityMap()
		{
			var id = 0;
			for (var node = _documentIdList.First; node is not null; node = node.Next)
			{
				node.Value = id ++;
			}

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

		private async ValueTask SetExternalResources(List<(Uri Uri, IExternalScriptConsumer Consumer)> externalScriptList, CancellationToken token)
		{
			foreach (var (uri, consumer) in externalScriptList)
			{
				token.ThrowIfCancellationRequested();

				await LoadAndSetContent(uri, consumer, token).ConfigureAwait(false);
			}
		}

		private async ValueTask LoadAndSetContent(Uri uri, IExternalScriptConsumer consumer, CancellationToken token)
		{
			uri = _baseUri.CombineWith(uri);
			var factoryContext = new FactoryContext(_resourceLoaderFactories, _securityContext);
			var resource = await factoryContext.GetResource(uri, token).ConfigureAwait(false);
			await using (resource.ConfigureAwait(false))
			{
				consumer.SetContent(await resource.GetContent(token).ConfigureAwait(false));
			}
		}

		private void RegisterEntity(IEntity entity) => _entities.Add(entity);

		private DocumentIdRecord NewDocumentId() => new(_documentIdList);

		private static DocumentIdRecord NewDocumentIdAfter(in DocumentIdRecord previous) => previous.After();

		protected override void Build(ref IStateMachine stateMachine, ref StateMachineEntity stateMachineProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref stateMachine, ref stateMachineProperties);

			if (!stateMachineProperties.States.IsDefaultOrEmpty && stateMachineProperties.Initial is null)
			{
				var initialDocId = NewDocumentIdAfter(documentId);
				var target = ImmutableArray.Create(stateMachineProperties.States[index: 0].As<StateEntityNode>());
				var transition = new TransitionNode(NewDocumentIdAfter(initialDocId), transition: default, target);
				stateMachineProperties.Initial = new InitialNode(initialDocId, transition);
			}

			var stateMachineNode = new StateMachineNode(documentId, stateMachineProperties);

			if (_dataModelNodeArray is not null && stateMachineNode.DataModel is not null)
			{
				_dataModelNodeArray.Remove(stateMachineNode.DataModel);
			}

			stateMachine = stateMachineNode;
			RegisterEntity(stateMachine);
		}

		protected override void Build(ref IState state, ref StateEntity stateProperties)
		{
			var documentId = NewDocumentId();

			CounterBefore(inParallel: false, out var saved);
			base.Build(ref state, ref stateProperties);
			CounterAfter(saved);

			StateNode newState;

			if (!stateProperties.States.IsDefaultOrEmpty)
			{
				if (stateProperties.Initial is null)
				{
					var initialDocId = NewDocumentIdAfter(documentId);
					var target = ImmutableArray.Create(stateProperties.States[index: 0].As<StateEntityNode>());
					var transition = new TransitionNode(NewDocumentIdAfter(initialDocId), transition: default, target);
					stateProperties.Initial = new InitialNode(initialDocId, transition);
				}

				newState = new CompoundNode(documentId, stateProperties);
			}
			else
			{
				newState = new StateNode(documentId, stateProperties);
			}

			_idMap.Add(newState.Id, newState);

			state = newState;
			RegisterEntity(state);
		}

		protected override void Build(ref IParallel parallel, ref ParallelEntity parallelProperties)
		{
			var documentId = NewDocumentId();

			CounterBefore(inParallel: false, out var saved);
			base.Build(ref parallel, ref parallelProperties);
			CounterAfter(saved);

			var newParallel = new ParallelNode(documentId, parallelProperties);
			_idMap.Add(newParallel.Id, newParallel);

			parallel = newParallel;
			RegisterEntity(parallel);
		}

		protected override void Build(ref IFinal final, ref FinalEntity finalProperties)
		{
			var documentId = NewDocumentId();

			CounterBefore(inParallel: false, out var saved);
			base.Build(ref final, ref finalProperties);
			CounterAfter(saved);

			var newFinal = new FinalNode(documentId, finalProperties);
			_idMap.Add(newFinal.Id, newFinal);

			final = newFinal;
			RegisterEntity(final);
		}

		protected override void Build(ref IHistory history, ref HistoryEntity historyProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref history, ref historyProperties);

			var newHistory = new HistoryNode(documentId, historyProperties);
			_idMap.Add(newHistory.Id, newHistory);

			history = newHistory;
			RegisterEntity(history);
		}

		protected override void Build(ref IInitial initial, ref InitialEntity initialProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref initial, ref initialProperties);

			initial = new InitialNode(documentId, initialProperties);
			RegisterEntity(initial);
		}

		protected override void Build(ref ITransition transition, ref TransitionEntity transitionProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref transition, ref transitionProperties);

			var newTransition = new TransitionNode(documentId, transitionProperties);

			if (!transitionProperties.Target.IsDefaultOrEmpty)
			{
				_targetMap.Add(newTransition);
			}

			transition = newTransition;
			RegisterEntity(transition);
		}

		protected override void Build(ref IOnEntry onEntry, ref OnEntryEntity onEntryProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref onEntry, ref onEntryProperties);

			onEntry = new OnEntryNode(documentId, onEntryProperties);
			RegisterEntity(onEntry);
		}

		protected override void Build(ref IOnExit onExit, ref OnExitEntity onExitProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref onExit, ref onExitProperties);

			onExit = new OnExitNode(documentId, onExitProperties);
			RegisterEntity(onExit);
		}

		protected override void Build(ref IDataModel dataModel, ref DataModelEntity dataModelProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref dataModel, ref dataModelProperties);

			var dataModelNode = new DataModelNode(documentId, dataModelProperties);

			(_dataModelNodeArray ??= ImmutableArray.CreateBuilder<DataModelNode>()).Add(dataModelNode);

			dataModel = dataModelNode;
			RegisterEntity(dataModel);
		}

		protected override void Build(ref IData data, ref DataEntity dataProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref data, ref dataProperties);

			data = new DataNode(documentId, dataProperties);
			RegisterEntity(data);
		}

		protected override void Build(ref IInvoke invoke, ref InvokeEntity invokeProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref invoke, ref invokeProperties);

			invoke = new InvokeNode(documentId, invokeProperties);
			RegisterEntity(invoke);
		}

		protected override void Build(ref IScriptExpression scriptExpression, ref ScriptExpression scriptExpressionProperties)
		{
			NewDocumentId();

			base.Build(ref scriptExpression, ref scriptExpressionProperties);

			scriptExpression = new ScriptExpressionNode(scriptExpressionProperties);
			RegisterEntity(scriptExpression);
		}

		protected override void Build(ref IExternalScriptExpression externalScriptExpression, ref ExternalScriptExpression externalScriptExpressionProperties)
		{
			NewDocumentId();

			base.Build(ref externalScriptExpression, ref externalScriptExpressionProperties);

			var externalScriptExpressionNode = new ExternalScriptExpressionNode(externalScriptExpressionProperties);
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

		protected override void Build(ref ICustomAction customAction, ref CustomActionEntity customActionProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref customAction, ref customActionProperties);

			customAction = new CustomActionNode(documentId, customActionProperties);
			RegisterEntity(customAction);
		}

		protected override void Build(ref IExternalDataExpression externalDataExpression, ref ExternalDataExpression externalDataExpressionProperties)
		{
			NewDocumentId();

			base.Build(ref externalDataExpression, ref externalDataExpressionProperties);

			externalDataExpression = new ExternalDataExpressionNode(externalDataExpressionProperties);
			RegisterEntity(externalDataExpression);
		}

		protected override void Build(ref IValueExpression valueExpression, ref ValueExpression valueExpressionProperties)
		{
			NewDocumentId();

			base.Build(ref valueExpression, ref valueExpressionProperties);

			valueExpression = new ValueExpressionNode(valueExpressionProperties);
			RegisterEntity(valueExpression);
		}

		protected override void Build(ref ILocationExpression locationExpression, ref LocationExpression locationExpressionProperties)
		{
			NewDocumentId();

			base.Build(ref locationExpression, ref locationExpressionProperties);

			locationExpression = new LocationExpressionNode(locationExpressionProperties);
			RegisterEntity(locationExpression);
		}

		protected override void Build(ref IConditionExpression conditionExpression, ref ConditionExpression conditionExpressionProperties)
		{
			NewDocumentId();

			base.Build(ref conditionExpression, ref conditionExpressionProperties);

			conditionExpression = new ConditionExpressionNode(conditionExpressionProperties);
			RegisterEntity(conditionExpression);
		}

		protected override void Build(ref IDoneData doneData, ref DoneDataEntity doneDataProperties)
		{
			NewDocumentId();

			base.Build(ref doneData, ref doneDataProperties);

			doneData = new DoneDataNode(doneDataProperties);
			RegisterEntity(doneData);
		}

		protected override void Build(ref IContent content, ref ContentEntity contentProperties)
		{
			NewDocumentId();

			base.Build(ref content, ref contentProperties);

			content = new ContentNode(contentProperties);
			RegisterEntity(content);
		}

		protected override void Build(ref IFinalize finalize, ref FinalizeEntity finalizeProperties)
		{
			NewDocumentId();

			base.Build(ref finalize, ref finalizeProperties);

			finalize = new FinalizeNode(finalizeProperties);
			RegisterEntity(finalize);
		}

		protected override void Build(ref IParam param, ref ParamEntity paramProperties)
		{
			NewDocumentId();

			var documentId = NewDocumentId();

			base.Build(ref param, ref paramProperties);

			param = new ParamNode(documentId, paramProperties);
			RegisterEntity(param);
		}

		protected override void Visit(ref IIdentifier entity)
		{
			NewDocumentId();

			base.Visit(ref entity);

			entity = new IdentifierNode(entity);
			RegisterEntity(entity);
		}

		protected override void Visit(ref IOutgoingEvent entity)
		{
			NewDocumentId();

			base.Visit(ref entity);

			entity = new EventNode(entity);
			RegisterEntity(entity);
		}

		protected override void Visit(ref IEventDescriptor entity)
		{
			NewDocumentId();

			base.Visit(ref entity);

			entity = new EventDescriptorNode(entity);
			RegisterEntity(entity);
		}

		protected override void Build(ref ICancel cancel, ref CancelEntity cancelProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref cancel, ref cancelProperties);

			cancel = new CancelNode(documentId, cancelProperties);
			RegisterEntity(cancel);
		}

		protected override void Build(ref IAssign assign, ref AssignEntity assignProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref assign, ref assignProperties);

			assign = new AssignNode(documentId, assignProperties);
			RegisterEntity(assign);
		}

		protected override void Build(ref IForEach forEach, ref ForEachEntity forEachProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref forEach, ref forEachProperties);

			forEach = new ForEachNode(documentId, forEachProperties);
			RegisterEntity(forEachProperties);
		}

		protected override void Build(ref IIf @if, ref IfEntity ifProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref @if, ref ifProperties);

			@if = new IfNode(documentId, ifProperties);
			RegisterEntity(@if);
		}

		protected override void Build(ref IElseIf elseIf, ref ElseIfEntity elseIfProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref elseIf, ref elseIfProperties);

			elseIf = new ElseIfNode(documentId, elseIfProperties);
			RegisterEntity(elseIf);
		}

		protected override void Build(ref IElse @else, ref ElseEntity elseProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref @else, ref elseProperties);

			@else = new ElseNode(documentId, elseProperties);
			RegisterEntity(@else);
		}

		protected override void Build(ref ILog log, ref LogEntity logProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref log, ref logProperties);

			log = new LogNode(documentId, logProperties);
			RegisterEntity(log);
		}

		protected override void Build(ref IRaise raise, ref RaiseEntity raiseProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref raise, ref raiseProperties);

			raise = new RaiseNode(documentId, raiseProperties);
			RegisterEntity(raise);
		}

		protected override void Build(ref ISend send, ref SendEntity sendProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref send, ref sendProperties);

			send = new SendNode(documentId, sendProperties);
			RegisterEntity(send);
		}

		protected override void Build(ref IScript script, ref ScriptEntity scriptProperties)
		{
			var documentId = NewDocumentId();

			base.Build(ref script, ref scriptProperties);

			script = new ScriptNode(documentId, scriptProperties);
			RegisterEntity(script);
		}

		protected override void VisitUnknown(ref IExecutableEntity entity)
		{
			var documentId = NewDocumentId();

			base.VisitUnknown(ref entity);

			if (!entity.Is<IStoreSupport>())
			{
				entity = new RuntimeExecNode(documentId, entity);
				RegisterEntity(entity);
			}
		}

		protected override void Visit(ref IExecutableEntity executableEntity)
		{
			if (_deepLevel == 0)
			{
				_preDataModelProcessor.PostProcess(ref executableEntity);
				_dataModelHandler.Process(ref executableEntity);
			}

			_deepLevel ++;
			base.Visit(ref executableEntity);
			_deepLevel --;
		}

		protected override void Visit(ref IDataModel dataModel)
		{
			if (_deepLevel == 0)
			{
				_dataModelHandler.Process(ref dataModel);
			}

			_deepLevel ++;
			base.Visit(ref dataModel);
			_deepLevel --;
		}

		protected override void Visit(ref IDoneData doneData)
		{
			if (_deepLevel == 0)
			{
				_dataModelHandler.Process(ref doneData);
			}

			_deepLevel ++;
			base.Visit(ref doneData);
			_deepLevel --;
		}

		protected override void Visit(ref IInvoke invoke)
		{
			if (_deepLevel == 0)
			{
				_dataModelHandler.Process(ref invoke);
			}

			_deepLevel ++;
			base.Visit(ref invoke);
			_deepLevel --;
		}
	}
}