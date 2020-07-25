using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class HistoryNode : StateEntityNode, IHistory, IAncestorProvider, IDebugEntityId
	{
		private readonly HistoryEntity _history;

		public HistoryNode(in DocumentIdRecord documentIdNode, in HistoryEntity history) : base(documentIdNode, children: null)
		{
			Infrastructure.Assert(history.Transition != null);

			_history = history;

			Id = history.Id ?? new IdentifierNode(Identifier.New());
			Transition = history.Transition.As<TransitionNode>();
			Transition.SetSource(this);
		}

		public TransitionNode Transition { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _history.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

	#endregion

	#region Interface IHistory

		public override IIdentifier Id   { get; }
		public          HistoryType Type => _history.Type;

		ITransition IHistory.Transition => _history.Transition!;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.HistoryNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.Add(Key.HistoryType, Type);
			bucket.AddEntity(Key.Id, Id);
			bucket.AddEntity(Key.Transition, Transition);
		}
	}
}