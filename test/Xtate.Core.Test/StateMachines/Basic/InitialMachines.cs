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

public class InitialMachines : IScxmlTestSource
{
	public static readonly string RootInitialAttribute = """
														 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
														   <state id="start">
														 	<transition target="done"/>
														   </state>
														   <final id="done"/>
														 </scxml>
														 """;

	public static readonly string DefaultInitialChild = """
														<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
														  <state id="parent" initial="child1">
															<state id="child1">
															  <transition target="done"/>
															</state>
															<state id="child2"/>
														  </state>
														  <final id="done"/>
														</scxml>
														""";

	public static readonly string RootNameAndDatamodel = """
														 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" name="RootName" datamodel="null" initial="done">
														   <final id="done"/>
														 </scxml>
														 """;

	public static readonly string RootBindingEarly = """
													 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" binding="early" initial="done">
													   <final id="done"/>
													 </scxml>
													 """;

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "Basic/Initial/RootInitialAttribute", RootInitialAttribute);
		yield return new ScxmlTestCase(Name: "Basic/Initial/DefaultInitialChild", DefaultInitialChild);
		yield return new ScxmlTestCase(Name: "Basic/Initial/RootNameAndDatamodel", RootNameAndDatamodel);
		yield return new ScxmlTestCase(Name: "Basic/Initial/RootBindingEarly", RootBindingEarly);
	}

#endregion
}