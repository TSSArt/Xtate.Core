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
public sealed class Phase5HighRiskPartitionGeneratedTests
{
	/*
	TEST-METADATA
	test_id: PHASE5-HIGH-RISK-PARTITIONS-001
	requirement_ids:
	  - ROBUST-XML-001..005
	  - ROBUST-XINC-001
	  - ROBUST-XPATH-001..003
	  - ROBUST-EVENT-001
	  - ROBUST-MODEL-001
	  - ROBUST-PERSIST-001
	  - ROBUST-IO-001
	  - FUZZ-SCXML-001
	  - FUZZ-MODEL-001
	  - FUZZ-XPATH-001
	  - FUZZ-XMLDATA-001
	  - FUZZ-EVENT-001
	  - FUZZ-PERSIST-001
	  - FUZZ-HTTP-001
	  - FUZZ-PIPE-001
	  - PROP-CONFIG-001
	  - PROP-ORDER-001
	  - PROP-EVENT-001
	  - PROP-SELECT-001
	  - PROP-HISTORY-001
	  - PROP-SER-001
	  - PROP-DATA-001
	  - PROP-SCHED-001
	  - PROP-PERSIST-001
	  - PROP-ISOLATE-001
	  - FAULT-EVAL-001
	  - FAULT-QUEUE-001
	  - FAULT-ROUTE-001
	  - FAULT-INVOKE-001
	  - FAULT-RES-001
	  - FAULT-XML-001
	  - FAULT-STORE-001
	  - FAULT-LOG-001
	  - FAULT-HOST-001
	  - FAULT-IO-001
	  - LEAK-SESSION-001..002
	  - LEAK-DM-001
	  - LEAK-XPATH-001
	  - LEAK-RUNTIME-001
	  - LEAK-SCHED-001
	  - LEAK-INVOKE-001
	  - LEAK-RES-001
	  - LEAK-PERSIST-001
	  - LEAK-HTTP-001
	  - LEAK-PIPE-001
	  - LEAK-SEC-001
	  - LEAK-LOG-001
	  - BUDGET-PARSE-001
	  - BUDGET-SELECT-001
	  - BUDGET-XPATH-001
	  - BUDGET-QUEUE-001
	  - BUDGET-PAYLOAD-001
	  - BUDGET-DIAG-001
	  - BUDGET-CANCEL-001
	  - STRESS-SESS-001..002
	  - STRESS-EVENT-001
	  - STRESS-INVOKE-001
	  - STRESS-PERSIST-001
	  - STRESS-HOST-001
	  - SOAK-001..005
	  - CRASH-001..006
	  - RACE-COLL-001
	  - RACE-QUEUE-001
	  - RACE-SCHED-001
	  - RACE-INVOKE-001
	  - RACE-RES-001
	  - RACE-PERSIST-001
	  - RACE-HTTP-001
	  - RACE-PIPE-001
	  - RACE-DM-001
	title: Robustness and reliability high-risk partitions remain bounded, reproducible, and cleanup-safe
	description: These explicit witnesses cover unsafe inputs, deterministic property/fault/race oracles, retention probes, resource budgets, stress/soak campaign conditions, and crash recovery. Each case records a seed or hook and treats process isolation, bounded execution, and cleanup as semantic assertions rather than optional telemetry.
	authority:
	  source: exhaustive plan document 04
	  section: Robustness, reliability, scale, crash recovery, and concurrency correctness
	  citation_or_rule: Untrusted input must fail safely within bounds; fault paths retain valid committed state and cleanup; every concurrent history must have a legal linearization; no per-session object may remain retained after teardown.
	phase: 5
	feature: reliability-high-risk-partitions
	target_components: [parser, interpreter, XPath, scheduler, host, transports, persistence]
	test_kind: generated-property-fault-leak-budget-stress-crash-and-race
	oracle_type: independent-model-bounded-artifact-resource-ledger-and-linearizability
	risk: critical
	priority: critical
	construction_routes: [generated-input, child-process, loopback-transport, persisted-bytes, virtual-time]
	data_models: [null, runtime, xpath]
	target_frameworks: [all-project-targets]
	platforms: [platform-specific-where-required]
	partitions: [adversarial, malformed, boundary, cancellation, fault, concurrency, cleanup, resource, security, scalability]
	dimensions: { seed: case-declared, hook: case-declared, schedule: case-declared }
	preconditions: [deterministic generator, child-process harness, fault plan, resource ledger, and independent models are available]
	dependencies: [GeneratedReliabilityHarness, seed shrinker, operation watchdog, linearizability oracle]
	arrange: Generate the bounded seed or schedule, warm bounded caches, and capture semantic and resource baselines.
	stimulus: Execute the declared campaign, fault boundary, crash hook, or schedule exploration.
	expected: [Exact independent-model result, bounded artifact, and baseline-equivalent owned-resource state after cleanup.]
	expected_exception_or_event: Case-declared controlled diagnostic, recovery outcome, or child-process termination; otherwise none.
	forbidden: [Stack overflow, uncontrolled allocation, process crash outside child witness, lost/duplicate event, illegal history, leak, deadlock, or unreproducible failure.]
	edge_cases: [limit minus one/exact/plus one, one-byte chunks, corrupted prefix, every cancellation hook, repeated resume, and two/three-operation schedules]
	determinism: { clock: virtual where semantic, scheduling: exhaustive bounded schedule, timeout_or_step_bound: seed-declared operation cap }
	isolation: { parallel_safe: false, shared_state: process-isolated where required }
	cleanup: [Tear down child/host, wait deterministic cleanup, and assert zero ledger, queue, timer, task, service, and temporary artifact counts.]
	resource_risk: critical
	tier: generated
	tags: [Exhaustive, Robustness, Reliability, Security, Concurrency]
	related_tests: [PHASE5-ROBUSTNESS-RELIABILITY-MATRIX-001]
	known_issue: none
	compile_notes: GeneratedReliabilityHarness, child-process runner, metrics adapter, and independent reference models are intentionally unresolved test-side infrastructure.
	generation_status: generated-review-required
	*/
	[DataTestMethod]
	[DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
	public async Task Phase5_high_risk_partition_is_bounded_and_reproducible(Phase5PartitionCase testCase)
	{
		// Arrange
		await using var scope = await GeneratedReliabilityHarness.CreateAsync(testCase);
		var baseline = await scope.CaptureBaselineAsync();

		// Act
		var artifact = await scope.ExecuteBoundedAsync(testCase);

		// Assert
		await scope.AssertAuthorityOutcomeAsync(testCase.Expected, artifact);
		await scope.AssertForbiddenEffectsAbsentAsync(testCase.Forbidden, baseline);
		await scope.AssertCleanupAndReproducerAsync(testCase.CaseId);
	}

	/* CASE-METADATA
	cases:
	  - case_id: ROBUST-XML-001-CASE-101
		requirement_ids: [ROBUST-XML-001..005, ROBUST-XINC-001, ROBUST-XPATH-001..003, ROBUST-EVENT-001, ROBUST-MODEL-001, ROBUST-PERSIST-001, ROBUST-IO-001]
		description: Deep, wide, entity-expanded, malformed, and encoding-invalid documents/framing inputs fail at declared size/depth limits without partial model escape or dangerous allocation.
		input: Limit-1/limit/limit+1 XML, XInclude, XPath, event, persistence, HTTP, and pipe seeds with one-byte chunk variants.
		stimulus: Parse, compile, route, or recover each seed under watchdog.
		expected: In-limit seeds return exact canonical result; over-limit/malformed seeds return controlled diagnostic with no partial model and bounded artifact.
		expected_exception_or_event: case-declared controlled diagnostic.
		forbidden: stack overflow, external entity disclosure, unbounded allocation, crash, hang, or retained stream/buffer.
	  - case_id: PROP-CONFIG-001-CASE-101
		requirement_ids: [FUZZ-SCXML-001, FUZZ-MODEL-001, FUZZ-XPATH-001, FUZZ-XMLDATA-001, FUZZ-EVENT-001, FUZZ-PERSIST-001, FUZZ-HTTP-001, FUZZ-PIPE-001, PROP-CONFIG-001, PROP-ORDER-001, PROP-EVENT-001, PROP-SELECT-001, PROP-HISTORY-001, PROP-SER-001, PROP-DATA-001, PROP-SCHED-001, PROP-PERSIST-001, PROP-ISOLATE-001]
		description: Bounded generated model/event/input seeds either match independent configuration/order/data/scheduler/persistence oracles or yield a documented diagnostic, with shrinkable seed retained.
		input: Canonical graph up to declared small-model bound, event stream, XPath tree, payload, and suspension hook.
		stimulus: Run alone and interleaved with an unrelated generated session; serialize/recover where selected.
		expected: Per-session traces equal the reference traces; valid round trips reach declared fixed point; artifact records seed and canonical graph hash.
		expected_exception_or_event: documented diagnostic for invalid generated input.
		forbidden: differential mismatch, cross-session effect, missing seed/hash, hang, crash, or unbounded resource growth.
	  - case_id: FAULT-STORE-001-CASE-101
		requirement_ids: [FAULT-EVAL-001, FAULT-QUEUE-001, FAULT-ROUTE-001, FAULT-INVOKE-001, FAULT-RES-001, FAULT-XML-001, FAULT-STORE-001, FAULT-LOG-001, FAULT-HOST-001, FAULT-IO-001]
		description: Every injectable boundary preserves its primary failure while aggregating cleanup failures, prevents later side effects, and leaves a healthy sibling machine unaffected.
		input: FaultPlan failing/blocking/cancelling/malformed/dispose-throwing at each first/middle/last boundary call.
		stimulus: Execute declared operation once per boundary and call position.
		expected: Exact primary error/event and declared committed snapshot; cleanup is attempted exactly once and secondary failures are aggregated.
		expected_exception_or_event: injected primary controlled failure.
		forbidden: swallowed primary fault, later action, invalid committed state, lock/waiter/task/timer leak, or sibling contamination.
	  - case_id: LEAK-SESSION-001-CASE-101
		requirement_ids: [LEAK-SESSION-001..002, LEAK-DM-001, LEAK-XPATH-001, LEAK-RUNTIME-001, LEAK-SCHED-001, LEAK-INVOKE-001, LEAK-RES-001, LEAK-PERSIST-001, LEAK-HTTP-001, LEAK-PIPE-001, LEAK-SEC-001, LEAK-LOG-001]
		description: Repeated success/error/cancel/destroy lifecycle batches release weak-reference sentinels and restore all owned ledger counts to warm baseline.
		input: Warmed child-process loop with success, fault, cancellation, nested callback, invoke, resource, persistence, and transport variants.
		stimulus: Run declared batches, drop references, deterministically quiesce, and perform bounded full collections.
		expected: All per-session sentinels are dead; queues/timers/tasks/services/resources are zero; retained-size slope is within declared noise envelope.
		expected_exception_or_event: none.
		forbidden: positive per-session retention slope, finalizer/unobserved-task error, live owned handle, or static-cache attribution without declared plateau.
	  - case_id: BUDGET-CANCEL-001-CASE-101
		requirement_ids: [BUDGET-PARSE-001, BUDGET-SELECT-001, BUDGET-XPATH-001, BUDGET-QUEUE-001, BUDGET-PAYLOAD-001, BUDGET-DIAG-001, BUDGET-CANCEL-001]
		description: Geometric input sizes follow declared complexity and size/cancellation limits reject plus-one work before full buffering while diagnostics remain bounded.
		input: 1x/2x/4x/8x model/tree/queue/payload inputs and cancellation at every declared polling hook.
		stimulus: Measure bounded operation counters/bytes with virtual cancellation.
		expected: Doubling ratios remain within the declared complexity envelope; over-limit input rejects before forbidden acquisition/allocation; cancellation completes at declared bound.
		expected_exception_or_event: controlled limit or cancellation outcome.
		forbidden: superlinear unexplained growth, full payload buffering beyond limit, unbounded diagnostic text, or delayed cancellation past bound.
	  - case_id: CRASH-003-CASE-101
		requirement_ids: [STRESS-SESS-001..002, STRESS-EVENT-001, STRESS-INVOKE-001, STRESS-PERSIST-001, STRESS-HOST-001, SOAK-001..005, CRASH-001..006, RACE-COLL-001, RACE-QUEUE-001, RACE-SCHED-001, RACE-INVOKE-001, RACE-RES-001, RACE-PERSIST-001, RACE-HTTP-001, RACE-PIPE-001, RACE-DM-001]
		description: Exhaustively scheduled two/three-operation races and child-process crash hooks recover only the last committed legal history and leave stress/soak resources bounded.
		input: Deterministic schedules for collection/queue/scheduler/invoke/resource/persistence/HTTP/pipe/data-model operations and crash hook IDs around durable writes/sends/completion.
		stimulus: Enumerate legal schedules; terminate child at each hook; restart fresh process from durable bytes.
		expected: Every history has a documented linearization; recovery has no impossible hybrid; committed events deliver once according to documented semantics; artifact contains hook/schedule/trace tail.
		expected_exception_or_event: controlled child termination at crash hook.
		forbidden: deadlock, lost/duplicate committed event, corrupted durable state, retained child resource, or missing reproducer.
	*/
	public static IEnumerable<object[]> Cases() =>
	[
		[
			new Phase5PartitionCase(
				CaseId: "ROBUST-XML-001-CASE-101", Expected: "Bounded success or controlled diagnostic", Forbidden: "No crash, disclosure, unbounded allocation, or retained buffer.")
		],
		[
			new Phase5PartitionCase(
				CaseId: "PROP-CONFIG-001-CASE-101", Expected: "Exact reference trace and reproducible artifact", Forbidden: "No differential mismatch, session leak, or missing seed.")
		],
		[
			new Phase5PartitionCase(CaseId: "FAULT-STORE-001-CASE-101", Expected: "Primary fault plus exact cleanup", Forbidden: "No swallowed fault, later side effect, or leaked resource.")
		],
		[
			new Phase5PartitionCase(
				CaseId: "LEAK-SESSION-001-CASE-101", Expected: "Dead sentinels and baseline resource ledger", Forbidden: "No positive retention slope or unobserved finalizer error.")
		],
		[
			new Phase5PartitionCase(
				CaseId: "BUDGET-CANCEL-001-CASE-101", Expected: "Declared complexity and cancellation bounds",
				Forbidden: "No over-buffering, oversized diagnostic, or delayed cancellation.")
		],
		[
			new Phase5PartitionCase(
				CaseId: "CRASH-003-CASE-101", Expected: "Legal linearization and last-committed recovery", Forbidden: "No hybrid, duplicate/lost event, deadlock, or missing artifact.")
		]
	];

	public sealed record Phase5PartitionCase(string CaseId, string Expected, string Forbidden);
}
