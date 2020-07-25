using System.Collections.Immutable;

namespace Xtate
{
	public struct CustomActionEntity : ICustomAction, IVisitorEntity<CustomActionEntity, ICustomAction>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface ICustomAction

		public string? Xml { get; set; }

		public ImmutableArray<ILocationExpression> Locations { get; set; }

		public ImmutableArray<IValueExpression> Values { get; set; }

	#endregion

	#region Interface IVisitorEntity<CustomActionEntity,ICustomAction>

		void IVisitorEntity<CustomActionEntity, ICustomAction>.Init(ICustomAction source)
		{
			Ancestor = source;
			Xml = source.Xml;
			Locations = source.Locations;
			Values = source.Values;
		}

		bool IVisitorEntity<CustomActionEntity, ICustomAction>.RefEquals(ref CustomActionEntity other) =>
				ReferenceEquals(Xml, other.Xml) &&
				Locations == other.Locations &&
				Values == other.Values;

	#endregion
	}
}