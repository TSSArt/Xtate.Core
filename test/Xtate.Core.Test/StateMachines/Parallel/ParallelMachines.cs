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

namespace Xtate.Test.StateMachines.Parallel;

public class ParallelMachines : IScxmlTestSource
{
	public static readonly string SimpleParallelTwoRegions = """
															 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="par">
															   <parallel id="par">
															 	<state id="left">
															 	  <final id="leftDone"/>
															 	</state>
															 	<state id="right">
															 	  <final id="rightDone"/>
															 	</state>
															 	<transition event="done.state.par" target="done"/>
															   </parallel>
															   <final id="done"/>
															 </scxml>
															 """;

	public static readonly string ParallelInsideCompound = """
														   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
														     <state id="outer" initial="par">
														   	<parallel id="par">
														   	  <state id="a">
														   		<final id="aDone"/>
														   	  </state>
														   	  <state id="b">
														   		<final id="bDone"/>
														   	  </state>
														   	  <transition event="done.state.par" target="done"/>
														   	</parallel>
														     </state>
														     <final id="done"/>
														   </scxml>
														   """;

	public static readonly string ParallelCompletionToDone = """
															 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="par">
															   <parallel id="par">
															 	<state id="r1">
															 	  <final id="r1Done"/>
															 	</state>
															 	<state id="r2">
															 	  <final id="r2Done"/>
															 	</state>
															 	<transition event="done.state.par" target="done"/>
															   </parallel>
															   <final id="done"/>
															 </scxml>
															 """;

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "Parallel/Basic/SimpleParallelTwoRegions", SimpleParallelTwoRegions);
		yield return new ScxmlTestCase(Name: "Parallel/Basic/ParallelInsideCompound", ParallelInsideCompound);
		yield return new ScxmlTestCase(Name: "Parallel/Completion/ParallelCompletionToDone", ParallelCompletionToDone);
	}

#endregion
}