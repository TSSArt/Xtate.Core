using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class CommunicationException : XtateException
	{
		public CommunicationException() { }

		public CommunicationException(string message) : base(message) { }

		public CommunicationException(string message, Exception innerException) : base(message, innerException) { }

		public CommunicationException(Exception inner, SessionId sessionId, SendId? sendId = default) : base(message: null, inner)
		{
			SessionId = sessionId;
			SendId = sendId;
		}

		protected CommunicationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public SessionId SessionId { get; } = default!;

		public SendId? SendId { get; }
	}
}