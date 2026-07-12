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

/// <summary>
///     SCXML machines that exercise the XPath data model and simple executable
///     constructs already known to work in the codebase.
/// </summary>
public class XPathMachines : IScxmlTestSource
{
	public static readonly string XPathRaiseOnEntry = """
													  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="first">
													    <state id="first">
													  	<onentry>
													  	  <raise event="go"/>
													  	</onentry>
													  	<transition event="go" target="done"/>
													    </state>
													    <final id="done"/>
													  </scxml>
													  """;

#region Interface IScxmlTestSource

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "XPath/XPathRaiseOnEntry", XPathRaiseOnEntry);
	}

#endregion
}