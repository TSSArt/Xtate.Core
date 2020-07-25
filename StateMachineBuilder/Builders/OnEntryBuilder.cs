using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class OnEntryBuilder : BuilderBase, IOnEntryBuilder
	{
		private ImmutableArray<IExecutableEntity>.Builder? _actions;

		public OnEntryBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IOnEntryBuilder

		public IOnEntry Build() => new OnEntryEntity { Ancestor = Ancestor, Action = _actions?.ToImmutable() ?? default };

		public void AddAction(IExecutableEntity action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			(_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);
		}

	#endregion
	}
}