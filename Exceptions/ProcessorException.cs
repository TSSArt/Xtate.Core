using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class ProcessorException : XtateException
	{
		public ProcessorException() { }

		public ProcessorException(string? message) : base(message) { }

		public ProcessorException(string? message, Exception? inner) : base(message, inner) { }

		protected ProcessorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}