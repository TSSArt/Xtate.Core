#region Copyright © 2019-2021 Sergii Artemenko

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

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using Xtate.Core;

namespace Xtate.DataModel.XPath
{
	[PublicAPI]
	internal sealed class XPathDataModelHandler : DataModelHandlerBase
	{
		private const string DataModelType = "xpath";

		internal XPathDataModelHandler() : base(DefaultErrorProcessor.Instance) { }

		private XPathDataModelHandler(IErrorProcessor? errorProcessor) : base(errorProcessor) { }

		public static IDataModelHandlerFactory Factory { get; } = new DataModelHandlerFactory();

		public override ITypeInfo TypeInfo => TypeInfo<XPathDataModelHandler>.Instance;

		public override string ConvertToText(DataModelValue value) => XmlConverter.ToXml(value, indent: true);

		public override void ExecutionContextCreated(IExecutionContext executionContext, out ImmutableDictionary<string, string> dataModelVars)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			base.ExecutionContextCreated(executionContext, out dataModelVars);

			executionContext.RuntimeItems[XPathEngine.Key] = new XPathEngine(executionContext);
		}

		protected override void Visit(ref IValueExpression valueExpression)
		{
			base.Visit(ref valueExpression);

			if (valueExpression.Expression is not null)
			{
				try
				{
					CompileValueExpression(ref valueExpression);
				}
				catch (XPathException ex)
				{
					AddErrorMessage(valueExpression, Resources.Exception_ErrorOnParsingXPathExpression, ex);
				}
				catch (ArgumentException ex)
				{
					AddErrorMessage(valueExpression, Resources.Exception_ErrorOnParsingXPathExpression, ex);
				}
			}
			else
			{
				AddErrorMessage(valueExpression, Resources.Exception_ValueExpressionMustBePresent);
			}
		}

		private void CompileValueExpression(ref IValueExpression valueExpression)
		{
			Debug.Assert(valueExpression.Expression is not null);

			var compiledExpression = new XPathCompiledExpression(valueExpression.Expression, valueExpression);

			switch (compiledExpression.ReturnType)
			{
				case XPathResultType.Any:
				case XPathResultType.Boolean:
				case XPathResultType.String:
				case XPathResultType.NodeSet:
				case XPathResultType.Number:
					valueExpression = new XPathValueExpressionEvaluator(valueExpression, compiledExpression);
					break;

				case XPathResultType.Error:
					AddErrorMessage(valueExpression, Resources.Exception_ResultOfXPathExpressionCantBeIdentified);
					break;
				default:
					Infra.Unexpected(compiledExpression.ReturnType);
					break;
			}
		}

		protected override void Visit(ref IConditionExpression conditionExpression)
		{
			base.Visit(ref conditionExpression);

			if (conditionExpression.Expression is not null)
			{
				try
				{
					CompileConditionExpression(ref conditionExpression);
				}
				catch (XPathException ex)
				{
					AddErrorMessage(conditionExpression, Resources.Exception_ErrorOnParsingXPathExpression, ex);
				}
				catch (ArgumentException ex)
				{
					AddErrorMessage(conditionExpression, Resources.Exception_ErrorOnParsingXPathExpression, ex);
				}
			}
			else
			{
				AddErrorMessage(conditionExpression, Resources.Exception_ValueExpressionMustBePresent);
			}
		}

		private void CompileConditionExpression(ref IConditionExpression conditionExpression)
		{
			Debug.Assert(conditionExpression.Expression is not null);

			var compiledExpression = new XPathCompiledExpression(conditionExpression.Expression, conditionExpression);

			switch (compiledExpression.ReturnType)
			{
				case XPathResultType.Boolean:
				case XPathResultType.Any:
					conditionExpression = new XPathConditionExpressionEvaluator(conditionExpression, compiledExpression);
					break;

				case XPathResultType.String:
				case XPathResultType.NodeSet:
				case XPathResultType.Number:
					AddErrorMessage(conditionExpression, Resources.Exception_ResultOfXPathExpressionShouldBeBooleanValue);
					break;

				case XPathResultType.Error:
					AddErrorMessage(conditionExpression, Resources.Exception_ResultOfXPathExpressionCantBeIdentified);
					break;
				default:
					Infra.Unexpected(compiledExpression.ReturnType);
					break;
			}
		}

		protected override void Visit(ref ILocationExpression locationExpression)
		{
			base.Visit(ref locationExpression);

			if (locationExpression.Expression is not null)
			{
				try
				{
					CompileLocationExpression(ref locationExpression);
				}
				catch (XPathException ex)
				{
					AddErrorMessage(locationExpression, Resources.Exception_ErrorOnParsingXPathExpression, ex);
				}
				catch (ArgumentException ex)
				{
					AddErrorMessage(locationExpression, Resources.Exception_ErrorOnParsingXPathExpression, ex);
				}
			}
			else
			{
				AddErrorMessage(locationExpression, Resources.Exception_ValueExpressionMustBePresent);
			}
		}

		private void CompileLocationExpression(ref ILocationExpression locationExpression)
		{
			Debug.Assert(locationExpression.Expression is not null);

			var compiledExpression = new XPathCompiledExpression(locationExpression.Expression, locationExpression);

			switch (compiledExpression.ReturnType)
			{
				case XPathResultType.NodeSet:
				case XPathResultType.Any:
					locationExpression = new XPathLocationExpressionEvaluator(locationExpression, compiledExpression);
					break;

				case XPathResultType.Boolean:
				case XPathResultType.String:
				case XPathResultType.Number:
					AddErrorMessage(locationExpression, Resources.Exception_ResultOfXPathExpressionShouldBeElement);
					break;

				case XPathResultType.Error:
					AddErrorMessage(locationExpression, Resources.Exception_ResultOfXPathExpressionCantBeIdentified);
					break;
				default:
					Infra.Unexpected(compiledExpression.ReturnType);
					break;
			}
		}

		protected override void Visit(ref IContentBody contentBody)
		{
			base.Visit(ref contentBody);

			contentBody = new XPathContentBodyEvaluator(contentBody);
		}

		protected override void Visit(ref IInlineContent inlineContent)
		{
			base.Visit(ref inlineContent);

			inlineContent = new XPathInlineContentEvaluator(inlineContent);
		}

		protected override void Visit(ref IExternalDataExpression externalDataExpression)
		{
			base.Visit(ref externalDataExpression);

			externalDataExpression = new XPathExternalDataExpressionEvaluator(externalDataExpression);
		}

		protected override void Visit(ref IForEach forEach)
		{
			base.Visit(ref forEach);

			forEach = new XPathForEachEvaluator(forEach);
		}

		protected override void Build(ref AssignEntity assignProperties)
		{
			var parsed = XPathLocationExpression.TryParseAssignType(assignProperties.Type, out var assignType);

			if (parsed)
			{
				Infra.NotNull(assignProperties.Location);

				assignProperties.Location = new XPathLocationExpression(assignProperties.Location, assignType, assignProperties.Attribute);
			}

			base.Build(ref assignProperties);
		}

		protected override void Visit(ref IAssign assign)
		{
			base.Visit(ref assign);

			if (!assign.Location.Is<XPathLocationExpression>(out var xPathLocationExpression))
			{
				AddErrorMessage(assign, Resources.Exception_UnexpectedTypeAttributeValue);
			}
			else if (xPathLocationExpression.AssignType == XPathAssignType.AddAttribute && string.IsNullOrEmpty(assign.Attribute))
			{
				AddErrorMessage(assign, Resources.ErrorMessage_AttrAttributeShouldNotBeEmpty);
			}
		}

		protected override void Visit(ref IScript script) => AddErrorMessage(script, Resources.ErrorMessage_ScriptingNotSupportedInXPATHDataModel);

		private static bool CanHandle(string dataModelType) => dataModelType == DataModelType;

		private class DataModelHandlerFactory : IDataModelHandlerFactory
		{
		#region Interface IDataModelHandlerFactory

			public ValueTask<IDataModelHandlerFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, string dataModelType, CancellationToken token) =>
				new(CanHandle(dataModelType) ? DataModelHandlerFactoryActivator.Instance : null);

		#endregion
		}

		private class DataModelHandlerFactoryActivator : IDataModelHandlerFactoryActivator
		{
			public static IDataModelHandlerFactoryActivator Instance { get; } = new DataModelHandlerFactoryActivator();

		#region Interface IDataModelHandlerFactoryActivator

			public ValueTask<IDataModelHandler> CreateHandler(IFactoryContext factoryContext,
															  string dataModelType,
															  IErrorProcessor? errorProcessor,
															  CancellationToken token)
			{
				Infra.Assert(CanHandle(dataModelType));

				return new ValueTask<IDataModelHandler>(new XPathDataModelHandler(errorProcessor));
			}

		#endregion
		}
	}
}