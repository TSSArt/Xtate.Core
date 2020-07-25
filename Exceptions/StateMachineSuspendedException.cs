using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class StateMachineSuspendedException : XtateException
	{
		public StateMachineSuspendedException() { }

		public StateMachineSuspendedException(string? message) : base(message) { }

		public StateMachineSuspendedException(string? message, Exception? inner) : base(message, inner) { }

		protected StateMachineSuspendedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}