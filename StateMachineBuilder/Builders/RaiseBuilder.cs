using System;

namespace Xtate.Builder
{
	public class RaiseBuilder : BuilderBase, IRaiseBuilder
	{
		private IOutgoingEvent? _event;

		public RaiseBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IRaiseBuilder

		public IRaise Build() => new RaiseEntity { OutgoingEvent = _event };

		public void SetEvent(IOutgoingEvent evt) => _event = evt ?? throw new ArgumentNullException(nameof(evt));

	#endregion
	}
}