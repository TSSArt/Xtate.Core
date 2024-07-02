#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using Xtate.CustomAction;
using Xtate.DataModel.Null;
using Xtate.DataModel.Runtime;
using Xtate.DataModel.XPath;
using Xtate.IoC;
using Xtate.Scxml;

namespace Xtate.DataModel;

public static class DataModelExtensions
{
	public static void RegisterDataModelHandlerBase(this IServiceCollection services)
	{
		if (services.IsRegistered<RegisteredDataModelHandlerBase>())
		{
			return;
		}

		services.RegisterLogging();

		services.AddTypeSync<DefaultAssignEvaluator, IAssign>();
		services.AddTypeSync<DefaultCancelEvaluator, ICancel>();
		services.AddTypeSync<DefaultContentBodyEvaluator, IContentBody>();
		services.AddTypeSync<DefaultCustomActionEvaluator, ICustomAction>();
		services.AddTypeSync<DefaultExternalDataExpressionEvaluator, IExternalDataExpression>();
		services.AddTypeSync<DefaultForEachEvaluator, IForEach>();
		services.AddTypeSync<DefaultIfEvaluator, IIf>();
		services.AddTypeSync<DefaultInlineContentEvaluator, IInlineContent>();
		services.AddTypeSync<DefaultLogEvaluator, ILog>();
		services.AddTypeSync<DefaultRaiseEvaluator, IRaise>();
		services.AddTypeSync<DefaultScriptEvaluator, IScript>();
		services.AddTypeSync<DefaultSendEvaluator, ISend>();

		services.AddTypeSync<CustomActionContainer, ICustomAction>();
		services.AddFactorySync<CustomActionFactory>().For<CustomActionBase, ICustomAction>();

		services.AddType<RegisteredDataModelHandlerBase>();
	}

	public static void RegisterNullDataModelHandler(this IServiceCollection services)
	{
		if (services.IsRegistered<NullDataModelHandler>())
		{
			return;
		}

		services.RegisterDataModelHandlerBase();
		services.RegisterErrorProcessor();

		services.AddTypeSync<NullConditionExpressionEvaluator, IConditionExpression, IIdentifier>();
		services.AddImplementation<NullDataModelHandler>().For<NullDataModelHandler>().For<IDataModelHandler>();
		services.AddImplementation<NullDataModelHandlerProvider>().For<IDataModelHandlerProvider>();
	}

	public static void RegisterRuntimeDataModelHandler(this IServiceCollection services)
	{
		if (services.IsRegistered<RuntimeDataModelHandler>())
		{
			return;
		}

		services.RegisterDataModelHandlerBase();
		services.RegisterErrorProcessor();

		services.AddTypeSync<RuntimeActionExecutor, RuntimeAction>();
		services.AddTypeSync<RuntimeValueEvaluator, RuntimeValue>();
		services.AddTypeSync<RuntimePredicateEvaluator, RuntimePredicate>();
		services.AddSharedType<RuntimeExecutionContext>(SharedWithin.Scope);
		services.AddImplementation<RuntimeDataModelHandler>().For<RuntimeDataModelHandler>().For<IDataModelHandler>();
		services.AddImplementation<RuntimeDataModelHandlerProvider>().For<IDataModelHandlerProvider>();
	}

	public static void RegisterXPathDataModelHandler(this IServiceCollection services)
	{
		if (services.IsRegistered<XPathDataModelHandler>())
		{
			return;
		}

		services.RegisterDataModelHandlerBase();
		services.RegisterErrorProcessor();
		services.RegisterNameTable();

		services.AddTypeSync<XPathValueExpressionEvaluator, IValueExpression, XPathCompiledExpression>();
		services.AddTypeSync<XPathConditionExpressionEvaluator, IConditionExpression, XPathCompiledExpression>();
		services.AddTypeSync<XPathLocationExpressionEvaluator, ILocationExpression, XPathCompiledExpression>();
		services.AddTypeSync<XPathLocationExpression, ILocationExpression, (XPathAssignType, string?)>();
		services.AddTypeSync<XPathContentBodyEvaluator, IContentBody>();
		services.AddTypeSync<XPathExternalDataExpressionEvaluator, IExternalDataExpression>();
		services.AddTypeSync<XPathForEachEvaluator, IForEach>();
		services.AddTypeSync<XPathInlineContentEvaluator, IInlineContent>();

		//services.AddType<XPathExpressionContextOld, IXmlNamespacesInfo?>();  //TODO:
		//services.AddType<XPathVarDescriptorOld, string>();

		services.AddTypeSync<XPathExpressionContext, IXmlNamespacesInfo?>();
		services.AddTypeSync<XPathVarDescriptor, string>();
		services.AddTypeSync<XPathCompiledExpression, string, IXmlNamespacesInfo?>();
		services.AddTypeSync<XPathXmlParserContextFactory>();
		services.AddSharedType<XPathEngine>(SharedWithin.Scope);

		services.AddImplementationSync<InFunctionProvider>().For<IXPathFunctionProvider>();
		services.AddTypeSync<InFunction>();

		services.AddImplementation<XPathDataModelHandler>().For<XPathDataModelHandler>().For<IDataModelHandler>();
		services.AddImplementation<XPathDataModelHandlerProvider>().For<IDataModelHandlerProvider>();
	}

	public static void RegisterDataModelHandlers(this IServiceCollection services)
	{
		if (services.IsRegistered<DataModelHandlerService>())
		{
			return;
		}

		services.RegisterNullDataModelHandler();
		services.RegisterRuntimeDataModelHandler();
		services.RegisterXPathDataModelHandler();
		services.RegisterErrorProcessor();

		services.AddType<UnknownDataModelHandler>();
		services.AddImplementation<DataModelHandlerService>().For<IDataModelHandlerService>();
		services.AddFactory<DataModelHandlerGetter>().For<IDataModelHandler>();
	}

	[UsedImplicitly]
	private class RegisteredDataModelHandlerBase;
}