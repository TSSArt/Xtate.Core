using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class PersistenceException : XtateException
	{
		public PersistenceException() { }

		public PersistenceException(string? message) : base(message) { }

		public PersistenceException(string? message, Exception? inner) : base(message, inner) { }

		protected PersistenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}