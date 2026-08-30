using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Generated;

[TestClass]
public sealed class Phase5RobustnessExplicitGeneratedTests
{
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
    [DataTestMethod]
    [DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
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

    public static readonly RobustnessCase[] ExplicitCases =
    [
        new("ROBUST-XML-001-CASE-001", "ROBUST-XML-001", "External general entity is rejected without resolver file or network I/O.", "SCXML DOCTYPE declares file URI entity and references it in data; resolver probe starts at zero calls.", "Parse via one-byte stream with budget 4096 bytes/100 reads.", 4101, 100, "Controlled XML/SCXML diagnostic; resolver call count is zero.", "File/network resolver call, accepted external entity text, process crash, or more than 100 reads.", "zero live readers/streams; 4096-byte allocation ceiling."),
        new("ROBUST-XPATH-003-CASE-001", "ROBUST-XPATH-003|XPATH-ASSIGN-010", "Last-target XPath mutation failure rolls back a large overlapping selection within its operation budget.", "100 writable selected nodes followed by one read-only overlapping target; deep-copy value has 64 descendants.", "Execute replacechildren with injected final-target access failure.", 4102, 5000, "error.execution and canonical post-tree equals pre-tree.", "Any early target mutation, skipped final access check, retained transaction buffer, or >5000 model operations.", "zero live iterators/snapshots; bounded copy buffer."),
        new("ROBUST-EVENT-001-CASE-001", "ROBUST-EVENT-001", "A 256-event external flood preserves FIFO until documented queue backpressure rejects later events.", "Waiting machine records event sequence; virtual host queue capacity is 256; payload is 1 KiB immutable value.", "Dispatch e000 through e256 in order then drain deterministically.", 4103, 1024, "Accepted events are e000..e255 in order; e256 gets documented capacity rejection.", "Reordered/duplicated payload, silent loss of an accepted event, unbounded queue growth, or retained payload.", "zero queued payloads and waiters after destruction."),
        new("FUZZ-SCXML-001-CASE-001", "FUZZ-SCXML-001", "Fixed grammar seed produces either a valid normalized model or one bounded diagnostic without a crash.", "Seed 0x5CXML001 generates namespace shadowing, reordered children, malformed event token, and one-byte UTF-8 chunks.", "Generate, parse, validate, and shrink the first invalid production under 2000 steps.", 1547956225, 2000, "Exactly one of valid canonical model or classified parse/validation diagnostic; shrink result is no larger than seed input.", "Unhandled exception, hang, stack overflow, nondeterministic classification, or leak of reader/generator state.", "zero generator buffers/readers; 2 MiB total allocation budget."),
        new("PROP-SELECT-001-CASE-001", "PROP-SELECT-001", "A fixed generated nested conflict graph selects the maximal non-conflicting descendant-preempting transition set.", "Seed 0x51EC7001 makes active parent/child siblings with ancestor and descendant same-event transitions.", "Deliver event go and compare selected trace/configuration to independent SCXML selection model.", 1374449665, 500, "Selected set equals reference set, is pairwise non-conflicting, and excludes the conflicting ancestor.", "Missing enabled descendant, selected conflict pair, ancestor preemption violation, or model/session divergence.", "no live generated graph or interpreter session."),
        new("PROP-PERSIST-001-CASE-001", "PROP-PERSIST-001", "Suspension before delayed-event dequeue recovers to the same configuration and exactly-once event trace as uninterrupted execution.", "Seed 0xPERS157 creates one delayed event and a pure transition; suspension gate is immediately before dequeue.", "Run uninterrupted reference, suspend gated run, kill host, resume durable bytes, then advance virtual clock.", 188641564, 1000, "Recovered final configuration/data/trace equal reference and delayed event appears exactly once.", "Lost or duplicate delayed event, divergent pure state, stale host membership, or durable/live disagreement.", "zero old/new host sessions, timers, storage handles, and tasks."),
        new("FAULT-STORE-001-CASE-001", "FAULT-STORE-001", "Checkpoint write fault preserves the prior committed journal and releases the storage lock.", "Committed journal has event old; faulting store throws during replacement checkpoint flush.", "Attempt replacement checkpoint, reopen store, then acquire lock for a clean retry.", 4104, 300, "Reopen contains only old event; retry lock acquisition succeeds; operation reports controlled storage failure.", "Replacement visible as committed, deadlocked lock, corrupt journal dispatch, or retained stream.", "zero transactions/semaphores/streams after close."),
        new("LEAK-XPATH-001|RACE-SCHED-001-CASE-001", "LEAK-XPATH-001|RACE-SCHED-001", "Concurrent compiled XPath disposal and same-ID schedule/cancel/fire leave no context, timer, or duplicate dispatch.", "Two sessions compile same expression; scheduler holds two events ID A; gates pause fire and dispose/cancel concurrently.", "Release cancel, fire, expression dispose, then session destroy according to recorded schedule 1-2-3-4.", 4105, 400, "At most one permitted dispatch per event; no disposed context use; all weak sentinels dead after bounded collection.", "Duplicate fire, use-after-dispose, retained XPath navigator/context, timer CTS, or deadlocked gate.", "resource ledger empty; weak sentinels collected within 8 bounded GC passes.")
    ];

    public sealed record RobustnessCase(string CaseId, string RequirementIds, string Description, string Fixture, string Stimulus, int Seed, int OperationBudget, string Expected, string Forbidden, string ResourceBudget);
}
