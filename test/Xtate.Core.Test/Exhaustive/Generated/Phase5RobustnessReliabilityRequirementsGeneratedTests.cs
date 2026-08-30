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
public sealed class Phase5RobustnessReliabilityRequirementsGeneratedTests
{
	private static readonly ExplicitReliabilityCase[] ExplicitCases =
	[
		new(
			CaseId: "ROBUST-XML-003-CASE-001", RequirementIds: "ROBUST-XML-003",
			Description: "A document with nesting depth max+1 is rejected before a recursive parser stack or model allocation exceeds the configured limit.",
			Fixture: "Fixed seed 4103 creates max+1 nested state elements; max is supplied by the parser policy.", Stimulus: "Parse once with the configured maximum depth.",
			Expected: "The parser reports the depth-limit diagnostic, creates no model, and records at most max+1 start-element visits.", ExpectedExceptionOrEvent: "XmlDepthLimitException",
			DurableState: "No durable state applies.", Forbidden: "Stack overflow; partial model; visits beyond max+1; retained reader.", Partitions: "adversarial|boundary|xml-depth|cleanup",
			Dimensions: "depth=max+1; seed=4103", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/platform-independent",
			CompileNotes: "Bounded parser probe and reader ledger are unresolved test-side helpers."),
		new(
			CaseId: "FAULT-STORE-001-CASE-001", RequirementIds: "FAULT-STORE-001",
			Description: "A checkpoint write failure preserves the last committed snapshot and releases the transaction lock without hiding the primary exception.",
			Fixture: "Committed snapshot S0; FaultPlan throws StorageWriteException on the first S1 write; lock ledger is enabled.", Stimulus: "Checkpoint S1, then reopen in a fresh scope.",
			Expected: "The primary result is StorageWriteException; recovery yields exactly S0; lock count is zero.", ExpectedExceptionOrEvent: "StorageWriteException",
			DurableState: "Recovered durable bytes equal S0.", Forbidden: "Partial S1; unreadable S0; a retained lock; cleanup exception replacing the store exception.",
			Partitions: "fault|persistence|recovery|cleanup", Dimensions: "fault=write-1; snapshots=S0/S1", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "FaultPlan, fresh-scope recovery, and lock ledger are unresolved test-side helpers."),
		new(
			CaseId: "LEAK-SCHED-001-CASE-001", RequirementIds: "LEAK-SCHED-001",
			Description: "Cancelled delayed sends do not retain session payloads, timers, or cancellation sources after deterministic teardown and bounded collection.",
			Fixture: "One thousand sessions schedule a large sentinel payload then cancel before virtual fire time; all session references are dropped.",
			Stimulus: "Advance virtual time past all fire times, dispose sessions, execute the bounded collection protocol.",
			Expected: "Every sentinel weak reference is dead; timer, send-ID, and cancellation-source ledger counts are zero.", ExpectedExceptionOrEvent: "none",
			DurableState: "No durable state applies.", Forbidden: "A live sentinel; timer or send-ID residue; cancellation source retained by callback.", Partitions: "leak|scheduler|cancel|cleanup",
			Dimensions: "sessions=1000; sends-per-session=1; virtual-fire-after-cancel", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Weak-reference collector and scheduler ledger are unresolved test-side helpers."),
		new(
			CaseId: "BUDGET-PAYLOAD-001-CASE-001", RequirementIds: "BUDGET-PAYLOAD-001",
			Description: "A one-byte-over-limit HTTP payload is rejected before full buffering and does not allocate a session-sized copy.",
			Fixture: "Loopback request declares and streams exactly configuredMaximum+1 bytes; allocation probe records maximum buffered bytes.",
			Stimulus: "Submit the request and complete its stream.",
			Expected: "The response is payload-too-large; buffered bytes never exceed the configured pre-buffer threshold; no session dispatch occurs.", ExpectedExceptionOrEvent: "none",
			DurableState: "No durable state applies.", Forbidden: "Full payload buffering; session creation; success response; retained request buffer.",
			Partitions: "resource|boundary|http|payload-limit", Dimensions: "payload=max+1; transport=http", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Loopback HTTP limiter and allocation probe are unresolved test-side helpers."),
		new(
			CaseId: "RACE-QUEUE-001-CASE-001", RequirementIds: "RACE-QUEUE-001",
			Description: "The enqueue/dequeue/close three-operation schedule has a legal linearization in which one event is delivered at most once and post-close enqueue is rejected.",
			Fixture: "Queue begins empty; schedule explorer enumerates enqueue(e), dequeue(), close() at every await hook.",
			Stimulus: "Explore all bounded interleavings and capture each operation history.",
			Expected: "Each history linearizes to FIFO enqueue-before-close delivery or close-before-enqueue rejection; all waiters complete and queue ledger is zero.",
			ExpectedExceptionOrEvent: "QueueClosedException-or-none-by-history", DurableState: "No durable state applies.",
			Forbidden: "Duplicate e; lost successful enqueue; blocked waiter; enqueue accepted after close; non-linearizable history.", Partitions: "concurrency|queue|linearizability|cleanup",
			Dimensions: "operations=3; schedules=all-bounded", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Schedule explorer, linearizability oracle, and queue ledger are unresolved test-side helpers."),
		new(
			CaseId: "CRASH-003-CASE-001", RequirementIds: "CRASH-003",
			Description: "A child process killed between scheduled-event journal write and commit recovers either the old journal or the complete new journal, never an impossible hybrid.",
			Fixture: "Child has journal J0 and schedules s1; crash hook terminates immediately after journal bytes write and before commit marker.",
			Stimulus: "Kill child at the hook and reopen only durable state in a fresh process.",
			Expected: "Recovery exposes exactly J0 or a fully committed J1 according to the storage commit protocol, with no malformed record and no duplicate s1 dispatch.",
			ExpectedExceptionOrEvent: "ChildProcessTerminated", DurableState: "Recovered journal equals J0 or valid J1 only.",
			Forbidden: "Hybrid journal; unreadable storage; duplicate s1; side effect before recovery decision.", Partitions: "crash|persistence|scheduler|recovery",
			Dimensions: "crash-point=after-write-before-commit; sendid=s1", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Child-process crash runner and journal validator are unresolved test-side helpers.")
	];

	/*
	TEST-METADATA
	test_id: PHASE5-ROBUSTNESS-RELIABILITY-EXPLICIT-MATRIX-001
	requirement_ids: Explicitly enumerated in the literal Case records below.
	title: Robustness and reliability cases retain reproducible safety, recovery, and resource oracles
	description: Each record has a fixed seed or deterministic schedule, literal fault or size boundary, exact observable safety result, forbidden effect, and post-cleanup resource result.
	authority: { source: exhaustive plan document 04, section: robustness, reliability, scale, faults, leaks, crash recovery, and races, citation_or_rule: untrusted input is bounded before dangerous allocation, committed state survives faults, and concurrent histories must be legal. }
	phase: 5
	feature: robustness-reliability-and-scale
	target_components: [parser,interpreter,XPath,scheduler,host,transports,persistence,resource-lifecycle]
	test_kind: generated-property-fault-leak-budget-and-race
	oracle_type: independent-model-bounded-artifact-trace-resource-ledger-and-linearizability
	risk: critical
	priority: critical
	construction_routes: [generated-input,child-process,loopback-transport,persisted-bytes,virtual-time]
	data_models: [null,runtime,xpath]
	target_frameworks: [all-project-targets]
	platforms: [platform-specific-where-required]
	partitions: [adversarial,boundary,malformed,fault,cancellation,concurrency,cleanup,resource,scalability]
	dimensions: { case_source: literal-record, reproducibility: fixed-seed-or-schedule }
	preconditions: [isolated deterministic generator, child-process harness, fault plan, resource ledger, and independent model]
	dependencies: [ExplicitReliabilityHarness,OperationWatchdog,LinearizabilityOracle]
	arrange: Create the literal bounded seed or schedule and capture semantic and resource baselines.
	stimulus: Execute the literal bounded campaign, fault hook, crash hook, or schedule exploration.
	expected: [the literal exact model, recovery, limit, artifact, and cleanup result]
	expected_exception_or_event: literal-record-specific
	forbidden: [the literal record forbidden effects]
	edge_cases: [limit-minus-one, limit, limit-plus-one, one-byte chunks, cancellation hooks, and three-operation schedules]
	determinism: { clock: virtual-where-semantic, scheduling: exhaustive-bounded, timeout_or_step_bound: '1,000 operations per literal record' }
	isolation: { parallel_safe: false, shared_state: process-isolated where required }
	cleanup: [tear down child or host and assert ledgers, queues, timers, and services are zero]
	resource_risk: critical
	tier: generated
	tags: [Exhaustive,Robustness,Reliability,Security,Concurrency]
	related_tests: []
	known_issue: none
	compile_notes: ExplicitReliabilityHarness, child-process runner, metrics adapter, and independent models are intentionally unresolved test-side helpers.
	generation_status: generated-review-required
	*/
	[DataTestMethod]
	[DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
	public async Task Explicit_reliability_case_is_bounded_safe_and_reproducible(ExplicitReliabilityCase testCase)
	{
		// Arrange
		await using var scope = await ExplicitReliabilityHarness.CreateAsync(testCase);
		var baseline = await scope.CaptureBaselineAsync();

		// Act
		var artifact = await scope.ExecuteBoundedAsync(testCase.Stimulus, maxOperations: 1_000);

		// Assert
		await scope.AssertExactOutcomeAsync(testCase.Expected, testCase.ExpectedExceptionOrEvent, artifact);
		await scope.AssertForbiddenEffectsAbsentAsync(testCase.Forbidden, baseline);
		await scope.AssertCleanupAndReproducerAsync(testCase.CaseId);
	}

	public static IEnumerable<object[]> Cases() => ExplicitCases.Select(testCase => new object[] { testCase });

	public sealed record ExplicitReliabilityCase(
		string CaseId,
		string RequirementIds,
		string Description,
		string Fixture,
		string Stimulus,
		string Expected,
		string ExpectedExceptionOrEvent,
		string DurableState,
		string Forbidden,
		string Partitions,
		string Dimensions,
		string Risk,
		string TargetFrameworksPlatforms,
		string CompileNotes);
}
