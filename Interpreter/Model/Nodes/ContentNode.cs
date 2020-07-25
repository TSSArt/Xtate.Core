using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ContentNode : IContent, IStoreSupport, IAncestorProvider
	{
		private readonly ContentEntity _content;

		public ContentNode(in ContentEntity content) => _content = content;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _content.Ancestor;

	#endregion

	#region Interface IContent

		public IValueExpression? Expression => _content.Expression;

		public IContentBody? Body => _content.Body;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ContentNode);
			bucket.AddEntity(Key.Expression, Expression);
			bucket.Add(Key.Body, Body?.Value);
		}

	#endregion
	}
}