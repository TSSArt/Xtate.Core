using System;
using System.Collections.Immutable;

namespace Xtate
{
	public struct SendEntity : ISend, IVisitorEntity<SendEntity, ISend>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface ISend

		public IContent?                           Content          { get; set; }
		public IValueExpression?                   DelayExpression  { get; set; }
		public int?                                DelayMs          { get; set; }
		public string?                             EventName        { get; set; }
		public IValueExpression?                   EventExpression  { get; set; }
		public string?                             Id               { get; set; }
		public ILocationExpression?                IdLocation       { get; set; }
		public ImmutableArray<ILocationExpression> NameList         { get; set; }
		public ImmutableArray<IParam>              Parameters       { get; set; }
		public Uri?                                Target           { get; set; }
		public IValueExpression?                   TargetExpression { get; set; }
		public Uri?                                Type             { get; set; }
		public IValueExpression?                   TypeExpression   { get; set; }

	#endregion

	#region Interface IVisitorEntity<SendEntity,ISend>

		void IVisitorEntity<SendEntity, ISend>.Init(ISend source)
		{
			Ancestor = source;
			Id = source.Id;
			IdLocation = source.IdLocation;
			EventName = source.EventName;
			EventExpression = source.EventExpression;
			Content = source.Content;
			Target = source.Target;
			TargetExpression = source.TargetExpression;
			Type = source.Type;
			TypeExpression = source.TypeExpression;
			Parameters = source.Parameters;
			DelayMs = source.DelayMs;
			DelayExpression = source.DelayExpression;
			NameList = source.NameList;
		}

		bool IVisitorEntity<SendEntity, ISend>.RefEquals(ref SendEntity other) =>
				DelayMs == other.DelayMs &&
				Parameters == other.Parameters &&
				NameList == other.NameList &&
				ReferenceEquals(DelayExpression, other.DelayExpression) &&
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(IdLocation, other.IdLocation) &&
				ReferenceEquals(EventName, other.EventName) &&
				ReferenceEquals(EventExpression, other.EventExpression) &&
				ReferenceEquals(Target, other.Target) &&
				ReferenceEquals(TargetExpression, other.TargetExpression) &&
				ReferenceEquals(Type, other.Type) &&
				ReferenceEquals(TypeExpression, other.TypeExpression) &&
				ReferenceEquals(Content, other.Content);

	#endregion
	}
}