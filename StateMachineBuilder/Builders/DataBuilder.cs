using System;

namespace Xtate.Builder
{
	public class DataBuilder : BuilderBase, IDataBuilder
	{
		private IValueExpression?        _expression;
		private string?                  _id;
		private IInlineContent?          _inlineContent;
		private IExternalDataExpression? _source;

		public DataBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IDataBuilder

		public IData Build() => new DataEntity { Ancestor = Ancestor, Id = _id, Source = _source, Expression = _expression, InlineContent = _inlineContent };

		public void SetId(string id)
		{
			if (string.IsNullOrEmpty(id)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(id));

			_id = id;
		}

		public void SetSource(IExternalDataExpression source) => _source = source ?? throw new ArgumentNullException(nameof(source));

		public void SetExpression(IValueExpression expression) => _expression = expression ?? throw new ArgumentNullException(nameof(expression));

		public void SetInlineContent(IInlineContent inlineContent) => _inlineContent = inlineContent ?? throw new ArgumentNullException(nameof(inlineContent));

	#endregion
	}
}