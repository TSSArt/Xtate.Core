namespace Xtate
{
	public struct RaiseEntity : IRaise, IVisitorEntity<RaiseEntity, IRaise>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IRaise

		public IOutgoingEvent? OutgoingEvent { get; set; }

	#endregion

	#region Interface IVisitorEntity<RaiseEntity,IRaise>

		void IVisitorEntity<RaiseEntity, IRaise>.Init(IRaise source)
		{
			Ancestor = source;
			OutgoingEvent = source.OutgoingEvent;
		}

		bool IVisitorEntity<RaiseEntity, IRaise>.RefEquals(ref RaiseEntity other) => ReferenceEquals(OutgoingEvent, other.OutgoingEvent);

	#endregion
	}
}