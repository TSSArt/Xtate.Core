using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Generated;

[TestClass]
public sealed class Phase1InfrastructureAndConformanceGeneratedTests
{
    /*
    TEST-METADATA
    test_id: PHASE1-INFRA-CONFORMANCE-MATRIX-101
    requirement_ids: [INFRA-SNAPSHOT-001, INFRA-INSTRUMENT-001, INFRA-HOOK-001, INFRA-EVENT-ORACLE-001, INFRA-CONFIG-ORACLE-001, INFRA-XPATH-ORACLE-001, INFRA-GENERATOR-001, INFRA-CHILD-001, INFRA-TIER-001, W3C-SCXML-MAP-001, W3C-XINCLUDE-MAP-001]
    title: Deterministic infrastructure and W3C corpus mapping preserve source-only oracle contracts
    description: Each case specifies the missing deterministic probe or conformance-corpus mapping needed by the exhaustive suite. It asserts exact snapshots, independent-oracle agreement, bounded child isolation, and a one-to-one corpus disposition without executing an external suite in generation mode.
    authority: { source: exhaustive plan documents 01 through 06, section: Phase 1 infrastructure and W3C conformance rows, citation_or_rule: Every generated scenario requires deterministic snapshots/oracles and every applicable corpus item has an explicit mapped or decided disposition. }
    phase: 1
    feature: deterministic-infrastructure-and-conformance-mapping
    target_components: [interpreter driver, trace recorder, corpus mapper]
    test_kind: declarative-infrastructure-and-conformance
    oracle_type: exact-snapshot-independent-oracle-and-corpus-disposition
    risk: critical
    priority: critical
    construction_routes: [synthetic-model, vendored-corpus-manifest]
    data_models: [none, xpath]
    target_frameworks: [all-project-targets]
    platforms: [platform-independent, child-process-where-required]
    partitions: [positive, negative, cancellation, cleanup, resource, security]
    dimensions: { probe: case-declared }
    preconditions: [test-side deterministic infrastructure adapters and corpus manifest are available]
    dependencies: [GeneratedInfrastructureHarness, W3cCorpusMapper]
    arrange: Construct the exact declared probe or corpus item and capture baseline snapshot/manifest disposition.
    stimulus: Execute the deterministic probe or map one corpus item once.
    expected: [Exact declared snapshot/oracle result or explicit mapped/unsupported corpus disposition.]
    expected_exception_or_event: case-declared controlled failure; otherwise none.
    forbidden: [Production-algorithm reuse by an independent oracle, unmapped corpus item, resource leak, or unbounded child process.]
    edge_cases: [cancellation at hook, corrupted fixture, duplicate corpus ID, child crash]
    determinism: { clock: virtual, scheduling: explicit, timeout_or_step_bound: 100 operations }
    isolation: { parallel_safe: true, shared_state: none }
    cleanup: [Assert zero owned streams, tasks, process handles, and retained session references.]
    resource_risk: high
    tier: fast
    tags: [Exhaustive, Infrastructure, Conformance]
    related_tests: [INFRA_SCHED_001, INFRA_TRACE_001, INFRA_RES_001]
    known_issue: none
    compile_notes: GeneratedInfrastructureHarness and W3cCorpusMapper are intentionally unresolved test-side helpers.
    generation_status: generated-uncompiled
    */
    [DataTestMethod]
    [DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
    public async Task Phase1_infrastructure_or_conformance_case_has_exact_disposition(Phase1Case testCase)
    {
        // Arrange
        await using var scope = await GeneratedInfrastructureHarness.CreateAsync(testCase);
        var before = await scope.CaptureSnapshotAsync();
        // Act
        var outcome = await scope.ExecuteAsync(testCase);
        // Assert
        await scope.AssertExactAsync(testCase.Expected, outcome);
        await scope.AssertForbiddenAbsentAsync(testCase.Forbidden, before);
        await scope.AssertCleanupAsync();
    }

    /* CASE-METADATA
    cases:
      - case_id: INFRA-SNAPSHOT-001-CASE-101
        requirement_ids: [INFRA-SNAPSHOT-001, INFRA-INSTRUMENT-001, INFRA-HOOK-001, INFRA-EVENT-ORACLE-001, INFRA-CONFIG-ORACLE-001, INFRA-XPATH-ORACLE-001, INFRA-GENERATOR-001, INFRA-CHILD-001, INFRA-TIER-001]
        description: Deterministic hooks produce a complete ordered state/data/queue/invoke/persistence snapshot and independent event/configuration/XPath oracles reject a deliberately divergent result.
        input: Scheduled callbacks, resource/stream/evaluator fakes, model generator seed, and divergent candidate result.
        stimulus: Drive one bounded macrostep and compare independent oracles.
        expected: Exact snapshot and independent-oracle rejection of divergent candidate; all hooks/tasks/resources released.
        expected_exception_or_event: controlled child failure for crash probe only.
        forbidden: Production algorithm used as oracle, omitted snapshot field, non-deterministic ordering, or leaked process handle.
      - case_id: W3C-SCXML-MAP-001-CASE-101
        requirement_ids: [W3C-SCXML-MAP-001, W3C-XINCLUDE-MAP-001]
        description: Every vendored W3C SCXML/XInclude corpus item has one stable ID mapped to an authority-derived witness or explicit unsupported disposition.
        input: Corpus manifest with accepted, rejected, unsupported, and duplicate-ID fixtures.
        stimulus: Map manifest items to source test IDs.
        expected: Every unique fixture has one disposition; duplicate/missing mapping fails with fixture ID and no fixture is silently skipped.
        expected_exception_or_event: controlled mapping diagnostic for duplicate/missing item.
        forbidden: Unmapped fixture, duplicate disposition, or passing claim without later execution artifact.
    */
    public static IEnumerable<object[]> Cases() =>
    [
        new object[] { new Phase1Case("INFRA-SNAPSHOT-001-CASE-101", "Complete deterministic snapshot and independent-oracle rejection", "No production-oracle reuse, nondeterminism, or leak.") },
        new object[] { new Phase1Case("W3C-SCXML-MAP-001-CASE-101", "One explicit disposition per corpus item", "No unmapped/silently skipped fixture or execution claim.") }
    ];
    public sealed record Phase1Case(string CaseId, string Expected, string Forbidden);
}
