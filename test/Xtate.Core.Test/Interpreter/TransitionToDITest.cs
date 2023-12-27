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

using Xtate.DataModel;
using Xtate.DataModel.Null;
using Xtate.DataModel.Runtime;
using Xtate.DataModel.XPath;
using Xtate.IoC;

namespace Xtate.Core.Test.Interpreter;

public static class ContainerBuilder
{
	public static void AddDataModelHandler(this IServiceCollection services)
	{
		services.AddTypeSync<DefaultAssignEvaluator, IAssign>();
		services.AddTypeSync<DefaultCancelEvaluator, ICancel>();
		services.AddTypeSync<DefaultContentBodyEvaluator, IContentBody>();
		services.AddTypeSync<DefaultCustomActionEvaluator, ICustomAction>();
		services.AddTypeSync<DefaultExternalDataExpressionEvaluator, IExternalDataExpression>();
		services.AddTypeSync<DefaultForEachEvaluator, IForEach>();
		services.AddTypeSync<DefaultIfEvaluator, IIf>();
		services.AddTypeSync<DefaultInlineContentEvaluator, IInlineContent>();
		services.AddTypeSync<DefaultInvokeEvaluator, IInvoke>();
		services.AddTypeSync<DefaultLogEvaluator, ILog>();
		services.AddTypeSync<DefaultParamEvaluator, IParam>();
		services.AddTypeSync<DefaultRaiseEvaluator, IRaise>();
		services.AddTypeSync<DefaultScriptEvaluator, IScript>();
		services.AddTypeSync<DefaultSendEvaluator, ISend>();

		services.AddType<UnknownDataModelHandler>();
		services.AddType<NullDataModelHandler>();
		services.AddType<RuntimeDataModelHandler>();

		services.AddImplementation<NullDataModelHandlerProvider>().For<IDataModelHandlerProvider>();
		services.AddImplementation<RuntimeDataModelHandlerProvider>().For<IDataModelHandlerProvider>();

		services.AddXPathDataModelHandler();

		services.AddImplementation<DataModelHandlerService>().For<IDataModelHandlerService>();
		services.AddFactory<DataModelHandlerGetter>().For<IDataModelHandler>();
	}

	public static void AddXPathDataModelHandler(this IServiceCollection services)
	{
		services.AddTypeSync<XPathValueExpressionEvaluator, IValueExpression, XPathCompiledExpression>();
		services.AddTypeSync<XPathConditionExpressionEvaluator, IConditionExpression, XPathCompiledExpression>();
		services.AddTypeSync<XPathLocationExpressionEvaluator, ILocationExpression, XPathCompiledExpression>();
		services.AddTypeSync<XPathContentBodyEvaluator, IContentBody>();
		services.AddTypeSync<XPathLocationExpression, ILocationExpression, (XPathAssignType, string?)>();
		services.AddTypeSync<XPathExternalDataExpressionEvaluator, IExternalDataExpression>();
		services.AddTypeSync<XPathForEachEvaluator, IForEach>();
		services.AddTypeSync<XPathInlineContentEvaluator, IInlineContent>();

		services.AddType<XPathDataModelHandler>();

		services.AddImplementation<XPathDataModelHandlerProvider>().For<IDataModelHandlerProvider>();
	}

	public static void AddStateMachineContext(this IServiceCollection services, SharedWithin sharedWithin)
	{
		services.AddSharedImplementation<StateMachineContext>(sharedWithin).For<IStateMachineContext>();
	}

	public static void AddInterpreterModel(this IServiceCollection services)
	{
		//services.AddForwarding(_ => new Mock<IPreDataModelProcessor>().Object);
		services.AddFactory<InterpreterModelBuilder>().For<IInterpreterModel>();
	}

	public static void AddStateMachineInterpreter(this IServiceCollection services)
	{
		//services.AddForwarding(_ => new Mock<ILoggerOld>().Object);
		services.AddForwarding(_ => new Mock<IResourceLoader>().Object);
		services.AddForwarding(_ => new Mock<IEventQueueReader>().Object);

		services.AddImplementation<ErrorProcessorService<Any>>().For<IErrorProcessorService<Any>>();
		services.AddDataModelHandler();
		services.AddInterpreterModel();
		services.AddStateMachineContext(SharedWithin.Scope);

		services.AddImplementation<StateMachineInterpreter>().For<IStateMachineInterpreter>();
	}
}

[TestClass]
public class TransitionToDiTest
{
	[TestMethod]
	public async Task MinimalTest()
	{
		var stateMachine = new StateMachineEntity { States = [new FinalEntity()] };

		var services = new ServiceCollection();
		services.RegisterStateMachineInterpreter();
		services.AddForwarding<IStateMachine>(_ => stateMachine);

		var provider = services.BuildProvider();

		var stateMachineInterpreter = await provider.GetRequiredService<IStateMachineInterpreter>();

		await stateMachineInterpreter.RunAsync();
	}

	[TestMethod]
	public async Task XPathMinimalTest()
	{
		var stateMachine = new StateMachineEntity
						   {
							   DataModelType = "xpath", States =
							   [
								   new FinalEntity
								   						   {
								   							   DoneData = new DoneDataEntity
								   										  {
								   											  Content = new ContentEntity
								   														{
								   															Body = new ContentBody
								   																   {
								   																	   Value = "DONE-DATA"
								   																   }
								   														}
								   										  }
								   						   },
							   ]
		};

		var services = new ServiceCollection();
		services.RegisterStateMachineInterpreter();
		services.AddForwarding<IStateMachine>(_ => stateMachine);

		var provider = services.BuildProvider();

		var stateMachineInterpreter = await provider.GetRequiredService<IStateMachineInterpreter>();

		var result = await stateMachineInterpreter.RunAsync();

		Assert.AreEqual(expected: "DONE-DATA", result);
	}

	[TestMethod]
	public async Task ParseScxmlTest()
	{
		const string xml = @"<scxml version='1.0' xmlns='http://www.w3.org/2005/07/scxml' datamodel='xpath' initial='errorSwitch'>
<datamodel>
  <data id='company'>
    <about xmlns=''>
      <name>Example company</name>
      <website>example.com</website>
      <CEO>John Doe</CEO>
    </about>
  </data>
  <!--data id='employees' src='http://example.com/employees.xml'/-->
  <data id='defaultdata'/>
</datamodel>
<state id='currentBehavior'/>
<final id='newBehavior'/>
<state id='errorSwitch' xmlns:fn='http://www.w3.org/2005/xpath-functions'>
	<datamodel>
		<data id='str'/>
	</datamodel>

	<onentry>
		<assign location='$str' expr=""'errorSwitch'""/>
	</onentry>

	<transition cond='In($str)' target='newBehavior'/>
	<transition target='currentBehavior'/>

	</state>
</scxml>";

		var services = new ServiceCollection();

		services.RegisterStateMachineFactory();

		services.AddForwarding<IScxmlStateMachine>(_ => new ScxmlStateMachine(xml));

		var provider = services.BuildProvider();

		var stateMachine = await provider.GetRequiredService<IStateMachine>();

		Assert.IsNotNull(stateMachine);
	}
}