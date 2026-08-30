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
public sealed class Phase4HostPersistenceIoSecurityRequirementsGeneratedTests
{
	private static readonly ExplicitHostRequirementCase[] ExplicitCases =
	[
		new(
			CaseId: "HOST-LIFE-007-CASE-001", RequirementIds: "HOST-LIFE-007",
			Description: "Destroy requested while start is blocked wins the deterministic lifecycle race and prevents the queued start continuation from publishing a running session.",
			Fixture: "Host start hook blocks after scope creation; destroy is issued at that hook.", Stimulus: "Release destroy, then release the start hook.",
			Expected: "Lifecycle trace is scope-created,destroying,destroyed; start returns the documented cancellation result and no running session is registered.",
			ExpectedExceptionOrEvent: "OperationCanceledException", DurableState: "No durable state applies.",
			Forbidden: "Running notification after destroyed; duplicate disposal; retained scope or start waiter.", Partitions: "concurrency|lifecycle|start-destroy-race|cleanup",
			Dimensions: "schedule=start-block,destroy,release-start", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Deterministic host lifecycle hooks and resource ledger are unresolved test-side helpers."),
		new(
			CaseId: "HOST-QUEUE-002-CASE-001", RequirementIds: "HOST-QUEUE-002",
			Description: "Closing an external event queue wakes a blocked dequeue once and rejects every later enqueue without retaining its payload.",
			Fixture: "Host queue has one blocked dequeue and a second large sentinel payload prepared after close.", Stimulus: "Close queue, await the blocked dequeue, then enqueue the sentinel.",
			Expected: "Blocked dequeue observes closed completion; later enqueue returns the documented rejection; sentinel weak reference is releasable after teardown.",
			ExpectedExceptionOrEvent: "QueueClosedException", DurableState: "No durable state applies.", Forbidden: "A second wakeup; accepted post-close event; retained sentinel; hung waiter.",
			Partitions: "negative|queue-close|waiter|cleanup", Dimensions: "waiters=1; enqueue-after-close=1", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Queue probe and bounded weak-reference helper are unresolved test-side helpers."),
		new(
			CaseId: "HOST-SCHED-003-CASE-001", RequirementIds: "HOST-SCHED-003",
			Description: "Cancelling a delayed send at the scheduler fire boundary prevents dispatch exactly once and removes its timer and send-ID registration.",
			Fixture: "Virtual scheduler has one delayed event sendid=s1 paused immediately before dispatch.", Stimulus: "Cancel s1 at the fire hook, then release the hook.",
			Expected: "No event reaches the target queue; s1 is absent from pending sends; timer and cancellation registration counts are zero.", ExpectedExceptionOrEvent: "none",
			DurableState: "No durable state applies.", Forbidden: "One dispatch after cancellation; a residual timer; a residual send-ID entry.",
			Partitions: "concurrency|scheduler|cancel-fire-race|cleanup", Dimensions: "send-count=1; cancellation-point=pre-dispatch", Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/supported-platforms", CompileNotes: "Virtual scheduler and send registry probe are unresolved test-side helpers."),
		new(
			CaseId: "IO-HTTP-012-CASE-001", RequirementIds: "IO-HTTP-012",
			Description: "An HTTP request whose declared content length exceeds the configured maximum is rejected before body buffering or session dispatch.",
			Fixture: "Loopback request declares Content-Length=max+1 and provides a body stream whose first read increments a counter.", Stimulus: "Submit the request once.",
			Expected: "Response is the documented payload-too-large status; body-read counter is 0; no machine session or persistence transaction is created.", ExpectedExceptionOrEvent: "none",
			DurableState: "No durable state applies.", Forbidden: "Reading even one body byte; dispatching a session; returning success; leaked request stream.",
			Partitions: "negative|http|size-boundary|pre-acquisition|cleanup", Dimensions: "content-length=max+1; body-reads=0", Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/supported-platforms", CompileNotes: "Loopback HTTP fixture and host transaction probe are unresolved test-side helpers."),
		new(
			CaseId: "IO-PIPE-006-CASE-001", RequirementIds: "IO-PIPE-006",
			Description: "A named-pipe client disconnect during a partial frame abandons only that request and releases its buffer and cancellation source.",
			Fixture: "Server receives a frame header and half its declared payload, then client disconnects.", Stimulus: "Drive read to disconnect and advance deterministic cleanup.",
			Expected: "No event is dispatched; the connection closes; pooled-buffer and cancellation-source ledger counts return to zero.", ExpectedExceptionOrEvent: "EndOfStreamException",
			DurableState: "No durable state applies.", Forbidden: "Dispatching a truncated event; retaining pooled bytes; stopping unrelated pipe accept loop.",
			Partitions: "negative|pipe|partial-frame|disconnect|cleanup", Dimensions: "frame-completeness=partial; client=disconnect", Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/Windows-and-supported-Unix", CompileNotes: "Named-pipe loopback fixture and pool ledger are unresolved test-side helpers."),
		new(
			CaseId: "RES-LOAD-003-CASE-001", RequirementIds: "RES-LOAD-003",
			Description: "A resource loader read failure closes the acquired stream once, returns no partial model, and preserves the primary read exception.",
			Fixture: "Resolver returns a tracking stream that throws ResourceReadException on its second read.", Stimulus: "Load the resource once.",
			Expected: "ResourceReadException is reported as the loader diagnostic; no partial model is exposed; DisposeAsync count is exactly 1.", ExpectedExceptionOrEvent: "ResourceReadException",
			DurableState: "No durable state applies.", Forbidden: "A partial model; a swallowed read failure; zero or multiple stream disposal.",
			Partitions: "negative|resource|read-fault|ownership|cleanup", Dimensions: "read-fault-call=2; stream-count=1", Risk: "high",
			TargetFrameworksPlatforms: "all-project-targets/supported-platforms", CompileNotes: "Tracking stream and resource loader diagnostic probe are unresolved test-side helpers."),
		new(
			CaseId: "SEC-CTX-004-CASE-001", RequirementIds: "SEC-CTX-004",
			Description: "A denied caller cannot obtain an authorized identity through nested asynchronous dispatch or a reused worker context.",
			Fixture: "Denied principal D and authorized principal A run on a reused deterministic worker; D submits a nested dispatch requiring A-only permission.",
			Stimulus: "Run D's request, release nested dispatch, then run A's independent request.",
			Expected: "D receives authorization denial with no protected side effect; A succeeds; post-request worker context contains neither principal.",
			ExpectedExceptionOrEvent: "UnauthorizedAccessException", DurableState: "No durable state applies.",
			Forbidden: "Nested dispatch inheriting A; protected operation under D; retained identity after either request.", Partitions: "security|authorization|async-context|worker-reuse|cleanup",
			Dimensions: "principals=denied/authorized; worker=reused", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Security context probe and deterministic worker are unresolved test-side helpers."),
		new(
			CaseId: "PERSIST-SUSP-005-CASE-001", RequirementIds: "PERSIST-SUSP-005",
			Description: "A checkpoint write fault before durable commit leaves the previous committed snapshot recoverable and does not publish the new transition side effect.",
			Fixture: "Store contains committed snapshot S0; suspend reaches checkpoint for S1 and FaultPlan throws before commit marker.",
			Stimulus: "Execute suspend and reopen storage in a fresh host.",
			Expected: "Suspend reports the store fault; recovery loads exactly S0; S1 side effect is absent; failed transaction resources are released.",
			ExpectedExceptionOrEvent: "StorageWriteException", DurableState: "Recovered durable bytes equal S0.",
			Forbidden: "Recovering partial S1; duplicate transition side effect; unreadable store; retained transaction lock.",
			Partitions: "negative|persistence|checkpoint|pre-commit|recovery|cleanup", Dimensions: "checkpoint-phase=before-commit; snapshots=S0/S1", Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Faulting store, fresh-host recovery fixture, and transaction ledger are unresolved test-side helpers."),
		new(
			CaseId: "PERSIST-SCHED-004-CASE-001", RequirementIds: "PERSIST-SCHED-004",
			Description: "A persisted delayed event cancelled before suspension is absent after resume and cannot fire from a stale journal record.",
			Fixture: "Session schedules sendid=s1 for virtual time 10, cancels s1 at time 5, then checkpoints and resumes in a fresh host.", Stimulus: "Advance resumed virtual clock past time 10.",
			Expected: "No s1 event is delivered; restored pending-send set excludes s1; scheduler journal contains no live s1 record.", ExpectedExceptionOrEvent: "none",
			DurableState: "Recovered durable state contains the cancellation tombstone or equivalent committed absence.",
			Forbidden: "Stale s1 dispatch; duplicate cancellation; retained scheduler entry.", Partitions: "persistence|scheduler|cancel|resume|cleanup",
			Dimensions: "sendid=s1; cancel-time=5; fire-time=10", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/supported-platforms",
			CompileNotes: "Persisted scheduler harness and journal inspector are unresolved test-side helpers.")
	];

	/*
	TEST-METADATA
	test_id: PHASE4-HOST-IO-PERSIST-EXPLICIT-MATRIX-001
	requirement_ids: Explicitly enumerated in the literal Case records below.
	title: Host, transport, security, resource, and persistence cases retain exact side-effect and teardown oracles
	description: Each literal record identifies the request, lifecycle point, fault or schedule, durable result, forbidden side effect, and owned-resource result; a requirement identifier alone never selects the fixture or oracle.
	authority: { source: exhaustive plan document 03 and documented public contracts, section: host lifecycle, I/O, security, resources, and persistence, citation_or_rule: validation precedes side effects, authorization never escalates, committed recovery is atomic, and ownership is released exactly once. }
	phase: 4
	feature: host-persistence-io-security
	target_components: [StateMachineHost,SecurityContext,resource-loaders,HttpProcessor,NamedPipeProcessor,persistence-services]
	test_kind: declarative-contract-fault-and-schedule
	oracle_type: exact-trace-response-durable-snapshot-and-resource-ledger
	risk: critical
	priority: critical
	construction_routes: [public-object-model,loopback-transport,persisted-bytes,dependency-injection]
	data_models: [null,runtime,xpath]
	target_frameworks: [all-project-targets]
	platforms: [Windows-and-supported-Unix]
	partitions: [positive,negative,boundary,malformed,cancellation,concurrency,cleanup,security]
	dimensions: { case_source: literal-record, schedule: deterministic }
	preconditions: [isolated deterministic host, storage, transport, and security fakes]
	dependencies: [ExplicitHostRequirementHarness,VirtualScheduler,FaultPlan,ResourceLedger]
	arrange: Construct the literal host/session/resource/transport/storage fixture and capture durable and resource snapshots.
	stimulus: Apply the literal operation, fault boundary, cancellation point, or deterministic interleaving.
	expected: [the literal exact trace, response, authorization result, durable snapshot, and zero-owned-resource ledger]
	expected_exception_or_event: literal-record-specific
	forbidden: [the literal record forbidden effects]
	edge_cases: [malformed framing, retry, cancellation, destroy races, corrupt durable data]
	determinism: { clock: virtual, scheduling: explicit, timeout_or_step_bound: '100 operations' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [dispose host and case resources and assert queues, services, waits, sockets, and ledger reach zero]
	resource_risk: critical
	tier: fast
	tags: [Exhaustive,Host,Persistence,IO,Security]
	related_tests: []
	known_issue: none
	compile_notes: ExplicitHostRequirementHarness, VirtualScheduler, FaultPlan, and ResourceLedger are intentionally unresolved test-side helpers.
	generation_status: generated-uncompiled
	*/
	[DataTestMethod]
	[DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
	public async Task Explicit_host_persistence_or_io_case_preserves_contract_and_cleanup(ExplicitHostRequirementCase testCase)
	{
		// Arrange
		await using var scope = await ExplicitHostRequirementHarness.CreateAsync(testCase);
		var before = await scope.SnapshotAsync();

		// Act
		var outcome = await scope.ExecuteAsync(testCase.Stimulus, maxOperations: 100);

		// Assert
		await scope.AssertExactAsync(testCase.Expected, testCase.ExpectedExceptionOrEvent, outcome);
		await scope.AssertForbiddenAbsentAsync(testCase.Forbidden, before);
		await scope.AssertAllOwnedResourcesReleasedAsync();
	}

	public static IEnumerable<object[]> Cases() => ExplicitCases.Select(testCase => new object[] { testCase });

	public sealed record ExplicitHostRequirementCase(
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
