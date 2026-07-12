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

public class OnEntryOnExitMachines : IScxmlTestSource
{
	public static readonly string SimpleOnEntry = """
												  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
												    <state id="start">
												  	<onentry>
												  	  <log label="start-entry"/>
												  	</onentry>
												  	<transition target="done"/>
												    </state>
												    <final id="done"/>
												  </scxml>
												  """;

	public static readonly string SimpleOnExit = """
												 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
												   <state id="start">
												 	<onexit>
												 	  <log label="start-exit"/>
												 	</onexit>
												 	<transition target="done"/>
												   </state>
												   <final id="done"/>
												 </scxml>
												 """;

	public static readonly string ParentChildEntryExitOrder = """
															  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="parent">
															    <state id="parent" initial="child">
															  	<onentry>
															  	  <log label="parent-entry"/>
															  	</onentry>
															  	<onexit>
															  	  <log label="parent-exit"/>
															  	</onexit>
															  	<state id="child">
															  	  <onentry>
															  		<log label="child-entry"/>
															  	  </onentry>
															  	  <onexit>
															  		<log label="child-exit"/>
															  	  </onexit>
															  	  <transition target="done"/>
															  	</state>
															    </state>
															    <final id="done"/>
															  </scxml>
															  """;

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "Basic/OnEntryOnExit/SimpleOnEntry", SimpleOnEntry);
		yield return new ScxmlTestCase(Name: "Basic/OnEntryOnExit/SimpleOnExit", SimpleOnExit);
		yield return new ScxmlTestCase(Name: "Basic/OnEntryOnExit/ParentChildEntryExitOrder", ParentChildEntryExitOrder);
	}

#endregion
}