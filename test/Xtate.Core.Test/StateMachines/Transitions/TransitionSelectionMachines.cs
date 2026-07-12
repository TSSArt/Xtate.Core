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

namespace Xtate.Test.StateMachines.Transitions;

public class TransitionSelectionMachines : IScxmlTestSource
{
	public static readonly string FirstMatchingTransitionInDocumentOrderWins = """
																			   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
																			     <state id="start">
																			   	<onentry>
																			   	  <raise event="go"/>
																			   	</onentry>
																			   	<transition event="go" target="first"/>
																			   	<transition event="go" target="second"/>
																			     </state>
																			     <final id="first">
																			   	<donedata><content>first</content></donedata>
																			     </final>
																			     <final id="second">
																			   	<donedata><content>second</content></donedata>
																			     </final>
																			   </scxml>
																			   """;

	public static readonly string FalseConditionFallsThroughToLaterTransition = """
																				<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
																				  <state id="start">
																					<onentry>
																					  <raise event="go"/>
																					</onentry>
																					<transition event="go" cond="false()" target="failed"/>
																					<transition event="go" cond="true()" target="done"/>
																				  </state>
																				  <final id="failed">
																					<donedata><content>failed</content></donedata>
																				  </final>
																				  <final id="done">
																					<donedata><content>condition-fallthrough</content></donedata>
																				  </final>
																				</scxml>
																				""";

	public static readonly string DescendantTransitionPreemptsAncestorTransition = """
																				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="parent">
																				     <state id="parent" initial="child">
																				   	<onentry>
																				   	  <raise event="go"/>
																				   	</onentry>
																				   	<transition event="go" target="ancestorWin"/>
																				   	<state id="child">
																				   	  <transition event="go" target="descendantWin"/>
																				   	</state>
																				     </state>
																				     <final id="ancestorWin">
																				   	<donedata><content>ancestor</content></donedata>
																				     </final>
																				     <final id="descendantWin">
																				   	<donedata><content>descendant</content></donedata>
																				     </final>
																				   </scxml>
																				   """;

	public static readonly string AncestorTransitionHandlesEventWhenChildDoesNot = """
																				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="parent">
																				     <state id="parent" initial="child">
																				   	<onentry>
																				   	  <raise event="go"/>
																				   	</onentry>
																				   	<transition event="go" target="done"/>
																				   	<state id="child">
																				   	  <transition event="other" target="failed"/>
																				   	</state>
																				     </state>
																				     <final id="failed">
																				   	<donedata><content>failed</content></donedata>
																				     </final>
																				     <final id="done">
																				   	<donedata><content>ancestor-fallback</content></donedata>
																				     </final>
																				   </scxml>
																				   """;

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "Transitions/Selection/FirstMatchingTransitionInDocumentOrderWins", FirstMatchingTransitionInDocumentOrderWins, ExpectedFinalData: "first");
		yield return new ScxmlTestCase(
			Name: "Transitions/Selection/FalseConditionFallsThroughToLaterTransition", FalseConditionFallsThroughToLaterTransition, ExpectedFinalData: "condition-fallthrough");
		yield return new ScxmlTestCase(Name: "Transitions/Selection/DescendantTransitionPreemptsAncestorTransition", DescendantTransitionPreemptsAncestorTransition, ExpectedFinalData: "descendant");
		yield return new ScxmlTestCase(
			Name: "Transitions/Selection/AncestorTransitionHandlesEventWhenChildDoesNot", AncestorTransitionHandlesEventWhenChildDoesNot, ExpectedFinalData: "ancestor-fallback");
	}

#endregion
}