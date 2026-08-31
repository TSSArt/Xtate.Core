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
public sealed class Phase4HighRiskPartitionGeneratedTests
{
	/*
	TEST-METADATA
	test_id: PHASE4-HIGH-RISK-PARTITIONS-001
	requirement_ids:
	  - HOST-OPT-001..003
	  - HOST-IOC-001..004
	  - HOST-LIFE-001..008
	  - HOST-QUEUE-001..002
	  - HOST-SCHED-001..005
	  - HOST-TASK-001
	  - IO-SCXML-001..006
	  - IO-REG-001
	  - IO-HTTP-001..003
	  - IO-HTTP-010..016
	  - IO-HTTP-020..029
	  - IO-PIPE-001..009
	  - RES-LOAD-001..004
	  - RES-OBJ-001..003
	  - RES-SEC-001
	  - EXT-SVC-001..006
	  - SEC-CTX-001..006
	  - PERSIST-STORE-001..003
	  - PERSIST-DATA-001..003
	  - PERSIST-LEVEL-001
	  - PERSIST-SUSP-001..012
	  - PERSIST-SCHED-001..006
	  - PLAT-COMPAT-001..005
	title: Host, I/O, security, and persistence high-risk partitions preserve isolation and cleanup
	description: The matrix supplies concrete discriminators for capability boundaries, asynchronous lifecycle linearization, untrusted transport framing, durable commit/recovery, and teardown. Each case has a paired successful and rejected/failing route so that a side effect, resource, or session leak cannot be hidden by a successful aggregate result.
	authority:
	  source: exhaustive plan document 03
	  section: Host, persistence, I/O, resources, security, and platform compatibility
	  citation_or_rule: Invalid input is rejected before side effects, sessions and permissions are isolated, committed persistence is recoverable, and owned resources are released exactly once.
	phase: 4
	feature: host-io-persistence-high-risk-partitions
	target_components:
	  - StateMachineHost
	  - EventScheduler
	  - HttpIoProcessor
	  - NamedPipeIoProcessor
	  - PersistentStateMachineStore
	test_kind: declarative-contract-fault-and-schedule
	oracle_type: exact-trace-response-durable-snapshot-and-resource-ledger
	risk: critical
	priority: critical
	construction_routes: [dependency-injection, loopback-transport, persisted-bytes]
	data_models: [null, runtime, xpath]
	target_frameworks: [all-project-targets]
	platforms: [Windows-and-supported-Unix]
	partitions: [positive, negative, boundary, malformed, cancellation, concurrency, cleanup, security]
	dimensions: { route: case-declared, fault_boundary: case-declared }
	preconditions: [deterministic host, loopback doubles, fault plan, and resource ledger are available]
	dependencies: [GeneratedHostRequirementHarness, virtual scheduler, durable-store model]
	arrange: Construct the exact case-declared host/session/transport/store fixture and capture queue, durable-state, authorization, and resource-ledger snapshots.
	stimulus: Apply the case-declared operation, cancellation point, malformed payload, crash boundary, or two-operation schedule.
	expected: [The exact declared trace/response/durable snapshot is produced and all owned resources reach zero after teardown.]
	expected_exception_or_event: Case-declared controlled error/event; otherwise none.
	forbidden: [Unauthorized acquisition, cross-session dispatch, partial commit, duplicate delivery, unbounded allocation, or retained owned resource.]
	edge_cases: [limit minus one/exact/plus one, simultaneous destroy/dispatch, partial transfer, cancellation after linearization]
	determinism: { clock: virtual, scheduling: explicit deterministic schedule, timeout_or_step_bound: 150 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [Dispose all scopes and assert zero queue, timer, task, stream, socket, pipe, lock, and service entries.]
	resource_risk: critical
	tier: fast
	tags: [Exhaustive, Host, IO, Persistence, Security]
	related_tests: [PHASE4-HOST-IO-PERSIST-MATRIX-001]
	known_issue: none
	compile_notes: GeneratedHostRequirementHarness and durable-store reference model are intentionally unresolved test-side helpers.
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	[DynamicData(nameof(Cases))]
	public async Task Phase4_high_risk_partition_has_exact_authority_oracle(Phase4PartitionCase testCase)
	{
		// Arrange
		await using var scope = await GeneratedHostRequirementHarness.CreateAsync(testCase);
		var before = await scope.SnapshotAsync();

		// Act
		var outcome = await scope.ExecuteAsync(testCase);

		// Assert
		await scope.AssertExactAsync(testCase.Expected, outcome);
		await scope.AssertForbiddenAbsentAsync(testCase.Forbidden, before);
		await scope.AssertAllOwnedResourcesReleasedAsync();
	}

	/* CASE-METADATA
	cases:
	  - case_id: HOST-LIFE-004-CASE-101
		requirement_ids: [HOST-IOC-002..004, HOST-LIFE-001..008, HOST-QUEUE-001..002]
		description: Concurrent destroy during a blocked dispatch has one linearization point, completes every waiter, removes one session once, and does not cancel another session.
		input: Two sessions, one blocked dispatch hook, ten concurrent destroy callers, and one caller cancellation.
		stimulus: Release dispatch and race destroys/cancellation.
		expected: One session removal and completion result; all destroy callers observe the same documented result; second session trace is unchanged.
		expected_exception_or_event: caller cancellation only for the cancelled caller.
		forbidden: Ghost event, duplicate disposal, lost waiter, or cross-session cancellation.
	  - case_id: HOST-SCHED-002-CASE-101
		requirement_ids: [HOST-SCHED-001..005, HOST-TASK-001]
		description: Send-ID group cancellation at wait/dispatch boundaries affects only its group and leaves no pending timer or unobserved task.
		input: Equal-due events in two send-ID groups and faulting router for one dispatch.
		stimulus: Cancel first group before, during, and after scheduler advance.
		expected: Only uncancelled events dispatch in insertion order; fault is monitored; pending set and task ledger are zero.
		expected_exception_or_event: controlled router fault for faulting event.
		forbidden: Dispatch of cancelled event, cancellation of other group, or unobserved task.
	  - case_id: IO-HTTP-014-CASE-101
		requirement_ids: [IO-HTTP-001..003, IO-HTTP-010..016, IO-HTTP-020..029]
		description: HTTP outbound/inbound byte limits are enforced at N plus one before network dispatch or unbounded buffering, including multibyte UTF-8 and chunked input.
		input: N-1, N, N+1 byte text/form/XML payloads and chunked stream without Content-Length.
		stimulus: Send and receive each payload.
		expected: N-1/N accept with exact content; N+1 returns the documented size failure/413 and records zero dispatches.
		expected_exception_or_event: controlled size-limit error for N+1.
		forbidden: Character-count sizing, outbound request, partial dispatch, sensitive internal error body, or retained content stream.
	  - case_id: IO-PIPE-004-CASE-101
		requirement_ids: [IO-PIPE-001..009]
		description: A split or corrupt named-pipe frame prefix is rejected before allocation beyond the configured limit and pooled buffers are cleared and returned.
		input: One-byte prefix chunks, negative/overflow/max-int length, N-1/N/N+1 bodies, and EOF before prefix/body.
		stimulus: Decode each frame.
		expected: Only complete in-limit frames deserialize to equivalent event; every invalid prefix/body returns controlled protocol error.
		expected_exception_or_event: controlled protocol error for invalid frames.
		forbidden: Large allocation from prefix, cross-request response acceptance, pool residue, or process crash.
	  - case_id: RES-SEC-001-CASE-101
		requirement_ids: [RES-LOAD-001..004, RES-OBJ-001..003, RES-SEC-001, SEC-CTX-001..006]
		description: Disabled external access or nested denied security scope prevents file/web/provider acquisition entirely, including cached and concurrent calls.
		input: Instrumented file/web/resx providers, disabled XInclude option, and nested child denial.
		stimulus: Request relative and absolute resources concurrently.
		expected: Controlled authorization failure and zero provider/open/network calls; allowed sibling scope remains usable.
		expected_exception_or_event: documented authorization error.
		forbidden: Cache bypass, permission escalation, caller-owned stream disposal, or retained buffer.
	  - case_id: PERSIST-SUSP-006-CASE-101
		requirement_ids: [EXT-SVC-001..006, PERSIST-STORE-001..003, PERSIST-DATA-001..003, PERSIST-LEVEL-001, PERSIST-SUSP-001..012, PERSIST-SCHED-001..006, PLAT-COMPAT-001..005]
		description: Suspend at each durable hook, restart from committed bytes, and replay delayed sends produces the uninterrupted semantic trace without exposing a partial checkpoint.
		input: Machine with data, invoke, delayed event, injected store failure before/during/after write, and cross-framework artifact pair.
		stimulus: Suspend/restart at each hook and advance virtual time.
		expected: Recovery selects last committed snapshot, replays each committed delayed event once, and matches uninterrupted trace modulo documented side effect boundary.
		expected_exception_or_event: controlled store failure at injected hook.
		forbidden: Hybrid graph, duplicate/lost committed send, incompatible artifact acceptance, or retained store lock.
	*/
	public static IEnumerable<object[]> Cases() =>
	[
		[
			new Phase4PartitionCase(
				CaseId: "HOST-LIFE-004-CASE-101", Expected: "One removal, bounded waiters, isolated sibling",
				Forbidden: "No ghost event, duplicate disposal, or cross-session cancellation.")
		],
		[
			new Phase4PartitionCase(
				CaseId: "HOST-SCHED-002-CASE-101", Expected: "Only uncancelled group dispatches in order",
				Forbidden: "No cancelled dispatch, collateral cancellation, or unobserved task.")
		],
		[
			new Phase4PartitionCase(
				CaseId: "IO-HTTP-014-CASE-101", Expected: "Byte limit accepts N and rejects N+1 before dispatch", Forbidden: "No character sizing, partial dispatch, or retained stream.")
		],
		[
			new Phase4PartitionCase(CaseId: "IO-PIPE-004-CASE-101", Expected: "Bounded valid decode and controlled invalid-frame error", Forbidden: "No dangerous allocation, pool residue, or crash.")
		],
		[
			new Phase4PartitionCase(CaseId: "RES-SEC-001-CASE-101", Expected: "Denied acquisition makes zero provider calls", Forbidden: "No escalation, cache bypass, or retained buffer.")
		],
		[
			new Phase4PartitionCase(CaseId: "PERSIST-SUSP-006-CASE-101", Expected: "Recover last committed snapshot and replay once", Forbidden: "No hybrid, duplicate/lost send, or leaked lock.")
		]
	];

	public sealed record Phase4PartitionCase(string CaseId, string Expected, string Forbidden);
}
