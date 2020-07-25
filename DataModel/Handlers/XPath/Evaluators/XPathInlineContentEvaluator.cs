using System;
using System.Xml;

namespace Xtate.DataModel.XPath
{
	internal class XPathInlineContentEvaluator : DefaultInlineContentEvaluator
	{
		public XPathInlineContentEvaluator(in InlineContent inlineContent) : base(inlineContent) { }

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