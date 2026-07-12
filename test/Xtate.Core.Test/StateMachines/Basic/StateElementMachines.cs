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

namespace Xtate.Test.StateMachines.Basic;

public class StateElementMachines : IScxmlTestSource
{
	public static readonly string InitialChildTransitionIsHonored = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
		  <state id="outer">
			<initial>
			  <transition target="selected"/>
			</initial>
			<final id="first">
			  <donedata><content>first</content></donedata>
			</final>
			<final id="selected">
			  <donedata><content>selected</content></donedata>
			</final>
			<transition event="done.state.outer" target="done"/>
		  </state>
		  <final id="done">
			<donedata><content>initial-child</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string InitialTransitionActionsRunInDocumentOrder = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="outer">
		  <datamodel>
			<data id="order" expr="''"/>
		  </datamodel>
		  <state id="outer">
			<initial>
			  <transition target="child">
				<assign location="$order" expr="concat($order, '1')"/>
				<assign location="$order" expr="concat($order, '2')"/>
				<assign location="$order" expr="concat($order, '3')"/>
			  </transition>
			</initial>
			<final id="child"/>
			<transition event="done.state.outer" target="done"/>
		  </state>
		  <final id="done">
			<donedata><content expr="$order"/></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string InitialTransitionCanEnterNestedDescendant = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
		  <state id="outer">
			<initial>
			  <transition target="grandchild"/>
			</initial>
			<state id="parent" initial="sibling">
			  <state id="sibling">
				<transition target="failed"/>
			  </state>
			  <state id="grandchild">
				<transition target="innerDone"/>
			  </state>
			  <final id="innerDone"/>
			</state>
			<transition event="done.state.parent" target="done"/>
		  </state>
		  <final id="failed">
			<donedata><content>failed</content></donedata>
		  </final>
		  <final id="done">
			<donedata><content>nested-descendant</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string FinalDoneDataWithExpression = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
		  <datamodel>
			<data id="value" expr="'expr-done-data'"/>
		  </datamodel>
		  <state id="start">
			<transition target="done"/>
		  </state>
		  <final id="done">
			<donedata><content expr="$value"/></donedata>
		  </final>
		</scxml>
		""";

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase("Basic/States/InitialChildTransitionIsHonored", InitialChildTransitionIsHonored, ExpectedFinalData: "initial-child");
		yield return new ScxmlTestCase("Basic/States/InitialTransitionActionsRunInDocumentOrder", InitialTransitionActionsRunInDocumentOrder, ExpectedFinalData: "123");
		yield return new ScxmlTestCase("Basic/States/InitialTransitionCanEnterNestedDescendant", InitialTransitionCanEnterNestedDescendant, ExpectedFinalData: "nested-descendant");
		yield return new ScxmlTestCase("Basic/States/FinalDoneDataWithExpression", FinalDoneDataWithExpression, ExpectedFinalData: "expr-done-data");
	}
}
