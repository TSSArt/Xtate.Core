using System;

namespace Xtate
{
	public struct DataEntity : IData, IVisitorEntity<DataEntity, IData>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IData

		public string?                  Id            { get; set; }
		public IExternalDataExpression? Source        { get; set; }
		public IValueExpression?        Expression    { get; set; }
		public IInlineContent?          InlineContent { get; set; }

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IVisitorEntity<DataEntity,IData>

		void IVisitorEntity<DataEntity, IData>.Init(IData source)
		{
			Ancestor = source;
			Id = source.Id;
			Source = source.Source;
			Expression = source.Expression;
			InlineContent = source.InlineContent;
		}

		bool IVisitorEntity<DataEntity, IData>.RefEquals(ref DataEntity other) =>
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(Source, other.Source) &&
				ReferenceEquals(InlineContent, other.InlineContent) &&
				ReferenceEquals(Expression, other.Expression);

	#endregion
	}
}