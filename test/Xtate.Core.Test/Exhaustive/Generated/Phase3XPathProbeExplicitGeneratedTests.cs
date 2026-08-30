using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Generated;

[TestClass]
public sealed class Phase3XPathProbeExplicitGeneratedTests
{
    /*
    TEST-METADATA
    test_id: XPATH-PROBE-EXPLICIT-MATRIX-001
    requirement_ids: [XPATH-PROBE-001,XPATH-PROBE-004,XPATH-PROBE-005,XPATH-PROBE-008,XPATH-PROBE-009,XPATH-PROBE-010]
    title: XPath high-risk semantic probes use independently reviewable fixtures
    description: Each explicit record supplies a discriminating XPath/SCXML fixture and complete state oracle, so an implementation cannot pass by merely accepting a requirement identifier or returning an unspecified document result.
    authority: { source: W3C XPath 1.0 and W3C SCXML XPath Data Model Note, section: effective boolean value, assignment, binding, foreach, and runtime context, citation_or_rule: XPath semantics and no-effect-on-error rules are asserted exactly per record }
    phase: 3
    feature: xpath-high-risk-probes
    target_components: [XPathDataModel, XPathEvaluator, XPathAssignmentAction, RuntimeDataModel]
    test_kind: integration-and-component
    oracle_type: exact-result-event-tree-and-resource-snapshot
    risk: critical
    priority: critical
    construction_routes: [scxml-text,xml-tree,public-object-model]
    data_models: [xpath,runtime]
    target_frameworks: [all-project-targets]
    platforms: [platform-independent]
    partitions: [positive,negative,no-effect-on-error,late-binding,foreach-index,cleanup]
    dimensions: { cases: six-explicit-high-risk-fixtures }
    preconditions: [isolated deterministic XPath test session]
    dependencies: [ExplicitXPathProbeHarness,IndependentXPathOracle,CompleteTreeSnapshot]
    arrange: Build the exact record fixture and capture its complete canonical XML/data/session snapshot.
    stimulus: Execute the record's one declared SCXML macrostep or direct XPath operation.
    expected: [record-specific exact result, event, tree, count, and cleanup outcome]
    expected_exception_or_event: record-specific error.execution or none
    forbidden: [record-specific silent coercion, partial mutation, stale context, wrong index, or retained resource]
    edge_cases: [NaN, empty node-set, multi-target mutation, re-entry, nested callback]
    determinism: { clock: virtual-or-not-applicable, scheduling: explicit-single-step, timeout_or_step_bound: '100 operations per record' }
    isolation: { parallel_safe: true, shared_state: none }
    cleanup: [dispose session and assert no iterator, callback, timer, or ambient context remains]
    resource_risk: xpath-context-and-tree-retention
    tier: fast
    tags: [Exhaustive,XPath,DataModel,HighRisk]
    related_tests: [DM-PROP-002,DM-PROP-003]
    known_issue: none
    compile_notes: ExplicitXPathProbeHarness, IndependentXPathOracle, and CompleteTreeSnapshot are planned test-side helpers.
    generation_status: generated-uncompiled
    */
    [DataTestMethod]
    [DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
    public async Task XPath_probe_has_exact_authority_derived_outcome(XPathProbeCase testCase)
    {
        // Arrange
        await using var session = await ExplicitXPathProbeHarness.CreateAsync(testCase);
        var before = await session.CaptureCompleteSnapshotAsync();

        // Act
        var outcome = await session.ExecuteAsync(testCase.Stimulus, maxOperations: 100);

        // Assert
        Assert.AreEqual(testCase.ExpectedExceptionOrEvent, outcome.ExceptionOrEvent);
        await session.AssertExactResultAsync(testCase.ExpectedResult, outcome);
        await session.AssertForbiddenEffectsAbsentAsync(testCase.ForbiddenResults, before);
        await session.AssertCanonicalTreeAsync(testCase.ExpectedTree);
        await session.AssertCleanupAsync();
    }

    public static IEnumerable<object[]> Cases() =>
        ExplicitCases.Select(testCase => new object[] { testCase });

    public static readonly XPathProbeCase[] ExplicitCases =
    [
        new(
            CaseId: "XPATH-PROBE-001-CASE-001",
            RequirementIds: "XPATH-PROBE-001|XPATH-EXPR-014",
            Description: "A nonempty node-set whose first text value is false has XPath effective boolean value true, while NaN has false effective boolean value.",
            Fixture: "XML <root><v>false</v></root>; expressions /root/v and number('not-a-number') in XPath conditions.",
            Stimulus: "Evaluate each expression once as an SCXML transition condition.",
            ExpectedResult: "The node-set condition selects its transition; the NaN condition does not select its transition.",
            ExpectedExceptionOrEvent: "none",
            ExpectedTree: "Input tree unchanged.",
            ForbiddenResults: "Parsing node text as XML boolean; treating NaN as true; error.execution for either valid expression.",
            Partitions: "positive|effective-boolean|node-set|NaN",
            Dimensions: "value-kind=node-set/number; node-text=false; numeric=NaN",
            Risk: "critical",
            TargetFrameworksPlatforms: "all-project-targets/platform-independent",
            CompileNotes: "ExplicitXPathProbeHarness and independent XPath evaluator are test-side helpers."),
        new(
            CaseId: "XPATH-PROBE-004-CASE-001",
            RequirementIds: "XPATH-PROBE-004|XPATH-ASSIGN-008",
            Description: "An assign location evaluating to an empty node-set raises error.execution and leaves every data node unchanged.",
            Fixture: "XPath data <datamodel><data id='x'>before</data></datamodel>; assign location=/datamodel/data[@id='missing'] value='after'.",
            Stimulus: "Execute the assign action in one deterministic macrostep.",
            ExpectedResult: "The machine queues error.execution and x remains text 'before'.",
            ExpectedExceptionOrEvent: "error.execution",
            ExpectedTree: "Canonical pre-state exactly equals canonical post-state.",
            ForbiddenResults: "Silent no-op reported as success; x changed to after; partial normalization or added data node.",
            Partitions: "negative|empty-location|no-effect-on-error",
            Dimensions: "location-cardinality=0; action=replacechildren; value=string",
            Risk: "critical",
            TargetFrameworksPlatforms: "all-project-targets/platform-independent",
            CompileNotes: "Error-event capture and canonical tree comparison are planned helpers."),
        new(
            CaseId: "XPATH-PROBE-005-CASE-001",
            RequirementIds: "XPATH-PROBE-005|XPATH-ASSIGN-010",
            Description: "A failure at the second selected assignment target rolls back the successful first-target mutation.",
            Fixture: "Two selected data elements a and b; a writable, b read-only; replacechildren location=/datamodel/data value='changed'.",
            Stimulus: "Execute one multi-target assign action.",
            ExpectedResult: "error.execution is queued and both a and b retain their original values and access metadata.",
            ExpectedExceptionOrEvent: "error.execution",
            ExpectedTree: "Complete canonical pre-state equals post-state, including node order and read-only metadata.",
            ForbiddenResults: "a changed while b rejects; retained transaction snapshot; success event.",
            Partitions: "negative|multi-target|atomicity|read-only",
            Dimensions: "target-count=2; failing-target=last; action=replacechildren",
            Risk: "critical",
            TargetFrameworksPlatforms: "all-project-targets/platform-independent",
            CompileNotes: "Mutation fault hook and canonical tree comparer are planned helpers."),
        new(
            CaseId: "XPATH-PROBE-008-CASE-001",
            RequirementIds: "XPATH-PROBE-008|XPATH-TREE-002",
            Description: "Late-bound state data evaluates once on first entry and is not reinitialized after exit and re-entry.",
            Fixture: "Late-binding SCXML state declares data x from a counting XPath extension expression, mutates x, exits, then re-enters the state.",
            Stimulus: "Start, mutate x, dispatch exit, then dispatch re-entry.",
            ExpectedResult: "Counting expression invocation count is 1 and x retains its mutation after re-entry.",
            ExpectedExceptionOrEvent: "none",
            ExpectedTree: "x has the post-mutation value after re-entry.",
            ForbiddenResults: "Second expression invocation; x reset to initialization value; leaked first-entry scope.",
            Partitions: "positive|late-binding|re-entry|state-data",
            Dimensions: "binding=late; entries=2; initialization-count=1",
            Risk: "critical",
            TargetFrameworksPlatforms: "all-project-targets/platform-independent",
            CompileNotes: "Counting XPath extension function and deterministic machine driver are planned helpers."),
        new(
            CaseId: "XPATH-PROBE-009-CASE-001",
            RequirementIds: "XPATH-PROBE-009|XPATH-FOREACH-001|XPATH-FOREACH-006",
            Description: "XPath foreach assigns one-based indexes in document order instead of the generic data-model zero-based index.",
            Fixture: "XPath array /root/item with item texts A,B,C and foreach item=i index=n appending n:i to ordered trace.",
            Stimulus: "Execute the foreach body once for the node-set.",
            ExpectedResult: "Trace is 1:A,2:B,3:C and outer variable bindings are restored after the loop.",
            ExpectedExceptionOrEvent: "none",
            ExpectedTree: "Source XML remains ordered A,B,C.",
            ForbiddenResults: "0:A first index; reverse iteration; retained i or n binding after loop.",
            Partitions: "positive|foreach|index-boundary|scope-cleanup",
            Dimensions: "node-count=3; index-origin=one; order=document",
            Risk: "high",
            TargetFrameworksPlatforms: "all-project-targets/platform-independent",
            CompileNotes: "Ordered trace and XPath foreach driver are planned helpers."),
        new(
            CaseId: "XPATH-PROBE-010-CASE-001",
            RequirementIds: "XPATH-PROBE-010|DM-RUNTIME-005",
            Description: "A runtime callback that captures a large session sentinel does not retain that sentinel through AsyncLocal after completion.",
            Fixture: "Two isolated runtime-data-model sessions; first callback captures sentinel and completes, second callback checks no first-session ambient data, then bounded weak-reference collection runs.",
            Stimulus: "Complete both callbacks, destroy both sessions, and run the bounded collection protocol.",
            ExpectedResult: "Second session observes only its own context and the first sentinel weak reference is dead.",
            ExpectedExceptionOrEvent: "none",
            ExpectedTree: "No persistent XPath tree applies.",
            ForbiddenResults: "First-session ambient context visible in second callback; live sentinel after teardown; retained callback scope.",
            Partitions: "cleanup|cross-session-isolation|async-context|weak-reference",
            Dimensions: "sessions=2; callback-continuation=async; context=AsyncLocal",
            Risk: "critical",
            TargetFrameworksPlatforms: "all-project-targets/platform-independent",
            CompileNotes: "Runtime callback probe and bounded weak-reference collection helper are planned test-side infrastructure.")
    ];

    public sealed record XPathProbeCase(
        string CaseId,
        string RequirementIds,
        string Description,
        string Fixture,
        string Stimulus,
        string ExpectedResult,
        string ExpectedExceptionOrEvent,
        string ExpectedTree,
        string ForbiddenResults,
        string Partitions,
        string Dimensions,
        string Risk,
        string TargetFrameworksPlatforms,
        string CompileNotes);
}
