using System;
using System.Runtime.Serialization;

namespace Xtate.DataModel.XPath
{
	[Serializable]
	public class XPathDataModelException : XtateException
	{
		public XPathDataModelException() { }

		public XPathDataModelException(string message) : base(message) { }

		public XPathDataModelException(string message, Exception inner) : base(message, inner) { }

		protected XPathDataModelException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}