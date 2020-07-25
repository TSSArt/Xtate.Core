using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class InitialNode : StateEntityNode, IInitial, IAncestorProvider, IDebugEntityId
	{
		private readonly InitialEntity _initial;

		public InitialNode(in DocumentIdRecord documentIdNode, in InitialEntity initial) : base(documentIdNode, children: null)
		{
			Infrastructure.Assert(initial.Transition != null);

			_initial = initial;
			Transition = initial.Transition.As<TransitionNode>();

			Transition.SetSource(this);
		}

		public InitialNode(in DocumentIdRecord documentIdNode, TransitionNode transition) : base(documentIdNode, children: null)
		{
			Transition = transition ?? throw new ArgumentNullException(nameof(transition));

			Transition.SetSource(this);
		}

		public TransitionNode Transition { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _initial.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		public FormattableString EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IInitial

		ITransition IInitial.Transition => _initial.Transition!;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.InitialNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Transition, Transition);
		}
	}
}