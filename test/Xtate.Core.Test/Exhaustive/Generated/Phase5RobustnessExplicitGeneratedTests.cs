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

namespace Xtate.Core.Test.Exhaustive.Generated;

[TestClass]
public sealed class Phase5RobustnessExplicitGeneratedTests
{
	private static readonly RobustnessCase[] ExplicitCases =
	[
		new(
			CaseId: "ROBUST-XML-001-CASE-001", RequirementIds: "ROBUST-XML-001", Description: "External general entity is rejected without resolver file or network I/O.",
			Fixture: "SCXML DOCTYPE declares file URI entity and references it in data; resolver probe starts at zero calls.", Stimulus: "Parse via one-byte stream with budget 4096 bytes/100 reads.",
			Seed: 4101, OperationBudget: 100, Expected: "Controlled XML/SCXML diagnostic; resolver call count is zero.",
			Forbidden: "File/network resolver call, accepted external entity text, process crash, or more than 100 reads.", ResourceBudget: "zero live readers/streams; 4096-byte allocation ceiling."),
		new(
			CaseId: "ROBUST-XPATH-003-CASE-001", RequirementIds: "ROBUST-XPATH-003|XPATH-ASSIGN-010",
			Description: "Last-target XPath mutation failure rolls back a large overlapping selection within its operation budget.",
			Fixture: "100 writable selected nodes followed by one read-only overlapping target; deep-copy value has 64 descendants.",
			Stimulus: "Execute replacechildren with injected final-target access failure.", Seed: 4102, OperationBudget: 5000, Expected: "error.execution and canonical post-tree equals pre-tree.",
			Forbidden: "Any early target mutation, skipped final access check, retained transaction buffer, or >5000 model operations.",
			ResourceBudget: "zero live iterators/snapshots; bounded copy buffer."),
		new(
			CaseId: "ROBUST-EVENT-001-CASE-001", RequirementIds: "ROBUST-EVENT-001", Description: "A 256-event external flood preserves FIFO until documented queue backpressure rejects later events.",
			Fixture: "Waiting machine records event sequence; virtual host queue capacity is 256; payload is 1 KiB immutable value.",
			Stimulus: "Dispatch e000 through e256 in order then drain deterministically.", Seed: 4103, OperationBudget: 1024,
			Expected: "Accepted events are e000..e255 in order; e256 gets documented capacity rejection.",
			Forbidden: "Reordered/duplicated payload, silent loss of an accepted event, unbounded queue growth, or retained payload.",
			ResourceBudget: "zero queued payloads and waiters after destruction."),
		new(
			CaseId: "FUZZ-SCXML-001-CASE-001", RequirementIds: "FUZZ-SCXML-001", Description: "Fixed grammar seed produces either a valid normalized model or one bounded diagnostic without a crash.",
			Fixture: "Seed 0x5CXML001 generates namespace shadowing, reordered children, malformed event token, and one-byte UTF-8 chunks.",
			Stimulus: "Generate, parse, validate, and shrink the first invalid production under 2000 steps.", Seed: 1547956225, OperationBudget: 2000,
			Expected: "Exactly one of valid canonical model or classified parse/validation diagnostic; shrink result is no larger than seed input.",
			Forbidden: "Unhandled exception, hang, stack overflow, nondeterministic classification, or leak of reader/generator state.",
			ResourceBudget: "zero generator buffers/readers; 2 MiB total allocation budget."),
		new(
			CaseId: "PROP-SELECT-001-CASE-001", RequirementIds: "PROP-SELECT-001",
			Description: "A fixed generated nested conflict graph selects the maximal non-conflicting descendant-preempting transition set.",
			Fixture: "Seed 0x51EC7001 makes active parent/child siblings with ancestor and descendant same-event transitions.",
			Stimulus: "Deliver event go and compare selected trace/configuration to independent SCXML selection model.", Seed: 1374449665, OperationBudget: 500,
			Expected: "Selected set equals reference set, is pairwise non-conflicting, and excludes the conflicting ancestor.",
			Forbidden: "Missing enabled descendant, selected conflict pair, ancestor preemption violation, or model/session divergence.",
			ResourceBudget: "no live generated graph or interpreter session."),
		new(
			CaseId: "PROP-PERSIST-001-CASE-001", RequirementIds: "PROP-PERSIST-001",
			Description: "Suspension before delayed-event dequeue recovers to the same configuration and exactly-once event trace as uninterrupted execution.",
			Fixture: "Seed 0xPERS157 creates one delayed event and a pure transition; suspension gate is immediately before dequeue.",
			Stimulus: "Run uninterrupted reference, suspend gated run, kill host, resume durable bytes, then advance virtual clock.", Seed: 188641564, OperationBudget: 1000,
			Expected: "Recovered final configuration/data/trace equal reference and delayed event appears exactly once.",
			Forbidden: "Lost or duplicate delayed event, divergent pure state, stale host membership, or durable/live disagreement.",
			ResourceBudget: "zero old/new host sessions, timers, storage handles, and tasks."),
		new(
			CaseId: "FAULT-STORE-001-CASE-001", RequirementIds: "FAULT-STORE-001", Description: "Checkpoint write fault preserves the prior committed journal and releases the storage lock.",
			Fixture: "Committed journal has event old; faulting store throws during replacement checkpoint flush.",
			Stimulus: "Attempt replacement checkpoint, reopen store, then acquire lock for a clean retry.", Seed: 4104, OperationBudget: 300,
			Expected: "Reopen contains only old event; retry lock acquisition succeeds; operation reports controlled storage failure.",
			Forbidden: "Replacement visible as committed, deadlocked lock, corrupt journal dispatch, or retained stream.", ResourceBudget: "zero transactions/semaphores/streams after close."),
		new(
			CaseId: "LEAK-XPATH-001|RACE-SCHED-001-CASE-001", RequirementIds: "LEAK-XPATH-001|RACE-SCHED-001",
			Description: "Concurrent compiled XPath disposal and same-ID schedule/cancel/fire leave no context, timer, or duplicate dispatch.",
			Fixture: "Two sessions compile same expression; scheduler holds two events ID A; gates pause fire and dispose/cancel concurrently.",
			Stimulus: "Release cancel, fire, expression dispose, then session destroy according to recorded schedule 1-2-3-4.", Seed: 4105, OperationBudget: 400,
			Expected: "At most one permitted dispatch per event; no disposed context use; all weak sentinels dead after bounded collection.",
			Forbidden: "Duplicate fire, use-after-dispose, retained XPath navigator/context, timer CTS, or deadlocked gate.",
			ResourceBudget: "resource ledger empty; weak sentinels collected within 8 bounded GC passes.")
	];

	/*
	TEST-METADATA
	test_id: PHASE5-EXPLICIT-ROBUSTNESS-001
	requirement_ids: [ROBUST-XML-001,ROBUST-XPATH-003,ROBUST-EVENT-001,FUZZ-SCXML-001,PROP-SELECT-001,PROP-PERSIST-001,FAULT-STORE-001,LEAK-XPATH-001,RACE-SCHED-001]
	title: Robustness and reliability campaigns use literal seeds schedules and budgets
	description: These cases make malicious input, generated-model, fault, retention, and race acceptance criteria explicit; a run cannot substitute an unbounded campaign or an unspecified plan result.
	authority: { source: exhaustive plan document 04, section: adversarial inputs through concurrency linearizability, citation_or_rule: each record states bounded input, exact accepted outcome, and forbidden resource or semantic failure }
	phase: 5
	feature: robustness-reliability
	target_components: [ScxmlParser,XPathAssignmentAction,StateMachineInterpreter,PersistenceStore,EventScheduler]
	test_kind: property-fault-leak-and-linearizability
	oracle_type: independent-model-trace-snapshot-and-resource-ledger
	risk: critical
	priority: critical
	construction_routes: [scxml-text,xml-stream,persisted-bytes,public-host-api]
	data_models: [null,xpath,runtime]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [adversarial-input,bounded-resource,property,recovery,fault-injection,leak,race]
	dimensions: { records: eight-fixed-seed-scenarios }
	preconditions: [deterministic clock, fixed seed generator, process-local resource ledger]
	dependencies: [RobustnessScenarioHarness,IndependentScxmlModel,ResourceLedger,DeterministicRaceGate]
	arrange: Create the literal record fixture and snapshot counters, model state, and resource ledger.
	stimulus: Run its fixed seed, bounded schedule, or injected fault exactly once.
	expected: [record-specific exact result, diagnostic, model equality, and ledger outcome]
	expected_exception_or_event: record-specific controlled rejection or none
	forbidden: [stack overflow, unauthorized I/O, silent partial mutation, unbounded work, leak, duplicate effect]
	edge_cases: [partial bytes, last-target fault, overdue event, cancellation at linearization]
	determinism: { clock: virtual, scheduling: explicit-seed-and-race-gates, timeout_or_step_bound: 'record-specific finite budget' }
	isolation: { parallel_safe: false, shared_state: process-resource-ledger }
	cleanup: [assert zero tracked resources and no surviving ambient context after each case]
	resource_risk: critical
	tier: nightly
	tags: [Exhaustive,Robustness,Property,FaultInjection,Leak,Race]
	related_tests: [DM-PROP-003,PROP-CONFIG-001]
	known_issue: none
	compile_notes: RobustnessScenarioHarness, IndependentScxmlModel, and deterministic resource probes are planned test-side helpers.
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	[DynamicData(nameof(Cases))]
	public async Task Robustness_case_satisfies_its_bounded_oracle(RobustnessCase testCase)
	{
		// Arrange
		await using var scenario = await RobustnessScenarioHarness.CreateAsync(testCase);
		var before = await scenario.CaptureSnapshotAsync();

		// Act
		var outcome = await scenario.ExecuteAsync(testCase.Stimulus);

		// Assert
		await scenario.AssertExactOutcomeAsync(testCase.Expected, outcome);
		await scenario.AssertForbiddenEffectsAbsentAsync(testCase.Forbidden, before);
		await scenario.AssertResourceBudgetAsync(testCase.ResourceBudget);
		await scenario.AssertCleanupAsync();
	}

	public static IEnumerable<object[]> Cases() => ExplicitCases.Select(testCase => new object[] { testCase });

	public sealed record RobustnessCase(
		string CaseId,
		string RequirementIds,
		string Description,
		string Fixture,
		string Stimulus,
		int Seed,
		int OperationBudget,
		string Expected,
		string Forbidden,
		string ResourceBudget);
}
