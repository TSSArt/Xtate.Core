using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Generated;

[TestClass]
public sealed class Phase3DataModelXPathRequirementsGeneratedTests
{
    /*
    TEST-METADATA
    test_id: PHASE3-DM-XPATH-EXPLICIT-MATRIX-001
    requirement_ids: Explicitly enumerated in the literal Case records below.
    title: Data-model and XPath cases preserve exact value, tree, event, and cleanup outcomes
    description: Each literal record provides one concrete fixture and authority-derived oracle, so a wrong conversion, XPath result, mutation, diagnostic, or retained scope is observable without treating a requirement identifier as semantic coverage.
    authority: { source: W3C XPath 1.0 and W3C SCXML XPath Data Model Note, section: data values, XPath evaluation, location assignment, foreach, and runtime context, citation_or_rule: Every record states its own exact observable result. }
    phase: 3
    feature: data-model-and-xpath
    target_components: [DataModelValue,NullDataModel,RuntimeDataModel,XPathDataModel,XPathAssignmentAction]
    test_kind: declarative-contract
    oracle_type: exact-result-event-canonical-tree-and-resource-snapshot
    risk: critical
    priority: critical
    construction_routes: [public-object-model,scxml-text,xml-tree]
    data_models: [null,runtime,xpath]
    target_frameworks: [all-project-targets]
    platforms: [platform-independent]
    partitions: [positive,negative,boundary,conversion,atomicity,concurrency,cleanup]
    dimensions: { case_source: literal-record, schedule: deterministic-single-step }
    preconditions: [isolated deterministic data-model session]
    dependencies: [ExplicitDataModelXPathHarness,IndependentXPathOracle,CompleteTreeSnapshot]
    arrange: Create the literal record fixture and capture a canonical pre-operation snapshot.
    stimulus: Execute exactly the literal record operation once with a 100-operation bound.
    expected: [the literal record exact result, event, tree, and cleanup state]
    expected_exception_or_event: literal-record-specific
    forbidden: [the literal record forbidden effects]
    edge_cases: [empty values, NaN, node-set cardinality, read-only data, cancellation, concurrent first use]
    determinism: { clock: virtual-or-not-applicable, scheduling: deterministic-single-step, timeout_or_step_bound: '100 operations' }
    isolation: { parallel_safe: true, shared_state: none }
    cleanup: [dispose the session and assert no iterator, callback, timer, or ambient context remains]
    resource_risk: xpath-context-and-tree-retention
    tier: fast
    tags: [Exhaustive,XPath,DataModel]
    related_tests: [XPATH-PROBE-EXPLICIT-MATRIX-001]
    known_issue: none
    compile_notes: ExplicitDataModelXPathHarness, IndependentXPathOracle, and CompleteTreeSnapshot are intentionally unresolved test-side helpers.
    generation_status: generated-uncompiled
    */
    [DataTestMethod]
    [DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
    public async Task Explicit_data_model_or_xpath_case_has_exact_authority_derived_outcome(ExplicitDataModelXPathCase testCase)
    {
        // Arrange
        await using var session = await ExplicitDataModelXPathHarness.CreateAsync(testCase);
        var before = await session.CaptureCompleteSnapshotAsync();

        // Act
        var outcome = await session.ExecuteAsync(testCase.Stimulus, maxOperations: 100);

        // Assert
        Assert.AreEqual(testCase.ExpectedExceptionOrEvent, outcome.ExceptionOrEvent);
        await session.AssertExactResultAsync(testCase.ExpectedResult, outcome);
        await session.AssertCanonicalTreeAsync(testCase.ExpectedTree);
        await session.AssertForbiddenEffectsAbsentAsync(testCase.ForbiddenResults, before);
        await session.AssertCleanupAsync();
    }

    public static IEnumerable<object[]> Cases() => ExplicitCases.Select(testCase => new object[] { testCase });

    public static readonly ExplicitDataModelXPathCase[] ExplicitCases =
    [
        new("DM-VALUE-001-CASE-001", "DM-VALUE-001", "Undefined is distinct from a defined null value when a data-model variable is read.", "Variables u=undefined and n=null in one null data model.", "Read u and n through the public value API.", "u reports Undefined and n reports Null; neither read creates a third variable.", "none", "No XML tree applies.", "Coercing undefined to null; materializing an empty-string variable.", "positive|undefined|null", "value-kind=undefined/null", "high", "all-project-targets/platform-independent", "ExplicitDataModelXPathHarness is unresolved test infrastructure."),
        new("DM-CONV-002-CASE-001", "DM-CONV-002", "XPath number conversion of whitespace-only text produces NaN rather than zero or an empty-string success.", "XPath context node text is three ASCII spaces.", "Evaluate number(.) once.", "The scalar result is numeric NaN and no mutation or error.execution event occurs.", "none", "Input tree is unchanged.", "Returning zero; trimming into a valid number; changing the text node.", "lexical|conversion|whitespace|NaN", "source=text; lexical-form=whitespace-only", "high", "all-project-targets/platform-independent", "IndependentXPathOracle is unresolved test infrastructure."),
        new("DM-NULL-004-CASE-001", "DM-NULL-004", "A null data model rejects an assign action without silently accepting or partially creating the target.", "SCXML uses datamodel='null' and executes assign location='x' expr='1'.", "Run the macrostep containing the assign action.", "The machine emits error.execution, has no variable x, and continues only according to the declared error policy.", "error.execution", "No XML tree applies.", "Successful assignment; a created x variable; an unreported no-op.", "negative|null-model|assignment|no-effect-on-error", "data-model=null; action=assign", "critical", "all-project-targets/platform-independent", "Null data-model driver and event recorder are unresolved test-side helpers."),
        new("DM-RUNTIME-003-CASE-001", "DM-RUNTIME-003", "A runtime data-model callback receives the current event payload but cannot expose it to an unrelated session.", "Two sessions each invoke a callback; session A receives event payload 'A' and session B receives 'B'.", "Dispatch both events in deterministic A-then-B order and inspect callback observations.", "A observes only 'A', B observes only 'B', and each callback scope is cleared on completion.", "none", "No XML tree applies.", "B observing A; shared ambient payload; a live callback scope after completion.", "positive|cross-session-isolation|event-payload|cleanup", "sessions=2; schedule=A-then-B", "critical", "all-project-targets/platform-independent", "Runtime callback probe and resource ledger are unresolved test-side helpers."),
        new("XPATH-TREE-003-CASE-001", "XPATH-TREE-003", "The child axis returns children in document order and excludes attributes.", "XML <r z='9'><a/><b/></r> with r as context node.", "Evaluate child::* and child::@* once.", "child::* yields a then b; child::@* yields an empty node-set.", "none", "Tree remains <r z='9'><a/><b/></r>.", "Returning attribute z through child::*; reverse order; mutation.", "positive|axis|document-order", "axis=child; child-count=2; attribute-count=1", "high", "all-project-targets/platform-independent", "IndependentXPathOracle is unresolved test infrastructure."),
        new("XPATH-COMP-002-CASE-001", "XPATH-COMP-002", "An unterminated string literal fails compilation before any expression evaluation or state mutation.", "Expression concat('unterminated) attached to a transition condition.", "Compile the model once.", "Compilation returns the expression diagnostic owned by that condition and no executable model is produced.", "XPathSyntaxException", "No source tree mutation occurs.", "Evaluating a partial expression; error.execution after state entry; a partially compiled model.", "negative|compile|lexical|unterminated-string", "expression=unterminated-literal; phase=compile", "critical", "all-project-targets/platform-independent", "XPathSyntaxException and compiled-model probe are unresolved test-side helpers."),
        new("XPATH-ASSIGN-008-CASE-001", "XPATH-ASSIGN-008", "An empty XPath location causes error.execution and leaves the complete data tree unchanged.", "XML <data><x>before</x></data>; assign location=/data/missing expr='after'.", "Execute the assign action once.", "error.execution is queued and canonical post-tree equals canonical pre-tree.", "error.execution", "<data><x>before</x></data> remains unchanged.", "Creating missing; changing x; reporting success without an error event.", "negative|assignment|empty-node-set|atomicity", "target-cardinality=0; action=replacechildren", "critical", "all-project-targets/platform-independent", "XPath assignment driver and canonical tree comparer are unresolved test-side helpers."),
        new("XPATH-FOREACH-001-CASE-001", "XPATH-FOREACH-001", "XPath foreach visits node-set members in document order and binds the one-based index required by its data-model contract.", "XML <r><i>A</i><i>B</i><i>C</i></r>; foreach array=/r/i item=v index=n appends n:v.", "Execute the foreach body once.", "Ordered trace is 1:A,2:B,3:C and v/n bindings are absent after loop completion.", "none", "Source XML remains ordered A,B,C.", "Zero-based first index; reverse iteration; retained loop bindings.", "positive|foreach|document-order|scope-cleanup", "node-count=3; index-origin=one", "high", "all-project-targets/platform-independent", "XPath foreach driver and ordered trace recorder are unresolved test-side helpers."),
        new("DM-PROP-003-CASE-001", "DM-PROP-003", "Concurrent first evaluation of the same compiled XPath expression is isolated per session and does not share mutable evaluation context.", "Two sessions share one compiled expression /r/v and have distinct XML values A and B.", "Open both first evaluations at a deterministic barrier, then release A followed by B.", "A returns A, B returns B, and both sessions dispose their evaluators after the barrier.", "none", "Each session tree remains unchanged.", "A returning B; B returning A; shared variable context; retained evaluator after disposal.", "concurrency|first-use|session-isolation|cleanup", "sessions=2; schedule=barrier-A-B; expression-cache=shared", "critical", "all-project-targets/platform-independent", "Deterministic barrier and evaluator resource ledger are unresolved test-side helpers.")
    ];

    public sealed record ExplicitDataModelXPathCase(
        string CaseId, string RequirementIds, string Description, string Fixture, string Stimulus,
        string ExpectedResult, string ExpectedExceptionOrEvent, string ExpectedTree, string ForbiddenResults,
        string Partitions, string Dimensions, string Risk, string TargetFrameworksPlatforms, string CompileNotes);
}
