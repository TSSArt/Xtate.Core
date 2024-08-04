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

using System.Xml;
using Xtate.Core;
using Xtate.IoC;

namespace Xtate.Test;

[TestClass]
public class ScxmlToDomTest
{
	private static IStateMachine GetStateMachine(string scxml)
	{
		var services = new ServiceCollection();
		services.AddModule<StateMachineFactoryModule>();
		services.AddConstant<IScxmlStateMachine>(new ScxmlStateMachine(scxml));
		var serviceProvider = services.BuildProvider();

		return serviceProvider.GetRequiredService<IStateMachine>().Result;
	}

	private static IStateMachine GetStateMachineWithRoot(string xml) => GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'>" + xml + "</scxml>");

	private static IStateMachine GetStateMachineXyzDataModel(string xml) => GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' datamodel='xyz'>" + xml + "</scxml>");

	[TestMethod]
	public void RootElementEmptyTest()
	{
		var sm = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'/>");
		Assert.IsNull(sm.DataModelType);
		Assert.AreEqual(BindingType.Early, sm.Binding);
		Assert.IsNull(sm.DataModel);
		Assert.IsNull(sm.Initial);
		Assert.IsNull(sm.Name);
		Assert.IsNull(sm.Script);
		Assert.IsTrue(sm.States.IsDefault);
	}

	[TestMethod]
	public void RootElementBeginEndTest()
	{
		var sm = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'></scxml>");
		Assert.IsNull(sm.DataModel);
		Assert.IsNull(sm.Initial);
		Assert.IsNull(sm.Name);
		Assert.IsNull(sm.Script);
		Assert.IsTrue(sm.States.IsDefault);
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementNameFailTest()
	{
		GetStateMachine("<no-scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'/>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementVersionFailTest()
	{
		GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='0.2'/>");
	}

	[TestMethod]
	[ExpectedException(typeof(XmlException))]
	public void RootElementUnknownAttributesTest()
	{
		GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' attr0='00' attr0='11' attr1='22' />");
	}

	[TestMethod]
	public void RootElementDataModelTypeTest()
	{
		var sm = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'/>");
		Assert.IsNull(sm.DataModelType);

		var sm1 = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' datamodel='null'/>");
		Assert.AreEqual(expected: "null", sm1.DataModelType);

		var sm2 = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' datamodel='ecmascript'/>");
		Assert.AreEqual(expected: "ecmascript", sm2.DataModelType);
	}

	[TestMethod]
	public void RootElementBindingTest()
	{
		var sm = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' binding='early'/>");
		Assert.AreEqual(BindingType.Early, sm.Binding);

		var sm2 = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' binding='late'/>");
		Assert.AreEqual(BindingType.Late, sm2.Binding);
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementInvalidEmptyBindingTest()
	{
		GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' binding=''/>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementInvalidWrongNameBindingTest()
	{
		GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' binding='invalid-binding'/>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementInvalidUpperCaseBindingTest()
	{
		GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' binding='Late'/>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementEmptyNameFailTest()
	{
		var sm = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' name=''/>");
		Assert.AreEqual(expected: "", sm.Name);
	}

	[TestMethod]
	public void RootElementNameTest()
	{
		var sm = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' name='It is name'/>");
		Assert.AreEqual(expected: "It is name", sm.Name);
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementEmptyInitialTest()
	{
		GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' initial=''/>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementSpaceInitialTest()
	{
		GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' initial=' '/>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void RootElementInitialFailTest()
	{
		GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' initial=' trg2  trg1 '/>");
	}

	[TestMethod]
	public void RootElementInitialTest()
	{
		var sm = GetStateMachine("<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' initial='trg2 trg1'><state/></scxml>");
		Assert.AreEqual((Identifier) "trg2", sm.Initial!.Transition!.Target[0]);
		Assert.AreEqual((Identifier) "trg1", sm.Initial.Transition.Target[1]);
		Assert.AreEqual(expected: 2, sm.Initial.Transition.Target.Length);
	}

	[TestMethod]
	public void DataModelTest()
	{
		var sm = GetStateMachineXyzDataModel("<datamodel></datamodel>");
		Assert.IsNotNull(sm.DataModel);
		Assert.IsTrue(sm.DataModel!.Data.IsDefault);
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void IncorrectXmlTest()
	{
		GetStateMachineWithRoot("<datamodel><data id='a'/><data id='b'></data><data id='c' src='c-src/><data id='d' expr='d-expr'/><data id='e'>e-body</data></datamodel>");
	}

	[TestMethod]
	public void DataModelWithDataTest()
	{
		var sm = GetStateMachineXyzDataModel("<datamodel><data id='a'/><data id='b'></data><data id='c' src='c-src'/><data id='d' expr='d-expr'/><data id='e'>e-body</data></datamodel>");
		Assert.IsNotNull(sm.DataModel);
		Assert.AreEqual(expected: 5, sm.DataModel!.Data.Length);

		Assert.AreEqual(expected: "a", sm.DataModel.Data[0].Id);
		Assert.IsNull(sm.DataModel.Data[0].Source);
		Assert.IsNull(sm.DataModel.Data[0].Expression);

		Assert.AreEqual(expected: "b", sm.DataModel.Data[1].Id);
		Assert.IsNull(sm.DataModel.Data[1].Source);
		Assert.AreEqual(expected: "", sm.DataModel.Data[1].InlineContent?.Value);

		Assert.AreEqual(expected: "c", sm.DataModel.Data[2].Id);
		Assert.AreEqual(expected: "c-src", sm.DataModel.Data[2].Source!.Uri!.ToString());
		Assert.IsNull(sm.DataModel.Data[2].Expression);

		Assert.AreEqual(expected: "d", sm.DataModel.Data[3].Id);
		Assert.IsNull(sm.DataModel.Data[3].Source);
		Assert.AreEqual(expected: "d-expr", sm.DataModel.Data[3].Expression!.Expression);

		Assert.AreEqual(expected: "e", sm.DataModel.Data[4].Id);
		Assert.IsNull(sm.DataModel.Data[4].Source);
		Assert.AreEqual(expected: "e-body", sm.DataModel.Data[4].InlineContent?.Value);
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void TwoDataModelTest()
	{
		GetStateMachineXyzDataModel("<datamodel/><datamodel/>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void DataNoIdTest()
	{
		GetStateMachineXyzDataModel("<datamodel><data></data></datamodel>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void DataSrcAndExprFailTest()
	{
		GetStateMachineXyzDataModel("<datamodel><data id='a' src='domain' expr='some-expr'/></datamodel>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void DataSrcAndBodyFailTest()
	{
		GetStateMachineXyzDataModel("<datamodel><data id='a' src='domain'>123</data></datamodel>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void DataBodyAndExprFailTest()
	{
		GetStateMachineXyzDataModel("<datamodel><data id='a' expr='some-expr'>123</data></datamodel>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void DataSrcAndBodyAndExprFailTest()
	{
		GetStateMachineXyzDataModel("<datamodel><data id='a' src='s-src' expr='some-expr'>123</data></datamodel>");
	}

	[TestMethod]
	public void GlobalScriptTest()
	{
		var sm = GetStateMachineXyzDataModel("<script/>");
		Assert.IsInstanceOfType(sm.Script, typeof(IScript));
		var script = (IScript) sm.Script!;
		Assert.IsNull(script.Content);
		Assert.IsNull(script.Source);
	}

	[TestMethod]
	public void GlobalScriptBodyTest()
	{
		var sm = GetStateMachineXyzDataModel("<script><any_script xmlns='aaa'>345</any_script></script>");
		Assert.IsInstanceOfType(sm.Script, typeof(IScript));
		var script = (IScript) sm.Script!;
		Assert.IsInstanceOfType(script.Content, typeof(IScriptExpression));
		var scriptExpression = script.Content;
		Assert.AreEqual(expected: "<any_script xmlns=\"aaa\">345</any_script>", scriptExpression!.Expression);
		Assert.IsNull(script.Source);
	}

	[TestMethod]
	public void GlobalScriptSrcTest()
	{
		var sm = GetStateMachineXyzDataModel("<script src='s-src'/>");
		Assert.IsInstanceOfType(sm.Script, typeof(IScript));
		var script = (IScript) sm.Script!;
		Assert.IsInstanceOfType(script.Source, typeof(IExternalScriptExpression));
		var externalScriptExpression = script.Source;
		Assert.AreEqual(expected: "s-src", externalScriptExpression!.Uri!.ToString());
		Assert.IsNull(script.Content);
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void GlobalScriptSrcAndBodyFailTest()
	{
		GetStateMachineXyzDataModel("<script src='s-src'>body</script>");
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void MultipleGlobalScriptFailTest()
	{
		GetStateMachineXyzDataModel("<script/><script/>");
	}

	[TestMethod]
	[DataRow("state")]
	[DataRow("parallel")]
	[DataRow("final")]
	public void MultipleStateTest(string element)
	{
		GetStateMachineWithRoot($"<{element}/>");
		GetStateMachineWithRoot($"<{element}></{element}>");
		GetStateMachineWithRoot($"<{element}/><{element}/>");
		GetStateMachineWithRoot($"<{element}/><{element}/><{element}/>");
	}

	[TestMethod]
	public void StateNoAttrTest()
	{
		var sm = GetStateMachineWithRoot("<state/>");
		Assert.IsNull(((IState) sm.States[0]).Id);
		Assert.IsNull(((IState) sm.States[0]).Initial);
	}

	[TestMethod]
	public void StateIdTest()
	{
		var sm = GetStateMachineWithRoot("<state id='a'/>");
		Assert.AreEqual((Identifier) "a", ((IState) sm.States[0]).Id);
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void StateIdFailTest()
	{
		var sm = GetStateMachineWithRoot("<state id='a b'/>");
		Assert.AreEqual((Identifier) "a", ((IState) sm.States[0]).Id);
	}

	[TestMethod]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void StateInitialFailForAtomicStateTest()
	{
		GetStateMachineWithRoot("<state initial='id id2'/>");
	}

	[TestMethod]
	public void StateInitialTest()
	{
		var sm = GetStateMachineWithRoot("<state initial='id id2'><parallel/></state>");
		Assert.IsNull(((IState) sm.States[0]).Id);
		Assert.AreEqual((Identifier) "id", ((IState) sm.States[0]).Initial!.Transition!.Target[0]);
		Assert.AreEqual((Identifier) "id2", ((IState) sm.States[0]).Initial!.Transition!.Target[1]);
	}

	[TestMethod]
	public void ParallelNoIdTest()
	{
		var sm = GetStateMachineWithRoot("<parallel/>");
		Assert.IsNull(((IParallel) sm.States[0]).Id);
	}

	[TestMethod]
	public void ParallelIdTest()
	{
		var sm = GetStateMachineWithRoot("<parallel id='a'/>");
		Assert.AreEqual((Identifier) "a", ((IParallel) sm.States[0]).Id);
	}

	[TestMethod]
	public void FinalNoIdTest()
	{
		var sm = GetStateMachineWithRoot("<final/>");
		Assert.IsNull(((IFinal) sm.States[0]).Id);
	}

	[TestMethod]
	public void FinalIdTest()
	{
		var sm = GetStateMachineWithRoot("<final id='a'/>");
		Assert.AreEqual((Identifier) "a", ((IFinal) sm.States[0]).Id);
	}

	[TestMethod]
	[DataRow("unknown")]
	[DataRow("initial")]
	[DataRow("history")]
	[DataRow("onentry")]
	[DataRow("onexit")]
	[DataRow("invoke")]
	[DataRow("transition")]
	[ExpectedException(typeof(StateMachineValidationException))]
	public void UnknownElementTest(string element)
	{
		GetStateMachineWithRoot($"<{element}/>");
	}

	[TestMethod]
	public void AtomicStateTest()
	{
		var sm = GetStateMachineWithRoot("<state><onentry/><onexit/><transition event='e'/><invoke type='tmp'/></state>");

		var state = (IState) sm.States[0];
		Assert.IsNull(state.Id);
		Assert.AreEqual(expected: 1, state.OnEntry.Length);
		Assert.AreEqual(expected: 1, state.OnExit.Length);
		Assert.AreEqual(expected: 1, state.Transitions.Length);
		Assert.AreEqual(expected: 1, state.Invoke.Length);
		Assert.IsNull(state.DataModel);
		Assert.IsTrue(state.HistoryStates.IsDefault);
		Assert.IsNull(state.Initial);
		Assert.IsTrue(state.States.IsDefault);
	}
}