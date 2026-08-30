using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Generated;

[TestClass]
public sealed class Phase4HostPersistenceSecurityExplicitGeneratedTests
{
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
    [DataTestMethod]
    [DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
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

    public static readonly HostScenarioCase[] ExplicitCases =
    [
        new("HOST-LIFE-003-CASE-001", "HOST-LIFE-003", "Dispatch racing destruction linearizes once: an event accepted before destroy is either processed or explicitly rejected, never lost after acceptance.", "Waiting session S with dispatch gate before queue write and destroy gate before removal.", "Release dispatch-write then destroy-removal; dispatch event e to S.", "Exactly one linearized result: e appears once in trace before removal, or dispatch receives documented rejected-session result before acceptance.", "No ghost event in a removed session; no duplicate completion; no retained queue waiter.", "No persistence required."),
        new("HOST-LIFE-007-CASE-001", "HOST-LIFE-007", "Caller cancellation while waiting for the controller lock cancels only that caller and leaves the active machine executable.", "Running session S holds controller gate; caller A dispatches with cancellable token; caller B remains uncancelled.", "Cancel A before lock release, release lock, then dispatch B event finish.", "A receives OperationCanceledException; B reaches final; S remains active until B completes.", "Cancellation of S lifetime, loss of B event, or a leaked lock waiter.", "No persistence required."),
        new("HOST-SCHED-002-CASE-001", "HOST-SCHED-002", "Cancelling one shared send ID before fire removes all and only that ID's delayed events.", "Virtual scheduler has A1,A2 with send ID A and B1 with send ID B at same due time.", "Cancel A, advance to due time, drain routing trace.", "Trace contains only B1; pending set has no A records and no B record after dispatch.", "A dispatch, B cancellation, duplicate routing, or retained scheduler entry.", "No persistence required."),
        new("PERSIST-STORE-002-CASE-001", "PERSIST-STORE-002", "A torn write after a durable checkpoint does not expose uncommitted replacement state on reopen.", "Store contains committed key k=old; write fake faults after partial bytes for attempted k=new.", "Begin replacement write, inject partial-write fault, close, then reopen fresh store.", "Read k returns old committed value and fault is PersistenceException or documented storage failure.", "k=new, malformed successful read, allocation of corrupt value, or leaked transaction/stream.", "Durable bytes equal prior committed checkpoint or documented recoverable prefix."),
        new("PERSIST-SUSP-009-CASE-001", "PERSIST-SUSP-009", "Concurrent suspension and delayed fire choose one winner and durable/live collections agree after recovery.", "Virtual clock schedules event d; suspend request and scheduler fire pause at shared linearization gate.", "Release suspend first, then fire; kill scope and resume from captured bytes.", "Recovered queue contains d exactly once if fire was not committed, otherwise trace contains d exactly once and durable queue omits d.", "Both queued and dispatched d, neither queued nor dispatched d, or live/durable disagreement.", "Recovered snapshot is consistent with the chosen single winner."),
        new("SEC-CTX-006-CASE-001", "SEC-CTX-005|SEC-CTX-006", "An untrusted SCXML session attempting external resource access is denied before loader invocation and cannot leak its security context into another session.", "Session U has no I/O permission and src points to protected URI; independent trusted session T records ambient permissions.", "Start U resource action, then run T callback after U denial.", "U receives authorization failure/error.communication before loader call; T observes only trusted permissions.", "Loader invocation for U, privilege escalation, retained U context, or altered T permissions.", "No persistent security-context data remains after both sessions dispose.")
    ];

    public sealed record HostScenarioCase(string CaseId, string RequirementIds, string Description, string Fixture, string Operation, string Expected, string Forbidden, string DurableOutcome);
}
