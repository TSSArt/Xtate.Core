using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class XPathSingleElementIterator : XPathNodeIterator
	{
		private readonly XPathNavigator _navigator;
		private          bool           _completed;

		public XPathSingleElementIterator(XPathNavigator navigator) => _navigator = navigator;

		public override XPathNavigator Current => _navigator;

		public override int CurrentPosition => _completed ? 1 : 0;

		public override XPathNodeIterator Clone() => new XPathSingleElementIterator(_navigator.Clone());

		public override bool MoveNext()
		{
			var completed = _completed;
			_completed = true;

			return !completed;
		}
	}
}