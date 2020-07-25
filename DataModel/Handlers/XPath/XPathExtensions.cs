using System;
using Xtate.DataModel.XPath;

namespace Xtate
{
	public static class XPathExtensions
	{
		public static StateMachineHostBuilder AddXPath(this StateMachineHostBuilder builder)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));

			builder.AddDataModelHandlerFactory(XPathDataModelHandler.Factory);

			return builder;
		}
	}
}
