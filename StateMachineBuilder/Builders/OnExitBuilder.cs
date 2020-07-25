using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class OnExitBuilder : BuilderBase, IOnExitBuilder
	{
		private ImmutableArray<IExecutableEntity>.Builder? _actions;

		public OnExitBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IOnExitBuilder

		public IOnExit Build() => new OnExitEntity { Ancestor = Ancestor, Action = _actions?.ToImmutable() ?? default };

		public void AddAction(IExecutableEntity action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			(_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);
		}

	#endregion
	}
}