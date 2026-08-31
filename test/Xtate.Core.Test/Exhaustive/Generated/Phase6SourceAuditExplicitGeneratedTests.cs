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
public sealed class Phase6SourceAuditExplicitGeneratedTests
{
	private static readonly SourceAuditCase[] ExplicitCases =
	[
		new(
			CaseId: "PHASE6-METADATA-001-CASE-001", RequirementIds: "PHASE6-METADATA-001", Description: "A TestMethod without immediately associated TEST-METADATA is rejected.",
			Source: "[TestMethod] public void Missing() { Assert.IsTrue(true); }", ["TEST-METADATA-MISSING"]),
		new(
			CaseId: "PHASE6-ID-001-CASE-001", RequirementIds: "PHASE6-ID-001", Description: "Two otherwise complete methods sharing one test_id are rejected as a duplicate.",
			Source: "/* TEST-METADATA test_id: DUP */ [TestMethod] void A(){} /* TEST-METADATA test_id: DUP */ [TestMethod] void B(){}", ["TEST-ID-DUPLICATE"]),
		new(
			CaseId: "PHASE6-CASE-001-CASE-001", RequirementIds: "PHASE6-CASE-001", Description: "A DynamicData case factory derived only from GeneratedRequirementCase.For is rejected.",
			Source: "[TestMethod][DynamicData(nameof(Cases))] void M(Case c){} IEnumerable<object[]> Cases()=>Ids.Select(id=>new object[]{GeneratedRequirementCase.For(id)});",
			["CASE-ID-ONLY-FACTORY"]),
		new(
			CaseId: "PHASE6-ORACLE-001-CASE-001", RequirementIds: "PHASE6-ORACLE-001",
			Description: "Metadata whose expected result says it matches a plan instead of stating an observable result is rejected.",
			Source: "/* TEST-METADATA expected: Matches the document oracle. forbidden: none */ [TestMethod] void M(){ Assert.IsTrue(true); }", ["METADATA-VAGUE-EXPECTED"]),
		new(
			CaseId: "PHASE6-BODY-001-CASE-001", RequirementIds: "PHASE6-BODY-001", Description: "A metadata-bearing method with only Arrange/Act/Assert comments is rejected as an empty test body.",
			Source: "/* TEST-METADATA test_id: BODY */ [TestMethod] void M(){ // Arrange // Act // Assert }", ["TEST-BODY-EMPTY"]),
		new(
			CaseId: "PHASE6-SCOPE-001-CASE-001", RequirementIds: "PHASE6-SCOPE-001",
			Description: "A generated source fixture in the permitted Exhaustive directory with complete metadata and explicit case record produces no diagnostics.",
			Source:
			"/* TEST-METADATA test_id: OK; expected: result is final; forbidden: trap state */ [TestMethod] void M(){ var result=Run(); Assert.AreEqual(Final,result); } record Case(string CaseId,string Expected);",
			[])
	];

	/*
	TEST-METADATA
	test_id: PHASE6-SOURCE-AUDIT-EXPLICIT-001
	requirement_ids: [PHASE6-METADATA-001,PHASE6-ID-001,PHASE6-CASE-001,PHASE6-ORACLE-001,PHASE6-BODY-001,PHASE6-SCOPE-001]
	title: Exhaustive test source completion gates have literal audit fixtures
	description: Each case passes a deliberately malformed or conforming C# source fixture to the source auditor and asserts the exact diagnostic set, preventing metadata completion from being inferred from broad ranges, comments, or identifiers alone.
	authority: { source: exhaustive source-generation runbook and remaining-work backlog, section: metadata contract and code-generation completion gates, citation_or_rule: all mandatory metadata, case, body, uniqueness, oracle, and scope rules are literal source invariants }
	phase: 6
	feature: source-completeness-audit
	target_components: [ExhaustiveSourceAuditor]
	test_kind: source-analysis
	oracle_type: exact-diagnostic-set
	risk: critical
	priority: critical
	construction_routes: [csharp-source-text]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,missing-metadata,duplicate-id,generic-shell,empty-body,scope]
	dimensions: { fixtures: six-literal-CSharp-source-snippets }
	preconditions: [auditor receives only the stated in-memory source files]
	dependencies: [ExhaustiveSourceAuditor]
	arrange: Load the literal source fixture and no unrelated source.
	stimulus: Run the metadata/case/body/scope audit once.
	expected: [the record's exact stable diagnostic IDs]
	expected_exception_or_event: none
	forbidden: [suppressed malformed source, extra unrelated diagnostic, runtime compilation or execution]
	edge_cases: [DataTestMethod, duplicate case ID, comment-only Arrange/Act/Assert]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one source scan per fixture' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [discard in-memory source and diagnostic collection]
	resource_risk: none
	tier: fast
	tags: [Exhaustive,SourceAudit,Phase6]
	related_tests: []
	known_issue: none
	compile_notes: ExhaustiveSourceAuditor is planned test-side source-analysis infrastructure; this suite must not invoke a compiler.
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	[DynamicData(nameof(Cases))]
	public void Source_audit_case_reports_exact_completion_gate_result(SourceAuditCase testCase)
	{
		// Arrange
		var auditor = new ExhaustiveSourceAuditor();
		var source = SourceAuditFixture.Create(testCase.Source);

		// Act
		var diagnostics = auditor.Audit(source);

		// Assert
		CollectionAssert.AreEquivalent(testCase.ExpectedDiagnostics, diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
		CollectionAssert.DoesNotContain(diagnostics.Select(diagnostic => diagnostic.Id).ToArray(), element: "RUNTIME-EXECUTION-ATTEMPT");
	}

	public static IEnumerable<object[]> Cases() => ExplicitCases.Select(testCase => new object[] { testCase });

	public sealed record SourceAuditCase(
		string CaseId,
		string RequirementIds,
		string Description,
		string Source,
		string[] ExpectedDiagnostics);
}
