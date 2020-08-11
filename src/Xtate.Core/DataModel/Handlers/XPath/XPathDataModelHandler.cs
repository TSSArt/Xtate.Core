#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal sealed class XPathDataModelHandler : DataModelHandlerBase
	{
		private const string DataModelType = "xpath";

		public static readonly IDataModelHandlerFactory Factory = new DataModelHandlerFactory();

		public XPathDataModelHandler() : base(DefaultErrorProcessor.Instance) { }

		private XPathDataModelHandler(IErrorProcessor errorProcessor) : base(errorProcessor) { }

		public override void ExecutionContextCreated(IExecutionContext executionContext, out ImmutableDictionary<string, string> dataModelVars)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			base.ExecutionContextCreated(executionContext, out dataModelVars);

			executionContext.RuntimeItems[XPathEngine.Key] = new XPathEngine(executionContext);
		}

		protected override void Build(ref IValueExpression valueExpression, ref ValueExpression valueExpressionProperties)
		{
			base.Build(ref valueExpression, ref valueExpressionProperties);

			if (valueExpressionProperties.Expression != null)
			{
				try
				{
					var compiledExpression = new XPathCompiledExpression(valueExpressionProperties.Expression, valueExpression);

					switch (compiledExpression.ReturnType)
					{
						case XPathResultType.Any:
						case XPathResultType.Boolean:
						case XPathResultType.String:
						case XPathResultType.NodeSet:
						case XPathResultType.Number:
							valueExpression = new XPathValueExpressionEvaluator(valueExpressionProperties, compiledExpression);
							break;

						case XPathResultType.Error:
							AddErrorMessage(valueExpression, Resources.Exception_Result_of_XPath_expression_can_t_be_identified);
							break;
						default:
							Infrastructure.UnexpectedValue();
							break;
					}
				}
				catch (XPathException ex)
				{
					AddErrorMessage(valueExpression, Resources.Exception_Error_on_parsing_XPath_expression, ex);
				}
				catch (ArgumentException ex)
				{
					AddErrorMessage(valueExpression, Resources.Exception_Error_on_parsing_XPath_expression, ex);
				}
			}
			else
			{
				AddErrorMessage(valueExpression, Resources.Exception_Value_Expression_must_be_present);
			}
		}

		protected override void Build(ref IConditionExpression conditionExpression, ref ConditionExpression conditionExpressionProperties)
		{
			base.Build(ref conditionExpression, ref conditionExpressionProperties);

			if (conditionExpressionProperties.Expression != null)
			{
				try
				{
					var compiledExpression = new XPathCompiledExpression(conditionExpressionProperties.Expression, conditionExpression);

					switch (compiledExpression.ReturnType)
					{
						case XPathResultType.Boolean:
						case XPathResultType.Any:
							conditionExpression = new XPathConditionExpressionEvaluator(conditionExpressionProperties, compiledExpression);
							break;

						case XPathResultType.String:
						case XPathResultType.NodeSet:
						case XPathResultType.Number:
							AddErrorMessage(conditionExpression, Resources.Exception_Result_of_XPath_expression_should_be_boolean_value);
							break;

						case XPathResultType.Error:
							AddErrorMessage(conditionExpression, Resources.Exception_Result_of_XPath_expression_can_t_be_identified);
							break;
						default:
							Infrastructure.UnexpectedValue();
							break;
					}
				}
				catch (XPathException ex)
				{
					AddErrorMessage(conditionExpression, Resources.Exception_Error_on_parsing_XPath_expression, ex);
				}
				catch (ArgumentException ex)
				{
					AddErrorMessage(conditionExpression, Resources.Exception_Error_on_parsing_XPath_expression, ex);
				}
			}
			else
			{
				AddErrorMessage(conditionExpression, Resources.Exception_Value_Expression_must_be_present);
			}
		}

		protected override void Build(ref ILocationExpression locationExpression, ref LocationExpression locationExpressionProperties)
		{
			base.Build(ref locationExpression, ref locationExpressionProperties);

			if (locationExpressionProperties.Expression != null)
			{
				try
				{
					var compiledExpression = new XPathCompiledExpression(locationExpressionProperties.Expression, locationExpression);

					switch (compiledExpression.ReturnType)
					{
						case XPathResultType.NodeSet:
						case XPathResultType.Any:
							locationExpression = new XPathLocationExpressionEvaluator(locationExpressionProperties, compiledExpression);
							break;

						case XPathResultType.Boolean:
						case XPathResultType.String:
						case XPathResultType.Number:
							AddErrorMessage(locationExpression, Resources.Exception_Result_of_XPath_expression_should_be_element);
							break;

						case XPathResultType.Error:
							AddErrorMessage(locationExpression, Resources.Exception_Result_of_XPath_expression_can_t_be_identified);
							break;
						default:
							Infrastructure.UnexpectedValue();
							break;
					}
				}
				catch (XPathException ex)
				{
					AddErrorMessage(locationExpression, Resources.Exception_Error_on_parsing_XPath_expression, ex);
				}
				catch (ArgumentException ex)
				{
					AddErrorMessage(locationExpression, Resources.Exception_Error_on_parsing_XPath_expression, ex);
				}
			}
			else
			{
				AddErrorMessage(locationExpression, Resources.Exception_Value_Expression_must_be_present);
			}
		}

		protected override void Build(ref IContentBody contentBody, ref ContentBody contentBodyProperties)
		{
			base.Build(ref contentBody, ref contentBodyProperties);

			contentBody = new XPathContentBodyEvaluator(contentBodyProperties);
		}

		protected override void Build(ref IInlineContent inlineContent, ref InlineContent inlineContentProperties)
		{
			base.Build(ref inlineContent, ref inlineContentProperties);

			inlineContent = new XPathInlineContentEvaluator(inlineContentProperties);
		}

		protected override void Build(ref IForEach forEach, ref ForEachEntity forEachProperties)
		{
			base.Build(ref forEach, ref forEachProperties);

			forEach = new XPathForEachEvaluator(forEachProperties);
		}

		protected override void Build(ref IAssign assign, ref AssignEntity assignProperties)
		{
			base.Build(ref assign, ref assignProperties);

			if (XPathAssignEvaluator.TryParseAssignType(assign.Type, out var assignType))
			{
				assign = new XPathAssignEvaluator(assignProperties);
			}
			else
			{
				AddErrorMessage(assign, Resources.Exception_Unexpected_type_attribute_value);
			}

			if (assignType == XPathAssignType.AddAttribute && string.IsNullOrEmpty(assign.Attribute))
			{
				AddErrorMessage(assign, Resources.ErrorMessage_attr_attribute_should_no_be_empty);
			}
		}

		protected override void Visit(ref IScript script) => AddErrorMessage(script, Resources.ErrorMessage_Scripting_not_supported_in_XPATH_data_model);

		private class DataModelHandlerFactory : IDataModelHandlerFactory, IDataModelHandlerFactoryActivator
		{
		#region Interface IDataModelHandlerFactory

			public ValueTask<IDataModelHandlerFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, string dataModelType, CancellationToken token) =>
					new ValueTask<IDataModelHandlerFactoryActivator?>(CanHandle(dataModelType) ? this : null);

		#endregion

		#region Interface IDataModelHandlerFactoryActivator

			public ValueTask<IDataModelHandler> CreateHandler(IFactoryContext factoryContext, string dataModelType, IErrorProcessor errorProcessor, CancellationToken token)
			{
				Infrastructure.Assert(CanHandle(dataModelType));

				return new ValueTask<IDataModelHandler>(new XPathDataModelHandler(errorProcessor));
			}

		#endregion

			private static bool CanHandle(string dataModelType) => dataModelType == DataModelType;
		}
	}
}