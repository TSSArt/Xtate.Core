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
public sealed class Phase4HostPersistenceSecurityExplicitGeneratedTests
{
	private static readonly HostScenarioCase[] ExplicitCases =
	[
		new(
			CaseId: "HOST-LIFE-003-CASE-001", RequirementIds: "HOST-LIFE-003",
			Description: "Dispatch racing destruction linearizes once: an event accepted before destroy is either processed or explicitly rejected, never lost after acceptance.",
			Fixture: "Waiting session S with dispatch gate before queue write and destroy gate before removal.", Operation: "Release dispatch-write then destroy-removal; dispatch event e to S.",
			Expected: "Exactly one linearized result: e appears once in trace before removal, or dispatch receives documented rejected-session result before acceptance.",
			Forbidden: "No ghost event in a removed session; no duplicate completion; no retained queue waiter.", DurableOutcome: "No persistence required."),
		new(
			CaseId: "HOST-LIFE-007-CASE-001", RequirementIds: "HOST-LIFE-007",
			Description: "Caller cancellation while waiting for the controller lock cancels only that caller and leaves the active machine executable.",
			Fixture: "Running session S holds controller gate; caller A dispatches with cancellable token; caller B remains uncancelled.",
			Operation: "Cancel A before lock release, release lock, then dispatch B event finish.",
			Expected: "A receives OperationCanceledException; B reaches final; S remains active until B completes.", Forbidden: "Cancellation of S lifetime, loss of B event, or a leaked lock waiter.",
			DurableOutcome: "No persistence required."),
		new(
			CaseId: "HOST-SCHED-002-CASE-001", RequirementIds: "HOST-SCHED-002", Description: "Cancelling one shared send ID before fire removes all and only that ID's delayed events.",
			Fixture: "Virtual scheduler has A1,A2 with send ID A and B1 with send ID B at same due time.", Operation: "Cancel A, advance to due time, drain routing trace.",
			Expected: "Trace contains only B1; pending set has no A records and no B record after dispatch.", Forbidden: "A dispatch, B cancellation, duplicate routing, or retained scheduler entry.",
			DurableOutcome: "No persistence required."),
		new(
			CaseId: "PERSIST-STORE-002-CASE-001", RequirementIds: "PERSIST-STORE-002", Description: "A torn write after a durable checkpoint does not expose uncommitted replacement state on reopen.",
			Fixture: "Store contains committed key k=old; write fake faults after partial bytes for attempted k=new.",
			Operation: "Begin replacement write, inject partial-write fault, close, then reopen fresh store.",
			Expected: "Read k returns old committed value and fault is PersistenceException or documented storage failure.",
			Forbidden: "k=new, malformed successful read, allocation of corrupt value, or leaked transaction/stream.",
			DurableOutcome: "Durable bytes equal prior committed checkpoint or documented recoverable prefix."),
		new(
			CaseId: "PERSIST-SUSP-009-CASE-001", RequirementIds: "PERSIST-SUSP-009",
			Description: "Concurrent suspension and delayed fire choose one winner and durable/live collections agree after recovery.",
			Fixture: "Virtual clock schedules event d; suspend request and scheduler fire pause at shared linearization gate.",
			Operation: "Release suspend first, then fire; kill scope and resume from captured bytes.",
			Expected: "Recovered queue contains d exactly once if fire was not committed, otherwise trace contains d exactly once and durable queue omits d.",
			Forbidden: "Both queued and dispatched d, neither queued nor dispatched d, or live/durable disagreement.",
			DurableOutcome: "Recovered snapshot is consistent with the chosen single winner."),
		new(
			CaseId: "SEC-CTX-006-CASE-001", RequirementIds: "SEC-CTX-005|SEC-CTX-006",
			Description: "An untrusted SCXML session attempting external resource access is denied before loader invocation and cannot leak its security context into another session.",
			Fixture: "Session U has no I/O permission and src points to protected URI; independent trusted session T records ambient permissions.",
			Operation: "Start U resource action, then run T callback after U denial.",
			Expected: "U receives authorization failure/error.communication before loader call; T observes only trusted permissions.",
			Forbidden: "Loader invocation for U, privilege escalation, retained U context, or altered T permissions.",
			DurableOutcome: "No persistent security-context data remains after both sessions dispose.")
	];

	/*
	TEST-METADATA
	test_id: PHASE4-EXPLICIT-HIGH-RISK-001
	requirement_ids: [HOST-LIFE-003,HOST-LIFE-004,HOST-LIFE-007,HOST-SCHED-002,PERSIST-STORE-002,PERSIST-SUSP-009,SEC-CTX-005,SEC-CTX-006]
	title: Host, persistence, scheduler, and security high-risk cases have explicit schedules
	description: Each record specifies the lifecycle hook, deterministic interleaving, durable-state oracle, and forbidden ghost effect, so host correctness cannot be represented by an ID-only factory.
	authority: { source: exhaustive plan document 03, section: host lifecycle, scheduler, persistence, and security, citation_or_rule: each listed operation has the record's stated linearization, failure, and isolation result }
	phase: 4
	feature: host-persistence-security
	target_components: [StateMachineHost,StateMachineController,EventScheduler,PersistenceStore,SecurityContext]
	test_kind: deterministic-integration-and-fault-injection
	oracle_type: exact-trace-durable-snapshot-and-resource-ledger
	risk: critical
	priority: critical
	construction_routes: [public-host-api,scxml-text,persisted-bytes]
	data_models: [null,runtime,xpath]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [lifecycle-race,cancellation,scheduler-cancellation,persistence-fault,recovery,security-isolation]
	dimensions: { cases: six-explicit-deterministic-interleavings }
	preconditions: [isolated host, virtual clock, blockable fakes, resource ledger]
	dependencies: [ExplicitHostScenarioHarness,DeterministicRaceGate,PersistenceReferenceModel,ResourceLedger]
	arrange: Create the record fixture, arm named gates, and capture host membership plus durable snapshot.
	stimulus: Release exactly the record's named gates and invoke its public operation once.
	expected: [record-specific membership, event, durable bytes, trace, and resource outcome]
	expected_exception_or_event: record-specific exception or platform event
	forbidden: [ghost dispatch, duplicate removal, unrelated cancellation, torn durable state, privilege leak]
	edge_cases: [before/after linearization, concurrent caller, injected partial failure]
	determinism: { clock: virtual, scheduling: named-race-gates, timeout_or_step_bound: '200 operations per record' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [dispose host and assert zero live session, timer, task, stream, service, and ambient security context]
	resource_risk: critical
	tier: fast
	tags: [Exhaustive,Host,Persistence,Security,FaultInjection]
	related_tests: [PROP-PERSIST-001,PROP-ISOLATE-001]
	known_issue: none
	compile_notes: ExplicitHostScenarioHarness, DeterministicRaceGate, and PersistenceReferenceModel are planned test-side helpers.
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	[DynamicData(nameof(Cases))]
	public async Task Host_persistence_and_security_case_has_exact_linearized_outcome(HostScenarioCase testCase)
	{
		// Arrange
		await using var scenario = await ExplicitHostScenarioHarness.CreateAsync(testCase);
		var before = await scenario.CaptureSnapshotAsync();
		scenario.ArmGates(testCase.CaseId);

		// Act
		var result = await scenario.ExecuteAsync(testCase.Operation, maxOperations: 200);

		// Assert
		await scenario.AssertExactOutcomeAsync(testCase.Expected, result);
		await scenario.AssertForbiddenEffectsAbsentAsync(testCase.Forbidden, before);
		await scenario.AssertDurableStateAsync(testCase.DurableOutcome);
		await scenario.AssertCleanupAsync();
	}

	public static IEnumerable<object[]> Cases() => ExplicitCases.Select(testCase => new object[] { testCase });

	public sealed record HostScenarioCase(
		string CaseId,
		string RequirementIds,
		string Description,
		string Fixture,
		string Operation,
		string Expected,
		string Forbidden,
		string DurableOutcome);
}
