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

using System.Xml.XPath;
using Xtate.Ancestor;
using Xtate.DataModel.Services;
using Xtate.DataModel.XPath;
using Xtate.DataModel.XPath.Services;
using Xtate.DataTypes;
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.NameTable;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class XPathEvaluatorCoverageTest
{
	[TestMethod]
	public async Task XPathFunctionBaseExposesArgumentMetadataInitializationAndInvokeAdapter()
	{
		var function = new TestXPathFunction();

		await function.Initialize();

		Assert.AreEqual(XPathResultType.String, function.ReturnType);
		CollectionAssert.AreEqual(new[] { XPathResultType.Number, XPathResultType.Boolean }, function.ArgTypes);
		Assert.AreEqual(expected: 2, function.Minargs);
		Assert.AreEqual(expected: 2, function.Maxargs);
		Assert.AreEqual(expected: "42:True", function.Invoke(null!, [42D, true], null!));
		CollectionAssert.AreEqual(new object[] { 42D, true }, function.LastArgs);
	}

	[TestMethod]
	public void XPathFunctionProviderBaseReturnsFactoryResultOnlyForConfiguredName()
	{
		var function = new TestXPathFunction();
		var provider = new TestXPathFunctionProvider
					   {
						   XPathFunctionFactory = () => function
					   };
		IXPathFunctionProvider functionProvider = provider;

		Assert.AreSame(function, functionProvider.TryGetFunction(ns: "urn:test", name: "fn"));
		Assert.IsNull(functionProvider.TryGetFunction(ns: "urn:test", name: "other"));
		Assert.IsNull(functionProvider.TryGetFunction(ns: "urn:other", name: "fn"));
	}

	[TestMethod]
	public async Task InlineContentEvaluatorExposesAncestorValueStringAndObjectEvaluation()
	{
		var inlineContent = new InlineContentSource { Value = "inline text" };
		var evaluator = new TestInlineContentEvaluator(inlineContent, new DataModelValue("object text"));

		Assert.AreSame(inlineContent, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual(expected: "inline text", evaluator.Value);
		Assert.AreEqual(expected: "inline text", await evaluator.EvaluateString());
		Assert.AreEqual(expected: "object text", DataModelValue.FromObject(await evaluator.EvaluateObject()).AsString());
	}

	[TestMethod]
	public async Task XPathInlineContentEvaluatorParsesXmlContentAndCachesObject()
	{
		var evaluator = new XPathInlineContentEvaluator(new InlineContentSource { Value = "<value>text</value>" })
						{
							Logger = CreateLogger<IInlineContent>(),
							XPathXmlParserContextFactory = CreateParserContextFactory()
						};

		Assert.AreEqual(expected: "<value>text</value>", evaluator.Value);
		Assert.AreEqual(expected: "<value>text</value>", await evaluator.EvaluateString());

		var first = await evaluator.EvaluateObject();
		var second = await evaluator.EvaluateObject();

		Assert.IsNotNull(first.ToObject());
		Assert.IsNotNull(second.ToObject());
	}

	[TestMethod]
	public async Task XPathContentBodyEvaluatorParsesXmlContentAndCachesObject()
	{
		var evaluator = new XPathContentBodyEvaluator(new ContentBodySource { Value = "<body>text</body>" })
						{
							Logger = CreateLogger<IContentBody>(),
							XPathXmlParserContextFactory = CreateParserContextFactory()
						};

		Assert.AreEqual(expected: "<body>text</body>", evaluator.Value);
		Assert.AreEqual(expected: "<body>text</body>", await evaluator.EvaluateString());

		var first = await evaluator.EvaluateObject();
		var second = await evaluator.EvaluateObject();

		Assert.IsNotNull(first.ToObject());
		Assert.IsNotNull(second.ToObject());
	}

	private static Deferred<ILogger<TSource>> CreateLogger<TSource>()
	{
		var logger = new Mock<ILogger<TSource>>();
		logger.Setup(static l => l.IsEnabled(It.IsAny<Level>())).Returns(true);

		return () => new ValueTask<ILogger<TSource>>(logger.Object);
	}

	private static XPathXmlParserContextFactory CreateParserContextFactory()
	{
		var nameTableProvider = new Mock<INameTableProvider>();
		nameTableProvider.Setup(static provider => provider.GetNameTable()).Returns(new System.Xml.NameTable());

		return new XPathXmlParserContextFactory { NameTableProvider = nameTableProvider.Object };
	}

	private sealed class TestXPathFunction() : XPathFunctionBase(XPathResultType.String, XPathResultType.Number, XPathResultType.Boolean)
	{
		public object[]? LastArgs { get; private set; }

		protected override object Invoke(object[] args)
		{
			LastArgs = args;

			return string.Join(separator: ":", args);
		}
	}

	private sealed class TestXPathFunctionProvider() : XPathFunctionProviderBase<TestXPathFunction>(ns: "urn:test", name: "fn");

	private sealed class InlineContentSource : IInlineContent
	{
	#region Interface IInlineContent

		public string? Value { get; init; }

	#endregion
	}

	private sealed class ContentBodySource : IContentBody
	{
	#region Interface IContentBody

		public string? Value { get; init; }

	#endregion
	}

	private sealed class TestInlineContentEvaluator(IInlineContent inlineContent, DataModelValue value) : InlineContentEvaluator(inlineContent)
	{
		public override ValueTask<IObject> EvaluateObject() => new(value);
	}
}