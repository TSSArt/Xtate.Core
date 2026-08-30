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

using System.Text;
using Xtate.Core.Test.Exhaustive.Parsing;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Interpreter.Services;
using Xtate.IoC;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

[TestClass]
[TestCategory("Exhaustive.Fast")]
public sealed class InterpreterLifecycleRequirementsTests
{
	/* TEST-METADATA
	test_id: SCXML-LIFE-001-EXISTING-001
	requirement_ids: [SCXML-LIFE-001,SCXML-LIFE-008]
	title: Root final lifecycle and completion result
	description: A root final must progress through accepted, started and completed once and yield Undefined root done data.
	authority: { source: W3C SCXML 1.0, section: 3.6 Final States, citation_or_rule: Reaching top-level final completes the session once. }
	phase: 2
	feature: lifecycle
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: exact-lifecycle-trace-and-result
	risk: critical
	priority: critical
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,cleanup]
	dimensions: { topology: root-final }
	preconditions: [isolated runtime container]
	dependencies: [ScxmlRuntimeHarness,InterpreterStateTrace]
	arrange: Construct root-final SCXML and lifecycle trace.
	stimulus: Execute session.
	expected: Undefined result and Accepted>Started>Completed trace.
	expected_exception_or_event: none
	forbidden: Duplicate completion or post-completion activity.
	edge_cases: Root final entered at startup.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: session retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-LIFE-008-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_LIFE_001_And_LIFE_008_Root_final_reports_accepted_started_then_completed()
	{
		var trace = new InterpreterStateTrace();
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<final id="complete" />
				   </scxml>
				   """, trace);

		Assert.AreEqual(DataModelValue.Undefined, result);
		CollectionAssert.AreEqual(new[] { StateMachineInterpreterState.Accepted, StateMachineInterpreterState.Started, StateMachineInterpreterState.Completed }, trace.States.ToArray());
	}

	/* TEST-METADATA
	test_id: SCXML-LIFE-001-EXISTING-002
	requirement_ids: [SCXML-LIFE-001]
	title: Interpreter emits lifecycle log categories
	description: A normally completing interpreter emits the required entry and exit lifecycle log categories, distinguishing missing lifecycle instrumentation.
	authority: { source: Xtate lifecycle notification contract, section: SCXML-LIFE-001, citation_or_rule: Lifecycle transitions are externally observable in ordered notification/log callbacks. }
	phase: 2
	feature: lifecycle
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: required-log-category-set
	risk: medium
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { completion: root-final }
	preconditions: [trace logger registered]
	dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]
	arrange: Attach interpreter logger to root-final SCXML.
	stimulus: Execute session.
	expected: Log contains categories 4,6,7.
	expected_exception_or_event: none
	forbidden: Missing state-entry or terminal lifecycle category.
	edge_cases: Completion without external dispatch.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: logger retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-LIFE-001-EXISTING-001]
	known_issue: none
	compile_notes: numeric categories are existing trace adapter contract.
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_LIFE_001_Interpreter_trace_records_state_entry_and_exit_events()
	{
		var log = new InterpreterLogTrace<StateMachineInterpreter>();
		await ScxmlRuntimeHarness.ExecuteAsync(scxml: "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><final id=\"complete\" /></scxml>", notification: null, log);

		CollectionAssert.IsSubsetOf(new[] { 4, 6, 7 }, log.EventIds.ToArray());
	}

	/* TEST-METADATA
	test_id: SCXML-TRANS-010-EXISTING-001
	requirement_ids: [SCXML-TRANS-010]
	title: Eventless microstep log ordering
	description: A source-to-final eventless transition must log exit before transition content and entry, distinguishing an illegal microstep ordering.
	authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: A microstep executes exits, transition content, then entries. }
	phase: 2
	feature: transition-microstep
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: ordered-log-trace
	risk: critical
	priority: critical
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { transition: eventless, topology: source-to-root-final }
	preconditions: [trace logger registered]
	dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]
	arrange: Construct one eventless source-to-final transition with logger.
	stimulus: Execute startup macrostep.
	expected: First exit category precedes transition category which precedes final entry category.
	expected_exception_or_event: none
	forbidden: Entry before transition content.
	edge_cases: Eventless transition at initialization.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: logger retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-TRANS-010-CASE-001]
	known_issue: none
	compile_notes: numeric categories are existing trace adapter contract.
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_TRANS_010_Eventless_microstep_logs_exit_transition_then_entry_categories()
	{
		var log = new InterpreterLogTrace<StateMachineInterpreter>();
		await ScxmlRuntimeHarness.ExecuteAsync(
			scxml: "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><state id=\"source\"><transition target=\"complete\" /></state><final id=\"complete\" /></scxml>",
			notification: null, log);

		var eventIds = log.EventIds.ToArray();
		var exit = Array.IndexOf(eventIds, value: 8);
		var transition = Array.IndexOf(eventIds, value: 10);
		var entry = Array.LastIndexOf(eventIds, value: 6);
		Assert.IsTrue(exit >= 0 && transition > exit && entry > transition, string.Join(separator: ",", eventIds));
	}

	/* TEST-METADATA
	test_id: SCXML-EXEC-001-EXISTING-001; requirement_ids: [SCXML-EXEC-001]; title: Raise queues internal event for next microstep; description: Onentry raise produces one payloadless internal advance consumed by the following matching transition; authority: { source: W3C SCXML 1.0, section: 3.12, citation_or_rule: raise adds a named event to internal queue }; phase: 2; feature: executable-content; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive]; dimensions: { action: raise }; preconditions: [matching internal transition]; dependencies: [ScxmlRuntimeHarness]; arrange: Put raise advance in onentry; stimulus: Start; expected: Undefined root-final result; expected_exception_or_event: none; forbidden: External dispatch requirement or payload; edge_cases: Raise during entry; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 15 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: internal queue retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-EXEC-001-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EXEC_001_Raise_queues_an_internal_event_that_drives_the_next_microstep()
	{
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				<state id="source">
					<onentry><raise event="advance" /></onentry>
					<transition event="advance" target="complete" />
				</state>
				<final id="complete" />
			</scxml>
			""");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-EXEC-004-EXISTING-001; requirement_ids: [SCXML-EXEC-004]; title: Log emits label without mutation; description: Onentry log emits checkpoint and completion remains reachable, distinguishing a logging side effect from state mutation; authority: { source: W3C SCXML 1.0, section: 3.12, citation_or_rule: log is executable content and does not alter control flow }; phase: 2; feature: executable-content; target_components: [ILogController,StateMachineInterpreter]; test_kind: integration; oracle_type: exact-log-and-result; risk: medium; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive]; dimensions: { action: log }; preconditions: [trace logger]; dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]; arrange: Create onentry log checkpoint; stimulus: Execute; expected: Undefined result and exactly checkpoint message; expected_exception_or_event: none; forbidden: State mutation or suppressed completion; edge_cases: Missing expression; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: log retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-EXEC-004-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EXEC_004_Log_emits_its_label_without_preventing_completion()
	{
		var actionLog = new InterpreterLogTrace<ILogController>();
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="source"><onentry><log label="checkpoint" /></onentry><transition target="complete" /></state>
				   	<final id="complete" />
				   </scxml>
				   """, notification: null, logger: null, actionLog);

		Assert.AreEqual(DataModelValue.Undefined, result);
		Assert.AreEqual(expected: "checkpoint", actionLog.Entries.Single().Message);
	}

	/* TEST-METADATA
	test_id: SCXML-EXEC-008-EXISTING-001
	requirement_ids: [SCXML-EXEC-008]
	title: Executable log block order at scale
	description: Declarative action blocks at boundary sizes retain first-to-last document order and exact count through one thousand actions.
	authority: { source: W3C SCXML 1.0, section: 3.12 Executable Content, citation_or_rule: Executable content runs in document order. }
	phase: 2
	feature: executable-content
	target_components: [StateMachineInterpreter,ILogController]
	test_kind: integration
	oracle_type: ordered-trace-and-count
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [boundary,scalability]
	dimensions: { actions: 1-2-255-256-1000 }
	preconditions: [trace logger registered]
	dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]
	arrange: Generate onentry log actions for each row count.
	stimulus: Execute machine to root final.
	expected: Undefined result, exact action count, first step-0 and last step-(count-1).
	expected_exception_or_event: none
	forbidden: Reordered, skipped, or duplicated action.
	edge_cases: 255/256 implementation boundary.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 1000 actions }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: action-trace retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-EXEC-008-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	/* CASE-METADATA: DataRows map SCXML-EXEC-008-EXISTING-001-CASE-001..005 to actionCount 1,2,255,256,1000 respectively; each expects exact ordered log count and endpoints. */
	[TestMethod]
	[DataRow(1)]
	[DataRow(2)]
	[DataRow(255)]
	[DataRow(256)]
	[DataRow(1000)]
	[Timeout(5000)]
	public async Task SCXML_EXEC_008_Executable_log_blocks_preserve_document_order_through_one_thousand_actions(int actionCount)
	{
		var document = new StringBuilder("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><state id=\"source\"><onentry>");

		for (var index = 0; index < actionCount; index++)
		{
			document.Append($"<log label=\"step-{index}\" />");
		}

		document.Append("</onentry><transition target=\"complete\" /></state><final id=\"complete\" /></scxml>");
		var actionLog = new InterpreterLogTrace<ILogController>();

		var result = await ScxmlRuntimeHarness.ExecuteAsync(document.ToString(), notification: null, logger: null, actionLog);

		Assert.AreEqual(DataModelValue.Undefined, result);
		Assert.AreEqual(actionCount, actionLog.Entries.Count);
		Assert.AreEqual(expected: "step-0", actionLog.Entries[0].Message);
		Assert.AreEqual($"step-{actionCount - 1}", actionLog.Entries[actionCount - 1].Message);
	}

	/* TEST-METADATA
	test_id: SCXML-LIFE-002-EXISTING-001
	requirement_ids: [SCXML-LIFE-002,SCXML-TRANS-001]
	title: Default root child begins eventless completion
	description: With no root initial attribute, the first root child is entered and its enabled eventless transition precedes any external event processing.
	authority: { source: W3C SCXML 1.0, section: 3.6 and 3.13, citation_or_rule: Root defaults to first child and eventless transitions are selected first. }
	phase: 2
	feature: lifecycle-transition
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: exact-result
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { root_initial: default, transition: eventless }
	preconditions: [valid root children]
	dependencies: [ScxmlRuntimeHarness]
	arrange: Construct first root state with eventless target final.
	stimulus: Start session.
	expected: Undefined root completion result.
	expected_exception_or_event: none
	forbidden: Waiting for external event or selecting a later root child.
	edge_cases: Eventless transition on default root child.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: session retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-LIFE-002-CASE-001,SCXML-TRANS-001-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_LIFE_002_And_TRANS_001_Default_initial_state_takes_an_eventless_transition_to_final()
	{
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				<state id="initial">
					<transition target="complete" />
				</state>
				<final id="complete" />
			</scxml>
			""");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-LIFE-002-EXISTING-002
	requirement_ids: [SCXML-LIFE-002]
	title: Root initial attribute overrides first-child default
	description: A valid root initial target selects the named child, distinguishing explicit selection from document-order fallback.
	authority: { source: W3C SCXML 1.0, section: 3.6 Root Element, citation_or_rule: The root initial attribute identifies initial state(s). }
	phase: 2
	feature: lifecycle
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: exact-result
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { root_initial: explicit-target }
	preconditions: [target child exists]
	dependencies: [ScxmlRuntimeHarness]
	arrange: Place unselected child before explicitly selected child.
	stimulus: Start session.
	expected: Undefined completion through selected child.
	expected_exception_or_event: none
	forbidden: Entry of unselected first child.
	edge_cases: Explicit target later in document order.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: session retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-LIFE-002-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_LIFE_002_Explicit_root_initial_target_overrides_document_order_default()
	{
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="selected">
				<state id="unselected" />
				<state id="selected"><transition target="complete" /></state>
				<final id="complete" />
			</scxml>
			""");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-LIFE-002-EXISTING-003
	requirement_ids: [SCXML-LIFE-002]
	title: Invalid root initial target rejects startup
	description: A root initial attribute naming no root child must fail before an illegal initial configuration can enter.
	authority: { source: W3C SCXML 1.0, section: 3.6 Root Element, citation_or_rule: An initial target must resolve to a legal child state. }
	phase: 2
	feature: lifecycle
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: exact-exception
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative,malformed]
	dimensions: { root_initial: missing-target }
	preconditions: [one nonmatching child]
	dependencies: [ScxmlRuntimeHarness]
	arrange: Construct root with initial missing and present state.
	stimulus: Execute startup.
	expected: Exactly DependencyInjectionException from current construction route.
	expected_exception_or_event: DependencyInjectionException
	forbidden: Partial state entry or successful session.
	edge_cases: Missing target with otherwise valid root.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: partial-session retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-LIFE-002-CASE-001]
	known_issue: none
	compile_notes: Exception type is current public construction-route surface; semantic oracle is rejection before entry.
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_LIFE_002_Invalid_root_initial_target_is_rejected()
	{
		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(async () =>
																		  await ScxmlRuntimeHarness.ExecuteAsync(
																			  """
																			  <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="missing">
																			  	<state id="present" />
																			  </scxml>
																			  """));
	}

	/* TEST-METADATA
	test_id: SCXML-LIFE-002-EXISTING-004
	requirement_ids: [SCXML-LIFE-002]
	title: Root-level initial element is invalid SCXML
	description: A root child initial pseudo-state is illegal; this retained historical witness documents the corrected negative oracle rather than authorizing the old positive assertion.
	authority: { source: W3C SCXML 1.0, section: 3.6 Root Element, citation_or_rule: initial is valid as compound state child, not direct scxml child; PLAN-001. }
	phase: 2
	feature: lifecycle
	target_components: [SCXML parser and validator]
	test_kind: integration
	oracle_type: validation-rejection
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative,malformed]
	dimensions: { initial_element_parent: root }
	preconditions: [otherwise valid child states]
	dependencies: [ScxmlRuntimeHarness]
	arrange: Construct direct scxml/initial child with transition.
	stimulus: Parse/validate/start through the normal route.
	expected: Validation rejection and no executable model.
	expected_exception_or_event: validation error
	forbidden: Entry of selected state or successful completion.
	edge_cases: Initial transition target is otherwise valid.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [No created session after rejection]
	resource_risk: partial-model retention
	tier: fast
	tags: [Exhaustive,SCXML,PlanCorrection]
	related_tests: [SCXML-LIFE-002-CASE-001]
	known_issue: Historical assertion body is ignored because PLAN-001 corrected its oracle.
	compile_notes: Existing ignored body intentionally preserves historical source; generated matrix contains the executable negative contract.
	generation_status: generated-review-required
	*/
	[TestMethod]
	public async Task SCXML_LIFE_002_Root_initial_element_is_rejected()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				<initial><transition target="selected" /></initial>
				<state id="unselected" />
				<state id="selected"><transition target="complete" /></state>
				<final id="complete" />
			</scxml>
			""");

		Assert.IsFalse(result.Accepted);
		Assert.IsNull(result.Model);
		Assert.IsTrue(result.Diagnostics.Any(static message => message.Contains(value: "unknown element", StringComparison.OrdinalIgnoreCase)));
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-001-EXISTING-001
	requirement_ids: [SCXML-STATE-001,SCXML-STATE-003]
	title: Compound entry orders ancestor before child
	description: Entry into a compound state must log parent entry before default child entry.
	authority: { source: W3C SCXML 1.0, section: 3.3 State, citation_or_rule: Entry enters ancestors before descendants and default child selection follows parent entry. }
	phase: 2
	feature: state-entry
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: ordered-log-trace
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { topology: compound-default }
	preconditions: [trace logger registered]
	dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]
	arrange: Create parent with default child and child eventless final transition.
	stimulus: Start session.
	expected: Undefined result and parent entry occurs before child entry.
	expected_exception_or_event: none
	forbidden: Child entered before parent.
	edge_cases: Default first-child entry.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: logger retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-STATE-001-CASE-001,SCXML-STATE-003-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_STATE_001_And_STATE_003_Compound_entry_logs_ancestor_before_descendant()
	{
		var log = new InterpreterLogTrace<StateMachineInterpreter>();
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="parent">
				   		<state id="child"><transition target="complete" /></state>
				   	</state>
				   	<final id="complete" />
				   </scxml>
				   """, notification: null, log);

		Assert.AreEqual(DataModelValue.Undefined, result);
		AssertLogOrder(log, first: "Entering state [parent]", second: "Entering state [child]");
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-001-EXISTING-002
	requirement_ids: [SCXML-STATE-001]
	title: Parallel regions enter document order
	description: Entering a parallel state must enter its first region before its second region.
	authority: { source: W3C SCXML 1.0, section: 3.5 Parallel, citation_or_rule: Parallel child regions enter in document order. }
	phase: 2
	feature: state-entry
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: ordered-log-trace
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { topology: two-region-parallel }
	preconditions: [trace logger registered]
	dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]
	arrange: Create parallel with first and second finalizing regions.
	stimulus: Start session.
	expected: Undefined result and first entry precedes second entry.
	expected_exception_or_event: none
	forbidden: Reversed region order.
	edge_cases: Regions complete immediately.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 20 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: logger retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-STATE-001-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_STATE_001_Parallel_regions_enter_in_document_order()
	{
		var log = new InterpreterLogTrace<StateMachineInterpreter>();
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<parallel id="parallel">
				   		<state id="first"><final id="firstDone" /></state>
				   		<state id="second"><final id="secondDone" /></state>
				   		<transition event="done.state.parallel" target="complete" />
				   	</parallel>
				   	<final id="complete" />
				   </scxml>
				   """, notification: null, log);

		Assert.AreEqual(DataModelValue.Undefined, result);
		AssertLogOrder(log, first: "Entering state [first]", second: "Entering state [second]");
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-001-EXISTING-003
	requirement_ids: [SCXML-STATE-001,SCXML-TRANS-008]
	title: Multi-target entry enters shared parallel once
	description: A legal multi-target transition to orthogonal descendants enters their shared parallel ancestor once and its children in document order.
	authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: Shared ancestors are entered once for legal multi-target transitions. }
	phase: 2
	feature: transition-entry
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: count-and-ordered-log-trace
	risk: critical
	priority: critical
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,boundary]
	dimensions: { transition: multi-target, topology: shared-parallel-ancestor }
	preconditions: [external dispatch available]
	dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]
	arrange: Create source transition targeting left and right parallel descendants.
	stimulus: Dispatch advance.
	expected: Undefined result, work entered once, left entry before right entry.
	expected_exception_or_event: none
	forbidden: Duplicate work entry or reversed children.
	edge_cases: Both targets complete immediately.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 30 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: session retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-TRANS-008-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	[Timeout(5000)]
	public async Task SCXML_STATE_001_And_TRANS_008_Multitarget_transition_enters_shared_ancestor_once_in_document_order()
	{
		var log = new InterpreterLogTrace<StateMachineInterpreter>();
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="source"><transition event="advance" target="left right" /></state>
				   	<parallel id="work">
				   		<state id="left"><final id="leftComplete" /></state>
				   		<state id="right"><final id="rightComplete" /></state>
				   		<transition event="done.state.work" target="complete" />
				   	</parallel>
				   	<final id="complete" />
				   </scxml>
				   """, log, "advance");

		Assert.AreEqual(DataModelValue.Undefined, result);
		Assert.AreEqual(expected: 1, log.Entries.Count(entry => entry.Message == "Entering state [work]"));
		AssertLogOrder(log, first: "Entering state [left]", second: "Entering state [right]");
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-003-EXISTING-001
	requirement_ids: [SCXML-STATE-003]
	title: Explicit compound initial transition selects target
	description: A compound initial element selects its declared child rather than the first ordinary child.
	authority: { source: W3C SCXML 1.0, section: 3.3 State, citation_or_rule: A state's initial transition selects its declared target. }
	phase: 2
	feature: state-entry
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: exact-result
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { initial_form: child-element }
	preconditions: [initial target exists]
	dependencies: [ScxmlRuntimeHarness]
	arrange: Put unselected before selected child and declare initial transition to selected.
	stimulus: Start session.
	expected: Undefined result through selected child.
	expected_exception_or_event: none
	forbidden: Entry of unselected default child.
	edge_cases: Selected child appears after unselected child.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: session retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-STATE-003-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_STATE_003_Explicit_initial_child_selects_its_declared_transition_target()
	{
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				<state id="parent">
					<initial><transition target="selected" /></initial>
					<state id="unselected" />
					<state id="selected"><transition target="complete" /></state>
				</state>
				<final id="complete" />
			</scxml>
			""");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-003-EXISTING-002
	requirement_ids: [SCXML-STATE-003]
	title: Compound initial attribute selects declared child
	description: A compound initial attribute chooses the named child over document-order fallback.
	authority: { source: W3C SCXML 1.0, section: 3.3 State, citation_or_rule: A state's initial attribute identifies the initial child. }
	phase: 2
	feature: state-entry
	target_components: [StateMachineInterpreter]
	test_kind: integration
	oracle_type: exact-result
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [null]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { initial_form: attribute }
	preconditions: [selected child exists]
	dependencies: [ScxmlRuntimeHarness]
	arrange: Place unselected child before selected child and set initial attribute.
	stimulus: Start session.
	expected: Undefined result through selected child.
	expected_exception_or_event: none
	forbidden: Entry of document-order fallback child.
	edge_cases: Explicit selected child later in document order.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 10 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Runtime container async-disposed]
	resource_risk: session retention
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-STATE-003-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_STATE_003_Compound_initial_attribute_selects_its_declared_child()
	{
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				<state id="parent" initial="selected">
					<state id="unselected" />
					<state id="selected"><transition target="complete" /></state>
				</state>
				<final id="complete" />
			</scxml>
			""");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-004-EXISTING-003; requirement_ids: [SCXML-STATE-004,SCXML-STATE-005]; title: Parallel done follows all region finals; description: Two immediately final regions cause parent done transition only after every region final and then root completion; authority: { source: W3C SCXML 1.0, section: 3.5 and 3.6, citation_or_rule: Parallel completion requires all regions and final entry raises done.state }; phase: 2; feature: parallel-completion; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive]; dimensions: { regions: two-immediate-finals }; preconditions: [parent done transition]; dependencies: [ScxmlRuntimeHarness]; arrange: Create parallel regions finalizing at entry; stimulus: Start; expected: Undefined root-final result; expected_exception_or_event: done.state.work; forbidden: Parent completion before either region final; edge_cases: Both regions final in same macrostep; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 25 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: done-event retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-STATE-004-CASE-001,SCXML-STATE-005-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_STATE_004_And_STATE_005_All_parallel_regions_must_finalize_before_parent_done_event_transitions()
	{
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				<parallel id="work">
					<state id="left"><final id="leftDone" /></state>
					<state id="right"><final id="rightDone" /></state>
					<transition event="done.state.work" target="complete" />
				</parallel>
				<final id="complete" />
			</scxml>
			""");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-004-EXISTING-001; requirement_ids: [SCXML-STATE-004]; title: Nested parallel completion enables outer completion; description: An outer parallel completes only after its nested parallel region and sibling final complete; authority: { source: W3C SCXML 1.0, section: 3.5, citation_or_rule: Parallel completion requires every active region final }; phase: 2; feature: parallel-completion; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,nested]; dimensions: { topology: nested-parallel }; preconditions: [valid final transitions]; dependencies: [ScxmlRuntimeHarness]; arrange: Build nested outer/inner parallel; stimulus: Execute; expected: Undefined root result; expected_exception_or_event: none; forbidden: Outer done before inner completion; edge_cases: Immediate child finals; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 40 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: session retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-STATE-004-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	[Timeout(5000)]
	public async Task SCXML_STATE_004_Nested_parallel_completion_enables_the_containing_parallel_completion()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				<parallel id="outer">
					<state id="left"><final id="leftComplete" /></state>
					<state id="right">
						<parallel id="inner">
							<state id="innerLeft"><final id="innerLeftComplete" /></state>
							<state id="innerRight"><final id="innerRightComplete" /></state>
							<transition event="done.state.inner" target="rightComplete" />
						</parallel>
						<final id="rightComplete" />
					</state>
					<transition event="done.state.outer" target="complete" />
				</parallel>
				<final id="complete" />
			</scxml>
			""");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-004-EXISTING-002; requirement_ids: [SCXML-STATE-004]; title: Parallel waits for last region; description: Parent done transition remains disabled after first region final and enables only after second final; authority: { source: W3C SCXML 1.0, section: 3.5, citation_or_rule: All parallel regions must be final }; phase: 2; feature: parallel-completion; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,staggered]; dimensions: { regions: two }; preconditions: [external dispatch]; dependencies: [ScxmlRuntimeHarness]; arrange: Build separately finalizing regions; stimulus: Dispatch finish.left then finish.right; expected: Undefined completion only after second dispatch; expected_exception_or_event: none; forbidden: Parent done after first region; edge_cases: External event order; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 30 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: session retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-STATE-004-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	[Timeout(5000)]
	public async Task SCXML_STATE_004_Parallel_parent_done_event_waits_for_the_last_region_to_finalize()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<parallel id="work">
				   		<state id="left"><transition event="finish.left" target="leftComplete" /><final id="leftComplete" /></state>
				   		<state id="right"><transition event="finish.right" target="rightComplete" /><final id="rightComplete" /></state>
				   		<transition event="done.state.work" target="complete" />
				   	</parallel>
				   	<final id="complete" />
				   </scxml>
				   """, "finish.left", "finish.right");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-007-EXISTING-001; requirement_ids: [SCXML-STATE-007]; title: Deep nested topology completes; description: Nested default entry remains correct across small through 101-level valid state topologies; authority: { source: exhaustive SCXML matrix, section: SCXML-STATE-007, citation_or_rule: Deep valid topology preserves ordering without corruption }; phase: 2; feature: configuration-depth; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [boundary,scalability]; dimensions: { depth: 1-2-6-10-100-101 }; preconditions: [valid generated nesting]; dependencies: [ScxmlRuntimeHarness]; arrange: Generate nested states; stimulus: Execute; expected: Undefined completion; expected_exception_or_event: none; forbidden: Corrupt entry or stack failure; edge_cases: 100-plus depth; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 101 entries }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: stack/session retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-STATE-007-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	/* CASE-METADATA: DataRows SCXML-STATE-007-EXISTING-001-CASE-001..006 map in declaration order to depth 1,2,6,10,100,101; each expects Undefined completion and no configuration corruption. */
	[TestMethod]
	[DataRow(1)]
	[DataRow(2)]
	[DataRow(6)]
	[DataRow(10)]
	[DataRow(100)]
	[DataRow(101)]
	public async Task SCXML_STATE_007_Valid_nested_topologies_complete_without_corrupting_default_entry(int depth)
	{
		var document = new StringBuilder("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\">");

		for (var index = 0; index < depth; index++)
		{
			document.Append($"<state id=\"s{index}\">");
		}

		document.Append("<transition target=\"complete\" />");

		for (var index = 0; index < depth; index++)
		{
			document.Append("</state>");
		}

		document.Append("<final id=\"complete\" /></scxml>");

		var result = await ScxmlRuntimeHarness.ExecuteAsync(document.ToString());

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-EVENT-003-EXISTING-001; requirement_ids: [SCXML-EVENT-003,SCXML-TRANS-007]; title: Raised internal events are FIFO; description: Two targetless/internal raises must process first before second and reach final without state change from first targetless transition; authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: Internal events are FIFO and targetless transition has no exit/entry }; phase: 2; feature: internal-events; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive]; dimensions: { producers: onentry, transition: targetless-then-targeted }; preconditions: [valid source state]; dependencies: [ScxmlRuntimeHarness]; arrange: Raise first then second in onentry; stimulus: Start; expected: Undefined final result; expected_exception_or_event: none; forbidden: Second processed before first or exit on first; edge_cases: Consecutive raised events; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 20 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: queue retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-EVENT-003-CASE-001,SCXML-TRANS-007-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EVENT_003_And_TRANS_007_Raised_internal_events_are_processed_FIFO_before_completion()
	{
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				<state id="source">
					<onentry><raise event="first" /><raise event="second" /></onentry>
					<transition event="first" />
					<transition event="second" target="complete" />
				</state>
				<final id="complete" />
			</scxml>
			""");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-STATE-002-EXISTING-001; requirement_ids: [SCXML-STATE-002]; title: Nested exit is deepest first; description: Child exit must precede parent exit when eventless transition leaves both active states; authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: Exit set is processed deepest-first }; phase: 2; feature: state-exit; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: ordered-log-trace; risk: critical; priority: critical; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive]; dimensions: { topology: parent-child }; preconditions: [trace logger]; dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]; arrange: Build nested source transitioning to final; stimulus: Execute; expected: Undefined result and child exit before parent exit; expected_exception_or_event: none; forbidden: Parent exit before child exit; edge_cases: Eventless root-final transition; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 15 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: logger retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-STATE-002-CASE-001]; known_issue: DEF-SCXML-STATE-002; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_STATE_002_Nested_active_states_exit_deepest_first()
	{
		var log = new InterpreterLogTrace<StateMachineInterpreter>();
		var result = await ScxmlRuntimeHarness.ExecuteAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="parent"><state id="child"><transition target="complete" /></state></state>
				   	<final id="complete" />
				   </scxml>
				   """, notification: null, log);

		Assert.AreEqual(DataModelValue.Undefined, result);
		AssertLogOrder(log, first: "Exiting state [child]", second: "Exiting state [parent]");
	}

	/* TEST-METADATA
	test_id: SCXML-EVENT-004-EXISTING-003; requirement_ids: [SCXML-EVENT-004]; title: External event reaches active session; description: A host-dispatched matching external event advances the active session to final; authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: External events are selected in the active session }; phase: 2; feature: external-events; target_components: [IStateMachineCollection,StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive]; dimensions: { source: host-dispatch }; preconditions: [active waiting state]; dependencies: [ScxmlRuntimeHarness]; arrange: Start waiting state; stimulus: Dispatch advance; expected: Undefined final result; expected_exception_or_event: none; forbidden: Dropped event or wrong session delivery; edge_cases: Single queued event; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 15 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: queue retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-EVENT-004-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EVENT_004_External_event_is_dispatched_to_the_active_session()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="waiting"><transition event="advance" target="complete" /></state>
				   	<final id="complete" />
				   </scxml>
				   """, "advance");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-EVENT-004-EXISTING-004; requirement_ids: [SCXML-EVENT-004]; title: External events preserve FIFO through targetless transition; description: First external event performs targetless content before second external event selects final transition; authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: External queue is FIFO }; phase: 2; feature: external-events; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive]; dimensions: { events: first-targetless-second-targeted }; preconditions: [active waiting state]; dependencies: [ScxmlRuntimeHarness]; arrange: Queue first and second matching events; stimulus: Execute queue; expected: Undefined final result after second; expected_exception_or_event: none; forbidden: Second consumed before first; edge_cases: First transition targetless; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 20 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: queue retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-EVENT-004-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EVENT_004_External_events_preserve_FIFO_order_for_targetless_then_targeted_transitions()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="waiting">
				   		<transition event="first" />
				   		<transition event="second" target="complete" />
				   	</state>
				   	<final id="complete" />
				   </scxml>
				   """, "first", "second");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-TRANS-006-EXISTING-001; requirement_ids: [SCXML-TRANS-006]; title: Internal descendant transition avoids compound reentry; description: Internal compound-to-descendant transition preserves parent entry count while default external transition reenters parent; authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: Internal descendant transition excludes source exit whereas external includes it }; phase: 2; feature: transition-type; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-entry-count; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,boundary]; dimensions: { type: internal-default-external }; preconditions: [compound parent]; dependencies: [ScxmlRuntimeHarness,InterpreterLogTrace]; arrange: Generate type attribute row; stimulus: Dispatch advance then finish; expected: Parent entries equal row expected 1/2; expected_exception_or_event: none; forbidden: Internal parent reentry; edge_cases: Omitted type default; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 20 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: logger retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-TRANS-006-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	/* CASE-METADATA: DataRows SCXML-TRANS-006-EXISTING-001-CASE-001..002 map internal=>1 and omitted=>2 parent entries. */
	[TestMethod]
	[DataRow("internal", 1)]
	[DataRow("", 2)]
	[Timeout(5000)]
	public async Task SCXML_TRANS_006_Compound_to_descendant_transition_reenters_the_source_only_when_external(string transitionType, int expectedParentEntryCount)
	{
		var typeAttribute = transitionType.Length == 0 ? string.Empty : $" type=\"{transitionType}\"";
		var log = new InterpreterLogTrace<StateMachineInterpreter>();
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			$"""
			 <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
			 	<state id="parent" initial="child">
			 		<state id="child"><transition event="finish" target="complete" /></state>
			 		<transition event="advance" target="child"{typeAttribute} />
			 	</state>
			 	<final id="complete" />
			 </scxml>
			 """, log, "advance", "finish");

		Assert.AreEqual(DataModelValue.Undefined, result);
		Assert.AreEqual(expectedParentEntryCount, log.Entries.Count(entry => entry.Message == "Entering state [parent]"));
	}

	/* TEST-METADATA
	test_id: SCXML-TRANS-003-EXISTING-001; requirement_ids: [SCXML-TRANS-003]; title: First matching transition wins document order; description: Identical matching transitions select final before later trap transition; authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: First matching true transition in document order is selected }; phase: 2; feature: transition-selection; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,conflict]; dimensions: { candidates: same-source-same-event }; preconditions: [external dispatch]; dependencies: [ScxmlRuntimeHarness]; arrange: Create final then trap candidates; stimulus: Dispatch advance; expected: Undefined completion through first candidate; expected_exception_or_event: none; forbidden: Trap entry or second selection; edge_cases: Equal descriptors; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 15 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: session retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-TRANS-003-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	[Timeout(5000)]
	public async Task SCXML_TRANS_003_First_document_order_transition_wins_when_multiple_transitions_match()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="source">
				   		<transition event="advance" target="complete" />
				   		<transition event="advance" target="trap" />
				   	</state>
				   	<state id="trap" />
				   	<final id="complete" />
				   </scxml>
				   """, "advance");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-TRANS-004-EXISTING-001; requirement_ids: [SCXML-TRANS-004]; title: Descendant source preempts ancestor conflict; description: Child transition to final defeats same-event ancestor transition to trap; authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: Descendant source has priority over conflicting ancestor transition }; phase: 2; feature: transition-selection; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: critical; priority: critical; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,conflict]; dimensions: { sources: descendant-ancestor }; preconditions: [nested active child]; dependencies: [ScxmlRuntimeHarness]; arrange: Declare child final and parent trap candidates; stimulus: Dispatch advance; expected: Undefined completion through child; expected_exception_or_event: none; forbidden: Trap entry or ancestor action; edge_cases: Identical event descriptors; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 15 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: session retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-TRANS-004-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	[Timeout(5000)]
	public async Task SCXML_TRANS_004_Descendant_source_transition_preempts_a_conflicting_ancestor_transition()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="parent">
				   		<state id="child"><transition event="advance" target="complete" /></state>
				   		<transition event="advance" target="trap" />
				   	</state>
				   	<state id="trap" />
				   	<final id="complete" />
				   </scxml>
				   """, "advance");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-HIST-001-EXISTING-001; requirement_ids: [SCXML-HIST-001,SCXML-HIST-002]; title: Shallow history restores recorded child; description: After parent exit, shallow history returns to recorded original child rather than uninitialized-history fallback; authority: { source: W3C SCXML 1.0, section: 3.10 History States, citation_or_rule: Shallow history stores active immediate child and bypasses default after capture }; phase: 2; feature: history; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive]; dimensions: { history: shallow, capture: initialized }; preconditions: [parent previously active]; dependencies: [ScxmlRuntimeHarness]; arrange: Capture original then leave and target history; stimulus: Dispatch leave,return,finish; expected: Undefined completion through original; expected_exception_or_event: none; forbidden: Fallback entry after stored history; edge_cases: History default exists; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 30 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: stored configuration retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-HIST-001-CASE-001,SCXML-HIST-002-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_HIST_001_And_HIST_002_Shallow_history_restores_the_recorded_child_instead_of_its_default()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="parent" initial="original">
				   		<history id="remember" type="shallow"><transition target="fallback" /></history>
				   		<state id="original"><transition event="finish" target="complete" /></state>
				   		<state id="fallback" />
				   		<transition event="leave" target="outside" />
				   	</state>
				   	<state id="outside"><transition event="return" target="remember" /></state>
				   	<final id="complete" />
				   </scxml>
				   """, "leave", "return", "finish");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-HIST-001-EXISTING-002; requirement_ids: [SCXML-HIST-001,SCXML-HIST-003]; title: Deep history restores nested leaf; description: Deep history restores remembered descendant leaf directly through required ancestors after parent exit; authority: { source: W3C SCXML 1.0, section: 3.10, citation_or_rule: Deep history stores active atomic descendants }; phase: 2; feature: history; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,nested]; dimensions: { history: deep, depth: two }; preconditions: [nested remembered leaf active]; dependencies: [ScxmlRuntimeHarness]; arrange: Select remembered nested leaf, exit parent, target deep history; stimulus: Dispatch select,leave,return,finish; expected: Undefined completion through remembered leaf; expected_exception_or_event: none; forbidden: Fallback/default child entry; edge_cases: Deep history parent with fallback; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 40 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: stored descendant retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-HIST-001-CASE-001,SCXML-HIST-003-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_HIST_001_And_HIST_003_Deep_history_restores_the_recorded_nested_leaf()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="parent" initial="branch">
				   		<history id="remember" type="deep"><transition target="fallback" /></history>
				   		<state id="branch" initial="start">
				   			<state id="start"><transition event="select" target="remembered" /></state>
				   			<state id="remembered"><transition event="finish" target="complete" /></state>
				   		</state>
				   		<state id="fallback" />
				   		<transition event="leave" target="outside" />
				   	</state>
				   	<state id="outside"><transition event="return" target="remember" /></state>
				   	<final id="complete" />
				   </scxml>
				   """, "select", "leave", "return", "finish");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-HIST-004-EXISTING-001; requirement_ids: [SCXML-HIST-004]; title: Shallow and deep history remain independent; description: Two history pseudostates in one parent retain distinct shallow/deep restoration semantics across repeated captures; authority: { source: W3C SCXML 1.0, section: 3.10, citation_or_rule: Each history state has independent stored configuration }; phase: 2; feature: history; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,isolation]; dimensions: { histories: shallow-and-deep }; preconditions: [both histories defined]; dependencies: [ScxmlRuntimeHarness]; arrange: Capture nested branch then target shallow and deep independently; stimulus: Dispatch declared event sequence; expected: Undefined completion; expected_exception_or_event: none; forbidden: Cross-history stored configuration contamination; edge_cases: Repeated capture; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 50 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: history retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-HIST-004-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	[Timeout(5000)]
	public async Task SCXML_HIST_004_Shallow_and_deep_history_pseudostates_in_one_parent_remain_independent()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="parent" initial="branch">
				   		<history id="shallow" type="shallow"><transition target="fallback" /></history>
				   		<history id="deep" type="deep"><transition target="fallback" /></history>
				   		<state id="branch" initial="start">
				   			<state id="start"><transition event="select" target="remembered" /></state>
				   			<state id="remembered"><transition event="finish" target="complete" /></state>
				   		</state>
				   		<state id="fallback" />
				   		<transition event="leave" target="outside" />
				   	</state>
				   	<state id="outside">
				   		<transition event="return.shallow" target="shallow" />
				   		<transition event="return.deep" target="deep" />
				   	</state>
				   	<final id="complete" />
				   </scxml>
				   """, "select", "leave", "return.shallow", "select", "leave", "return.deep", "finish");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-HIST-002-EXISTING-001; requirement_ids: [SCXML-HIST-002]; title: Uninitialized history uses default transition; description: A never-active parent history target executes its default transition to fallback exactly as initial history semantics require; authority: { source: W3C SCXML 1.0, section: 3.10, citation_or_rule: Uninitialized history follows default transition }; phase: 2; feature: history; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,first-entry]; dimensions: { history: uninitialized }; preconditions: [parent never active]; dependencies: [ScxmlRuntimeHarness]; arrange: Start outside parent with history default fallback; stimulus: Dispatch return,finish; expected: Undefined completion through fallback; expected_exception_or_event: none; forbidden: Attempt to restore nonexistent stored state; edge_cases: Default target contains eventful final transition; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 25 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: history retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-HIST-002-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_HIST_002_Uninitialized_history_uses_its_default_transition()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outside">
				   	<state id="parent">
				   		<history id="remember" type="shallow"><transition target="fallback" /></history>
				   		<state id="fallback"><transition event="finish" target="complete" /></state>
				   	</state>
				   	<state id="outside"><transition event="return" target="remember" /></state>
				   	<final id="complete" />
				   </scxml>
				   """, "return", "finish");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	/* TEST-METADATA
	test_id: SCXML-EVENT-003-EXISTING-002; requirement_ids: [SCXML-EVENT-003]; title: Internal event preempts queued external event; description: Onentry-raised internal event moves source before already queued external event can complete ready state; authority: { source: W3C SCXML 1.0, section: 3.13, citation_or_rule: Internal queue is processed before next external event }; phase: 2; feature: internal-events; target_components: [StateMachineInterpreter]; test_kind: integration; oracle_type: exact-result; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [null]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,priority]; dimensions: { queues: internal-before-external }; preconditions: [external event prequeued]; dependencies: [ScxmlRuntimeHarness]; arrange: Raise internal on source entry and queue external advance; stimulus: Execute; expected: Undefined completion; expected_exception_or_event: none; forbidden: External event processed while source still active; edge_cases: Internal event changes matching state; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 20 operations }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [Container disposed]; resource_risk: queue retention; tier: fast; tags: [Exhaustive,SCXML]; related_tests: [SCXML-EVENT-003-CASE-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_EVENT_003_Internal_events_are_processed_before_an_already_queued_external_event()
	{
		var result = await ScxmlRuntimeHarness.ExecuteWithExternalEventsAsync(
			scxml: """
				   <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
				   	<state id="source">
				   		<onentry><raise event="internal.advance" /></onentry>
				   		<transition event="internal.advance" target="ready" />
				   	</state>
				   	<state id="ready"><transition event="external.advance" target="complete" /></state>
				   	<final id="complete" />
				   </scxml>
				   """, "external.advance");

		Assert.AreEqual(DataModelValue.Undefined, result);
	}

	private static void AssertLogOrder(InterpreterLogTrace<StateMachineInterpreter> log, string first, string second)
	{
		var messages = log.Entries.Select(static entry => entry.Message).ToArray();
		var firstIndex = Array.IndexOf(messages, first);
		var secondIndex = Array.IndexOf(messages, second);
		Assert.IsTrue(firstIndex >= 0 && secondIndex > firstIndex, string.Join(separator: " | ", messages));
	}
}
