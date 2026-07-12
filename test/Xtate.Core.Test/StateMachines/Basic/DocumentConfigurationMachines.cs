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

public class DocumentConfigurationMachines : IScxmlTestSource
{
	public static readonly string RootInitialTargetsParallelDescendants = """
																		  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="leftStart rightStart">
																		    <parallel id="par">
																		  	<state id="left" initial="leftStart">
																		  	  <state id="leftStart">
																		  		<transition target="leftDone"/>
																		  	  </state>
																		  	  <final id="leftDone"/>
																		  	</state>
																		  	<state id="right" initial="rightStart">
																		  	  <state id="rightStart">
																		  		<transition target="rightDone"/>
																		  	  </state>
																		  	  <final id="rightDone"/>
																		  	</state>
																		  	<transition event="done.state.par" target="done"/>
																		    </parallel>
																		    <final id="done">
																		  	<donedata><content>parallel-initial</content></donedata>
																		    </final>
																		  </scxml>
																		  """;

	public static readonly string RootInitialTargetEntersAncestorAndDefaultDescendant = """
																						<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
																						  <state id="outer" initial="inner">
																							<state id="inner">
																							  <transition target="done"/>
																							</state>
																						  </state>
																						  <final id="done">
																							<donedata><content>ancestor-default</content></donedata>
																						  </final>
																						</scxml>
																						""";

	public static readonly string BindingDefaultIsEarly = """
														  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
														    <datamodel>
														  	<data id="value" expr="'early-default'"/>
														    </datamodel>
														    <state id="start">
														  	<transition target="done"/>
														    </state>
														    <final id="done">
														  	<donedata><content expr="$value"/></donedata>
														    </final>
														  </scxml>
														  """;

	public static readonly string MissingDataModelUsesPlatformDefault = """
																		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="done">
																		  <final id="done">
																			<donedata><content>default-datamodel</content></donedata>
																		  </final>
																		</scxml>
																		""";

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "Basic/DocumentConfiguration/RootInitialTargetsParallelDescendants", RootInitialTargetsParallelDescendants, ExpectedFinalData: "parallel-initial");
		yield return new ScxmlTestCase(
			Name: "Basic/DocumentConfiguration/RootInitialTargetEntersAncestorAndDefaultDescendant", RootInitialTargetEntersAncestorAndDefaultDescendant, ExpectedFinalData: "ancestor-default");
		yield return new ScxmlTestCase(Name: "Basic/DocumentConfiguration/BindingDefaultIsEarly", BindingDefaultIsEarly, ExpectedFinalData: "early-default");
		yield return new ScxmlTestCase(Name: "Basic/DocumentConfiguration/MissingDataModelUsesPlatformDefault", MissingDataModelUsesPlatformDefault, ExpectedFinalData: "default-datamodel");
	}

#endregion
}