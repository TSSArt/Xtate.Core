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

using System.IO;
using System.Xml;
using Xtate.Builder;
using Xtate.CustomAction;
using Xtate.DataModel;
using Xtate.DataModel.Null;
using Xtate.DataModel.Runtime;
using Xtate.DataModel.XPath;
using Xtate.IoC;
using Xtate.Scxml;
using Xtate.XInclude;

namespace Xtate.Core.Test;

public class MyActionProvider() : ActionProvider<MyAction>(ns: "http://xtate.net/scxml/customaction/my", name: "myAction");

public class MyAction(XmlReader xmlReader) : SyncAction
{
	private readonly ObjectValue _input = new(xmlReader.GetAttribute("sourceExpr"), xmlReader.GetAttribute("source"));

	private readonly Location _output = new(xmlReader.GetAttribute("destination"));

	protected override IEnumerable<Value> GetValues() { yield return _input; }

	protected override IEnumerable<Location> GetLocations() { yield return _output; }

	protected override DataModelValue Evaluate() => _input.Value;
}

[TestClass]
public class RegisterClassTest
{
	[TestMethod]
	public async Task NullDataModelHandlerTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<NullDataModelHandlerModule>();
		services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Container).For<IAssemblyTypeInfo>();
		var provider = services.BuildProvider();

		var dataModelHandler = await provider.GetRequiredService<IDataModelHandler>();
		var caseSensitivity = await provider.GetRequiredService<ICaseSensitivity>();
		var typeInfo = provider.GetRequiredServiceSync<IAssemblyTypeInfo, Type>(dataModelHandler.GetType());

		// Act

		IExecutableEntity ifEntity = new IfEntity { Action = [new LogEntity()], Condition = new ConditionExpression { Expression = "In(SomeState)" } };

		dataModelHandler.Process(ref ifEntity);

		// Assert

		Assert.AreEqual(expected: "Xtate.DataModel.Null.NullDataModelHandler", typeInfo.FullTypeName);
		Assert.IsFalse(caseSensitivity.CaseInsensitive);
	}

	[TestMethod]
	public async Task RuntimeDataModelHandlerTest()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddModule<RuntimeDataModelHandlerModule>();
		services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Container).For<IAssemblyTypeInfo>();
		services.AddMock<IInStateController>();
		services.AddMock<ILogController>();
		services.AddMock<IEventController>();
		services.AddMock<IInvokeController>();
		services.AddMock<IDataModelController>();
		var provider = services.BuildProvider();

		var dataModelHandler = await provider.GetRequiredService<IDataModelHandler>();
		var caseSensitivity = await provider.GetRequiredService<ICaseSensitivity>();
		var typeInfo = provider.GetRequiredServiceSync<IAssemblyTypeInfo, Type>(dataModelHandler.GetType());

		// Act

		IExecutableEntity ifEntity = new IfEntity { Action = [new LogEntity()], Condition = RuntimePredicate.GetPredicate(() => !Runtime.InState("4")) };

		dataModelHandler.Process(ref ifEntity);

		var booleanEvaluator = (IBooleanEvaluator) ((IIf) ifEntity).Condition!;
		var val = await booleanEvaluator.EvaluateBoolean();

		// Assert

		Assert.AreEqual(expected: "Xtate.DataModel.Runtime.RuntimeDataModelHandler", typeInfo.FullTypeName);
		Assert.IsFalse(caseSensitivity.CaseInsensitive);
		Assert.IsTrue(val);
	}

	[TestMethod]
	public async Task XPathDataModelHandlerTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<XPathDataModelHandlerModule>();
		services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Container).For<IAssemblyTypeInfo>();
		services.AddMock<IInStateController>();
		var provider = services.BuildProvider();

		var dataModelHandler = await provider.GetRequiredService<IDataModelHandler>();
		var typeInfo = provider.GetRequiredServiceSync<IAssemblyTypeInfo, Type>(dataModelHandler.GetType());
		var caseSensitivity = await provider.GetRequiredService<ICaseSensitivity>();

		// Act

		IExecutableEntity ifEntity = new IfEntity { Action = [new LogEntity()], Condition = new ConditionExpression { Expression = "In('st') = false()" } };

		dataModelHandler.Process(ref ifEntity);

		var booleanEvaluator = (IBooleanEvaluator) ((IIf) ifEntity).Condition!;
		var val = await booleanEvaluator.EvaluateBoolean();

		// Assert

		Assert.AreEqual(expected: "Xtate.DataModel.XPath.XPathDataModelHandler", typeInfo.FullTypeName);
		Assert.IsFalse(caseSensitivity.CaseInsensitive);
		Assert.IsTrue(val);
	}

	[TestMethod]
	public void RuntimeNotInActionTest()
	{
		Assert.ThrowsException<InvalidOperationException>(() => Runtime.InState("2"));
	}

	[TestMethod]
	public void StateMachineBuilderTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<StateMachineBuilderModule>();
		var provider = services.BuildProvider();

		var stateMachineBuilder = provider.GetRequiredServiceSync<IStateMachineBuilder>();
		stateMachineBuilder.SetDataModelType("runtime");

		var stateBuilder = provider.GetRequiredServiceSync<IStateBuilder>();
		stateBuilder.SetId(Identifier.FromString("test"));

		stateMachineBuilder.AddState(stateBuilder.Build());

		// Act

		var stateMachine = stateMachineBuilder.Build();

		// Assert

		Assert.IsNotNull(stateMachine);
		Assert.AreEqual(expected: "runtime", stateMachine.DataModelType);
		Assert.AreEqual(expected: 1, stateMachine.States.Length);
		Assert.AreEqual(expected: "test", ((IState) stateMachine.States[0]).Id?.Value);
	}

	[TestMethod]
	public void StateMachineFluentBuilderTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<StateMachineFluentBuilderModule>();
		var provider = services.BuildProvider();

		var stateMachineBuilder = provider.GetRequiredServiceSync<StateMachineFluentBuilder>();

		// Act

		var stateMachine = stateMachineBuilder.BeginState("test").EndState().Build();

		// Assert

		Assert.IsNotNull(stateMachine);
		Assert.AreEqual(expected: "runtime", stateMachine.DataModelType);
		Assert.AreEqual(expected: 1, stateMachine.States.Length);
		Assert.AreEqual(expected: "test", ((IState) stateMachine.States[0]).Id?.Value);
	}

	[TestMethod]
	public async Task ScxmlBuilderTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<ScxmlModule>();
		var provider = services.BuildProvider();

		const string xml = @"<scxml version='1.0' xmlns='http://www.w3.org/2005/07/scxml' datamodel='xpath'><state xmlns:eee='qval' id='test'></state></scxml>";

		using var textReader = new StringReader(xml);
		using var reader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true });

		var scxmlDeserializer = await provider.GetRequiredService<IScxmlDeserializer>();

		// Act

		var stateMachine = await scxmlDeserializer.Deserialize(reader);

		// Assert

		Assert.IsNotNull(stateMachine);
		Assert.AreEqual(expected: "xpath", stateMachine.DataModelType);
		Assert.AreEqual(expected: 1, stateMachine.States.Length);
		Assert.AreEqual(expected: "test", ((IState) stateMachine.States[0]).Id?.Value);
	}

	[TestMethod]
	public async Task ScxmlDtdXIncludeBuilderTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<ScxmlModule>();
		services.AddImplementation<DefaultIoBoundTask>().For<IIoBoundTask>();
		var provider = services.BuildProvider();

		var uri = new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/DtdSingleIncludeSource.scxml");

		var resolver = await provider.GetRequiredService<XmlResolver>();
		var resourceLoaderService = await provider.GetRequiredService<IResourceLoader>();
		var resource = await resourceLoaderService.Request(uri);

		var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = resolver, DtdProcessing = DtdProcessing.Parse };
		var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true), xmlReaderSettings, uri.ToString());

		var scxmlDeserializer = await provider.GetRequiredService<IScxmlDeserializer>();

		// Act

		var stateMachine = await scxmlDeserializer.Deserialize(xmlReader);

		// Assert

		Assert.IsNotNull(stateMachine);
		Assert.IsNull(stateMachine.DataModelType);
		Assert.AreEqual(expected: 3, stateMachine.States.Length);
		Assert.AreEqual(expected: "state0", ((IState) stateMachine.States[0]).Id?.Value);
		Assert.AreEqual(expected: "state1", ((IState) stateMachine.States[1]).Id?.Value);
		Assert.AreEqual(expected: "fin", ((IFinal) stateMachine.States[2]).Id?.Value);
	}

	[TestMethod]
	public async Task ScxmlXIncludeBuilderTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<ScxmlModule>();
		services.AddImplementation<DefaultIoBoundTask>().For<IIoBoundTask>();
		services.AddConstant<IXIncludeOptions>(new XIncludeOptions());
		var provider = services.BuildProvider();

		var uri = new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/SingleIncludeSource.scxml");

		var resolver = await provider.GetRequiredService<XmlResolver>();
		var resourceLoaderService = await provider.GetRequiredService<IResourceLoader>();
		var resource = await resourceLoaderService.Request(uri);

		var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = resolver };
		var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true), xmlReaderSettings, uri.ToString());

		var scxmlDeserializer = await provider.GetRequiredService<IScxmlDeserializer>();

		// Act

		var stateMachine = await scxmlDeserializer.Deserialize(xmlReader);

		// Assert

		Assert.IsNotNull(stateMachine);
		Assert.IsNull(stateMachine.DataModelType);
		Assert.AreEqual(expected: 3, stateMachine.States.Length);
		Assert.AreEqual(expected: "state0", ((IState) stateMachine.States[0]).Id?.Value);
		Assert.AreEqual(expected: "state1", ((IState) stateMachine.States[1]).Id?.Value);
		Assert.AreEqual(expected: "fin", ((IFinal) stateMachine.States[2]).Id?.Value);
	}

	[TestMethod]
	public async Task ScxmlSerializerBuilderTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<ScxmlModule>();
		var provider = services.BuildProvider();

		// ReSharper disable once UseAwaitUsing
		using var textWriter = new StringWriter();

		// ReSharper disable once UseAwaitUsing
		using var writer = XmlWriter.Create(textWriter, new XmlWriterSettings { Async = true });

		var scxmlSerializer = await provider.GetRequiredService<IScxmlSerializer>();

		// Act

		await scxmlSerializer.Serialize(new StateMachineEntity(), writer);

		await writer.FlushAsync();
		await textWriter.FlushAsync();

		// Assert

		Assert.AreEqual(expected: "<?xml version=\"1.0\" encoding=\"utf-16\"?><scxml version=\"1.0\" xmlns=\"http://www.w3.org/2005/07/scxml\" />", textWriter.ToString());
	}

	[TestMethod]
	public async Task DataModelHandlersEmptyTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<DataModelHandlersModule>();
		services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Container).For<IAssemblyTypeInfo>();
		var provider = services.BuildProvider();

		var dataModelHandler = await provider.GetService<IDataModelHandler>();

		//var typeInfo = provider.GetRequiredServiceSync<ITypeInfo, Type>(dataModelHandler.GetType());

		// Act

		// Assert

		//Assert.AreEqual("Xtate.DataModel.Null.NullDataModelHandler", typeInfo.FullTypeName);
		Assert.IsNull(dataModelHandler);
	}

	[TestMethod]
	public async Task DataModelHandlersXPathTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<DataModelHandlersModule>();
		services.AddConstant<IStateMachine>(new StateMachineEntity { DataModelType = "xpath" });
		services.AddSharedImplementationSync<AssemblyTypeInfo, Type>(SharedWithin.Container).For<IAssemblyTypeInfo>();
		var provider = services.BuildProvider();

		var dataModelHandler = await provider.GetRequiredService<IDataModelHandler>();
		var typeInfo = provider.GetRequiredServiceSync<IAssemblyTypeInfo, Type>(dataModelHandler.GetType());

		// Act

		// Assert

		Assert.AreEqual(expected: "Xtate.DataModel.XPath.XPathDataModelHandler", typeInfo.FullTypeName);
	}

	[TestMethod]
	public async Task CustomActionTest()
	{
		// Arrange

		var services = new ServiceCollection();
		services.AddModule<ScxmlModule>();
		var provider = services.BuildProvider();

		const string xml = """
						   <scxml xmlns="http://www.w3.org/2005/07/scxml" xmlns:my="http://xtate.net/scxml/customaction/my" version="1.0" datamodel="xpath" initial="init">
						     <state id="init">
						       <onentry>
						         <my:myAction source="emailContent" destination="emailContent" />
						       </onentry>
						     </state>
						   </scxml>
						   """;

		using var textReader = new StringReader(xml);
		using var reader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true, NameTable = provider.GetRequiredServiceSync<INameTableProvider>().GetNameTable() });

		var scxmlDeserializer = await provider.GetRequiredService<IScxmlDeserializer>();
		var stateMachine = await scxmlDeserializer.Deserialize(reader);

		var services2 = new ServiceCollection();
		services2.AddModule<InterpreterModelBuilderModule>();
		services2.AddModule<ToolsModule>();
		services2.AddImplementationSync<MyActionProvider>().For<IActionProvider>();
		services2.AddTypeSync<MyAction, XmlReader>();
		services2.AddConstant(provider.GetRequiredServiceSync<INameTableProvider>());
		services2.AddConstant(stateMachine);
		var provider2 = services2.BuildProvider();

		var interpreterModelBuilder = await provider2.GetRequiredService<InterpreterModelBuilder>();

		// Act
		var interpreterModel = await interpreterModelBuilder.BuildModel();
		await interpreterModel.Root.States[0].OnEntry[0].ActionEvaluators[0].Execute();

		// Assert

		Assert.IsNotNull(stateMachine);
	}

	[TestMethod]
	public async Task InterpreterModelBuilderTest()
	{
		// Arrange

		const string xml = """
						   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="init">
						     <state id="init">
						     </state>
						   </scxml>
						   """;

		var services = new ServiceCollection();
		services.AddModule<StateMachineFactoryModule>();
		services.AddModule<InterpreterModelBuilderModule>();
		services.AddConstant<IScxmlStateMachine>(new ScxmlStringStateMachine(xml));
		var provider = services.BuildProvider();

		var interpreterModelBuilder = await provider.GetRequiredService<InterpreterModelBuilder>();

		// Act
		var stateMachineNode = await interpreterModelBuilder.BuildModel();

		// Assert

		Assert.IsNotNull(stateMachineNode);
	}

	[TestMethod]
	public async Task StateMachineInterpreterTest()
	{
		// Arrange
		const string xml = """
						   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="init">
						     <final id="init">
						         <donedata>
						             <content>HELLO</content>
						         </donedata>
						     </final>
						   </scxml>
						   """;

		var services = new ServiceCollection();
		services.AddConstant<IScxmlStateMachine>(new ScxmlStringStateMachine(xml));
		services.AddModule<StateMachineFactoryModule>();
		services.AddModule<StateMachineInterpreterModule>();
		services.AddImplementation<TraceLogWriter<Any>>().For<ILogWriter<Any>>();

		var provider = services.BuildProvider();

		var stateMachineInterpreter = await provider.GetRequiredService<IStateMachineInterpreter>();

		// Act

		var result = await stateMachineInterpreter.RunAsync();

		// Assert

		Assert.AreEqual(expected: "HELLO", result);
	}
}