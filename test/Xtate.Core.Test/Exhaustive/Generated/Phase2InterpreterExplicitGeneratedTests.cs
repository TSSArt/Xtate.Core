using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Generated;

[TestClass]
public sealed class Phase2InterpreterExplicitGeneratedTests
{
    /*
    TEST-METADATA
    test_id: PHASE2-INTERPRETER-EXPLICIT-001
    requirement_ids: [SCXML-TRANS-011,SCXML-TRANS-012,SCXML-TRANS-013,SCXML-HIST-005,SCXML-EVENT-005,SCXML-EXEC-003,SCXML-SEND-004,SCXML-INVOKE-006,SCXML-ERROR-006]
    title: Interpreter high-risk lifecycle and executable-content scenarios have explicit traces
    description: Each case states an exact SCXML fixture, deterministic event sequence or cancellation hook, terminal trace, and forbidden side effect; requirement coverage is not inferred from a broad interpreter matrix.
    authority: { source: W3C SCXML 1.0; exhaustive plan document 01, section: transitions through error handling, citation_or_rule: transition, history, queue, executable content, send, invoke, and termination behavior is exact per record }
    phase: 2
    feature: interpreter-semantics
    target_components: [StateMachineInterpreter,EventQueue,EventScheduler,InvokeController]
    test_kind: deterministic-end-to-end
    oracle_type: ordered-trace-final-configuration-event-and-cleanup
    risk: critical
    priority: critical
    construction_routes: [scxml-text]
    data_models: [null,xpath,runtime]
    target_frameworks: [all-project-targets]
    platforms: [platform-independent]
    partitions: [condition-error,action-failure,cancellation,history-persistence,invoke-finalize,delayed-send,termination]
    dimensions: { records: seven-explicit-interpreter-schedules }
    preconditions: [isolated interpreter with virtual clock, trace recorder, and blockable doubles]
    dependencies: [ExplicitInterpreterScenarioHarness,InterpreterReferenceTrace]
    arrange: Create the literal SCXML and record trace before the stated operation.
    stimulus: Execute the record sequence with named fault/cancellation hook.
    expected: [record-specific ordered trace, final state, event, and resource result]
    expected_exception_or_event: record-specific platform event or none
    forbidden: [partial transition, out-of-order queue item, duplicate side effect, stale invoke, retained resource]
    edge_cases: [condition error, last action failure, cancellation after linearization]
    determinism: { clock: virtual, scheduling: single-step-with-named-hook, timeout_or_step_bound: '200 operations' }
    isolation: { parallel_safe: true, shared_state: none }
    cleanup: [zero live queues timers invokes scopes and waiters]
    resource_risk: critical
    tier: fast
    tags: [Exhaustive,SCXML,Interpreter]
    related_tests: []
    known_issue: none
    compile_notes: ExplicitInterpreterScenarioHarness and InterpreterReferenceTrace are planned test-side helpers.
    generation_status: generated-uncompiled
    */
    [DataTestMethod]
    [DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
    public async Task Interpreter_case_matches_its_exact_trace(InterpreterScenarioCase testCase)
    {
        // Arrange
        await using var scenario = await ExplicitInterpreterScenarioHarness.CreateAsync(testCase);
        scenario.Configure(testCase.Hook);

        // Act
        var outcome = await scenario.ExecuteAsync(testCase.Events, maxOperations: 200);

        // Assert
        await scenario.AssertTraceAsync(testCase.ExpectedTrace, outcome);
        await scenario.AssertForbiddenEffectsAbsentAsync(testCase.Forbidden);
        await scenario.AssertCleanupAsync();
    }

    public static IEnumerable<object[]> Cases() => ExplicitCases.Select(testCase => new object[] { testCase });

    public static readonly InterpreterScenarioCase[] ExplicitCases =
    [
        new("SCXML-TRANS-011-CASE-001", "SCXML-TRANS-011", "Condition evaluator failure queues error.execution and treats that candidate as false while an alternative transition completes.", "Nested source has failing cond='bad()' to trap then unconditional event transition to final.", "go", "ConditionError(bad),Queue(error.execution),Select(alternative),Enter(final)", "Trap entry, failed-transition actions, or missing error.execution."),
        new("SCXML-TRANS-012-CASE-001", "SCXML-TRANS-012", "Failure in the second transition action preserves first action then stops remaining actions and entries.", "Transition has trace A, fault action B, trace C, target final with onentry D.", "go with B throws", "Exit(source),Action(A),ActionFault(B),Queue(error.execution)", "Action C, entry D, rollback of already completed A, or unobserved task fault."),
        new("SCXML-TRANS-013-CASE-001", "SCXML-TRANS-013", "Cancellation at a blocked transition action leaves either stable source or fully committed target but never a hybrid configuration.", "Source to target transition pauses in awaited action after exit-set calculation.", "go; cancel at action gate", "Cancellation result and snapshot equals documented before or after linearization configuration.", "Source and target both active, stale queued event, or retained cancellation registration."),
        new("SCXML-HIST-005-CASE-001", "SCXML-HIST-005", "Corrupt persisted shallow-history state is rejected on resume instead of restoring an illegal child configuration.", "Persist parent history pointing to missing child ID then resume in fresh host.", "suspend; corrupt history ID; resume", "Controlled persistence/validation error and no active illegal configuration.", "Fallback to arbitrary child, partial session registration, or retained storage handle."),
        new("SCXML-EVENT-005|SCXML-INVOKE-006-CASE-001", "SCXML-EVENT-005|SCXML-INVOKE-006", "Matching invoke event runs finalize before transition selection; stale invoke ID does neither.", "Active invoke I has finalize trace F and event go transition trace T; stale event uses J.", "deliver J:go then I:go", "Only second event traces F then T and reaches final.", "Finalize for J, T before F, duplicate finalize, or retained invoke I."),
        new("SCXML-EXEC-003-CASE-001", "SCXML-EXEC-003", "Foreach restores item/index scope after an inner body failure.", "Runtime list [a,b], outer variables item=outer,index=9; body fails while processing b.", "start", "Trace contains a then failure; post-failure scope has item=outer,index=9.", "Leaked a/b scope binding, processing after failure, or retained iterator."),
        new("SCXML-SEND-004|SCXML-ERROR-006-CASE-001", "SCXML-SEND-004|SCXML-ERROR-006", "Destroy racing a zero-delay send cancels pending routing and completes waiters once.", "Machine schedules ID s with delay zero; router pauses before dispatch; destroy has cleanup trace.", "start; release destroy before router gate", "One destroy completion; router receives no dispatch; scheduler pending set empty.", "Post-destroy event, duplicate destroy completion, live timer/task, or hung waiter.")
    ];

    public sealed record InterpreterScenarioCase(string CaseId, string RequirementIds, string Description, string Scxml, string Events, string ExpectedTrace, string Forbidden)
    {
        public string Hook => "record-defined deterministic hook";
    }
}
