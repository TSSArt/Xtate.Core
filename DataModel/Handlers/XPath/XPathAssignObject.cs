namespace Xtate.DataModel.XPath
{
	internal sealed class XPathAssignObject : XPathObject
	{
		public XPathAssignObject(IObject inner, XPathAssignType type, string? attrName) : base(inner)
		{
			AssignType = type;
			AssignAttributeName = attrName;
		}

		public XPathAssignType AssignType { get; }

		public string? AssignAttributeName { get; }
	}
}