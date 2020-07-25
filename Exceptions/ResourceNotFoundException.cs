using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class ResourceNotFoundException : XtateException
	{
		public ResourceNotFoundException() { }

		public ResourceNotFoundException(string? message) : base(message) { }

		public ResourceNotFoundException(string? message, Exception? inner) : base(message, inner) { }

		protected ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}