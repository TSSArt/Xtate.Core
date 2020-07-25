using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class IfBuilder : BuilderBase, IIfBuilder
	{
		private ImmutableArray<IExecutableEntity>.Builder? _actions;
		private IConditionExpression?                      _condition;

		public IfBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IIfBuilder

		public IIf Build() => new IfEntity { Ancestor = Ancestor, Condition = _condition, Action = _actions?.ToImmutable() ?? default };

		public void SetCondition(IConditionExpression condition) => _condition = condition ?? throw new ArgumentNullException(nameof(condition));

		public void AddAction(IExecutableEntity action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			(_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);
		}

	#endregion
	}
}