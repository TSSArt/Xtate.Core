// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.StateMachine;
using Xtate.NameTable;
using Xtate.DataModel.XPath.Services;
using Xtate.DataTypes;
using Xtate.StateMachine.Validator;

namespace Xtate.Core.Test.UnitTests.DataModel;

[TestClass]
public class XPathDataModelHandlerCoverageTest
{
	[TestMethod]
	[DataRow("unknown", null, false)]
	[DataRow("addattribute", "", false)]
	[DataRow("addattribute", "name", true)]
	[DataRow("replacechildren", null, true)]
	public void AssignmentValidatesTypeAndRequiredAttributeName(string type, string? attribute, bool valid)
	{
		var errors = new Mock<IErrorProcessorService<XPathDataModelHandler>>();
		IDataModelHandler handler = CreateHandler(errors.Object, null!, null!);
		IExecutableEntity assign = new AssignEntity
		{
			Type = type, Attribute = attribute,
			Location = new LocationExpression { Expression = "target" },
			Expression = new ValueExpression { Expression = "'new'" }
		};
		handler.Process(ref assign);
		Assert.IsInstanceOfType<DefaultAssignEvaluator>(assign);
		if (valid) errors.VerifyNoOtherCalls();
		else
		{
			var expected = type == "unknown" ? Resources.Exception_UnexpectedTypeAttributeValue : Resources.ErrorMessage_AttrAttributeShouldNotBeEmpty;
			errors.Verify(value => value.AddError(It.IsAny<object>(), expected, null), Times.Once);
		}
	}

	[TestMethod]
	public void ScriptsAreRejectedWithTheXPathScriptingDiagnostic()
	{
		var errors = new Mock<IErrorProcessorService<XPathDataModelHandler>>();
		IDataModelHandler handler = CreateHandler(errors.Object, null!, null!);
		IExecutableEntity script = new ScriptEntity();
		handler.Process(ref script);
		errors.Verify(value => value.AddError(It.IsAny<object>(), Resources.ErrorMessage_ScriptingNotSupportedInXPATHDataModel, null), Times.Once);
	}

	[TestMethod]
	public async Task ContentAndInlineBodiesAreParsedUsingXPathXmlEvaluators()
	{
		IDataModelHandler handler = CreateHandler(Mock.Of<IErrorProcessorService<XPathDataModelHandler>>(MockBehavior.Strict), null!, null!);
		IContentBody body = new ContentBody { Value = "<root>body</root>" };
		IInlineContent inline = new InlineContent { Value = "<root>inline</root>" };
		handler.Process(ref body);
		handler.Process(ref inline);
		Assert.AreEqual("body", (await ((IObjectEvaluator)body).EvaluateObject()).ToObject() is DataModelList bodyList ? bodyList["root"].AsString() : null);
		Assert.AreEqual("inline", (await ((IObjectEvaluator)inline).EvaluateObject()).ToObject() is DataModelList inlineList ? inlineList["root"].AsString() : null);
	}

	[TestMethod]
	public void HandlerConvertsValuesToIndentedXml()
	{
		var handler = CreateHandler(Mock.Of<IErrorProcessorService<XPathDataModelHandler>>(), null!, null!);

		var text = handler.ConvertToText(new DataModelValue("value"));

		StringAssert.Contains(text, substring: "value");
	}

	[TestMethod]
	[DataRow("value", null)]
	[DataRow("condition", null)]
	[DataRow("location", null)]
	[DataRow("value", "[")]
	[DataRow("condition", "[")]
	[DataRow("location", "[")]
	[DataRow("condition", "'text'")]
	[DataRow("condition", "1")]
	[DataRow("condition", "/root")]
	[DataRow("location", "true()")]
	[DataRow("location", "'text'")]
	[DataRow("location", "1")]
	public void InvalidExpressionsReportOneValidationError(string kind, string? expression)
	{
		var errors = new Mock<IErrorProcessorService<XPathDataModelHandler>>();
		var handler = CreateHandler(errors.Object, null!, null!);
		Process(handler, kind, expression);
		errors.Verify(value => value.AddError(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Exception?>()), Times.Once);
	}

	[TestMethod]
	[DataRow("value")]
	[DataRow("condition")]
	[DataRow("location")]
	public void CompilationArgumentFailureIsReportedAsValidationError(string kind)
	{
		var errors = new Mock<IErrorProcessorService<XPathDataModelHandler>>();
		var exception = new ArgumentException("invalid namespace configuration");
		var handler = CreateHandler(errors.Object, null!, null!, (_, _) => throw exception);
		Process(handler, kind, "valid");
		errors.Verify(value => value.AddError(It.IsAny<object>(), Resources.Exception_ErrorOnParsingXPathExpression, exception), Times.Once);
	}

	[TestMethod]
	[DataRow("value", "'text'")]
	[DataRow("value", "1")]
	[DataRow("value", "true()")]
	[DataRow("value", "/root")]
	[DataRow("condition", "true()")]
	[DataRow("location", "/root")]
	public void ValidExpressionsAreReplacedWithEvaluators(string kind, string expression)
	{
		var errors = new Mock<IErrorProcessorService<XPathDataModelHandler>>(MockBehavior.Strict);
		var handler = CreateHandler(errors.Object, null!, null!);
		var result = Process(handler, kind, expression);
		Assert.IsTrue(result is XPathValueExpressionEvaluator or XPathConditionExpressionEvaluator or XPathLocationExpressionEvaluator);
		errors.VerifyNoOtherCalls();
	}

	private static object Process(IDataModelHandler handler, string kind, string? expression)
	{
		switch (kind)
		{
			case "value":
				IValueExpression value = Mock.Of<IValueExpression>(item => item.Expression == expression);
				handler.Process(ref value);
				return value;
			case "condition":
				IConditionExpression condition = Mock.Of<IConditionExpression>(item => item.Expression == expression);
				handler.Process(ref condition);
				return condition;
			default:
				ILocationExpression location = Mock.Of<ILocationExpression>(item => item.Expression == expression);
				handler.Process(ref location);
				return location;
		}
	}

	private static XPathCompiledExpression Compile(string expression, IXmlNamespacesInfo? namespaces)
	{
		var names = Mock.Of<INameTableProvider>(value => value.GetNameTable() == new System.Xml.NameTable());
		var context = new XPathExpressionContext(names, namespaces)
		{
			FunctionProviders = [], XPathVarDescriptorFactory = null!, XPathEngineFactory = null!
		};
		return new XPathCompiledExpression(expression, namespaces, _ => context);
	}
	private static TestXPathDataModelHandler CreateHandler(IErrorProcessorService<XPathDataModelHandler> errorProcessor,
														   XPathForEachEvaluator forEachEvaluator,
														   XPathExternalDataExpressionEvaluator externalDataEvaluator, Func<string, IXmlNamespacesInfo?, XPathCompiledExpression>? compile = null) =>
		new()
		{
			XPathErrorProcessorService = errorProcessor,
			XPathForEachEvaluatorFactory = _ => forEachEvaluator,
			XPathExternalDataExpressionEvaluatorFactory = _ => externalDataEvaluator,
			XPathContentBodyEvaluatorFactory = value => new(value) { XPathXmlParserContextFactory = new() { NameTableProvider = null }, Logger = null! },
			XPathInlineContentEvaluatorFactory = value => new(value) { XPathXmlParserContextFactory = new() { NameTableProvider = null }, Logger = null! },
			XPathValueExpressionEvaluatorFactory = (value, compiled) => new(value, compiled) { EngineFactory = null! },
			XPathConditionExpressionEvaluatorFactory = (value, compiled) => new(value, compiled) { EngineFactory = null! },
			XPathLocationExpressionEvaluatorFactory = (value, compiled) => new(value, compiled) { EngineFactory = null! },
			XPathCompiledExpressionFactory = compile ?? Compile,
			DefaultLogEvaluatorFactory = _ => null!,
			DefaultSendEvaluatorFactory = _ => null!,
			DefaultCancelEvaluatorFactory = _ => null!,
			DefaultIfEvaluatorFactory = _ => null!,
			DefaultRaiseEvaluatorFactory = _ => null!,
			DefaultForEachEvaluatorFactory = _ => null!,
			DefaultAssignEvaluatorFactory = value => new(value),
			DefaultScriptEvaluatorFactory = _ => null!,
			DefaultCustomActionEvaluatorFactory = _ => null!,
			DefaultContentBodyEvaluatorFactory = _ => null!,
			DefaultInlineContentEvaluatorFactory = _ => null!,
			DefaultExternalDataExpressionEvaluatorFactory = _ => null!,
			CustomActionContainerFactory = _ => null!
		};

	private sealed class TestXPathDataModelHandler : XPathDataModelHandler;
}
