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

/// <summary>
///     A collection of basic SCXML state machine test cases covering fundamental
///     state machine behaviour: single-final states, sequential transitions, and
///     simple on-entry/on-exit processing.
/// </summary>
public class BasicMachines : IScxmlTestSource
{
	// ─────────────────────────────────────────────
	// Machine definitions (raw string literals)
	// ─────────────────────────────────────────────

	/// <summary>
	///     The simplest possible state machine: one final state reached immediately.
	/// </summary>
	public static readonly string SingleFinalState = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="done">
		  <final id="done"/>
		</scxml>
		""";

	/// <summary>
	///     Two states with an eventless transition leading to a final state.
	/// </summary>
	public static readonly string TwoStatesEventlessTransition = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="s1">
		  <state id="s1">
			<transition target="done"/>
		  </state>
		  <final id="done"/>
		</scxml>
		""";

	/// <summary>
	///     Three sequential states joined by eventless transitions.
	/// </summary>
	public static readonly string ThreeSequentialStates = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="s1">
		  <state id="s1">
			<transition target="s2"/>
		  </state>
		  <state id="s2">
			<transition target="done"/>
		  </state>
		  <final id="done"/>
		</scxml>
		""";

	/// <summary>
	///     Verifies that <c>&lt;onentry&gt;</c> and <c>&lt;onexit&gt;</c> blocks are
	///     executed in the expected order as the machine passes through states.
	/// </summary>
	public static readonly string OnEntryOnExit = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="s1">
		  <state id="s1">
			<onentry>
			  <log label="entering s1"/>
			</onentry>
			<onexit>
			  <log label="exiting s1"/>
			</onexit>
			<transition target="done"/>
		  </state>
		  <final id="done"/>
		</scxml>
		""";

	/// <summary>
	///     A final state that carries a string value in its done-data content.
	/// </summary>
	public static readonly string FinalStateWithDoneData = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="done">
		  <final id="done">
			<donedata>
			  <content>hello</content>
			</donedata>
		  </final>
		</scxml>
		""";

	// ─────────────────────────────────────────────
	// IScxmlTestSource
	// ─────────────────────────────────────────────

	/// <inheritdoc />
	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase("Basic/SingleFinalState",               SingleFinalState);
		yield return new ScxmlTestCase("Basic/TwoStatesEventlessTransition",   TwoStatesEventlessTransition);
		yield return new ScxmlTestCase("Basic/ThreeSequentialStates",          ThreeSequentialStates);
		yield return new ScxmlTestCase("Basic/OnEntryOnExit",                  OnEntryOnExit);
		yield return new ScxmlTestCase("Basic/FinalStateWithDoneData",         FinalStateWithDoneData, ExpectedFinalData: "hello");
	}
}
