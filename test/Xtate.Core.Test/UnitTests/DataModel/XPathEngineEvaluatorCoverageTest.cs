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

using System.IO;
using System.Net.Mime;
using System.Text;
using System.Xml.XPath;
using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.DataModel.XPath;
using Xtate.DataModel.XPath.Internal;
using Xtate.DataModel.XPath.Services;
using Xtate.DataTypes;
using Xtate.NameTable;
using Xtate.ResourceLoaders;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class XPathEngineEvaluatorCoverageTest
{
	[TestMethod]
	public void EngineCreatesRootAndScopedVariablesWithNearestScopePrecedence()
	{
		var root = new DataModelList { ["existing"] = "root" };
		var engine = CreateEngine(root);

		Assert.AreEqual(expected: "root", FirstValue(engine.GetVariable("existing")));
		Assert.AreEqual(string.Empty, FirstValue(engine.GetVariable("created")));
		Assert.IsTrue(root.ContainsKey(key: "created", caseInsensitive: false));

		engine.EnterScope();
		var local = Compile(expression: "local", engine);
		engine.DeclareVariable(local);
		Assert.IsFalse(root.ContainsKey(key: "local", caseInsensitive: false));
		Assert.AreEqual(string.Empty, FirstValue(engine.GetVariable("local")));
		Assert.AreEqual(expected: "local", engine.GetName(local));
		engine.LeaveScope();

		Assert.IsFalse(root.ContainsKey(key: "local", caseInsensitive: false));
		engine.DeclareVariable(Compile(expression: "ignored", engine));
		Assert.IsFalse(root.ContainsKey(key: "ignored", caseInsensitive: false));
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => engine.GetVariable(string.Empty));
	}

	[TestMethod]
	public async Task EngineEvaluatesScalarAndNodeSetResultsWithOptionalRootStripping()
	{
		var items = new DataModelList { { "item", "one" }, { "item", "two" } };
		var root = new DataModelList { ["number"] = 7, ["items"] = items };
		var engine = CreateEngine(root);

		var number = await engine.EvalObject(Compile(expression: "number(number)", engine), stripRoots: false);
		Assert.AreEqual(expected: 7, number.AsInteger());
		var nodes = await engine.EvalObject(Compile(expression: "items/item", engine), stripRoots: true);
		var iterator = nodes.AsIterator();
		Assert.IsTrue(iterator.MoveNext());
		Assert.AreEqual(expected: "one", iterator.Current!.Value);
		Assert.IsTrue(iterator.MoveNext());
		Assert.AreEqual(expected: "two", iterator.Current!.Value);
	}

	[TestMethod]
	public async Task EngineAssignmentCoversEveryMutationModeAndNonNodeResults()
	{
		foreach (var assignType in new[]
								   {
									   XPathAssignType.ReplaceChildren,
									   XPathAssignType.FirstChild,
									   XPathAssignType.LastChild,
									   XPathAssignType.PreviousSibling,
									   XPathAssignType.NextSibling,
									   XPathAssignType.Replace,
									   XPathAssignType.Delete
								   })
		{
			var root = new DataModelList { ["before"] = "before", ["target"] = new DataModelList { ["child"] = "old" }, ["after"] = "after" };
			var engine = CreateEngine(root);
			await engine.Assign(Compile(expression: "target", engine), assignType, attributeName: null, new XPathObject("new"));
		}

		var attributeRoot = new DataModelList { ["target"] = "value" };
		var attributeEngine = CreateEngine(attributeRoot);
		await attributeEngine.Assign(Compile(expression: "target", attributeEngine), XPathAssignType.AddAttribute, attributeName: "attr", new XPathObject("attribute"));
		Assert.Contains(substring: "attr=\"attribute\"", DataModelConverter.ToXml(attributeRoot));

		var unchangedRoot = new DataModelList { ["target"] = "value" };
		var unchangedEngine = CreateEngine(unchangedRoot);
		await unchangedEngine.Assign(Compile(expression: "1 + 1", unchangedEngine), XPathAssignType.Delete, attributeName: null, new XPathObject("ignored"));
		Assert.AreEqual(expected: "value", unchangedRoot["target"].AsString());
		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () =>
																	   await unchangedEngine.Assign(
																		   Compile(expression: "target", unchangedEngine), XPathAssignType.Unknown, attributeName: null, new XPathObject("ignored")));
	}

	[TestMethod]
	public async Task ValueEvaluatorProvidesObjectStringIntegerAndArrayViews()
	{
		var items = new DataModelList { { "item", "one" }, { "item", "two" } };
		var root = new DataModelList { ["number"] = 7, ["items"] = items };
		var engine = CreateEngine(root);
		var expression = Mock.Of<IValueExpression>(e => e.Expression == "number");
		var scalar = new XPathValueExpressionEvaluator(expression, Compile(expression: "number", engine)) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };

		Assert.AreSame(expression, ((IAncestorProvider) scalar).Ancestor);
		Assert.AreEqual(expected: "number", scalar.Expression);
		Assert.AreEqual(expected: "7", await ((IStringEvaluator) scalar).EvaluateString());
		Assert.AreEqual(expected: 7, await ((IIntegerEvaluator) scalar).EvaluateInteger());
		Assert.AreEqual(expected: "7", (await ((IObjectEvaluator) scalar).EvaluateObject()).ToObject());

		var array = new XPathValueExpressionEvaluator(Mock.Of<IValueExpression>(), Compile(expression: "items/item", engine)) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };
		var values = await array.EvaluateArray();
		Assert.HasCount(expected: 2, values);
		Assert.AreEqual(expected: "one", values[0].ToObject());
		Assert.AreEqual(expected: "two", values[1].ToObject());
	}

	[TestMethod]
	public async Task ValueEvaluatorProvidesStringIntegerNodeSetAndArrayViews()
	{
		var items = new DataModelList { { "item", "one" }, { "item", "two" } };
		var root = new DataModelList { ["number"] = 7, ["items"] = items };
		var engine = CreateEngine(root);
		var expression = Mock.Of<IValueExpression>(e => e.Expression == "number");
		var scalar = new XPathValueExpressionEvaluator(expression, Compile(expression: "number", engine)) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };

		Assert.AreSame(expression, ((IAncestorProvider) scalar).Ancestor);
		Assert.AreEqual(expected: "number", scalar.Expression);
		Assert.AreEqual(expected: "7", await ((IStringEvaluator) scalar).EvaluateString());
		Assert.AreEqual(expected: 7, await ((IIntegerEvaluator) scalar).EvaluateInteger());
		Assert.AreEqual(XPathObjectType.NodeSet, ((XPathObject) await ((IObjectEvaluator) scalar).EvaluateObject()).Type);

		var array = new XPathValueExpressionEvaluator(Mock.Of<IValueExpression>(), Compile(expression: "items/item", engine)) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };
		var values = await array.EvaluateArray();
		Assert.HasCount(expected: 2, values);
		Assert.AreEqual(expected: "one", ((XPathObject) values[0]).AsString());
		Assert.AreEqual(expected: "two", ((XPathObject) values[1]).AsString());
	}

	[TestMethod]
	public async Task LocationEvaluatorUsesDefaultAndExplicitAssignmentAndDeclaresVariables()
	{
		var root = new DataModelList { ["target"] = "old" };
		var engine = CreateEngine(root);
		var source = Mock.Of<ILocationExpression>(e => e.Expression == "target");
		var evaluator = new XPathLocationExpressionEvaluator(source, Compile(expression: "target", engine)) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual(expected: "target", evaluator.Expression);
		Assert.AreEqual(expected: "target", await evaluator.GetName());
		Assert.AreEqual(expected: "old", (await evaluator.GetValue()).ToObject());
		await evaluator.SetValue(new XPathObject("new"));
		Assert.AreEqual(expected: "new", root["target"].AsString());

		engine.EnterScope();
		var localSource = new XPathLocationExpression(Mock.Of<ILocationExpression>(e => e.Expression == "local"), XPathAssignType.Replace, attribute: null);
		var local = new XPathLocationExpressionEvaluator(localSource, Compile(expression: "local", engine)) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };
		await local.DeclareLocalVariable();
		Assert.AreEqual(string.Empty, FirstValue(engine.GetVariable("local")));
		engine.LeaveScope();
	}

	[TestMethod]
	public async Task LocationEvaluatorGetsNameValueAndDeclaresScopedVariablesWithoutMutation()
	{
		var root = new DataModelList { ["target"] = "old" };
		var engine = CreateEngine(root);
		var source = Mock.Of<ILocationExpression>(e => e.Expression == "target");
		var evaluator = new XPathLocationExpressionEvaluator(source, Compile(expression: "target", engine)) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };

		Assert.AreSame(source, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual(expected: "target", evaluator.Expression);
		Assert.AreEqual(expected: "target", await evaluator.GetName());
		Assert.AreEqual(expected: "old", (await evaluator.GetValue()).ToObject());

		engine.EnterScope();
		var localSource = new XPathLocationExpression(Mock.Of<ILocationExpression>(e => e.Expression == "local"), XPathAssignType.Replace, attribute: null);
		var local = new XPathLocationExpressionEvaluator(localSource, Compile(expression: "local", engine)) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };
		await local.DeclareLocalVariable();
		Assert.AreEqual(string.Empty, FirstValue(engine.GetVariable("local")));
		engine.LeaveScope();
	}

	[TestMethod]
	public async Task ExpressionContextResolvesNamespacesVariablesFunctionsAndInitializesDescriptorsOnce()
	{
		var nameTableProvider = new Mock<INameTableProvider>();
		nameTableProvider.Setup(static p => p.GetNameTable()).Returns(new System.Xml.NameTable());
		var namespaces = new Mock<IXmlNamespacesInfo>();
		namespaces.SetupGet(static n => n.Namespaces).Returns(ImmutableArray.Create<(string Prefix, string Namespace)>(("p", "urn:test")));
		var variable = new CapturingVariableDescriptor { Name = "variable" };
		var function = new CapturingFunction();
		var provider = new Mock<IXPathFunctionProvider>();
		provider.Setup(static p => p.TryGetFunction(string.Empty, "function")).Returns(function);
		var engine = CreateEngine(new DataModelList());
		var context = new XPathExpressionContext(nameTableProvider.Object, namespaces.Object)
					  {
						  FunctionProviders = [provider.Object],
						  XPathVarDescriptorFactory = _ => variable,
						  XPathEngineFactory = () => new ValueTask<XPathEngine>(engine)
					  };

		Assert.IsFalse(context.Whitespace);
		Assert.IsFalse(context.PreserveWhitespace(new DataModelXPathNavigator(DataModelValue.Undefined)));
		Assert.IsTrue(context.CompareDocument(baseUri: "a", nextbaseUri: "b") < 0);
		Assert.AreEqual(expected: "urn:test", context.LookupNamespace("p"));
		Assert.AreSame(variable, context.ResolveVariable(string.Empty, name: "variable"));
		Assert.AreSame(function, context.ResolveFunction(string.Empty, name: "function", []));
		await context.EnsureInitialized();
		await context.EnsureInitialized();
		Assert.AreSame(engine, variable.InitializedEngine);
		Assert.AreEqual(expected: 1, function.InitializeCount);

		Assert.ThrowsExactly<XPathDataModelException>([ExcludeFromCodeCoverage]() => context.ResolveVariable(prefix: "p", name: "variable"));
		Assert.ThrowsExactly<XPathDataModelException>([ExcludeFromCodeCoverage]() => context.ResolveFunction(string.Empty, name: "missing", []));
		Assert.ThrowsExactly<XPathDataModelException>([ExcludeFromCodeCoverage]() => context.LookupNamespace("missing"));
	}

	[TestMethod]
	public async Task ExternalDataEvaluatorRejectsUnsupportedMediaTypes()
	{
		var evaluator = CreateExternalDataEvaluator();
		await using var resource = new Resource(new MemoryStream(Encoding.UTF8.GetBytes("data")), new ContentType("text/plain"));

		await Assert.ThrowsExactlyAsync<XPathDataModelException>([ExcludeFromCodeCoverage] async () => await evaluator.Parse(resource));
	}

	[TestMethod]
	public async Task ExternalDataEvaluatorParsesApplicationAndTextXmlResources()
	{
		var evaluator = CreateExternalDataEvaluator();

		foreach (var mediaType in new[] { "application/xml", "text/xml" })
		{
			await using var resource = new Resource(new MemoryStream(Encoding.UTF8.GetBytes("<root>value</root>")), new ContentType(mediaType));
			var result = await evaluator.Parse(resource);
			Assert.AreEqual(expected: "value", result.AsList()["root"].AsString());
		}
	}

	[TestMethod]
	public async Task ForEachEvaluatorCreatesAndRemovesItemAndIndexScopeForEmptyArray()
	{
		var root = new DataModelList();
		var engine = CreateEngine(root);
		var withIndex = CreateForEachEvaluator(engine, arrayExpression: "missing/item", includeIndex: true);
		var withoutIndex = CreateForEachEvaluator(engine, arrayExpression: "missing/item", includeIndex: false);

		await withIndex.Execute();
		await withoutIndex.Execute();

		Assert.IsFalse(root.ContainsKey(key: "item", caseInsensitive: false));
		Assert.IsFalse(root.ContainsKey(key: "index", caseInsensitive: false));
	}

	[TestMethod]
	public async Task ForEachEvaluatorProcessesNonEmptyArrayAndLeavesScope()
	{
		var items = new DataModelList { { "item", "one" }, { "item", "two" } };
		var root = new DataModelList { ["items"] = items };
		var engine = CreateEngine(root);
		var evaluator = CreateForEachEvaluator(engine, arrayExpression: "items/item", includeIndex: true);

		await evaluator.Execute();

		Assert.IsFalse(root.ContainsKey(key: "item", caseInsensitive: false));
		Assert.IsFalse(root.ContainsKey(key: "index", caseInsensitive: false));
	}

	private static XPathEngine CreateEngine(DataModelList dataModel) => new(Mock.Of<IDataModelController>(controller => controller.DataModel == dataModel));

	private static XPathCompiledExpression Compile(string expression, XPathEngine engine)
	{
		var nameTableProvider = new Mock<INameTableProvider>();
		nameTableProvider.Setup(static p => p.GetNameTable()).Returns(new System.Xml.NameTable());
		var context = new XPathExpressionContext(nameTableProvider.Object, xmlNamespacesInfo: null)
					  {
						  FunctionProviders = [],
						  XPathVarDescriptorFactory = static name => new XPathVarDescriptor { Name = name },
						  XPathEngineFactory = () => new ValueTask<XPathEngine>(engine)
					  };

		return new XPathCompiledExpression(expression, xmlNamespacesInfo: null, _ => context);
	}

	private static TestXPathExternalDataExpressionEvaluator CreateExternalDataEvaluator() =>
		new(Mock.Of<IExternalDataExpression>())
		{
			XPathXmlParserContextFactory = new XPathXmlParserContextFactory { NameTableProvider = null },
			DataConverter = null!,
			ResourceLoader = null!
		};

	private static XPathForEachEvaluator CreateForEachEvaluator(XPathEngine engine, string arrayExpression, bool includeIndex)
	{
		var array = new XPathValueExpressionEvaluator(Mock.Of<IValueExpression>(e => e.Expression == arrayExpression), Compile(arrayExpression, engine))
					{
						EngineFactory = () => new ValueTask<XPathEngine>(engine)
					};
		var item = new XPathLocationExpressionEvaluator(Mock.Of<ILocationExpression>(e => e.Expression == "item"), Compile(expression: "item", engine))
				   {
					   EngineFactory = () => new ValueTask<XPathEngine>(engine)
				   };
		var index = includeIndex
			? new XPathLocationExpressionEvaluator(Mock.Of<ILocationExpression>(e => e.Expression == "index"), Compile(expression: "index", engine))
			  {
				  EngineFactory = () => new ValueTask<XPathEngine>(engine)
			  }
			: null;
		var forEach = new Mock<IForEach>();
		forEach.SetupGet(static f => f.Array).Returns(array);
		forEach.SetupGet(static f => f.Item).Returns(item);
		forEach.SetupGet(static f => f.Index).Returns(index);
		forEach.SetupGet(static f => f.Action).Returns([]);

		return new XPathForEachEvaluator(forEach.Object) { EngineFactory = () => new ValueTask<XPathEngine>(engine) };
	}

	private static string FirstValue(object variable)
	{
		var iterator = (XPathNodeIterator) variable;
		Assert.IsTrue(iterator.MoveNext());

		return iterator.Current!.Value;
	}

	private sealed class CapturingVariableDescriptor : XPathVarDescriptor
	{
		public XPathEngine? InitializedEngine { get; private set; }

		public override ValueTask Initialize(XPathEngine engine)
		{
			InitializedEngine = engine;

			return base.Initialize(engine);
		}
	}

	private sealed class CapturingFunction() : XPathFunctionBase(XPathResultType.String)
	{
		public int InitializeCount { get; private set; }

		public override ValueTask Initialize()
		{
			InitializeCount ++;

			return ValueTask.CompletedTask;
		}

		protected override object Invoke(object[] args) => "result";
	}

	private sealed class TestXPathExternalDataExpressionEvaluator(IExternalDataExpression expression) : XPathExternalDataExpressionEvaluator(expression)
	{
		public ValueTask<DataModelValue> Parse(Resource resource) => ParseToDataModel(resource);
	}
}