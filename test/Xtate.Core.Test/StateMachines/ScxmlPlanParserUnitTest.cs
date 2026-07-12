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

using Xtate.Class;
using Xtate.IoC;
using Xtate.StateMachine;
using Xtate.StateMachine.Validator;

namespace Xtate.Test.StateMachines;

[TestClass]
public class ScxmlPlanParserUnitTest
{
	private static Task<IStateMachine> Parse(string scxml)
	{
		var services = new ServiceCollection();
		var smc = new ScxmlStringStateMachine(scxml);
		smc.AddServices(services);
		var serviceProvider = services.BuildProvider();

		return serviceProvider.GetRequiredService<IStateMachine>().AsTask();
	}

	[ExcludeFromCodeCoverage]
	private static TState FindState<TState>(IStateMachine stateMachine, string id)
		where TState : IStateEntity
	{
		foreach (var state in stateMachine.States)
		{
			if (state is TState typedState && state.Id?.ToString() == id)
			{
				return typedState;
			}
		}

		Assert.Fail($"State '{id}' was not found.");

		throw new InvalidOperationException();
	}

	[TestMethod]
	public async Task RootContentModelAcceptsTopLevelDatamodelScriptStatesParallelFinalAndComments()
	{
		var stateMachine = await Parse("""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
			  <!-- top-level comment is ignored by the SCXML reader -->
			  <datamodel>
				<data id="value" expr="'ok'"/>
			  </datamodel>
			  <script/>
			  <state id="start"/>
			  <parallel id="par"/>
			  <final id="done"/>
			</scxml>
			""");

		Assert.IsNotNull(stateMachine.DataModel);
		Assert.IsNotNull(stateMachine.Script);
		Assert.AreEqual(3, stateMachine.States.Length);
	}

	[TestMethod]
	public async Task DataElementsAcceptExpressionInlineContentEmptyValueAndSourceForms()
	{
		var stateMachine = await Parse("""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath">
			  <datamodel>
				<data id="exprValue" expr="'expr'"/>
				<data id="inlineValue"><root xmlns="">text</root></data>
				<data id="emptyValue"/>
				<data id="sourceValue" src="res://Xtate.Core.Test/Xtate.Core.Test/Scxml/XInclude/SingleIncludeTarget.scxml"/>
			  </datamodel>
			</scxml>
			""");

		Assert.AreEqual(4, stateMachine.DataModel!.Data.Length);
	}

	[TestMethod]
	public async Task ExecutableLogFormsAreAccepted()
	{
		await Parse("""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
			  <state id="start">
				<onentry>
				  <log/>
				  <log label="label-only"/>
				  <log expr="'expr-only'"/>
				  <log label="both" expr="'expr'"/>
				</onentry>
				<transition target="done"/>
			  </state>
			  <final id="done"/>
			</scxml>
			""");
	}

	[TestMethod]
	public async Task ScriptFormsAreAccepted()
	{
		var stateMachine = await Parse("""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
			  <script>1 + 1</script>
			  <state id="start">
				<onentry>
				  <script>2 + 2</script>
				</onentry>
				<transition target="done"/>
			  </state>
			  <final id="done"/>
			</scxml>
			""");

		Assert.IsNotNull(stateMachine.Script);
	}

	[TestMethod]
	public async Task XmlReaderAcceptsEntityReferenceCDataCommentAndProcessingInstruction()
	{
		var stateMachine = await Parse("""
			<?xml version="1.0"?>
			<?xtate-test processing-instruction?>
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="done">
			  <!-- comments are ignored -->
			  <final id="done">
				<donedata>
				  <content><![CDATA[value &amp; more]]></content>
				</donedata>
			  </final>
			</scxml>
			""");

		Assert.AreEqual("done", stateMachine.Initial!.Transition!.Target[0].ToString());
	}

	[TestMethod]
	public async Task DoneDataAcceptsContentExpressionInlineXmlWithNamespacesAndParameters()
	{
		var stateMachine = await Parse("""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath">
			  <final id="contentExpr">
				<donedata>
				  <content expr="'expr-value'"/>
				</donedata>
			  </final>
			  <final id="contentBody">
				<donedata>
				  <content><ns:payload xmlns:ns="urn:xtate:test">text</ns:payload></content>
				</donedata>
			  </final>
			  <final id="params">
				<donedata>
				  <param name="exprParam" expr="'value'"/>
				  <param name="locationParam" location="value"/>
				</donedata>
			  </final>
			</scxml>
			""");

		var contentExprFinal = FindState<IFinal>(stateMachine, "contentExpr");
		var contentBodyFinal = FindState<IFinal>(stateMachine, "contentBody");
		var paramsFinal = FindState<IFinal>(stateMachine, "params");

		Assert.IsNotNull(contentExprFinal.DoneData!.Content!.Expression);
		Assert.IsNotNull(contentBodyFinal.DoneData!.Content!.Body);
		StringAssert.Contains(contentBodyFinal.DoneData.Content.Body.Value!, "urn:xtate:test");
		Assert.AreEqual(2, paramsFinal.DoneData!.Parameters.Length);
		Assert.IsNotNull(paramsFinal.DoneData.Parameters[0].Expression);
		Assert.IsNotNull(paramsFinal.DoneData.Parameters[1].Location);
	}

	[TestMethod]
	public async Task TransitionWithEmptyEventDescriptorIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(
			() => Parse("""
				<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
				  <state id="start">
					<transition event="" target="done"/>
				  </state>
				  <final id="done"/>
				</scxml>
				"""));
	}

	[TestMethod]
	public async Task ParamWithoutNameOrWithBothExpressionAndLocationIsRejected()
	{
		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(
			() => Parse("""
				<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				  <final id="done">
					<donedata>
					  <param expr="'value'"/>
					</donedata>
				  </final>
				</scxml>
				"""));

		await Assert.ThrowsExactlyAsync<StateMachineValidationException>(
			() => Parse("""
				<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				  <final id="done">
					<donedata>
					  <param name="conflicting" expr="'value'" location="value"/>
					</donedata>
				  </final>
				</scxml>
				"""));
	}
}
