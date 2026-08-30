using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Generated;

/// <summary>Source-only evidence gates for the later validation campaign.</summary>
[TestClass]
public sealed class Phase6CoverageMutationCompatibilityGeneratedTests
{
    /*
    TEST-METADATA
    test_id: PHASE6-EVIDENCE-GATES-001
    requirement_ids:
      - PHASE6-NORMATIVE-SCXML
      - PHASE6-NORMATIVE-XPATH
      - PHASE6-PUBLIC-MATRIX
      - PHASE6-CRITICAL-COVERAGE
      - PHASE6-OTHER-COVERAGE
      - PHASE6-CRITICAL-MUTATION
      - PHASE6-OTHER-MUTATION
      - PHASE6-GENERATED-BUDGET
      - PHASE6-FUZZ-REPLAY
      - PHASE6-RESOURCE-GATE
      - PHASE6-STRESS-GATE
      - PHASE6-SCALABILITY-GATE
      - PHASE6-SOAK-CRASH-GATE
      - PHASE6-PLATFORM-GATE
      - PHASE6-COMPATIBILITY-GATE
      - PHASE6-FINAL-EVIDENCE
    title: Validation evidence gates reject incomplete or authority-divergent campaign evidence
    description: Each declarative gate verifies one Phase 6 acceptance criterion against immutable campaign artifacts. A missing requirement row, below-threshold critical metric, unbounded generated case, leaked resource, unsupported platform result, or authority-divergent compatibility result must prevent final sign-off rather than being hidden by aggregate percentages.
    authority:
      source: exhaustive plan document 05, Phase 6; exhaustive plan document 06
      section: Coverage, mutation, final compatibility, and source-generation completion
      citation_or_rule: Critical normative evidence and semantic mutation targets require 100 percent; other in-scope branch and mutation targets require their declared threshold; execution-derived gates are evaluated only in the later validation campaign.
    phase: 6
    feature: final-evidence-and-compatibility
    target_components:
      - normative requirement ledger exporter
      - coverage and mutation artifact readers
      - generated case budget verifier
      - resource and platform result aggregators
    test_kind: declarative-validation-gate
    oracle_type: exact-gate-decision-and-missing-evidence-list
    risk: critical
    priority: critical
    construction_routes:
      - immutable-campaign-artifacts
      - source-metadata-scan
    data_models:
      - none
    target_frameworks:
      - all-project-targets
    platforms:
      - all-supported-platform-lanes
    partitions:
      - complete
      - missing-evidence
      - threshold-boundary
      - authority-divergence
      - cleanup
    dimensions:
      gate: case-declared Phase 6 gate
      artifact_state: complete or deliberately incomplete
    preconditions:
      - later validation campaign supplies immutable bounded artifacts
    dependencies:
      - PhaseSixEvidenceHarness
      - source metadata scanner
    arrange: Load the exact case-declared artifact set, including all required ledger rows and the relevant threshold boundary values.
    stimulus: Evaluate the single named Phase 6 gate once without changing any artifact.
    expected:
      - The declared complete artifact set passes the gate.
      - The paired incomplete or below-threshold artifact set fails with the exact missing row or metric named by the case.
    expected_exception_or_event: none
    forbidden:
      - Aggregate success masking a missing critical row, unsupported lane, authority mismatch, retained resource, or below-threshold metric.
    edge_cases:
      - Exact threshold equality, a single missing ledger row, and a single incompatible target-framework result.
    determinism:
      clock: not-applicable
      scheduling: deterministic artifact evaluation
      timeout_or_step_bound: 200 artifact records
    isolation:
      parallel_safe: true
      shared_state: none
    cleanup:
      - Dispose artifact readers and assert the evidence harness retains no result stream or temporary directory handle.
    resource_risk: low
    tier: validation
    tags:
      - Exhaustive
      - Phase6
      - Evidence
    related_tests: []
    known_issue: none
    compile_notes: PhaseSixEvidenceHarness and immutable campaign-artifact contracts are intentionally unresolved test-side infrastructure; this source must not execute during generation mode.
    generation_status: generated-review-required
    */
    [DataTestMethod]
    [DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
    public async Task Phase6_gate_has_exact_pass_and_fail_discriminators(PhaseSixGateCase testCase)
    {
        // Arrange
        await using var scope = await PhaseSixEvidenceHarness.LoadAsync(testCase);

        // Act
        var decision = await scope.EvaluateAsync();

        // Assert
        await scope.AssertExactDecisionAsync(testCase.ExpectedDecision, decision);
        await scope.AssertForbiddenMaskingAbsentAsync(testCase.Forbidden);
        await scope.AssertDisposedAsync();
    }

    /*
    CASE-METADATA
    cases:
      - case_id: PHASE6-NORMATIVE-SCXML-CASE-001
        requirement_ids: [PHASE6-NORMATIVE-SCXML]
        description: Every SCXML normative ledger statement has an authority-linked test witness; removing one statement fails with that statement identifier.
        input: Complete SCXML ledger, then the same ledger with SCXML-PARSE-001 evidence removed.
        stimulus: Evaluate normative SCXML evidence gate.
        expected: Complete ledger passes; missing-row ledger fails and names SCXML-PARSE-001.
        expected_exception_or_event: none
        forbidden: A passing result based solely on aggregate test count.
        partitions: [complete, missing-evidence]
        dimensions: { authority: W3C-SCXML, removed_row: SCXML-PARSE-001 }
        risk: critical
        target_frameworks_platforms: all-project-targets/platform-independent
        compile_notes: PhaseSixEvidenceHarness unresolved.
      - case_id: PHASE6-NORMATIVE-XPATH-CASE-001
        requirement_ids: [PHASE6-NORMATIVE-XPATH]
        description: Every XPath data-model normative statement has a direct witness; removing the no-effect-on-error assignment row fails exactly that gate.
        input: Complete XPath ledger, then ledger without XPATH-ASSIGN-010 evidence.
        stimulus: Evaluate normative XPath evidence gate.
        expected: Complete ledger passes; incomplete ledger fails and names XPATH-ASSIGN-010.
        expected_exception_or_event: none
        forbidden: Treating an imported fixture as the only evidence for a normative row.
        partitions: [complete, missing-evidence]
        dimensions: { authority: XPath-data-model-note, removed_row: XPATH-ASSIGN-010 }
        risk: critical
        target_frameworks_platforms: all-project-targets/platform-independent
        compile_notes: PhaseSixEvidenceHarness unresolved.
      - case_id: PHASE6-CRITICAL-COVERAGE-CASE-001
        requirement_ids: [PHASE6-CRITICAL-COVERAGE]
        description: Critical parser, queue, XPath, persistence, limit, and security branches require exactly 100 percent evidence coverage.
        input: Critical branch artifact at 100 percent, then the same artifact at 99.99 percent.
        stimulus: Evaluate critical coverage gate.
        expected: 100 percent passes; 99.99 percent fails with the uncovered branch identifier.
        expected_exception_or_event: none
        forbidden: Rounding 99.99 percent to a passing critical result.
        partitions: [threshold-boundary]
        dimensions: { threshold: 100-percent }
        risk: critical
        target_frameworks_platforms: all-project-targets/platform-independent
        compile_notes: PhaseSixEvidenceHarness unresolved.
      - case_id: PHASE6-OTHER-COVERAGE-CASE-001
        requirement_ids: [PHASE6-OTHER-COVERAGE]
        description: Non-critical in-scope branches pass at 95 percent and fail below 95 percent without weakening critical requirements.
        input: Non-critical coverage artifacts at 95.00 and 94.99 percent.
        stimulus: Evaluate non-critical coverage gate.
        expected: 95.00 percent passes; 94.99 percent fails.
        expected_exception_or_event: none
        forbidden: Applying the non-critical threshold to a critical branch.
        partitions: [threshold-boundary]
        dimensions: { threshold: 95-percent }
        risk: high
        target_frameworks_platforms: all-project-targets/platform-independent
        compile_notes: PhaseSixEvidenceHarness unresolved.
      - case_id: PHASE6-CRITICAL-MUTATION-CASE-001
        requirement_ids: [PHASE6-CRITICAL-MUTATION]
        description: A surviving critical semantic mutant prevents sign-off even when aggregate mutation score appears high.
        input: Mutation artifact with all critical mutants killed, then one surviving XPath effective-boolean mutant.
        stimulus: Evaluate critical mutation gate.
        expected: All-killed artifact passes; surviving critical mutant fails and names its mutation identifier.
        expected_exception_or_event: none
        forbidden: Aggregate score masking a surviving critical mutant.
        partitions: [complete, authority-divergence]
        dimensions: { target: XPath-effective-boolean }
        risk: critical
        target_frameworks_platforms: all-project-targets/platform-independent
        compile_notes: PhaseSixEvidenceHarness unresolved.
      - case_id: PHASE6-RESOURCE-GATE-CASE-001
        requirement_ids: [PHASE6-RESOURCE-GATE]
        description: A retained per-session timer rejects final evidence even when every semantic result passed.
        input: Complete campaign artifacts with one net8 session retaining exactly one timer after deterministic teardown.
        stimulus: Evaluate the resource gate.
        expected: The resource gate fails and names the net8 retained timer.
        expected_exception_or_event: none
        forbidden: Passing because semantic coverage is complete; accepting a nonzero owned-resource ledger.
        partitions: [cleanup, retained-resource]
        dimensions: { lane: net8, residue: timer-count-1 }
        risk: critical
        target_frameworks_platforms: all-project-targets/all-supported-platform-lanes
        compile_notes: PhaseSixEvidenceHarness unresolved.
      - case_id: PHASE6-PLATFORM-GATE-CASE-001
        requirement_ids: [PHASE6-PLATFORM-GATE]
        description: A missing supported framework/platform lane rejects final evidence even when every reported lane passed.
        input: Complete artifacts except the required net8 supported-Unix lane result.
        stimulus: Evaluate the platform gate.
        expected: The platform gate fails and names the missing net8 supported-Unix lane.
        expected_exception_or_event: none
        forbidden: Treating another successful lane as evidence for the absent lane.
        partitions: [missing-evidence, platform]
        dimensions: { missing-lane: net8-supported-Unix }
        risk: critical
        target_frameworks_platforms: all-project-targets/all-supported-platform-lanes
        compile_notes: PhaseSixEvidenceHarness unresolved.
      - case_id: PHASE6-COMPATIBILITY-GATE-CASE-001
        requirement_ids: [PHASE6-COMPATIBILITY-GATE]
        description: An authority-divergent compatibility result blocks final evidence until it is explicitly triaged.
        input: Complete lane artifacts with one Windows result that differs from the authority oracle and has no reviewed disposition.
        stimulus: Evaluate the compatibility gate.
        expected: The compatibility gate fails and names the untriaged Windows authority divergence.
        expected_exception_or_event: none
        forbidden: Passing an untriaged divergence because other lanes match.
        partitions: [authority-divergence, compatibility]
        dimensions: { lane: Windows; disposition: absent }
        risk: critical
        target_frameworks_platforms: all-project-targets/all-supported-platform-lanes
        compile_notes: PhaseSixEvidenceHarness unresolved.
    */
    public static IEnumerable<object[]> Cases() =>
    [
        new object[] { new PhaseSixGateCase("PHASE6-NORMATIVE-SCXML-CASE-001", "pass complete ledger; fail missing SCXML-PARSE-001", "No aggregate count may mask missing SCXML evidence.") },
        new object[] { new PhaseSixGateCase("PHASE6-NORMATIVE-XPATH-CASE-001", "pass complete ledger; fail missing XPATH-ASSIGN-010", "No imported fixture may be the sole normative witness.") },
        new object[] { new PhaseSixGateCase("PHASE6-CRITICAL-COVERAGE-CASE-001", "pass 100.00%; fail 99.99%", "No rounding may pass below 100% critical coverage.") },
        new object[] { new PhaseSixGateCase("PHASE6-OTHER-COVERAGE-CASE-001", "pass 95.00%; fail 94.99%", "No non-critical threshold may apply to critical code.") },
        new object[] { new PhaseSixGateCase("PHASE6-CRITICAL-MUTATION-CASE-001", "pass all critical mutants killed; fail one survivor", "No aggregate mutation score may mask a critical survivor.") },
        new object[] { new PhaseSixGateCase("PHASE6-RESOURCE-GATE-CASE-001", "fail retained timer on net8", "No nonzero owned-resource ledger may pass.") },
        new object[] { new PhaseSixGateCase("PHASE6-PLATFORM-GATE-CASE-001", "fail missing net8 supported-Unix lane", "No successful lane may substitute for an absent lane.") },
        new object[] { new PhaseSixGateCase("PHASE6-COMPATIBILITY-GATE-CASE-001", "fail untriaged Windows authority divergence", "No untriaged authority divergence may pass.") }
    ];

    public sealed record PhaseSixGateCase(string CaseId, string ExpectedDecision, string Forbidden);
}
