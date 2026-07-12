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

namespace Xtate.Test.StateMachines.XPath;

public class ExecutableContentMachines : IScxmlTestSource
{
	public static readonly string AssignExpressionControlsTransition = """
																	   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
																	     <datamodel>
																	   	<data id="marker" expr="'unset'"/>
																	     </datamodel>
																	     <state id="start">
																	   	<onentry>
																	   	  <assign location="$marker" expr="'set'"/>
																	   	</onentry>
																	   	<transition cond="$marker = 'set'" target="done"/>
																	   	<transition target="failed"/>
																	     </state>
																	     <final id="failed">
																	   	<donedata><content>failed</content></donedata>
																	     </final>
																	     <final id="done">
																	   	<donedata><content>assigned</content></donedata>
																	     </final>
																	   </scxml>
																	   """;

	public static readonly string IfFirstTrueBranchWins = """
														  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
														    <datamodel>
														  	<data id="result" expr="'unset'"/>
														    </datamodel>
														    <state id="start">
														  	<onentry>
														  	  <if cond="true()">
														  		<assign location="$result" expr="'first'"/>
														  	  <elseif cond="true()"/>
														  		<assign location="$result" expr="'second'"/>
														  	  <else/>
														  		<assign location="$result" expr="'else'"/>
														  	  </if>
														  	</onentry>
														  	<transition target="done"/>
														    </state>
														    <final id="done">
														  	<donedata><content expr="$result"/></donedata>
														    </final>
														  </scxml>
														  """;

	public static readonly string IfElseBranchRunsWhenNoConditionMatches = """
																		   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
																		     <datamodel>
																		   	<data id="result" expr="'unset'"/>
																		     </datamodel>
																		     <state id="start">
																		   	<onentry>
																		   	  <if cond="false()">
																		   		<assign location="$result" expr="'first'"/>
																		   	  <elseif cond="false()"/>
																		   		<assign location="$result" expr="'second'"/>
																		   	  <else/>
																		   		<assign location="$result" expr="'else'"/>
																		   	  </if>
																		   	</onentry>
																		   	<transition target="done"/>
																		     </state>
																		     <final id="done">
																		   	<donedata><content expr="$result"/></donedata>
																		     </final>
																		   </scxml>
																		   """;

	public static readonly string MultipleOnEntryActionsRunInDocumentOrder = """
																			 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
																			   <datamodel>
																			 	<data id="result" expr="'0'"/>
																			   </datamodel>
																			   <state id="start">
																			 	<onentry>
																			 	  <assign location="$result" expr="'1'"/>
																			 	  <assign location="$result" expr="concat($result, '2')"/>
																			 	  <assign location="$result" expr="concat($result, '3')"/>
																			 	</onentry>
																			 	<transition target="done"/>
																			   </state>
																			   <final id="done">
																			 	<donedata><content expr="$result"/></donedata>
																			   </final>
																			 </scxml>
																			 """;

	public static readonly string ParentChildEntryOrderIsAncestorBeforeDescendant = """
																					<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="parent">
																					  <datamodel>
																						<data id="order" expr="''"/>
																					  </datamodel>
																					  <state id="parent" initial="child">
																						<onentry>
																						  <assign location="$order" expr="concat($order, 'parent-entry;')"/>
																						</onentry>
																						<state id="child">
																						  <onentry>
																							<assign location="$order" expr="concat($order, 'child-entry;')"/>
																						  </onentry>
																						  <transition target="done"/>
																						</state>
																					  </state>
																					  <final id="done">
																						<donedata><content expr="$order"/></donedata>
																					  </final>
																					</scxml>
																					""";

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "XPath/ExecutableContent/AssignExpressionControlsTransition", AssignExpressionControlsTransition, ExpectedFinalData: "assigned");
		yield return new ScxmlTestCase(Name: "XPath/ExecutableContent/IfFirstTrueBranchWins", IfFirstTrueBranchWins, ExpectedFinalData: "first");
		yield return new ScxmlTestCase(Name: "XPath/ExecutableContent/IfElseBranchRunsWhenNoConditionMatches", IfElseBranchRunsWhenNoConditionMatches, ExpectedFinalData: "else");
		yield return new ScxmlTestCase(Name: "XPath/ExecutableContent/MultipleOnEntryActionsRunInDocumentOrder", MultipleOnEntryActionsRunInDocumentOrder, ExpectedFinalData: "123");
		yield return new ScxmlTestCase(
			Name: "XPath/ExecutableContent/ParentChildEntryOrderIsAncestorBeforeDescendant", ParentChildEntryOrderIsAncestorBeforeDescendant, ExpectedFinalData: "parent-entry;child-entry;");
	}

#endregion
}