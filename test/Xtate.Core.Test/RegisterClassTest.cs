using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.Builder;
using Xtate.CustomAction;
using Xtate.DataModel;
using Xtate.DataModel.Runtime;
using Xtate.IoC;
using Xtate.Scxml;
using Xtate.XInclude;

namespace Xtate.Core.Test
{
	public class MyActionProvider() : CustomActionProvider<MyAction>("http://xtate.net/scxml/customaction/my", "myAction") { }


	public class MyAction : CustomActionBase
	{
		private readonly Value _input;
		private readonly Location _output;

		public MyAction(XmlReader xmlReader)
		{
			_input = new StringValue(xmlReader.GetAttribute("sourceExpr"), xmlReader.GetAttribute("source"));
			_output = new Location(xmlReader.GetAttribute("destination"));
		}

		public override IEnumerable<Value>    GetValues()    { yield return _input; }

		public override IEnumerable<Location> GetLocations() { yield return _output;}

		protected override DataModelValue Evaluate(IReadOnlyDictionary<string, DataModelValue> arguments) => base.Evaluate(arguments);

		protected override ValueTask<DataModelValue> EvaluateAsync(IReadOnlyDictionary<string, DataModelValue> arguments) => base.EvaluateAsync(arguments);

		public override async ValueTask Execute()
		{
			await _output.CopyFrom(_input);
		}
	}



	[TestClass]
	public class RegisterClassTest
	{
		[TestMethod]
		public async Task NullDataModelHandlerTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterNullDataModelHandler();
			services.AddSharedImplementationSync<TypeInfoBase, Type>(SharedWithin.Container).For<ITypeInfo>();
			var provider = services.BuildProvider();

			var dataModelHandler = await provider.GetRequiredService<IDataModelHandler>();
			var typeInfo = provider.GetRequiredServiceSync<ITypeInfo, Type>(dataModelHandler.GetType());

			// Act

			IExecutableEntity ifEntity = new IfEntity { Action = ImmutableArray.Create<IExecutableEntity>(new LogEntity()), Condition = new ConditionExpression { Expression = "In(SomeState)" } };

			dataModelHandler.Process(ref ifEntity);

			// Assert

			Assert.AreEqual("Xtate.DataModel.Null.NullDataModelHandler", typeInfo.FullTypeName);
			Assert.IsFalse(dataModelHandler.CaseInsensitive);
		}

		[TestMethod]
		public async Task RuntimeDataModelHandlerTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterRuntimeDataModelHandler();
			services.AddSharedImplementationSync<TypeInfoBase, Type>(SharedWithin.Container).For<ITypeInfo>();
			var provider = services.BuildProvider();

			var dataModelHandler = await provider.GetRequiredService<IDataModelHandler>();
			var typeInfo = provider.GetRequiredServiceSync<ITypeInfo, Type>(dataModelHandler.GetType());

			// Act

			IExecutableEntity ifEntity = new IfEntity { Action = ImmutableArray.Create<IExecutableEntity>(new LogEntity()), Condition = RuntimePredicate.GetPredicate(() => !Runtime.InState("4")) };

			dataModelHandler.Process(ref ifEntity);

			var booleanEvaluator = (IBooleanEvaluator)((IIf)ifEntity).Condition!;
			var val = await booleanEvaluator.EvaluateBoolean();

			// Assert

			Assert.AreEqual("Xtate.DataModel.Runtime.RuntimeDataModelHandler", typeInfo.FullTypeName);
			Assert.IsFalse(dataModelHandler.CaseInsensitive);
			Assert.IsTrue(val);
		}

		[TestMethod]
		public async Task XPathDataModelHandlerTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterXPathDataModelHandler();
			services.AddSharedImplementationSync<TypeInfoBase, Type>(SharedWithin.Container).For<ITypeInfo>();
			var provider = services.BuildProvider();

			var dataModelHandler = await provider.GetRequiredService<IDataModelHandler>();
			var typeInfo = provider.GetRequiredServiceSync<ITypeInfo, Type>(dataModelHandler.GetType());

			// Act

			IExecutableEntity ifEntity = new IfEntity { Action = ImmutableArray.Create<IExecutableEntity>(new LogEntity()), Condition = new ConditionExpression { Expression = "In('st') = false()" } };

			dataModelHandler.Process(ref ifEntity);

			var booleanEvaluator = (IBooleanEvaluator)((IIf)ifEntity).Condition!;
			var val = await booleanEvaluator.EvaluateBoolean();
			
			// Assert

			Assert.AreEqual("Xtate.DataModel.XPath.XPathDataModelHandler", typeInfo.FullTypeName);
			Assert.IsFalse(dataModelHandler.CaseInsensitive);
			Assert.IsTrue(val);
		}

		[TestMethod]
		public void RuntimeNotInActionTest()
		{
			Assert.ThrowsException<InfrastructureException>(() => Runtime.InState("2"));
		}

		[TestMethod]
		public void StateMachineBuilderTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterStateMachineBuilder();
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
			Assert.AreEqual("runtime", stateMachine.DataModelType);
			Assert.AreEqual(1, stateMachine.States.Length);
			Assert.AreEqual("test", ((IState)stateMachine.States[0]).Id?.Value);
		}

		[TestMethod]
		public void StateMachineFluentBuilderTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterStateMachineFluentBuilder();
			var provider = services.BuildProvider();

			var stateMachineBuilder = provider.GetRequiredServiceSync<StateMachineFluentBuilder>();

			// Act

			var stateMachine = stateMachineBuilder.BeginState("test").EndState().Build();

			// Assert

			Assert.IsNotNull(stateMachine);
			Assert.AreEqual("runtime", stateMachine.DataModelType);
			Assert.AreEqual(1, stateMachine.States.Length);
			Assert.AreEqual("test", ((IState)stateMachine.States[0]).Id?.Value);
		}

		[TestMethod]
		public async Task ScxmlBuilderTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterScxml();
			var provider = services.BuildProvider();

			const string xml = @"<scxml version='1.0' xmlns='http://www.w3.org/2005/07/scxml' datamodel='xpath'><state xmlns:eee='qval' id='test'></state></scxml>";

			using var textReader = new StringReader(xml);
			using var reader = XmlReader.Create(textReader);

			var scxmlDeserializer = await provider.GetRequiredService<IScxmlDeserializer>();

			// Act

			var stateMachine = await scxmlDeserializer.Deserialize(reader);

			// Assert

			Assert.IsNotNull(stateMachine);
			Assert.AreEqual("xpath", stateMachine.DataModelType);
			Assert.AreEqual(1, stateMachine.States.Length);
			Assert.AreEqual("test", ((IState)stateMachine.States[0]).Id?.Value);
		}

		[TestMethod]
		public async Task ScxmlDtdXIncludeBuilderTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterScxml();
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
			Assert.AreEqual(3, stateMachine.States.Length);
			Assert.AreEqual("state0", ((IState)stateMachine.States[0]).Id?.Value);
			Assert.AreEqual("state1", ((IState)stateMachine.States[1]).Id?.Value);
			Assert.AreEqual("fin", ((IFinal)stateMachine.States[2]).Id?.Value);
		}

		[TestMethod]
		public async Task ScxmlXIncludeBuilderTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterScxml();
			services.AddImplementation<DefaultIoBoundTask>().For<IIoBoundTask>();
			services.AddForwarding<IXIncludeOptions>(sp => new XIncludeOptions{});
			var provider = services.BuildProvider();

			var uri = new Uri("res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/SingleIncludeSource.scxml");

			var resolver = await provider.GetRequiredService<XmlResolver>();
			var resourceLoaderService = await provider.GetRequiredService<IResourceLoader>();
			var resource = await resourceLoaderService.Request(uri);
			
			var xmlReaderSettings = new XmlReaderSettings { Async = true, XmlResolver = resolver};
			var xmlReader = XmlReader.Create(await resource.GetStream(doNotCache: true), xmlReaderSettings, uri.ToString());

			var scxmlDeserializer = await provider.GetRequiredService<IScxmlDeserializer>();

			// Act

			var stateMachine = await scxmlDeserializer.Deserialize(xmlReader);

			// Assert

			Assert.IsNotNull(stateMachine);
			Assert.IsNull(stateMachine.DataModelType);
			Assert.AreEqual(3, stateMachine.States.Length);
			Assert.AreEqual("state0", ((IState)stateMachine.States[0]).Id?.Value);
			Assert.AreEqual("state1", ((IState)stateMachine.States[1]).Id?.Value);
			Assert.AreEqual("fin", ((IFinal)stateMachine.States[2]).Id?.Value);
		}

		[TestMethod]
		public async Task ScxmlSerializerBuilderTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterScxml();
			var provider = services.BuildProvider();

			using var textWriter = new StringWriter();
			using var writer = XmlWriter.Create(textWriter, new XmlWriterSettings { Async = true });

			var scxmlSerializer = await provider.GetRequiredService<IScxmlSerializer>();

			// Act

			await scxmlSerializer.Serialize(new StateMachineEntity(), writer);

			await writer.FlushAsync();
			await textWriter.FlushAsync();

			// Assert

			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?><scxml version=\"1.0\" xmlns=\"http://www.w3.org/2005/07/scxml\" />", textWriter.ToString());
		}

		[TestMethod]
		public async Task DataModelHandlersEmptyTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterDataModelHandlers();
			services.AddSharedImplementationSync<TypeInfoBase, Type>(SharedWithin.Container).For<ITypeInfo>();
			var provider = services.BuildProvider();

			var dataModelHandler = await provider.GetOptionalService<IDataModelHandler>();
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
			services.RegisterDataModelHandlers();
			services.AddForwarding<IStateMachine>(sp => new StateMachineEntity {DataModelType = "xpath"});
			services.AddSharedImplementationSync<TypeInfoBase, Type>(SharedWithin.Container).For<ITypeInfo>();
			var provider = services.BuildProvider();

			var dataModelHandler = await provider.GetRequiredService<IDataModelHandler>();
			var typeInfo = provider.GetRequiredServiceSync<ITypeInfo, Type>(dataModelHandler.GetType());

			// Act


			// Assert

			Assert.AreEqual("Xtate.DataModel.XPath.XPathDataModelHandler", typeInfo.FullTypeName);
		}

		[TestMethod]
		public async Task CustomActionTest()
		{
			// Arrange

			var services = new ServiceCollection();
			services.RegisterScxml();
			var provider = services.BuildProvider();

			const string xml = @"
  <scxml xmlns=""http://www.w3.org/2005/07/scxml"" xmlns:my=""http://xtate.net/scxml/customaction/my"" version=""1.0"" datamodel=""xpath"" initial=""init"">
    <state id=""init"">
      <onentry>
        <my:myAction source=""emailContent"" destination=""emailContent"" />        
      </onentry>
    </state>
  </scxml>";

			using var textReader = new StringReader(xml);
			using var reader = XmlReader.Create(textReader, new XmlReaderSettings() {NameTable = provider.GetRequiredServiceSync<INameTableProvider>().GetNameTable()});

			var scxmlDeserializer = await provider.GetRequiredService<IScxmlDeserializer>();
			var stateMachine = await scxmlDeserializer.Deserialize(reader);

			var services2 = new ServiceCollection();
			services2.RegisterInterpreterModelBuilder();
			services2.AddSharedImplementationSync<MyActionProvider>(SharedWithin.Scope).For<ICustomActionProvider>();
			services2.AddTypeSync<MyAction, XmlReader>();
			services2.AddForwarding(_ => provider.GetRequiredServiceSync<INameTableProvider>());
			services2.AddForwarding(_ => stateMachine);
			var provider2 = services2.BuildProvider();

			var interpreterModelBuilder = await provider2.GetRequiredService<InterpreterModelBuilder>();

			// Act
			var stateMachineNode = await interpreterModelBuilder.Build2(stateMachine);
			await stateMachineNode.States[0].OnEntry[0].ActionEvaluators[0].Execute();

			// Assert

			Assert.IsNotNull(stateMachine);
		}

		[TestMethod]
		public async Task InterpreterModelBuilderTest()
		{
			// Arrange

			const string xml = @"
  <scxml xmlns=""http://www.w3.org/2005/07/scxml"" version=""1.0"" datamodel=""xpath"" initial=""init"">
    <state id=""init"">
    </state>
  </scxml>";

			var services = new ServiceCollection();
			services.RegisterStateMachineFactory();
			services.RegisterInterpreterModelBuilder();
			services.AddForwarding<IScxmlStateMachine>(_ => new ScxmlStateMachine(xml));
			var provider = services.BuildProvider();

			var interpreterModelBuilder = await provider.GetRequiredService<InterpreterModelBuilder>();
			var stateMachine = await provider.GetRequiredService<IStateMachine>();

			// Act
			var stateMachineNode = await interpreterModelBuilder.Build2(stateMachine);

			// Assert

			Assert.IsNotNull(stateMachineNode);
		}

		[TestMethod]
		public async Task StateMachineInterpreterTest()
		{
			// Arrange
			const string xml = @"
  <scxml xmlns=""http://www.w3.org/2005/07/scxml"" version=""1.0"" datamodel=""xpath"" initial=""init"">
    <final id=""init"">
        <donedata>
            <content>HELLO</content>
        </donedata>
    </final>
  </scxml>";

			var services = new ServiceCollection();
			services.AddForwarding<IScxmlStateMachine>(_ => new ScxmlStateMachine(xml));
			services.RegisterStateMachineFactory();
			services.RegisterStateMachineInterpreter();
			services.AddImplementation<TraceLogWriter>().For<ILogWriter>();

			var provider = services.BuildProvider();

			var stateMachineInterpreter = await provider.GetRequiredService<IStateMachineInterpreter>();

			// Act

			var result = await stateMachineInterpreter.RunAsync();

			// Assert

			Assert.AreEqual("HELLO", result);
		}
	}
}
