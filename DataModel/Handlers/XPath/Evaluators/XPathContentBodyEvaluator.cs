using System;
using System.Xml;

namespace Xtate.DataModel.XPath
{
	internal class XPathContentBodyEvaluator : DefaultContentBodyEvaluator
	{
		public XPathContentBodyEvaluator(in ContentBody contentBody) : base(contentBody) { }

		protected override DataModelValue ParseToDataModel(ref Exception? parseException)
		{
			try
			{
				return XmlConverter.FromXml(Value, this);
			}
			catch (XmlException ex)
			{
				parseException = ex;

				return Value.NormalizeSpaces();
			}
		}
	}
}