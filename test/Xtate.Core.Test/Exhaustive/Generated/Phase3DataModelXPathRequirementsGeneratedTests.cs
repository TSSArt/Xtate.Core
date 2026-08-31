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
public sealed class Phase3DataModelXPathRequirementsGeneratedTests
{
	private static readonly ExplicitDataModelXPathCase[] ExplicitCases =
	[
		new(
			CaseId: "DM-VALUE-001-CASE-001", RequirementIds: "DM-VALUE-001", Description: "Undefined is distinct from a defined null value when a data-model variable is read.",
			Fixture: "Variables u=undefined and n=null in one null data model.", Stimulus: "Read u and n through the public value API.",
			ExpectedResult: "u reports Undefined and n reports Null; neither read creates a third variable.", ExpectedExceptionOrEvent: "none", ExpectedTree: "No XML tree applies.",
			ForbiddenResults: "Coercing undefined to null; materializing an empty-string variable.", Partitions: "positive|undefined|null", Dimensions: "value-kind=undefined/null", Risk: "high",
			TargetFrameworksPlatforms: "all-project-targets/platform-independent", CompileNotes: "ExplicitDataModelXPathHarness is unresolved test infrastructure."),
		new(
			CaseId: "DM-CONV-002-CASE-001", RequirementIds: "DM-CONV-002", Description: "XPath number conversion of whitespace-only text produces NaN rather than zero or an empty-string success.",
			Fixture: "XPath context node text is three ASCII spaces.", Stimulus: "Evaluate number(.) once.",
			ExpectedResult: "The scalar result is numeric NaN and no mutation or error.execution event occurs.", ExpectedExceptionOrEvent: "none", ExpectedTree: "Input tree is unchanged.",
			ForbiddenResults: "Returning zero; trimming into a valid number; changing the text node.", Partitions: "lexical|conversion|whitespace|NaN",
			Dimensions: "source=text; lexical-form=whitespace-only", Risk: "high", TargetFrameworksPlatforms: "all-project-targets/platform-independent",
			CompileNotes: "IndependentXPathOracle is unresolved test infrastructure."),
		new(
			CaseId: "DM-NULL-004-CASE-001", RequirementIds: "DM-NULL-004", Description: "A null data model rejects an assign action without silently accepting or partially creating the target.",
			Fixture: "SCXML uses datamodel='null' and executes assign location='x' expr='1'.", Stimulus: "Run the macrostep containing the assign action.",
			ExpectedResult: "The machine emits error.execution, has no variable x, and continues only according to the declared error policy.", ExpectedExceptionOrEvent: "error.execution",
			ExpectedTree: "No XML tree applies.", ForbiddenResults: "Successful assignment; a created x variable; an unreported no-op.",
			Partitions: "negative|null-model|assignment|no-effect-on-error", Dimensions: "data-model=null; action=assign", Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/platform-independent", CompileNotes: "Null data-model driver and event recorder are unresolved test-side helpers."),
		new(
			CaseId: "DM-RUNTIME-003-CASE-001", RequirementIds: "DM-RUNTIME-003",
			Description: "A runtime data-model callback receives the current event payload but cannot expose it to an unrelated session.",
			Fixture: "Two sessions each invoke a callback; session A receives event payload 'A' and session B receives 'B'.",
			Stimulus: "Dispatch both events in deterministic A-then-B order and inspect callback observations.",
			ExpectedResult: "A observes only 'A', B observes only 'B', and each callback scope is cleared on completion.", ExpectedExceptionOrEvent: "none", ExpectedTree: "No XML tree applies.",
			ForbiddenResults: "B observing A; shared ambient payload; a live callback scope after completion.", Partitions: "positive|cross-session-isolation|event-payload|cleanup",
			Dimensions: "sessions=2; schedule=A-then-B", Risk: "critical", TargetFrameworksPlatforms: "all-project-targets/platform-independent",
			CompileNotes: "Runtime callback probe and resource ledger are unresolved test-side helpers."),
		new(
			CaseId: "XPATH-TREE-003-CASE-001", RequirementIds: "XPATH-TREE-003", Description: "The child axis returns children in document order and excludes attributes.",
			Fixture: "XML <r z='9'><a/><b/></r> with r as context node.", Stimulus: "Evaluate child::* and child::@* once.",
			ExpectedResult: "child::* yields a then b; child::@* yields an empty node-set.", ExpectedExceptionOrEvent: "none", ExpectedTree: "Tree remains <r z='9'><a/><b/></r>.",
			ForbiddenResults: "Returning attribute z through child::*; reverse order; mutation.", Partitions: "positive|axis|document-order",
			Dimensions: "axis=child; child-count=2; attribute-count=1", Risk: "high", TargetFrameworksPlatforms: "all-project-targets/platform-independent",
			CompileNotes: "IndependentXPathOracle is unresolved test infrastructure."),
		new(
			CaseId: "XPATH-COMP-002-CASE-001", RequirementIds: "XPATH-COMP-002", Description: "An unterminated string literal fails compilation before any expression evaluation or state mutation.",
			Fixture: "Expression concat('unterminated) attached to a transition condition.", Stimulus: "Compile the model once.",
			ExpectedResult: "Compilation returns the expression diagnostic owned by that condition and no executable model is produced.", ExpectedExceptionOrEvent: "XPathSyntaxException",
			ExpectedTree: "No source tree mutation occurs.", ForbiddenResults: "Evaluating a partial expression; error.execution after state entry; a partially compiled model.",
			Partitions: "negative|compile|lexical|unterminated-string", Dimensions: "expression=unterminated-literal; phase=compile", Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/platform-independent", CompileNotes: "XPathSyntaxException and compiled-model probe are unresolved test-side helpers."),
		new(
			CaseId: "XPATH-ASSIGN-008-CASE-001", RequirementIds: "XPATH-ASSIGN-008", Description: "An empty XPath location causes error.execution and leaves the complete data tree unchanged.",
			Fixture: "XML <data><x>before</x></data>; assign location=/data/missing expr='after'.", Stimulus: "Execute the assign action once.",
			ExpectedResult: "error.execution is queued and canonical post-tree equals canonical pre-tree.", ExpectedExceptionOrEvent: "error.execution",
			ExpectedTree: "<data><x>before</x></data> remains unchanged.", ForbiddenResults: "Creating missing; changing x; reporting success without an error event.",
			Partitions: "negative|assignment|empty-node-set|atomicity", Dimensions: "target-cardinality=0; action=replacechildren", Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/platform-independent", CompileNotes: "XPath assignment driver and canonical tree comparer are unresolved test-side helpers."),
		new(
			CaseId: "XPATH-FOREACH-001-CASE-001", RequirementIds: "XPATH-FOREACH-001",
			Description: "XPath foreach visits node-set members in document order and binds the one-based index required by its data-model contract.",
			Fixture: "XML <r><i>A</i><i>B</i><i>C</i></r>; foreach array=/r/i item=v index=n appends n:v.", Stimulus: "Execute the foreach body once.",
			ExpectedResult: "Ordered trace is 1:A,2:B,3:C and v/n bindings are absent after loop completion.", ExpectedExceptionOrEvent: "none", ExpectedTree: "Source XML remains ordered A,B,C.",
			ForbiddenResults: "Zero-based first index; reverse iteration; retained loop bindings.", Partitions: "positive|foreach|document-order|scope-cleanup",
			Dimensions: "node-count=3; index-origin=one", Risk: "high", TargetFrameworksPlatforms: "all-project-targets/platform-independent",
			CompileNotes: "XPath foreach driver and ordered trace recorder are unresolved test-side helpers."),
		new(
			CaseId: "DM-PROP-003-CASE-001", RequirementIds: "DM-PROP-003",
			Description: "Concurrent first evaluation of the same compiled XPath expression is isolated per session and does not share mutable evaluation context.",
			Fixture: "Two sessions share one compiled expression /r/v and have distinct XML values A and B.",
			Stimulus: "Open both first evaluations at a deterministic barrier, then release A followed by B.",
			ExpectedResult: "A returns A, B returns B, and both sessions dispose their evaluators after the barrier.", ExpectedExceptionOrEvent: "none",
			ExpectedTree: "Each session tree remains unchanged.", ForbiddenResults: "A returning B; B returning A; shared variable context; retained evaluator after disposal.",
			Partitions: "concurrency|first-use|session-isolation|cleanup", Dimensions: "sessions=2; schedule=barrier-A-B; expression-cache=shared", Risk: "critical",
			TargetFrameworksPlatforms: "all-project-targets/platform-independent", CompileNotes: "Deterministic barrier and evaluator resource ledger are unresolved test-side helpers.")
	];

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
	[TestMethod]
	[DynamicData(nameof(Cases))]
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

	public sealed record ExplicitDataModelXPathCase(
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
