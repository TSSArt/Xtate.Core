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

namespace Xtate.Test.StateMachines.History;

public class DeepHistoryMachines : IScxmlTestSource
{
	public static readonly string DeepHistoryRestoresNestedChild = """
																   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
																     <state id="outer" initial="parent">
																   	<history id="hist" type="deep">
																   	  <transition target="parent"/>
																   	</history>
																   	<state id="parent" initial="child1">
																   	  <state id="child1">
																   		<transition target="child2"/>
																   	  </state>
																   	  <state id="child2">
																   		<transition target="done"/>
																   	  </state>
																   	</state>
																     </state>
																     <final id="done"/>
																   </scxml>
																   """;

	public static readonly string DeepHistoryDefaultTransition = """
																 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
																   <state id="outer">
																 	<history id="hist" type="deep">
																 	  <transition target="done"/>
																 	</history>
																 	<state id="child">
																 	  <transition target="done"/>
																 	</state>
																   </state>
																   <final id="done"/>
																 </scxml>
																 """;

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "History/Deep/DeepHistoryRestoresNestedChild", DeepHistoryRestoresNestedChild);
		yield return new ScxmlTestCase(Name: "History/Deep/DeepHistoryDefaultTransition", DeepHistoryDefaultTransition);
	}

#endregion
}