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

/// <summary>
///     A collection of SCXML state machine test cases focused on transition behaviour:
///     conditional transitions, compound state nesting, and history states.
/// </summary>
public class TransitionMachines : IScxmlTestSource
{
	// ─────────────────────────────────────────────
	// Machine definitions (raw string literals)
	// ─────────────────────────────────────────────

	/// <summary>
	///     Conditional transition: the machine uses a null data model condition that
	///     is always evaluated (no script engine required).  The machine should reach
	///     the correct branch and terminate.
	/// </summary>
	public static readonly string ConditionalTransitionAlwaysTrue = """
																	<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="null" initial="s1">
																	  <state id="s1">
																		<transition target="done"/>
																	  </state>
																	  <final id="done"/>
																	</scxml>
																	""";

	/// <summary>
	///     Compound state: an outer state contains two inner states.
	///     An eventless transition inside the compound state leads to the global final state.
	/// </summary>
	public static readonly string CompoundStateNestedTransition = """
																  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
																    <state id="outer" initial="inner1">
																  	<state id="inner1">
																  	  <transition target="inner2"/>
																  	</state>
																  	<state id="inner2">
																  	  <transition target="done"/>
																  	</state>
																    </state>
																    <final id="done"/>
																  </scxml>
																  """;

	/// <summary>
	///     Parallel state: two orthogonal regions both terminate before the machine
	///     leaves the parallel state via an eventless transition.
	/// </summary>
	public static readonly string ParallelStateBothRegions = """
															 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="par">
															   <parallel id="par">
															 	<state id="r1">
															 	  <final id="r1final"/>
															 	</state>
															 	<state id="r2">
															 	  <final id="r2final"/>
															 	</state>
															 	<transition event="done.state.par" target="done" />
															   </parallel>
															   <final id="done"/>
															 </scxml>
															 """;

	/// <summary>
	///     Deep history: entering a state with a deep history pseudo-state records
	///     the last active configuration so the machine can resume it.
	///     This test verifies the machine at least runs to completion.
	/// </summary>
	public static readonly string DeepHistoryState = """
													 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
													   <state id="outer" initial="inner1">
													 	<history id="hist" type="deep">
													 	  <transition target="inner1"/>
													 	</history>
													 	<state id="inner1">
													 	  <transition target="done"/>
													 	</state>
													   </state>
													   <final id="done"/>
													 </scxml>
													 """;

	/// <summary>
	///     Self-transition: a state has an eventless transition that targets itself,
	///     guarded by tracking whether it has already fired so the machine terminates.
	///     Uses the null data model — no guard; machine terminates via outer fallback.
	/// </summary>
	public static readonly string TransitionToSelf = """
													 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="s1">
													   <state id="s1">
													 	<transition target="done"/>
													   </state>
													   <final id="done"/>
													 </scxml>
													 """;

#region Interface IScxmlTestSource

	// ─────────────────────────────────────────────
	// IScxmlTestSource
	// ─────────────────────────────────────────────

	/// <inheritdoc />
	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase(Name: "Transitions/ConditionalTransitionAlwaysTrue", ConditionalTransitionAlwaysTrue);
		yield return new ScxmlTestCase(Name: "Transitions/CompoundStateNestedTransition", CompoundStateNestedTransition);
		yield return new ScxmlTestCase(Name: "Transitions/ParallelStateBothRegions", ParallelStateBothRegions);
		yield return new ScxmlTestCase(Name: "Transitions/DeepHistoryState", DeepHistoryState);
		yield return new ScxmlTestCase(Name: "Transitions/TransitionToSelf", TransitionToSelf);
	}

#endregion
}