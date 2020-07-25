using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal sealed class InFunction : XPathFunctionDescriptorBase
	{
		public InFunction() : base(string.Empty, name: @"In", new[] { XPathResultType.String }, XPathResultType.Boolean) { }

		protected override object Invoke(XPathResolver resolver, object[] args) => resolver.ExecutionContext?.InState((Identifier) XPathObject.ToString(args[0])) ?? false;
	}
}