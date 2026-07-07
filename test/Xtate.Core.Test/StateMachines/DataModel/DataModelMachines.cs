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

/// <summary>
///     A collection of SCXML state machine test cases focused on data model scenarios:
///     null data model, static data assignment, and inline content in done-data.
///     All machines use the built-in <c>null</c> data model to avoid a JavaScript
///     or XPath engine dependency.
/// </summary>
public class DataModelMachines : IScxmlTestSource
{
	// ─────────────────────────────────────────────
	// Machine definitions (raw string literals)
	// ─────────────────────────────────────────────

	/// <summary>
	///     Explicit null data model declaration — runs to completion without any
	///     scripting support.
	/// </summary>
	public static readonly string ExplicitNullDataModel = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="null" initial="done">
		  <final id="done"/>
		</scxml>
		""";
	
	/// <summary>
	///     Final state with inline XML content in done-data.
	///     The content is a plain string literal; no expression evaluation needed.
	/// </summary>
	public static readonly string FinalStateWithStringContent = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="done">
		  <final id="done">
			<donedata>
			  <content>result-value</content>
			</donedata>
		  </final>
		</scxml>
		""";

	/// <summary>
	///     A machine with a named initial state and a simple log action in onentry.
	///     Verifies that named initial attributes are resolved correctly.
	/// </summary>
	public static readonly string NamedInitialState = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" name="NamedMachine" initial="start">
		  <state id="start">
			<onentry>
			  <log label="started"/>
			</onentry>
			<transition target="done"/>
		  </state>
		  <final id="done"/>
		</scxml>
		""";

	/// <summary>
	///     Two independent final states; the machine uses an explicit initial
	///     attribute to choose between them.  Verifies that the <c>initial</c>
	///     attribute on <c>&lt;scxml&gt;</c> is honoured.
	/// </summary>
	public static readonly string ExplicitInitialSelectsFinalState = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="second">
		  <final id="first">
			<donedata><content>first</content></donedata>
		  </final>
		  <final id="second">
			<donedata><content>second</content></donedata>
		  </final>
		</scxml>
		""";

	// ─────────────────────────────────────────────
	// IScxmlTestSource
	// ─────────────────────────────────────────────

	/// <inheritdoc />
	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase("DataModel/ExplicitNullDataModel",              ExplicitNullDataModel);
		yield return new ScxmlTestCase("DataModel/FinalStateWithStringContent",        FinalStateWithStringContent,        ExpectedFinalData: "result-value");
		yield return new ScxmlTestCase("DataModel/NamedInitialState",                  NamedInitialState);
		yield return new ScxmlTestCase("DataModel/ExplicitInitialSelectsFinalState",   ExplicitInitialSelectsFinalState,   ExpectedFinalData: "second");
	}
}
