using System;

namespace Xtate.Builder
{
	public class LogBuilder : BuilderBase, ILogBuilder
	{
		private IValueExpression? _expression;
		private string?           _label;

		public LogBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface ILogBuilder

		public ILog Build() => new LogEntity { Ancestor = Ancestor, Label = _label, Expression = _expression };

		public void SetLabel(string label)
		{
			if (string.IsNullOrEmpty(label)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(label));

			_label = label;
		}

		public void SetExpression(IValueExpression expression) => _expression = expression ?? throw new ArgumentNullException(nameof(expression));

	#endregion
	}
}