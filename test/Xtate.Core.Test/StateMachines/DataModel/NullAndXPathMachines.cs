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

namespace Xtate.Test.StateMachines.DataModel;

public class NullAndXPathMachines : IScxmlTestSource
{
	public static readonly string NullDataModelSimpleTransition = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="null" initial="start">
		  <state id="start">
			<transition target="done"/>
		  </state>
		  <final id="done"/>
		</scxml>
		""";

	public static readonly string XPathFinalContent = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
		  <datamodel>
			<data id="value" expr="'assigned'"/>
		  </datamodel>
		  <state id="start">
			<transition target="done"/>
		  </state>
		  <final id="done">
			<donedata>
			  <content>assigned</content>
			</donedata>
		  </final>
		</scxml>
		""";

	public static readonly string XPathConditionUsesData = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="check">
		  <datamodel>
			<data id="flag" expr="true()"/>
		  </datamodel>
		  <state id="check">
			<transition cond="$flag = true()" target="done"/>
			<transition target="fallback"/>
		  </state>
		  <final id="fallback"/>
		  <final id="done"/>
		</scxml>
		""";

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase("DataModel/Null/NullDataModelSimpleTransition", NullDataModelSimpleTransition);
		yield return new ScxmlTestCase("DataModel/XPath/XPathFinalContent", XPathFinalContent, ExpectedFinalData: "assigned");
		yield return new ScxmlTestCase("DataModel/XPath/XPathConditionUsesData", XPathConditionUsesData);
	}
}
