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
public sealed class Phase3HighRiskPartitionGeneratedTests
{
	/*
	TEST-METADATA
	test_id: PHASE3-HIGH-RISK-PARTITIONS-001
	requirement_ids:
	  - DM-VALUE-001..006
	  - DM-LIST-001..005
	  - DM-CONV-001..003
	  - DM-HANDLER-001..004
	  - DM-NULL-001..005
	  - DM-RUNTIME-001..007
	  - XPATH-TREE-001..013
	  - XPATH-COMP-001..007
	  - XPATH-EXPR-001..014
	  - XPATH-SYS-001..007
	  - XPATH-ASSIGN-001..013
	  - XPATH-FOREACH-001..006
	  - XPATH-CONTENT-001..005
	  - XPATH-PROBE-001..012
	  - DM-PROP-001..006
	title: High-risk data-model and XPath partitions preserve exact results and rollback
	description: This supplemental matrix expands the broad Phase 3 requirement records into discriminating authority-derived witnesses. It proves exact conversion, XPath, system-variable, assignment, iteration, content, and differential outcomes, including the partitions where a silent coercion, partial mutation, or ambient-context leak would otherwise escape a one-row requirement table.
	authority:
	  source: W3C XPath 1.0, W3C SCXML XPath Data Model Note, exhaustive plan document 02
	  section: Document 02 sections 1 through 13
	  citation_or_rule: XPath effective-boolean conversion, node-set assignment atomicity, system-variable immutability, and foreach ordering are authority-defined rather than implementation-defined.
	phase: 3
	feature: xpath-high-risk-partitions
	target_components:
	  - XPathDataModelHandler
	  - DataModelValue
	  - XPathEvaluator
	  - XPathAssignAction
	test_kind: declarative-contract-and-differential
	oracle_type: independent-XPath-result-canonical-tree-and-resource-ledger
	risk: critical
	priority: critical
	construction_routes:
	  - xml-tree
	  - scxml-text
	  - public-object-model
	data_models:
	  - xpath
	  - runtime
	  - null
	target_frameworks:
	  - all-project-targets
	platforms:
	  - platform-independent
	partitions:
	  - positive
	  - negative
	  - boundary
	  - malformed
	  - cancellation
	  - concurrency
	  - cleanup
	dimensions:
	  value_kind: case-declared
	  tree_topology: case-declared
	  action: case-declared
	preconditions:
	  - independent XPath 1.0 oracle and deterministic data-model harness are available
	dependencies:
	  - GeneratedDataModelXPathHarness
	  - XPathReferenceOracle
	  - CompleteXmlTreeSnapshotComparer
	arrange: Construct the exact case-declared XML/data fixture and capture pre-operation tree, variable bindings, event trace, and owned-resource snapshot.
	stimulus: Execute the single declared expression, assignment, foreach body, conversion, or lifecycle callback under deterministic scheduling.
	expected:
	  - The exact case-declared XPath result, error event, canonical XML snapshot, variable scope, and resource ledger result.
	expected_exception_or_event: Case-declared diagnostic or error.execution; otherwise none.
	forbidden:
	  - Partial mutation, Boolean parsing of XPath effective-boolean values, zero-based XPath foreach index, writable system variables, cross-session context, or retained owned resource.
	edge_cases:
	  - NaN, nonempty false-looking strings, multi-node sets, overlapping targets, namespace nodes, cancellation, and concurrent first use.
	determinism:
	  clock: virtual-or-not-applicable
	  scheduling: explicit deterministic schedule
	  timeout_or_step_bound: 100 operations
	isolation:
	  parallel_safe: true
	  shared_state: none
	cleanup:
	  - Dispose the case scope and prove zero pending callback, iterator, stream, or ambient runtime context.
	resource_risk: high
	tier: fast
	tags:
	  - Exhaustive
	  - XPath
	  - DataModel
	related_tests:
	  - PHASE3-DM-XPATH-MATRIX-001
	known_issue: none
	compile_notes: GeneratedDataModelXPathHarness, XPathReferenceOracle, and CompleteXmlTreeSnapshotComparer are intentionally unresolved test-side helpers.
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	[DynamicData(nameof(Cases))]
	public async Task Phase3_high_risk_partition_has_exact_authority_oracle(Phase3PartitionCase testCase)
	{
		// Arrange
		await using var scope = await GeneratedDataModelXPathHarness.CreateAsync(testCase);
		var before = await scope.CaptureCompleteSnapshotAsync();

		// Act
		var result = await scope.ExecuteAsync(testCase);

		// Assert
		await scope.AssertExactOutcomeAsync(testCase.Expected, result);
		await scope.AssertForbiddenEffectsAbsentAsync(testCase.Forbidden, before);
		await scope.AssertCleanupAsync();
	}

	/*
	CASE-METADATA
	cases:
	  - case_id: DM-VALUE-001-CASE-101
		requirement_ids: [DM-VALUE-001, DM-VALUE-002, DM-CONV-001]
		description: Conversion round trip preserves null, undefined, string, Boolean, integer, fractional, XML, and metadata-distinguished values without collapsing null into undefined.
		input: One value of each supported kind plus null and undefined sentinels.
		stimulus: Convert through public value, XML, and persistence routes.
		expected: Each supported value preserves its declared kind/value/order; null and undefined remain distinct.
		expected_exception_or_event: Unsupported cyclic conversion diagnostic where applicable.
		forbidden: Silent stringification or null/undefined aliasing.
	  - case_id: DM-LIST-001-CASE-101
		requirement_ids: [DM-LIST-001, DM-LIST-004, DM-LIST-005]
		description: A keyed nested list retains insertion order, duplicate-key contract, access metadata, and deep-copy isolation across mutation.
		input: Nested keyed list with metadata and a source child mutated after copy.
		stimulus: Copy then mutate source and destination independently.
		expected: Ordered snapshots differ only at the mutated side and no child identity is shared.
		expected_exception_or_event: none
		forbidden: Reordered entries, metadata loss, or aliasing.
	  - case_id: DM-RUNTIME-001-CASE-101
		requirement_ids: [DM-HANDLER-001..004, DM-NULL-001..005, DM-RUNTIME-001..007]
		description: Two concurrently initialized sessions select their declared data-model handlers and isolate ambient callbacks while the null data model permits only In().
		input: XPath, runtime, and null sessions with interleaved callbacks.
		stimulus: Initialize and evaluate the permitted and prohibited operations concurrently.
		expected: Each session observes only its own variables/context; null In() succeeds and other evaluation APIs reject.
		expected_exception_or_event: Case-declared unsupported-operation diagnostic.
		forbidden: Handler fallback, cross-session AsyncLocal state, or null-model variable access.
	  - case_id: XPATH-TREE-001-CASE-101
		requirement_ids: [XPATH-TREE-001..013, XPATH-COMP-001..007]
		description: Namespace shadowing is captured at lexical compilation time and a later redeclaration cannot alter a compiled expression's QName binding.
		input: Ancestor prefix binding, lexical child shadow binding, and post-compile redeclaration.
		stimulus: Compile then evaluate before and after redeclaration.
		expected: Evaluation selects only nodes in the lexical child namespace in both evaluations.
		expected_exception_or_event: none
		forbidden: Late namespace lookup, unknown-prefix acceptance, or cross-machine namespace reuse.
	  - case_id: XPATH-EXPR-014-CASE-101
		requirement_ids: [XPATH-EXPR-001..014, XPATH-PROBE-001, XPATH-PROBE-002]
		description: Effective boolean value follows XPath rather than lexical Boolean parsing for nonempty false-looking string/node-set and NaN.
		input: "false", "0", empty string, nonempty node set containing "false", empty node set, zero, and NaN.
		stimulus: Evaluate each as an SCXML XPath condition.
		expected: Nonempty string/node set are true; empty string/set, signed zero, and NaN are false.
		expected_exception_or_event: none
		forbidden: Parsing text false/0 as false or treating NaN as true.
	  - case_id: XPATH-SYS-004-CASE-101
		requirement_ids: [XPATH-SYS-001..007, XPATH-PROBE-003]
		description: Reserved system-variable roots and descendants are immutable and In() converts a node-set argument through the required string semantics.
		input: Active/inactive ID node set and assignment locations rooted at _event and _sessionid.
		stimulus: Invoke In() and attempt replacechildren/delete/addattribute assignments.
		expected: In() uses the declared conversion result; every system-variable mutation raises error.execution with identical pre- / post-snapshot.
		expected_exception_or_event: error.execution for each mutation.
		forbidden: All-node extension for In(), partial system mutation, or writable descendant.
	  - case_id: XPATH-ASSIGN-010-CASE-101
		requirement_ids: [XPATH-ASSIGN-001..013, XPATH-PROBE-004, XPATH-PROBE-005, XPATH-PROBE-011]
		description: Every assignment action evaluates value once and rolls back all selected targets when the middle target fails.
		input: Three selected targets, counting value function, and injected mutation failure at target two for each of eight actions.
		stimulus: Execute assignment once per action.
		expected: Value function count is one; error.execution occurs; canonical tree equals pre-operation snapshot.
		expected_exception_or_event: error.execution.
		forbidden: First-target mutation, a second value evaluation, stale navigator path, or silently ignored empty/non-node-set location.
	  - case_id: XPATH-ASSIGN-010-CASE-102
		requirement_ids: [XPATH-ASSIGN-010]
		description: Delete assignment rolls back the first deletion when the second selected node rejects deletion because it is read-only.
		input: XML data tree with sibling targets a and b; a writable, b marked read-only; delete action location=/data/*.
		stimulus: Execute the delete action once with the mutation failure hook at b.
		expected: error.execution is queued and canonical post-tree equals the complete pre-operation tree with both a and b present in original order.
		expected_exception_or_event: error.execution
		forbidden: a removed while b remains; a committed partial tree; an empty successful result; retained mutation transaction.
		partitions: [negative,delete-action,multi-target,atomicity,read-only]
		dimensions: { action: delete, target_count: 2, failure_target: second }
		risk: critical
		target_frameworks_platforms: all-project-targets/platform-independent
		compile_notes: GeneratedDataModelXPathHarness and CompleteXmlTreeSnapshotComparer are unresolved test-side helpers.
	  - case_id: XPATH-FOREACH-001-CASE-101
		requirement_ids: [XPATH-FOREACH-001..006, XPATH-PROBE-009]
		description: XPath foreach snapshots a node set in document order, binds a shallow item copy, and counts index from one while nested scopes restore exact outer bindings.
		input: Three document-ordered nodes, nested loop with colliding item/index names, and inner cancellation.
		stimulus: Execute outer then inner foreach and cancel inner body.
		expected: Outer indices are 1,2,3; item mutations do not mutate source; outer bindings are restored after cancellation.
		expected_exception_or_event: cancellation at inner declared hook.
		forbidden: Zero-based index, skipped/duplicated iteration, source mutation via item, or leaked inner binding.
	  - case_id: XPATH-CONTENT-004-CASE-101
		requirement_ids: [XPATH-CONTENT-001..005, XPATH-PROBE-006, XPATH-PROBE-007, XPATH-PROBE-012]
		description: Failed inline/external XML content has bounded exactly-once acquisition/parse behavior and successful mixed content preserves text-element-text order.
		input: Text-element-text XML, malformed chunked external stream, and repeated execution after failure.
		stimulus: Parse successful fixture then execute malformed fixture twice.
		expected: Successful value preserves all siblings; malformed fixture returns the same controlled failure with one bounded cached parse/acquisition result.
		expected_exception_or_event: controlled XML diagnostic for malformed fixture.
		forbidden: Truncated mixed content, repeated unbounded parse/logging, retained stream, or culture-dependent decimal serialization.
	  - case_id: DM-PROP-003-CASE-101
		requirement_ids: [DM-PROP-001..006]
		description: Generated valid and invalid small mutation tuples match an independent functional tree oracle, with seeds retained for shrinking.
		input: Bounded canonical trees, actions, locations, values, and one invalid tuple per action.
		stimulus: Apply each tuple to Xtate and independent oracle.
		expected: Valid canonical snapshots are equal; invalid tuples reject and leave Xtate snapshot unchanged with reproducible seed.
		expected_exception_or_event: case-declared diagnostic for invalid tuple.
		forbidden: Differential mismatch, partial mutation, missing seed, crash, hang, or cross-session state.
	*/
	public static IEnumerable<object[]> Cases() =>
	[
		[new Phase3PartitionCase(CaseId: "DM-VALUE-001-CASE-101", Expected: "Exact kind-preserving round trip", Forbidden: "No null/undefined aliasing or silent stringification.")],
		[new Phase3PartitionCase(CaseId: "DM-LIST-001-CASE-101", Expected: "Ordered deep copy with metadata", Forbidden: "No shared child identity or reordered entries.")],
		[new Phase3PartitionCase(CaseId: "DM-RUNTIME-001-CASE-101", Expected: "Handler and ambient isolation", Forbidden: "No cross-session context or null-model evaluation.")],
		[new Phase3PartitionCase(CaseId: "XPATH-TREE-001-CASE-101", Expected: "Lexical namespace capture", Forbidden: "No late namespace lookup or context reuse.")],
		[new Phase3PartitionCase(CaseId: "XPATH-EXPR-014-CASE-101", Expected: "XPath effective boolean values", Forbidden: "No lexical Boolean parsing or NaN truthiness.")],
		[new Phase3PartitionCase(CaseId: "XPATH-SYS-004-CASE-101", Expected: "System variable immutability and In conversion", Forbidden: "No partial reserved-variable mutation.")],
		[new Phase3PartitionCase(CaseId: "XPATH-ASSIGN-010-CASE-101", Expected: "Eight-action rollback", Forbidden: "No partial target mutation or repeated value evaluation.")],
		[
			new Phase3PartitionCase(
				CaseId: "XPATH-ASSIGN-010-CASE-102", Expected: "Delete rollback preserves both targets after the second target rejects mutation",
				Forbidden: "No first-target deletion, partial tree, or retained transaction.")
		],
		[new Phase3PartitionCase(CaseId: "XPATH-FOREACH-001-CASE-101", Expected: "One-based shallow-copy iteration", Forbidden: "No zero-based index or scope leak.")],
		[
			new Phase3PartitionCase(CaseId: "XPATH-CONTENT-004-CASE-101", Expected: "Content fidelity and cached failure", Forbidden: "No truncation, repeated acquisition, or retained stream.")
		],
		[
			new Phase3PartitionCase(CaseId: "DM-PROP-003-CASE-101", Expected: "Mutation differential with reproducible seed", Forbidden: "No mismatch, partial mutation, or missing reproducer.")
		]
	];

	public sealed record Phase3PartitionCase(string CaseId, string Expected, string Forbidden);
}
