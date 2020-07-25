using System;

namespace Xtate.Builder
{
	public class ContentBuilder : BuilderBase, IContentBuilder
	{
		private IContentBody?     _body;
		private IValueExpression? _expression;

		public ContentBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IContentBuilder

		public IContent Build() => new ContentEntity { Ancestor = Ancestor, Expression = _expression, Body = _body };

		public void SetExpression(IValueExpression expression) => _expression = expression ?? throw new ArgumentNullException(nameof(expression));

		public void SetBody(IContentBody body) => _body = body ?? throw new ArgumentNullException(nameof(body));

	#endregion
	}
}