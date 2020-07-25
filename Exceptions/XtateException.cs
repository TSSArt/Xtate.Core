using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class XtateException : Exception
	{
		public XtateException() { }

		public XtateException(string? message) : base(message) { }

		public XtateException(string? message, Exception? inner) : base(message, inner) { }

		protected XtateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}