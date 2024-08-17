// Copyright © 2019-2024 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Xtate.DataModel.XPath;
using Xtate.IoC;
using Xtate.Scxml;

namespace Xtate.DataModel;

public class XPathDataModelHandlerModule : Module<DataModelHandlerBaseModule, ErrorProcessorModule, NameTableModule>
{
	protected override void AddServices()
	{
		Services.AddTypeSync<XPathValueExpressionEvaluator, IValueExpression, XPathCompiledExpression>();
		Services.AddTypeSync<XPathConditionExpressionEvaluator, IConditionExpression, XPathCompiledExpression>();
		Services.AddTypeSync<XPathLocationExpressionEvaluator, ILocationExpression, XPathCompiledExpression>();
		Services.AddTypeSync<XPathLocationExpression, ILocationExpression, (XPathAssignType, string?)>();
		Services.AddTypeSync<XPathContentBodyEvaluator, IContentBody>();
		Services.AddTypeSync<XPathExternalDataExpressionEvaluator, IExternalDataExpression>();
		Services.AddTypeSync<XPathForEachEvaluator, IForEach>();
		Services.AddTypeSync<XPathInlineContentEvaluator, IInlineContent>();

		//services.AddType<XPathExpressionContextOld, IXmlNamespacesInfo?>();  //TODO:
		//services.AddType<XPathVarDescriptorOld, string>();

		Services.AddTypeSync<XPathExpressionContext, IXmlNamespacesInfo?>();
		Services.AddTypeSync<XPathVarDescriptor, string>();
		Services.AddTypeSync<XPathCompiledExpression, string, IXmlNamespacesInfo?>();
		Services.AddTypeSync<XPathXmlParserContextFactory>();
		Services.AddSharedType<XPathEngine>(SharedWithin.Scope);

		Services.AddImplementationSync<InFunctionProvider>().For<IXPathFunctionProvider>();
		Services.AddTypeSync<InFunction>();

		Services.AddImplementation<XPathDataModelHandlerProvider>().For<IDataModelHandlerProvider>();

		var implementation = Services.AddImplementation<XPathDataModelHandler>().For<XPathDataModelHandler>();

		if (!Services.IsRegistered<IDataModelHandler>())
		{
			implementation.For<IDataModelHandler>();
		}
	}
}