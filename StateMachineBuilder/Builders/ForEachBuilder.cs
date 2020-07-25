using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class ForEachBuilder : BuilderBase, IForEachBuilder
	{
		private ImmutableArray<IExecutableEntity>.Builder? _actions;
		private IValueExpression?                          _array;
		private ILocationExpression?                       _index;
		private ILocationExpression?                       _item;

		public ForEachBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IForEachBuilder

		public IForEach Build() => new ForEachEntity { Ancestor = Ancestor, Array = _array, Item = _item, Index = _index, Action = _actions?.ToImmutable() ?? default };

		public void SetArray(IValueExpression array) => _array = array ?? throw new ArgumentNullException(nameof(array));

		public void SetItem(ILocationExpression item)
		{
			_item = item ?? throw new ArgumentNullException(nameof(item));
		}

		public void SetIndex(ILocationExpression index)
		{
			_index = index ?? throw new ArgumentNullException(nameof(index));
		}

		public void AddAction(IExecutableEntity action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			(_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);
		}

	#endregion
	}
}