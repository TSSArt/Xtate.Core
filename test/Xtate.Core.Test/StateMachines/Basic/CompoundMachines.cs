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

public class CompoundMachines : IScxmlTestSource
{
	public static readonly string NestedCompoundToDone = """
														 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
														   <state id="outer" initial="inner">
														 	<state id="inner">
														 	  <transition target="done"/>
														 	</state>
														   </state>
														   <final id="done"/>
														 </scxml>
														 """;

	public static readonly string CompoundCompletionEventToDone = """
																  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
																    <state id="outer" initial="inner">
																  	<state id="inner">
																  	  <transition target="innerFinal"/>
																  	</state>
																  	<final id="innerFinal"/>
																  	<transition event="done.state.outer" target="done"/>
																    </state>
																    <final id="done"/>
																  </scxml>
																  """;

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "Basic/Compound/NestedCompoundToDone", NestedCompoundToDone);
		yield return new ScxmlTestCase(Name: "Basic/Compound/CompoundCompletionEventToDone", CompoundCompletionEventToDone);
	}

#endregion
}