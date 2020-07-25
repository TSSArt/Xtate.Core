using System;

namespace Xtate.Builder
{
	public class ParamBuilder : BuilderBase, IParamBuilder
	{
		private IValueExpression?    _expression;
		private ILocationExpression? _location;
		private string?              _name;

		public ParamBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IParamBuilder

		public IParam Build() => new ParamEntity { Ancestor = Ancestor, Name = _name, Expression = _expression, Location = _location };

		public void SetName(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

			_name = name;
		}

		public void SetExpression(IValueExpression expression) => _expression = expression ?? throw new ArgumentNullException(nameof(expression));

		public void SetLocation(ILocationExpression location) => _location = location ?? throw new ArgumentNullException(nameof(location));

	#endregion
	}
}