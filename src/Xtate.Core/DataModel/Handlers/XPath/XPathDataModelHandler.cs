﻿#region Copyright © 2019-2023 Sergii Artemenko

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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Xml.XPath;
using Xtate.Core;
using Xtate.Scxml;

namespace Xtate.DataModel.XPath;

public class XPathDataModelHandler : DataModelHandlerBase
{
	public required Func<IForEach, XPathForEachEvaluator>                                                  XPathForEachEvaluatorFactory                { private get; init; }
	public required Func<IContentBody, XPathContentBodyEvaluator>                                          XPathContentBodyEvaluatorFactory            { private get; init; }
	public required Func<IInlineContent, XPathInlineContentEvaluator>                                      XPathInlineContentEvaluatorFactory          { private get; init; }
	public required Func<IExternalDataExpression, XPathExternalDataExpressionEvaluator>                    XPathExternalDataExpressionEvaluatorFactory { private get; init; }
	public required IErrorProcessorService<XPathDataModelHandler>                                          XPathErrorProcessorService                  { private get; init; }
	public required Func<IValueExpression, XPathCompiledExpression, XPathValueExpressionEvaluator>         XPathValueExpressionEvaluatorFactory        { private get; init; }
	public required Func<IConditionExpression, XPathCompiledExpression, XPathConditionExpressionEvaluator> XPathConditionExpressionEvaluatorFactory    { private get; init; }
	public required Func<ILocationExpression, XPathCompiledExpression, XPathLocationExpressionEvaluator>   XPathLocationExpressionEvaluatorFactory     { private get; init; }

	public required Func<string, IXmlNamespacesInfo?, XPathCompiledExpression>                             XPathCompiledExpressionFactory              { private get; init; }

	public override string ConvertToText(DataModelValue value) => XmlConverter.ToXml(value, indent: true);

	protected override IForEach GetEvaluator(IForEach forEach) => XPathForEachEvaluatorFactory(forEach);

	protected override IContentBody GetEvaluator(IContentBody contentBody) => XPathContentBodyEvaluatorFactory(contentBody);

	protected override IExternalDataExpression GetEvaluator(IExternalDataExpression externalDataExpression) => XPathExternalDataExpressionEvaluatorFactory(externalDataExpression);

	protected override IInlineContent GetEvaluator(IInlineContent inlineContent) => XPathInlineContentEvaluatorFactory(inlineContent);

	/*
	private XPathExpressionContext CreateContext(object entity)
	{
		Infra.Requires(entity);

		var xPathExpressionContext = XPathExpressionContextFactory();

		if (entity.Is(out IXmlNamespacesInfo xmlNamespacesInfo))
		{
			foreach (var prefixNamespace in xmlNamespacesInfo.Namespaces)
			{
				xPathExpressionContext.AddNamespace(prefixNamespace.Prefix, prefixNamespace.Namespace);
			}
		}

		return xPathExpressionContext;
	}*/

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

		var xmlNamespacesInfo = valueExpression.Is<IXmlNamespacesInfo>(out var info) ? info : default;
		var compiledExpression = XPathCompiledExpressionFactory(valueExpression.Expression, xmlNamespacesInfo);

		switch (compiledExpression.ReturnType)
		{
			case XPathResultType.Any:
			case XPathResultType.Boolean:
			case XPathResultType.String:
			case XPathResultType.NodeSet:
			case XPathResultType.Number:
				valueExpression = XPathValueExpressionEvaluatorFactory(valueExpression, compiledExpression);
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

		var xmlNamespacesInfo = conditionExpression.Is<IXmlNamespacesInfo>(out var info) ? info : default;
		var compiledExpression = XPathCompiledExpressionFactory(conditionExpression.Expression, xmlNamespacesInfo);

		switch (compiledExpression.ReturnType)
		{
			case XPathResultType.Boolean:
			case XPathResultType.Any:
				conditionExpression = XPathConditionExpressionEvaluatorFactory(conditionExpression, compiledExpression);
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

		var xmlNamespacesInfo = locationExpression.Is<IXmlNamespacesInfo>(out var info) ? info : default;
		var compiledExpression = XPathCompiledExpressionFactory(locationExpression.Expression, xmlNamespacesInfo);

		switch (compiledExpression.ReturnType)
		{
			case XPathResultType.NodeSet:
			case XPathResultType.Any:
				locationExpression = XPathLocationExpressionEvaluatorFactory(locationExpression, compiledExpression);
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

	private void AddErrorMessage(object entity, string message, Exception? exception = default) => XPathErrorProcessorService.AddError(entity, message, exception);
}