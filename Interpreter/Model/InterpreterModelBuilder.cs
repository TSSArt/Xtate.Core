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
		private readonly LinkedList<int>                                    _documentIdList = new();
		private readonly List<IEntity>                                      _entities       = new();
		private readonly Dictionary<IIdentifier, StateEntityNode>           _idMap          = new(IdentifierEqualityComparer.Instance);
		private readonly Parameters                                         _parameters;
		private readonly PreDataModelProcessor                              _preDataModelProcessor;
		private readonly List<TransitionNode>                               _targetMap = new();
		private          int                                                _counter;
		private          ImmutableArray<DataModelNode>.Builder?             _dataModelNodeArray;
		private          int                                                _deepLevel;
		private          List<(Uri Uri, IExternalScriptConsumer Consumer)>? _externalScriptList;
		private          bool                                               _inParallel;

		public InterpreterModelBuilder(Parameters parameters)
		{
			_parameters = parameters;
			_preDataModelProcessor = new PreDataModelProcessor(parameters);
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
			_dataModelNodeArray = default;
			_externalScriptList = default;
			_inParallel = false;
			_deepLevel = 0;
			_counter = 0;

			await _preDataModelProcessor.PreProcessStateMachine(token).ConfigureAwait(false);

			var stateMachine = _parameters.StateMachine;
			Visit(ref stateMachine);

			foreach (var transition in _targetMap)
			{
				if (!transition.TryMapTarget(_idMap))
				{
					_parameters.ErrorProcessor.AddError<InterpreterModelBuilder>(entity: null, Resources.ErrorMessage_TargetIdDoesNotExists);
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
			uri = _parameters.BaseUri.CombineWith(uri);
			var factoryContext = new FactoryContext(_parameters.ResourceLoaderFactories, _parameters.SecurityContext, _parameters.Logger, _parameters.LoggerContext);
			var resource = await factoryContext.GetResource(uri, token).ConfigureAwait(false);
			await using (resource.ConfigureAwait(false))
			{
				consumer.SetContent(await resource.GetContent(token).ConfigureAwait(false));
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
				var transition = new TransitionNode(transitionNodeDocumentId, target);
				stateMachineProperties.Initial = new InitialNode(initialNodeDocumentId, transition);
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

			var newStateMachine = new StateMachineNode(documentId, stateMachine);

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
				var transition = new TransitionNode(transitionNodeDocumentId, target);
				stateProperties.Initial = new InitialNode(initialNodeDocumentId, transition);
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
				? new CompoundNode(documentId, state)
				: new StateNode(documentId, state);

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

			var newParallel = new ParallelNode(documentId, parallel);
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

			var newFinal = new FinalNode(documentId, final);
			_idMap.Add(newFinal.Id, newFinal);

			final = newFinal;
			RegisterEntity(final);
		}

		protected override void Visit(ref IHistory history)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref history);

			var newHistory = new HistoryNode(documentId, history);
			_idMap.Add(newHistory.Id, newHistory);

			history = newHistory;
			RegisterEntity(history);
		}

		protected override void Visit(ref IInitial initial)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref initial);

			initial = new InitialNode(documentId, initial);
			RegisterEntity(initial);
		}

		protected override void Visit(ref ITransition transition)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref transition);

			var newTransition = new TransitionNode(documentId, transition);

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

			onEntry = new OnEntryNode(documentId, onEntry);
			RegisterEntity(onEntry);
		}

		protected override void Visit(ref IOnExit onExit)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref onExit);

			onExit = new OnExitNode(documentId, onExit);
			RegisterEntity(onExit);
		}

		private void VisitInternal(ref IDataModel dataModel)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref dataModel);

			var dataModelNode = new DataModelNode(documentId, dataModel);

			(_dataModelNodeArray ??= ImmutableArray.CreateBuilder<DataModelNode>()).Add(dataModelNode);

			dataModel = dataModelNode;
			RegisterEntity(dataModel);
		}

		protected override void Visit(ref IData data)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref data);

			data = new DataNode(documentId, data);
			RegisterEntity(data);
		}

		private void VisitInternal(ref IInvoke invoke)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref invoke);

			invoke = new InvokeNode(documentId, invoke);
			RegisterEntity(invoke);
		}

		protected override void Visit(ref IScriptExpression scriptExpression)
		{
			CreateDocumentId();

			base.Visit(ref scriptExpression);

			scriptExpression = new ScriptExpressionNode(scriptExpression);
			RegisterEntity(scriptExpression);
		}

		protected override void Visit(ref IExternalScriptExpression externalScriptExpression)
		{
			CreateDocumentId();

			base.Visit(ref externalScriptExpression);

			var externalScriptExpressionNode = new ExternalScriptExpressionNode(externalScriptExpression);
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

			customAction = new CustomActionNode(documentId, customAction);
			RegisterEntity(customAction);
		}

		protected override void Visit(ref IExternalDataExpression externalDataExpression)
		{
			CreateDocumentId();

			base.Visit(ref externalDataExpression);

			externalDataExpression = new ExternalDataExpressionNode(externalDataExpression);
			RegisterEntity(externalDataExpression);
		}

		protected override void Visit(ref IValueExpression valueExpression)
		{
			CreateDocumentId();

			base.Visit(ref valueExpression);

			valueExpression = new ValueExpressionNode(valueExpression);
			RegisterEntity(valueExpression);
		}

		protected override void Visit(ref ILocationExpression locationExpression)
		{
			CreateDocumentId();

			base.Visit(ref locationExpression);

			locationExpression = new LocationExpressionNode(locationExpression);
			RegisterEntity(locationExpression);
		}

		protected override void Visit(ref IConditionExpression conditionExpression)
		{
			CreateDocumentId();

			base.Visit(ref conditionExpression);

			conditionExpression = new ConditionExpressionNode(conditionExpression);
			RegisterEntity(conditionExpression);
		}

		private void VisitInternal(ref IDoneData doneData)
		{
			CreateDocumentId();

			base.Visit(ref doneData);

			doneData = new DoneDataNode(doneData);
			RegisterEntity(doneData);
		}

		protected override void Visit(ref IContent content)
		{
			CreateDocumentId();

			base.Visit(ref content);

			content = new ContentNode(content);
			RegisterEntity(content);
		}

		protected override void Visit(ref IFinalize finalize)
		{
			CreateDocumentId();

			base.Visit(ref finalize);

			finalize = new FinalizeNode(finalize);
			RegisterEntity(finalize);
		}

		protected override void Visit(ref IParam param)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref param);

			param = new ParamNode(documentId, param);
			RegisterEntity(param);
		}

		protected override void Visit(ref IIdentifier entity)
		{
			CreateDocumentId();

			base.Visit(ref entity);

			entity = new IdentifierNode(entity);
			RegisterEntity(entity);
		}

		protected override void Visit(ref IOutgoingEvent entity)
		{
			CreateDocumentId();

			base.Visit(ref entity);

			entity = new EventNode(entity);
			RegisterEntity(entity);
		}

		protected override void Visit(ref IEventDescriptor entity)
		{
			CreateDocumentId();

			base.Visit(ref entity);

			entity = new EventDescriptorNode(entity);
			RegisterEntity(entity);
		}

		protected override void Visit(ref ICancel cancel)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref cancel);

			cancel = new CancelNode(documentId, cancel);
			RegisterEntity(cancel);
		}

		protected override void Visit(ref IAssign assign)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref assign);

			assign = new AssignNode(documentId, assign);
			RegisterEntity(assign);
		}

		protected override void Visit(ref IForEach forEach)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref forEach);

			forEach = new ForEachNode(documentId, forEach);
			RegisterEntity(forEach);
		}

		protected override void Visit(ref IIf @if)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref @if);

			@if = new IfNode(documentId, @if);
			RegisterEntity(@if);
		}

		protected override void Visit(ref IElseIf elseIf)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref elseIf);

			elseIf = new ElseIfNode(documentId, elseIf);
			RegisterEntity(elseIf);
		}

		protected override void Visit(ref IElse @else)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref @else);

			@else = new ElseNode(documentId, @else);
			RegisterEntity(@else);
		}

		protected override void Visit(ref ILog log)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref log);

			log = new LogNode(documentId, log);
			RegisterEntity(log);
		}

		protected override void Visit(ref IRaise raise)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref raise);

			raise = new RaiseNode(documentId, raise);
			RegisterEntity(raise);
		}

		protected override void Visit(ref ISend send)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref send);

			send = new SendNode(documentId, send);
			RegisterEntity(send);
		}

		protected override void Visit(ref IScript script)
		{
			var documentId = CreateDocumentId();

			base.Visit(ref script);

			script = new ScriptNode(documentId, script);
			RegisterEntity(script);
		}

		protected override void VisitUnknown(ref IExecutableEntity entity)
		{
			var documentId = CreateDocumentId();

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
				_parameters.DataModelHandler.Process(ref executableEntity);
			}

			_deepLevel ++;
			base.Visit(ref executableEntity);
			_deepLevel --;
		}

		protected override void Visit(ref IDataModel dataModel)
		{
			if (_deepLevel == 0)
			{
				_parameters.DataModelHandler.Process(ref dataModel);
			}

			_deepLevel ++;
			VisitInternal(ref dataModel);
			_deepLevel --;
		}

		protected override void Visit(ref IDoneData doneData)
		{
			if (_deepLevel == 0)
			{
				_parameters.DataModelHandler.Process(ref doneData);
			}

			_deepLevel ++;
			VisitInternal(ref doneData);
			_deepLevel --;
		}

		protected override void Visit(ref IInvoke invoke)
		{
			if (_deepLevel == 0)
			{
				_parameters.DataModelHandler.Process(ref invoke);
			}

			_deepLevel ++;
			VisitInternal(ref invoke);
			_deepLevel --;
		}

		[PublicAPI]
		internal record Parameters
		{
			public Parameters(IStateMachine stateMachine, IDataModelHandler dataModelHandler)
			{
				StateMachine = stateMachine;
				DataModelHandler = dataModelHandler;
			}

			public Uri?                                   BaseUri                 { get; init; }
			public ImmutableArray<ICustomActionFactory>   CustomActionProviders   { get; init; }
			public IDataModelHandler                      DataModelHandler        { get; init; }
			public IErrorProcessor?                       ErrorProcessor          { get; init; }
			public ILogger?                               Logger                  { get; init; }
			public ILoggerContext?                        LoggerContext           { get; init; }
			public ISecurityContext?                      SecurityContext         { get; init; }
			public ImmutableArray<IResourceLoaderFactory> ResourceLoaderFactories { get; init; }
			public IStateMachine                          StateMachine            { get; init; }
		}
	}
}