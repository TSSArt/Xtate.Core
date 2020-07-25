using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class FinalizeBuilder : BuilderBase, IFinalizeBuilder
	{
		private ImmutableArray<IExecutableEntity>.Builder? _actions;

		public FinalizeBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IFinalizeBuilder

		public IFinalize Build() => new FinalizeEntity { Ancestor = Ancestor, Action = _actions?.ToImmutable() ?? default };

		public void AddAction(IExecutableEntity action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			(_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);
		}

	#endregion
	}
}