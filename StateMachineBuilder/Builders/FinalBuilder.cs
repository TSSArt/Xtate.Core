using System;
using System.Collections.Immutable;

namespace Xtate.Builder
{
	public class FinalBuilder : BuilderBase, IFinalBuilder
	{
		private IDoneData?                        _doneData;
		private IIdentifier?                      _id;
		private ImmutableArray<IOnEntry>.Builder? _onEntryList;
		private ImmutableArray<IOnExit>.Builder?  _onExitList;

		public FinalBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IFinalBuilder

		public IFinal Build() =>
				new FinalEntity { Ancestor = Ancestor, Id = _id, OnEntry = _onEntryList?.ToImmutable() ?? default, OnExit = _onExitList?.ToImmutable() ?? default, DoneData = _doneData };

		public void SetId(IIdentifier id) => _id = id ?? throw new ArgumentNullException(nameof(id));

		public void AddOnEntry(IOnEntry onEntry)
		{
			if (onEntry == null) throw new ArgumentNullException(nameof(onEntry));

			(_onEntryList ??= ImmutableArray.CreateBuilder<IOnEntry>()).Add(onEntry);
		}

		public void AddOnExit(IOnExit onExit)
		{
			if (onExit == null) throw new ArgumentNullException(nameof(onExit));

			(_onExitList ??= ImmutableArray.CreateBuilder<IOnExit>()).Add(onExit);
		}

		public void SetDoneData(IDoneData doneData) => _doneData = doneData ?? throw new ArgumentNullException(nameof(doneData));

	#endregion
	}
}