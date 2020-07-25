using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class TransitionNode : ITransition, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
	{
		private readonly TransitionEntity _transition;
		private          DocumentIdRecord _documentIdNode;

		public TransitionNode(in DocumentIdRecord documentIdNode, in TransitionEntity transition, ImmutableArray<StateEntityNode> target = default)
		{
			_transition = transition;
			_documentIdNode = documentIdNode;
			TargetState = target;
			ActionEvaluators = transition.Action.AsArrayOf<IExecutableEntity, IExecEvaluator>(true);
			ConditionEvaluator = transition.Condition?.As<IBooleanEvaluator>();
			Source = null!;
		}

		public ImmutableArray<StateEntityNode> TargetState        { get; private set; }
		public StateEntityNode                 Source             { get; private set; }
		public ImmutableArray<IExecEvaluator>  ActionEvaluators   { get; }
		public IBooleanEvaluator?              ConditionEvaluator { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _transition.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		public FormattableString EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IDocumentId

		public int DocumentId => _documentIdNode.Value;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.TransitionNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntityList(Key.Event, EventDescriptors);
			bucket.AddEntity(Key.Condition, Condition);
			bucket.AddEntityList(Key.Target, Target);
			bucket.Add(Key.TransitionType, Type);
			bucket.AddEntityList(Key.Action, Action);
		}

	#endregion

	#region Interface ITransition

		public ImmutableArray<IEventDescriptor> EventDescriptors => _transition.EventDescriptors;

		public IExecutableEntity? Condition => _transition.Condition;

		public ImmutableArray<IIdentifier> Target => _transition.Target;

		public TransitionType Type => _transition.Type;

		public ImmutableArray<IExecutableEntity> Action => _transition.Action;

	#endregion

		public bool TryMapTarget(Dictionary<IIdentifier, StateEntityNode> idMap)
		{
			TargetState = ImmutableArray.CreateRange(Target, (id, map) => map.TryGetValue(id, out var node) ? node : null!, idMap);

			foreach (var node in TargetState)
			{
				if (node == null)
				{
					return false;
				}
			}

			return true;
		}

		public void SetSource(StateEntityNode source) => Source = source;
	}
}