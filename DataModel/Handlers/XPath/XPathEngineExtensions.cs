namespace Xtate.DataModel.XPath
{
	internal static class XPathEngineExtensions
	{
		public static XPathEngine Engine(this IExecutionContext executionContext) => XPathEngine.GetEngine(executionContext);
	}
}