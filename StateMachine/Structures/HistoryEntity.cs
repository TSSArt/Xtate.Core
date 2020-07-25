using System;

namespace Xtate
{
	public struct HistoryEntity : IHistory, IVisitorEntity<HistoryEntity, IHistory>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IHistory

		public IIdentifier? Id         { get; set; }
		public HistoryType  Type       { get; set; }
		public ITransition? Transition { get; set; }

	#endregion

	#region Interface IVisitorEntity<HistoryEntity,IHistory>

		void IVisitorEntity<HistoryEntity, IHistory>.Init(IHistory source)
		{
			Ancestor = source;
			Id = source.Id;
			Type = source.Type;
			Transition = source.Transition;
		}

		bool IVisitorEntity<HistoryEntity, IHistory>.RefEquals(ref HistoryEntity other) =>
				Type == other.Type &&
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(Transition, other.Transition);

	#endregion
	}
}