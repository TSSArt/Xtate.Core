using System;
using System.Runtime.Serialization;

namespace Xtate
{
	[Serializable]
	public class InfrastructureException : XtateException
	{
		public InfrastructureException() { }

		public InfrastructureException(string? message) : base(message) { }

		public InfrastructureException(string? message, Exception? inner) : base(message, inner) { }

		protected InfrastructureException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}