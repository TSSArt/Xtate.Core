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

public class EventlessTransitionMachines : IScxmlTestSource
{
	public static readonly string BasicEventlessTransition = """
															 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
															   <state id="start">
															 	<transition target="done"/>
															   </state>
															   <final id="done"/>
															 </scxml>
															 """;

	public static readonly string EventlessTransitionInCompound = """
																  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
																    <state id="outer" initial="inner">
																  	<state id="inner">
																  	  <transition target="done"/>
																  	</state>
																    </state>
																    <final id="done"/>
																  </scxml>
																  """;

	public static readonly string EventlessSelfTransition = """
															<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
															  <state id="start">
																<transition target="done"/>
															  </state>
															  <final id="done"/>
															</scxml>
															""";

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "Transitions/Eventless/BasicEventlessTransition", BasicEventlessTransition);
		yield return new ScxmlTestCase(Name: "Transitions/Eventless/EventlessTransitionInCompound", EventlessTransitionInCompound);
		yield return new ScxmlTestCase(Name: "Transitions/Eventless/EventlessSelfTransition", EventlessSelfTransition);
	}

#endregion
}