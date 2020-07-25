using System;

namespace Xtate.Builder
{
	public class AssignBuilder : BuilderBase, IAssignBuilder
	{
		private string?              _attribute;
		private IValueExpression?    _expression;
		private IInlineContent?      _inlineContent;
		private ILocationExpression? _location;
		private string?              _type;

		public AssignBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IAssignBuilder

		public IAssign Build() =>
				new AssignEntity
				{
						Ancestor = Ancestor, Location = _location, Expression = _expression,
						InlineContent = _inlineContent, Type = _type, Attribute = _attribute
				};

		public void SetLocation(ILocationExpression location) => _location = location ?? throw new ArgumentNullException(nameof(location));

		public void SetExpression(IValueExpression expression) => _expression = expression ?? throw new ArgumentNullException(nameof(expression));

		public void SetInlineContent(IInlineContent inlineContent) => _inlineContent = inlineContent ?? throw new ArgumentNullException(nameof(inlineContent));

		public void SetType(string type) => _type = type ?? throw new ArgumentNullException(nameof(type));

		public void SetAttribute(string attribute) => _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));

	#endregion
	}
}