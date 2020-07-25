using System;
using System.Collections.Immutable;

namespace Xtate
{
	public struct FinalEntity : IFinal, IVisitorEntity<FinalEntity, IFinal>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IFinal

		public IIdentifier?             Id       { get; set; }
		public ImmutableArray<IOnEntry> OnEntry  { get; set; }
		public ImmutableArray<IOnExit>  OnExit   { get; set; }
		public IDoneData?               DoneData { get; set; }

	#endregion

	#region Interface IVisitorEntity<FinalEntity,IFinal>

		void IVisitorEntity<FinalEntity, IFinal>.Init(IFinal source)
		{
			Ancestor = source;
			Id = source.Id;
			OnEntry = source.OnEntry;
			OnExit = source.OnExit;
			DoneData = source.DoneData;
		}

		bool IVisitorEntity<FinalEntity, IFinal>.RefEquals(ref FinalEntity other) =>
				OnExit == other.OnExit &&
				OnEntry == other.OnEntry &&
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(DoneData, other.DoneData);

	#endregion
	}
}