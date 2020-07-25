using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class XPathEmptyIterator : XPathNodeIterator
	{
		public static readonly XPathEmptyIterator Instance = new XPathEmptyIterator();

		public override XPathNavigator Current => default!;

		public override int CurrentPosition => 0;

		public override XPathNodeIterator Clone() => this;

		public override bool MoveNext() => false;
	}
}