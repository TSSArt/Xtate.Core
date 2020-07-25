using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class PlatformException : XtateException
	{
		public PlatformException() { }

		public PlatformException(string message) : base(message) { }

		public PlatformException(string message, Exception innerException) : base(message, innerException) { }

		public PlatformException(Exception inner, SessionId sessionId) : base(message: null, inner) => SessionId = sessionId;

		protected PlatformException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public SessionId SessionId { get; } = default!;
	}
}