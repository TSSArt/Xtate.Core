using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class StateMachineUnhandledErrorException : XtateException
	{
		public StateMachineUnhandledErrorException() { }

		public StateMachineUnhandledErrorException(string? message) : base(message) { }

		public StateMachineUnhandledErrorException(string? message, Exception? inner) : base(message, inner) { }

		protected StateMachineUnhandledErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public StateMachineUnhandledErrorException(string message, Exception? innerException, UnhandledErrorBehaviour unhandledErrorBehaviour) : base(message, innerException) =>
				UnhandledErrorBehaviour = unhandledErrorBehaviour;

		public UnhandledErrorBehaviour UnhandledErrorBehaviour { get; }
	}
}