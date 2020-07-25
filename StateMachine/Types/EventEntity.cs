using System;
using System.Collections.Immutable;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public struct EventEntity : IOutgoingEvent
	{
		public static readonly Uri InternalTarget = new Uri(uriString: "_internal", UriKind.Relative);
		public static readonly Uri ParentTarget   = new Uri(uriString: "_parent", UriKind.Relative);

		public EventEntity(string? val) : this()
		{
			if (!string.IsNullOrEmpty(val))
			{
				NameParts = EventName.ToParts(val);
			}
		}

		public string? RawData { get; set; }

	#region Interface IOutgoingEvent

		public DataModelValue              Data      { get; set; }
		public int                         DelayMs   { get; set; }
		public ImmutableArray<IIdentifier> NameParts { get; set; }
		public SendId?                     SendId    { get; set; }
		public Uri?                        Target    { get; set; }
		public Uri?                        Type      { get; set; }

	#endregion
	}
}