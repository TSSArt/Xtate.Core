using System;

namespace Xtate.Builder
{
	public class ElseIfBuilder : BuilderBase, IElseIfBuilder
	{
		private IConditionExpression? _condition;

		public ElseIfBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IElseIfBuilder

		public IElseIf Build() => new ElseIfEntity { Ancestor = Ancestor, Condition = _condition };

		public void SetCondition(IConditionExpression condition) => _condition = condition ?? throw new ArgumentNullException(nameof(condition));

	#endregion
	}
}