using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class ExecutionException : XtateException
	{
		public ExecutionException() { }

		public ExecutionException(string? message) : base(message) { }

		public ExecutionException(string? message, Exception? inner) : base(message, inner) { }

		protected ExecutionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}