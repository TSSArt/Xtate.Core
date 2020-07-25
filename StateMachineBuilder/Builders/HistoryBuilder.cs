using System;

namespace Xtate.Builder
{
	public class HistoryBuilder : BuilderBase, IHistoryBuilder
	{
		private IIdentifier? _id;
		private ITransition? _transition;
		private HistoryType  _type;

		public HistoryBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IHistoryBuilder

		public IHistory Build() => new HistoryEntity { Ancestor = Ancestor, Id = _id, Type = _type, Transition = _transition };

		public void SetId(IIdentifier id) => _id = id ?? throw new ArgumentNullException(nameof(id));

		public void SetType(HistoryType type) => _type = type;

		public void SetTransition(ITransition transition) => _transition = transition ?? throw new ArgumentNullException(nameof(transition));

	#endregion
	}
}