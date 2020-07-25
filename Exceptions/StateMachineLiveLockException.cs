using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class StateMachineLiveLockException : XtateException
	{
		public StateMachineLiveLockException() { }

		public StateMachineLiveLockException(string message) : base(message) { }

		public StateMachineLiveLockException(string message, Exception inner) : base(message, inner) { }

		protected StateMachineLiveLockException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}