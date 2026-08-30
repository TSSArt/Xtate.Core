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

using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Xtate.Scxml.Services;
using Xtate.StateMachine;

namespace Xtate.Core.Test.Exhaustive.Parsing;

[TestClass]
[TestCategory("Exhaustive.Fast")]
public sealed class ScxmlParserRequirementsTests
{
	private const string ScxmlNamespace = "http://www.w3.org/2005/07/scxml";

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-001-EXISTING-001
	requirement_ids: [SCXML-PARSE-001]
	title: Existing SCXML-PARSE-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-001; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_001_Accepts_exact_scxml_namespace_and_root_element()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"idle\" /></scxml>", baseUri: "urn:exhaustive:parse-001");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: 1, result.Model!.States.Length);
		Assert.AreEqual(expected: "idle", result.Model.States[0].Id!.Value);
	}

	/* TEST-METADATA
	test_id: SCXML-PARSE-001-EXISTING-002; requirement_ids: [SCXML-PARSE-001]; title: Prefix bound to canonical SCXML namespace is accepted; description: A root and child using prefix s bound to the canonical SCXML namespace parse as one idle state, distinguishing namespace-URI recognition from a prefix-only parser; authority: { source: W3C SCXML 1.0, section: 3.2, citation_or_rule: SCXML vocabulary is identified by its namespace URI }; phase: 1; feature: namespace-parsing; target_components: [ScxmlParser]; test_kind: parser-unit; oracle_type: exact-model; risk: high; priority: high; construction_routes: [scxml-text]; data_models: [none]; target_frameworks: [all-project-targets]; platforms: [platform-independent]; partitions: [positive,prefixed-namespace]; dimensions: { prefix: s, namespace: canonical }; preconditions: [well-formed version-1.0 document]; dependencies: [ScxmlParserHarness]; arrange: Parse prefixed root and state XML; stimulus: Parse once; expected: Accepted result with one state id idle; expected_exception_or_event: none; forbidden: Prefix rejection, namespace loss, or wrong state ID; edge_cases: Root and child share prefix; determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 'one parse' }; isolation: { parallel_safe: true, shared_state: none }; cleanup: [parser result has no open reader]; resource_risk: reader retention; tier: fast; tags: [Exhaustive,SCXML,Parsing]; related_tests: [SCXML-PARSE-001-EXISTING-001]; known_issue: none; compile_notes: none; generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_001_Accepts_a_prefix_bound_to_the_exact_scxml_namespace()
	{
		var result = await ScxmlParserHarness.ParseAsync("<s:scxml xmlns:s=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><s:state id=\"idle\" /></s:scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "idle", result.Model!.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-001-EXISTING-003
	requirement_ids: [SCXML-PARSE-001]
	title: Existing SCXML-PARSE-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-001; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_001_Accepts_an_xml_declaration_before_the_scxml_root()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"ready\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "ready", result.Model!.States[0].Id!.Value);
	}

	[TestMethod]
	[DataRow("<scxml version=\"1.0\" />")]
	[DataRow("<scxml xmlns=\"urn:wrong\" version=\"1.0\" />")]
	[DataRow("<SCXML xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" />")]
	[DataRow("<s:scxml xmlns:s=\"urn:lookalike\" version=\"1.0\" />")]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><scxml /></scxml>")]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-PARSE-001-EXISTING-PARAM-001
		description: A root in no namespace is rejected because the SCXML root must use the exact SCXML namespace.
		input: <scxml version="1.0" />
		expected: Parse result Accepted is false.
		forbidden: A usable state machine model.
	  - case_id: SCXML-PARSE-001-EXISTING-PARAM-002
		description: A root in urn:wrong is rejected instead of being treated as SCXML by local-name matching.
		input: <scxml xmlns="urn:wrong" version="1.0" />
		expected: Parse result Accepted is false.
		forbidden: Namespace-agnostic root acceptance.
	  - case_id: SCXML-PARSE-001-EXISTING-PARAM-003
		description: Uppercase SCXML is rejected because XML element local names are case-sensitive.
		input: <SCXML xmlns="http://www.w3.org/2005/07/scxml" version="1.0" />
		expected: Parse result Accepted is false.
		forbidden: Case-insensitive root acceptance.
	  - case_id: SCXML-PARSE-001-EXISTING-PARAM-004
		description: A prefixed root in the unrelated urn:lookalike namespace is rejected.
		input: <s:scxml xmlns:s="urn:lookalike" version="1.0" />
		expected: Parse result Accepted is false.
		forbidden: Acceptance based solely on the scxml local name.
	  - case_id: SCXML-PARSE-001-EXISTING-PARAM-005
		description: A nested SCXML element does not make an otherwise valid document root configuration acceptable.
		input: <scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0"><scxml /></scxml>
		expected: Parse result Accepted is false.
		forbidden: A model containing a nested root element as a state child.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-PARSE-001-EXISTING-PARAM-001
	requirement_ids: [SCXML-PARSE-001]
	title: Reject roots that do not have the exact SCXML qualified name
	description: This table proves that root recognition checks both the exact SCXML namespace and the case-sensitive local name. Incorrect behavior is observable as Accepted=true for an XML document that cannot form an SCXML state machine.
	authority: { source: W3C SCXML 1.0, section: 3.2 The scxml Element, citation_or_rule: The document element is scxml in the SCXML namespace. }
	phase: 1
	feature: scxml-root-recognition
	target_components: [ScxmlParser]
	test_kind: unit
	oracle_type: exact-row-result
	risk: high
	priority: high
	construction_routes: [scxml-text]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [namespace-negative,local-name-negative,nested-root-negative]
	dimensions: { namespace: absent/wrong/lookalike/correct, local_name: lower/upper, root_topology: direct/nested }
	preconditions: [well-formed XML text]
	dependencies: [ScxmlParserHarness]
	arrange: Construct the exact XML document from the selected DataRow and assign a stable base URI.
	stimulus: Parse the document once through ScxmlParserHarness.ParseAsync.
	expected: Each row returns Accepted=false and no usable model.
	expected_exception_or_event: parser diagnostic or rejection result; no runtime event
	forbidden: [Accepted=true,usable-model,namespace-agnostic-or-case-insensitive-recognition]
	edge_cases: [empty default namespace,wrong default namespace,prefixed lookalike,uppercase local name,nested scxml]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: 'one parse operation' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [parser result retains no open reader or stream]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing, Parameterized]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	public async Task SCXML_PARSE_001_Rejects_wrong_root_namespace_or_name(string document)
	{
		var result = await ScxmlParserHarness.ParseAsync(document, baseUri: "urn:exhaustive:parse-001-invalid");
		Assert.IsFalse(result.Accepted, Describe(result));
		Assert.IsNull(result.Model, message: "A rejected root name or namespace must not expose a usable SCXML model.");
	}

	[TestMethod]
	[DataRow("1.0", true)]
	[DataRow("", false)]
	[DataRow(" 1.0", false)]
	[DataRow("1.0 ", false)]
	[DataRow("1", false)]
	[DataRow("1.00", false)]
	[DataRow("1.0.0", false)]
	[DataRow("1.0\" version=\"1.0", false)]
	[DataRow("１.０", false)]
	[DataRow("1,0", false)]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-PARSE-002-EXISTING-PARAM-002-ROWS
		description: Each declared DataRow is an independently reported lexical or configuration partition for SCXML-PARSE-002.
		partition: parameterized-existing
		input: The exact DataRow arguments immediately above this method.
		expected: Each row satisfies the method's explicit expected-result assertion.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-PARSE-002-EXISTING-PARAM-002
	requirement_ids: [SCXML-PARSE-002]
	title: Existing parameterized SCXML-PARSE-002 authority witness
	description: The existing concrete table is retained and each row distinguishes the authority-required acceptance or rejection result asserted by this method.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived parameter partition }
	phase: 1
	feature: phase-1-existing-parameterized-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-row-result
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [parameterized-existing]
	dimensions: { input: declared-DataRow }
	preconditions: [existing DataRow fixture]
	dependencies: [existing exhaustive harness]
	arrange: Use the exact DataRow input.
	stimulus: Invoke the existing parser, reader, validator, or runtime operation.
	expected: The concrete assertion in this existing method for that row.
	expected_exception_or_event: row-specific diagnostic or none
	forbidden: [result opposite to the asserted row expectation]
	edge_cases: [all declared rows]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing, Parameterized]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	public async Task SCXML_PARSE_002_Enforces_the_exact_required_version(string version, bool accepted)
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"{version}\" />");
		Assert.AreEqual(accepted, result.Accepted, Describe(result));
	}

	[TestMethod]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-PARSE-002-EXISTING-PARAM-003-ROWS
		description: Each declared DataRow is an independently reported lexical or configuration partition for SCXML-PARSE-002.
		partition: parameterized-existing
		input: The exact DataRow arguments immediately above this method.
		expected: Each row satisfies the method's explicit expected-result assertion.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-PARSE-002-EXISTING-PARAM-003
	requirement_ids: [SCXML-PARSE-002]
	title: Existing parameterized SCXML-PARSE-002 authority witness
	description: The existing concrete table is retained and each row distinguishes the authority-required acceptance or rejection result asserted by this method.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived parameter partition }
	phase: 1
	feature: phase-1-existing-parameterized-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-row-result
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [parameterized-existing]
	dimensions: { input: declared-DataRow }
	preconditions: [existing DataRow fixture]
	dependencies: [existing exhaustive harness]
	arrange: Use the exact DataRow input.
	stimulus: Invoke the existing parser, reader, validator, or runtime operation.
	expected: The concrete assertion in this existing method for that row.
	expected_exception_or_event: row-specific diagnostic or none
	forbidden: [result opposite to the asserted row expectation]
	edge_cases: [all declared rows]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing, Parameterized]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	public async Task SCXML_PARSE_002_Rejects_missing_required_version()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" />");
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-002-EXISTING-003
	requirement_ids: [SCXML-PARSE-002]
	title: Existing SCXML-PARSE-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-002; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_002_Does_not_treat_a_foreign_version_attribute_as_the_required_version()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" xmlns:f=\"urn:foreign\" f:version=\"1.0\" />");

		Assert.IsFalse(result.Accepted, Describe(result));
	}

	[TestMethod]
	public async Task SCXML_PARSE_003_Preserves_root_initial_name_and_binding_attributes()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\" initial=\"idle\" name=\"unicode-Δ\" binding=\"late\"><state id=\"idle\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "unicode-Δ", result.Model!.Name);
		Assert.AreEqual(BindingType.Late, result.Model.Binding);
		Assert.IsNotNull(result.Model.Initial);
		Assert.AreEqual(expected: "idle", result.Model.Initial.Transition!.Target.Array[0].Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-003-EXISTING-004
	requirement_ids: [SCXML-PARSE-003]
	title: Existing SCXML-PARSE-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-003; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_003_Accepts_the_optional_root_datamodel_attribute()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\" datamodel=\"null\"><state id=\"ready\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "ready", result.Model!.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-003-EXISTING-005
	requirement_ids: [SCXML-PARSE-003]
	title: Existing SCXML-PARSE-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-003; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_003_Accepts_xml_base_on_the_root()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" xmlns:xml=\"http://www.w3.org/XML/1998/namespace\" xml:base=\"https://example.invalid/base/\" version=\"1.0\"><state id=\"ready\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "ready", result.Model!.States[0].Id!.Value);
	}

	[TestMethod]
	[DataRow("binding=\"LATE\"")]
	[DataRow("binding=\" late\"")]
	[DataRow("unknown=\"value\"")]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-PARSE-003-EXISTING-PARAM-004-ROWS
		description: Each declared DataRow is an independently reported lexical or configuration partition for SCXML-PARSE-003.
		partition: parameterized-existing
		input: The exact DataRow arguments immediately above this method.
		expected: Each row satisfies the method's explicit expected-result assertion.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-PARSE-003-EXISTING-PARAM-004
	requirement_ids: [SCXML-PARSE-003]
	title: Existing parameterized SCXML-PARSE-003 authority witness
	description: The existing concrete table is retained and each row distinguishes the authority-required acceptance or rejection result asserted by this method.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived parameter partition }
	phase: 1
	feature: phase-1-existing-parameterized-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-row-result
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [parameterized-existing]
	dimensions: { input: declared-DataRow }
	preconditions: [existing DataRow fixture]
	dependencies: [existing exhaustive harness]
	arrange: Use the exact DataRow input.
	stimulus: Invoke the existing parser, reader, validator, or runtime operation.
	expected: The concrete assertion in this existing method for that row.
	expected_exception_or_event: row-specific diagnostic or none
	forbidden: [result opposite to the asserted row expectation]
	edge_cases: [all declared rows]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing, Parameterized]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	public async Task SCXML_PARSE_003_Rejects_invalid_or_unknown_root_attributes(string attributes)
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\" {attributes} />");
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-003-EXISTING-006
	requirement_ids: [SCXML-PARSE-003]
	title: Existing SCXML-PARSE-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-003; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_003_Accepts_foreign_qualified_extension_attributes()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" xmlns:foreign=\"urn:foreign\" version=\"1.0\" foreign:extension=\"value\" />");
		Assert.IsTrue(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-004-EXISTING-007
	requirement_ids: [SCXML-PARSE-004]
	title: Existing SCXML-PARSE-004 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-004; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_004_Preserves_legal_root_state_and_final_child_order_while_ignoring_comments_and_processing_instructions()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><!--before--><state id=\"first\" /><?fixture keep?><final id=\"last\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: 2, result.Model!.States.Length);
		Assert.AreEqual(expected: "first", result.Model.States[0].Id!.Value);
		Assert.AreEqual(expected: "last", result.Model.States[1].Id!.Value);
	}

	[TestMethod]
	[DataRow("<raise event=\"unexpected\" />")]
	[DataRow("unexpected text")]
	[DataRow("<foreign xmlns=\"urn:foreign\" />")]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-PARSE-004-EXISTING-PARAM-005-ROWS
		description: Each declared DataRow is an independently reported lexical or configuration partition for SCXML-PARSE-004.
		partition: parameterized-existing
		input: The exact DataRow arguments immediately above this method.
		expected: Each row satisfies the method's explicit expected-result assertion.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-PARSE-004-EXISTING-PARAM-005
	requirement_ids: [SCXML-PARSE-004]
	title: Existing parameterized SCXML-PARSE-004 authority witness
	description: The existing concrete table is retained and each row distinguishes the authority-required acceptance or rejection result asserted by this method.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived parameter partition }
	phase: 1
	feature: phase-1-existing-parameterized-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-row-result
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [parameterized-existing]
	dimensions: { input: declared-DataRow }
	preconditions: [existing DataRow fixture]
	dependencies: [existing exhaustive harness]
	arrange: Use the exact DataRow input.
	stimulus: Invoke the existing parser, reader, validator, or runtime operation.
	expected: The concrete assertion in this existing method for that row.
	expected_exception_or_event: row-specific diagnostic or none
	forbidden: [result opposite to the asserted row expectation]
	edge_cases: [all declared rows]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing, Parameterized]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	public async Task SCXML_PARSE_004_Rejects_executable_content_and_non_whitespace_text_at_root(string child)
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\">{child}</scxml>");
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	[TestMethod]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><state id=\"unfinished\">")]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"></state>")]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" version=\"1.0\" />")]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\">&notAnEntity;</scxml>")]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><p:state id=\"bad\" /></scxml>")]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><state id=\"bad\" xmlns:p=\"urn:one\" xmlns:p=\"urn:two\" /></scxml>")]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" /><scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" />")]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-PARSE-021-EXISTING-PARAM-006-ROWS
		description: Each declared DataRow is an independently reported lexical or configuration partition for SCXML-PARSE-021.
		partition: parameterized-existing
		input: The exact DataRow arguments immediately above this method.
		expected: Each row satisfies the method's explicit expected-result assertion.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-PARSE-021-EXISTING-PARAM-006
	requirement_ids: [SCXML-PARSE-021]
	title: Existing parameterized SCXML-PARSE-021 authority witness
	description: The existing concrete table is retained and each row distinguishes the authority-required acceptance or rejection result asserted by this method.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived parameter partition }
	phase: 1
	feature: phase-1-existing-parameterized-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-row-result
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [parameterized-existing]
	dimensions: { input: declared-DataRow }
	preconditions: [existing DataRow fixture]
	dependencies: [existing exhaustive harness]
	arrange: Use the exact DataRow input.
	stimulus: Invoke the existing parser, reader, validator, or runtime operation.
	expected: The concrete assertion in this existing method for that row.
	expected_exception_or_event: row-specific diagnostic or none
	forbidden: [result opposite to the asserted row expectation]
	edge_cases: [all declared rows]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing, Parameterized]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	public async Task SCXML_PARSE_021_Malformed_xml_never_returns_a_partial_model(string document)
	{
		var result = await ScxmlParserHarness.ParseAsync(document, baseUri: "urn:exhaustive:malformed");
		Assert.IsNull(result.Model, Describe(result));
		Assert.IsFalse(result.Accepted, Describe(result));
		Assert.IsTrue(result.Exception is not null || result.Diagnostics.Count > 0, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-024-EXISTING-008
	requirement_ids: [SCXML-PARSE-024]
	title: Existing SCXML-PARSE-024 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-024; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_024_Prohibits_document_type_declarations_before_any_external_entity_is_resolved()
	{
		const string document = "<!DOCTYPE scxml [<!ENTITY xxe SYSTEM 'file:///not-a-real-secret'>]><scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\">&xxe;</scxml>";
		var result = await ScxmlParserHarness.ParseAsync(document, baseUri: "urn:exhaustive:xxe");

		Assert.IsNull(result.Model, Describe(result));
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-004-EXISTING-009
	requirement_ids: [SCXML-PARSE-004]
	title: Existing SCXML-PARSE-004 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-004; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_004_Rejects_executable_content_as_a_root_child()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><onentry><raise event=\"illegal\" /></onentry></scxml>");

		Assert.IsFalse(result.Accepted, Describe(result));
	}

	[TestMethod]
	public async Task SCXML_PARSE_004_Accepts_a_legal_root_script_child()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><script>global = 1</script><state id=\"ready\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.IsNotNull(result.Model!.Script);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-004-EXISTING-010
	requirement_ids: [SCXML-PARSE-004]
	title: Existing SCXML-PARSE-004 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-004; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_004_Accepts_legal_datamodel_and_script_root_children_in_mixed_order()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><script>global = 1</script><datamodel /><state id=\"ready\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "ready", result.Model!.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-005-EXISTING-011
	requirement_ids: [SCXML-PARSE-005]
	title: Existing SCXML-PARSE-005 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-005; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_005_Rejects_an_unknown_unqualified_state_attribute()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"ready\" unexpected=\"value\" /></scxml>");

		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-005-EXISTING-012
	requirement_ids: [SCXML-PARSE-005]
	title: Existing SCXML-PARSE-005 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-005; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_005_Accepts_a_foreign_qualified_unknown_state_attribute()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" xmlns:f=\"urn:foreign\" version=\"1.0\"><state id=\"ready\" f:unexpected=\"value\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
	}

	[TestMethod]
	public async Task SCXML_PARSE_004_Ignores_root_comments_and_processing_instructions()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<!--before--><?fixture value?><scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><!--inside--><state id=\"ready\" /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "ready", result.Model!.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-003-EXISTING-013
	requirement_ids: [SCXML-PARSE-003]
	title: Existing SCXML-PARSE-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-003; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_003_Accepts_foreign_qualified_root_attributes()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" xmlns:ext=\"urn:foreign\" version=\"1.0\" ext:unknown=\"value\" />");

		Assert.IsTrue(result.Accepted, Describe(result));
	}

	[TestMethod]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><p:state /></scxml>")]
	[DataRow("<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><state id=\"s\"/></scxml>&")]
	public async Task SCXML_PARSE_021_Rejects_additional_malformed_xml_without_returning_a_model(string document)
	{
		var result = await ScxmlParserHarness.ParseAsync(document, baseUri: "urn:exhaustive:malformed-extra");

		Assert.IsNull(result.Model, Describe(result));
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-021-EXISTING-014
	requirement_ids: [SCXML-PARSE-021]
	title: Existing SCXML-PARSE-021 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-021; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_021_Mismatched_closing_tag_never_returns_a_partial_model()
	{
		var result = await ScxmlParserHarness.ParseAsync(xml: "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><state id=\"s\"></scxml>", baseUri: "urn:exhaustive:mismatched");

		Assert.IsNull(result.Model, Describe(result));
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	[TestMethod]
	[DataRow("0", 0)]
	[DataRow("2s", 2000)]
	[DataRow("125ms", 125)]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-PARSE-020-EXISTING-PARAM-007-ROWS
		description: Each declared DataRow is an independently reported lexical or configuration partition for SCXML-PARSE-020.
		partition: parameterized-existing
		input: The exact DataRow arguments immediately above this method.
		expected: Each row satisfies the method's explicit expected-result assertion.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-PARSE-020-EXISTING-PARAM-007
	requirement_ids: [SCXML-PARSE-020]
	title: Existing parameterized SCXML-PARSE-020 authority witness
	description: The existing concrete table is retained and each row distinguishes the authority-required acceptance or rejection result asserted by this method.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived parameter partition }
	phase: 1
	feature: phase-1-existing-parameterized-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-row-result
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [parameterized-existing]
	dimensions: { input: declared-DataRow }
	preconditions: [existing DataRow fixture]
	dependencies: [existing exhaustive harness]
	arrange: Use the exact DataRow input.
	stimulus: Invoke the existing parser, reader, validator, or runtime operation.
	expected: The concrete assertion in this existing method for that row.
	expected_exception_or_event: row-specific diagnostic or none
	forbidden: [result opposite to the asserted row expectation]
	edge_cases: [all declared rows]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing, Parameterized]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	public async Task SCXML_PARSE_020_Parses_exact_supported_delay_forms_without_overflow_or_unit_loss(string lexicalDelay, int expectedMilliseconds)
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\"><onentry><send event=\"event\" delay=\"{lexicalDelay}\" /></onentry></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		var send = (ISend)state.OnEntry[0].Action[0];
		Assert.AreEqual(expectedMilliseconds, send.DelayMs);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-020-EXISTING-015
	requirement_ids: [SCXML-PARSE-020]
	title: Existing SCXML-PARSE-020 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-020; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_020_Rejects_fractional_delay_instead_of_coercing_it()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\"><onentry><send event=\"event\" delay=\"1.5s\" /></onentry></state></scxml>");
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-020-EXISTING-016
	requirement_ids: [SCXML-PARSE-020]
	title: Existing SCXML-PARSE-020 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-020; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_020_Rejects_seconds_to_milliseconds_overflow_instead_of_wrapping()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\"><onentry><send event=\"event\" delay=\"2147484s\" /></onentry></state></scxml>");
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	[TestMethod]
	[DataRow("-1s")]
	[DataRow("1S")]
	[DataRow("1")]
	[DataRow("1MS")]
	public async Task SCXML_PARSE_020_Rejects_invalid_delay_lexical_boundaries(string lexicalDelay)
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\"><onentry><send event=\"event\" delay=\"{lexicalDelay}\" /></onentry></state></scxml>");

		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-020-EXISTING-017
	requirement_ids: [SCXML-PARSE-020]
	title: Existing SCXML-PARSE-020 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-020; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_020_Rejects_whitespace_padded_delay_without_normalizing_it()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\"><onentry><send event=\"event\" delay=\" 1s\" /></onentry></state></scxml>");

		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-020-EXISTING-018
	requirement_ids: [SCXML-PARSE-020]
	title: Existing SCXML-PARSE-020 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-020; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_020_Accepts_an_expression_form_for_delay()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"ready\"><onentry><send event=\"tick\" delayexpr=\"'1s'\" /></onentry></state></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "ready", result.Model!.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-023-EXISTING-019
	requirement_ids: [SCXML-PARSE-023]
	title: Existing SCXML-PARSE-023 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-023; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_023_Accepts_a_non_seekable_one_byte_async_stream_without_partial_model()
	{
		// ReSharper disable once UseAwaitUsing
		using var stream = new OneByteReadStream(Encoding.UTF8.GetBytes($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\" /></scxml>"));
		var result = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:one-byte");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.IsTrue(stream.ReadCalls > 1, message: "The fixture must force incremental reads.");
		Assert.IsTrue(stream.CanRead, message: "The parser does not own the caller-provided stream.");
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-021-EXISTING-020
	requirement_ids: [SCXML-PARSE-021]
	title: Existing SCXML-PARSE-021 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-021; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_021_And_PARSE_023_Read_failure_after_partial_input_returns_no_model()
	{
		// ReSharper disable once UseAwaitUsing
		using var stream = new FailingReadStream(Encoding.UTF8.GetBytes($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"partial\" /></scxml>"));
		var result = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:read-failure");

		Assert.IsNull(result.Model, Describe(result));
		Assert.IsFalse(result.Accepted, Describe(result));
		Assert.IsTrue(stream.CanRead, message: "A parser failure must not dispose the caller-owned stream.");
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-021-EXISTING-021
	requirement_ids: [SCXML-PARSE-021]
	title: Existing SCXML-PARSE-021 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-021; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_021_Rejects_illegal_xml_control_characters_without_a_model()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"bad\">\u0001</state></scxml>");

		Assert.IsNull(result.Model, Describe(result));
		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-022-EXISTING-022
	requirement_ids: [SCXML-PARSE-022]
	title: Existing SCXML-PARSE-022 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-022; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_022_Accepts_utf16_little_endian_with_bom_and_preserves_unicode_identifiers()
	{
		var xml = $"<?xml version=\"1.0\" encoding=\"utf-16\"?><scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"Δstate\" /></scxml>";
		var preamble = Encoding.Unicode.GetPreamble();
		var encodedXml = Encoding.Unicode.GetBytes(xml);
		var bytes = new byte[preamble.Length + encodedXml.Length];
		Buffer.BlockCopy(preamble, srcOffset: 0, bytes, dstOffset: 0, preamble.Length);
		Buffer.BlockCopy(encodedXml, srcOffset: 0, bytes, preamble.Length, encodedXml.Length);

		using var stream = new MemoryStream(bytes, writable: false);
		var result = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:utf16");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "Δstate", result.Model!.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-022-EXISTING-023
	requirement_ids: [SCXML-PARSE-022]
	title: Existing SCXML-PARSE-022 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-022; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_022_Accepts_utf16_big_endian_with_bom_and_preserves_unicode_identifiers()
	{
		var xml = $"<?xml version=\"1.0\" encoding=\"utf-16\"?><scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"漢字\" /></scxml>";
		var encoding = Encoding.BigEndianUnicode;
		var preamble = encoding.GetPreamble();
		var encodedXml = encoding.GetBytes(xml);
		var bytes = new byte[preamble.Length + encodedXml.Length];
		Buffer.BlockCopy(preamble, srcOffset: 0, bytes, dstOffset: 0, preamble.Length);
		Buffer.BlockCopy(encodedXml, srcOffset: 0, bytes, preamble.Length, encodedXml.Length);

		using var stream = new MemoryStream(bytes, writable: false);
		var result = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:utf16be");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "漢字", result.Model!.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-022-EXISTING-024
	requirement_ids: [SCXML-PARSE-022]
	title: Existing SCXML-PARSE-022 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-022; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_022_Accepts_utf8_without_a_bom_and_preserves_unicode_identifiers()
	{
		var xml = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"café\" /></scxml>";
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml), writable: false);
		var result = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:utf8-no-bom");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.AreEqual(expected: "café", result.Model!.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-021-EXISTING-025
	requirement_ids: [SCXML-PARSE-021]
	title: Existing SCXML-PARSE-021 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-021; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_021_Rejects_an_incomplete_utf8_sequence_without_returning_a_model()
	{
		var prefix = Encoding.UTF8.GetBytes($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"");
		var suffix = " /></scxml>"u8.ToArray();
		var bytes = new byte[prefix.Length + 2 + suffix.Length];
		Buffer.BlockCopy(prefix, srcOffset: 0, bytes, dstOffset: 0, prefix.Length);
		bytes[prefix.Length] = 0xC3;
		bytes[prefix.Length + 1] = 0x28;
		Buffer.BlockCopy(suffix, srcOffset: 0, bytes, prefix.Length + 2, suffix.Length);

		using var stream = new MemoryStream(bytes, writable: false);
		var result = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:invalid-utf8");

		Assert.IsFalse(result.Accepted, Describe(result));
		Assert.IsNull(result.Model, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-022-EXISTING-026
	requirement_ids: [SCXML-PARSE-022]
	title: Existing SCXML-PARSE-022 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-022; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_022_Rejects_an_xml_encoding_declaration_that_mismatches_the_bytes()
	{
		var xml = $"<?xml version=\"1.0\" encoding=\"utf-16\"?><scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"ready\" /></scxml>";
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml), writable: false);
		var result = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:encoding-mismatch");

		Assert.IsFalse(result.Accepted, Describe(result));
		Assert.IsNull(result.Model, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-023-EXISTING-027
	requirement_ids: [SCXML-PARSE-023]
	title: Existing SCXML-PARSE-023 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-023; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_023_A_second_parse_attempt_on_a_consumed_stream_does_not_reuse_the_first_model()
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"once\" /></scxml>"), writable: false);
		var first = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:repeat-first");
		var second = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:repeat-second");

		Assert.IsTrue(first.Accepted, Describe(first));
		Assert.IsNull(second.Model, Describe(second));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-023-EXISTING-028
	requirement_ids: [SCXML-PARSE-023]
	title: Existing SCXML-PARSE-023 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-023; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_023_Does_not_close_a_caller_owned_stream()
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"owned\" /></scxml>"), writable: false);
		var result = await ScxmlParserHarness.ParseStreamAsync(stream, baseUri: "urn:exhaustive:caller-owned");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.IsTrue(stream.CanRead);
		Assert.AreEqual(expected: -1, stream.ReadByte());
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-010-EXISTING-029
	requirement_ids: [SCXML-PARSE-010]
	title: Existing SCXML-PARSE-010 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-010; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_010_Preserves_transition_event_target_lists_and_internal_type()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"source\"><transition event=\"alpha beta\" target=\"first second\" type=\"internal\" /></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		var transition = state.Transitions[0];
		CollectionAssert.AreEqual(new[] { "alpha", "beta" }, transition.EventDescriptors.Array.Select(static item => item.Value).ToArray());
		CollectionAssert.AreEqual(new[] { "first", "second" }, transition.Target.Array.Select(static item => item.Value).ToArray());
		Assert.AreEqual(TransitionType.Internal, transition.Type);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-012-EXISTING-030
	requirement_ids: [SCXML-PARSE-012]
	title: Existing SCXML-PARSE-012 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-012; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_012_Preserves_raise_and_log_executable_content_in_document_order()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\"><onentry><raise event=\"raised.event\" /><log label=\"checkpoint\" expr=\"value\" /></onentry></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		var actions = state.OnEntry[0].Action;
		Assert.IsInstanceOfType<IRaise>(actions[0]);
		Assert.IsInstanceOfType<ILog>(actions[1]);
		Assert.AreEqual(expected: "checkpoint", ((ILog)actions[1]).Label);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-006-EXISTING-031
	requirement_ids: [SCXML-PARSE-006]
	title: Existing SCXML-PARSE-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-006; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_006_Preserves_nested_compound_state_structure_and_ids()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"parent\"><state id=\"child\" /></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var parent = (IState)result.Model!.States[0];
		Assert.AreEqual(expected: "parent", parent.Id!.Value);
		Assert.AreEqual(expected: 1, parent.States.Length);
		Assert.AreEqual(expected: "child", parent.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-006-EXISTING-032
	requirement_ids: [SCXML-PARSE-006]
	title: Existing SCXML-PARSE-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-006; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_006_Preserves_a_compound_state_initial_attribute()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"parent\" initial=\"child\"><state id=\"child\" /></state></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		var parent = (IState)result.Model!.States[0];
		Assert.IsNotNull(parent.Initial);
		Assert.AreEqual(expected: "child", parent.Initial!.Transition!.Target.Array[0].Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-006-EXISTING-033
	requirement_ids: [SCXML-PARSE-006]
	title: Existing SCXML-PARSE-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-006; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_006_Accepts_an_atomic_state_without_an_id()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state /></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		Assert.IsNull(result.Model!.States[0].Id);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-008-EXISTING-034
	requirement_ids: [SCXML-PARSE-008]
	title: Existing SCXML-PARSE-008 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-008; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_008_Parses_root_final_as_a_final_state_with_its_identifier()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><final id=\"complete\" /></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		Assert.IsInstanceOfType<IFinal>(result.Model!.States[0]);
		Assert.AreEqual(expected: "complete", result.Model.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-008-EXISTING-035
	requirement_ids: [SCXML-PARSE-008]
	title: Existing SCXML-PARSE-008 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-008; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_008_Rejects_repeated_donedata_under_a_final_state()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><final id=\"complete\"><donedata /><donedata /></final></scxml>");

		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-007-EXISTING-036
	requirement_ids: [SCXML-PARSE-007]
	title: Existing SCXML-PARSE-007 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-007; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_007_Parses_parallel_regions_in_document_order()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><parallel id=\"all\"><state id=\"left\" /><state id=\"right\" /></parallel></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		Assert.IsInstanceOfType<IParallel>(result.Model!.States[0]);
		var parallel = (IParallel)result.Model.States[0];
		CollectionAssert.AreEqual(new[] { "left", "right" }, parallel.States.Select(static item => item.Id!.Value).ToArray());
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-007-EXISTING-037
	requirement_ids: [SCXML-PARSE-007]
	title: Existing SCXML-PARSE-007 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-007; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_007_Rejects_an_initial_attribute_on_parallel()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><parallel id=\"root\" initial=\"region\"><state id=\"region\" /></parallel></scxml>");

		Assert.IsFalse(result.Accepted, Describe(result));
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-009-EXISTING-038
	requirement_ids: [SCXML-PARSE-009]
	title: Existing SCXML-PARSE-009 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-009; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_009_Parses_explicit_compound_initial_transition_target()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"parent\"><initial><transition target=\"child\" /></initial><state id=\"child\" /></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var parent = (IState)result.Model!.States[0];
		Assert.IsNotNull(parent.Initial);
		Assert.AreEqual(expected: "child", parent.Initial.Transition!.Target.Array[0].Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-009-EXISTING-039
	requirement_ids: [SCXML-PARSE-009]
	title: Existing SCXML-PARSE-009 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-009; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_009_Parses_history_identifier_type_and_default_transition()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"parent\"><history id=\"resume\" type=\"deep\"><transition target=\"child\" /></history><state id=\"child\" /></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var parent = (IState)result.Model!.States[0];
		Assert.AreEqual(expected: 1, parent.HistoryStates.Length);
		var history = parent.HistoryStates[0];
		Assert.AreEqual(expected: "resume", history.Id!.Value);
		Assert.AreEqual(HistoryType.Deep, history.Type);
		Assert.AreEqual(expected: "child", history.Transition!.Target.Array[0].Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-013-EXISTING-040
	requirement_ids: [SCXML-PARSE-013]
	title: Existing SCXML-PARSE-013 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-013; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_013_Preserves_if_branch_markers_and_actions_in_document_order()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"active\"><onentry><if cond=\"first\"><raise event=\"one\" /><elseif cond=\"second\" /><log label=\"two\" /><else /><raise event=\"three\" /></if></onentry></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		var conditional = (IIf)state.OnEntry[0].Action[0];
		Assert.IsNotNull(conditional.Condition);
		Assert.AreEqual(expected: 5, conditional.Action.Length);
		Assert.IsInstanceOfType<IRaise>(conditional.Action[0]);
		Assert.IsInstanceOfType<IElseIf>(conditional.Action[1]);
		Assert.IsInstanceOfType<ILog>(conditional.Action[2]);
		Assert.IsInstanceOfType<IElse>(conditional.Action[3]);
		Assert.IsInstanceOfType<IRaise>(conditional.Action[4]);
		Assert.AreEqual(expected: "one", ((IRaise)conditional.Action[0]).OutgoingEvent!.Name.ToString());
		Assert.AreEqual(expected: "two", ((ILog)conditional.Action[2]).Label);
		Assert.AreEqual(expected: "three", ((IRaise)conditional.Action[4]).OutgoingEvent!.Name.ToString());
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-014-EXISTING-041
	requirement_ids: [SCXML-PARSE-014]
	title: Existing SCXML-PARSE-014 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-014; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_014_Parses_foreach_required_optional_attributes_and_executable_content()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"active\"><onentry><foreach array=\"items\" item=\"item\" index=\"position\"><log label=\"each\" /></foreach></onentry></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		var loop = (IForEach)state.OnEntry[0].Action[0];
		Assert.IsNotNull(loop.Array);
		Assert.IsNotNull(loop.Item);
		Assert.IsNotNull(loop.Index);
		Assert.AreEqual(expected: 1, loop.Action.Length);
		Assert.IsInstanceOfType<ILog>(loop.Action[0]);
		Assert.AreEqual(expected: "each", ((ILog)loop.Action[0]).Label);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-015-EXISTING-042
	requirement_ids: [SCXML-PARSE-015]
	title: Existing SCXML-PARSE-015 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-015; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_015_Parses_literal_send_routing_delay_payload_and_parameters()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"active\"><onentry><send event=\"notify\" target=\"#_parent\" type=\"http://www.w3.org/TR/scxml/#SCXMLEventProcessor\" id=\"outbound\" delay=\"2s\" namelist=\"first second\"><param name=\"alpha\" expr=\"value\" /><param name=\"beta\" location=\"source\" /><content>payload</content></send></onentry></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		var send = (ISend)state.OnEntry[0].Action[0];
		Assert.AreEqual(expected: "notify", send.EventName);
		Assert.AreEqual(expected: "#_parent", send.Target!.OriginalString);
		Assert.AreEqual(expected: "http://www.w3.org/TR/scxml/#SCXMLEventProcessor", send.Type!.OriginalString);
		Assert.AreEqual(expected: "outbound", send.Id);
		Assert.AreEqual(expected: 2000, send.DelayMs);
		Assert.AreEqual(expected: 2, send.NameList.Length);
		CollectionAssert.AreEqual(new[] { "alpha", "beta" }, send.Parameters.Select(static parameter => parameter.Name).ToArray());
		Assert.IsNotNull(send.Parameters[0].Expression);
		Assert.IsNotNull(send.Parameters[1].Location);
		Assert.IsNotNull(send.Content);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-016-EXISTING-043
	requirement_ids: [SCXML-PARSE-016]
	title: Existing SCXML-PARSE-016 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-016; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_016_Parses_literal_invoke_attributes_parameters_finalize_and_content()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"active\"><invoke type=\"http://www.w3.org/TR/scxml/\" src=\"https://example.test/child.scxml\" id=\"child\" namelist=\"first second\" autoforward=\"true\"><param name=\"input\" expr=\"value\" /><finalize><log label=\"finish\" /></finalize><content>inline child</content></invoke></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		Assert.AreEqual(expected: 1, state.Invoke.Length);
		var invoke = state.Invoke[0];
		Assert.AreEqual(expected: "http://www.w3.org/TR/scxml/", invoke.Type!.OriginalString);
		Assert.AreEqual(expected: "https://example.test/child.scxml", invoke.Source!.OriginalString);
		Assert.AreEqual(expected: "child", invoke.Id);
		Assert.IsTrue(invoke.AutoForward);
		Assert.AreEqual(expected: 2, invoke.NameList.Length);
		Assert.AreEqual(expected: 1, invoke.Parameters.Length);
		Assert.AreEqual(expected: "input", invoke.Parameters[0].Name);
		Assert.IsNotNull(invoke.Parameters[0].Expression);
		Assert.IsNotNull(invoke.Finalize);
		Assert.AreEqual(expected: 1, invoke.Finalize!.Action.Length);
		Assert.IsInstanceOfType<ILog>(invoke.Finalize.Action[0]);
		Assert.AreEqual(expected: "finish", ((ILog)invoke.Finalize.Action[0]).Label);
		Assert.IsNotNull(invoke.Content);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-017-EXISTING-044
	requirement_ids: [SCXML-PARSE-017]
	title: Existing SCXML-PARSE-017 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-017; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_017_Parses_data_declarations_and_final_done_data_payload()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><datamodel><data id=\"calculated\" expr=\"value\" /><data id=\"inline\">text payload</data></datamodel><final id=\"complete\"><donedata><param name=\"result\" expr=\"calculated\" /><content>done payload</content></donedata></final></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var dataModel = result.Model!.DataModel;
		Assert.IsNotNull(dataModel);
		Assert.AreEqual(expected: 2, dataModel.Data.Length);
		Assert.AreEqual(expected: "calculated", dataModel.Data[0].Id);
		Assert.IsNotNull(dataModel.Data[0].Expression);
		Assert.AreEqual(expected: "inline", dataModel.Data[1].Id);
		Assert.IsNotNull(dataModel.Data[1].InlineContent);

		var final = (IFinal)result.Model.States[0];
		Assert.IsNotNull(final.DoneData);
		Assert.AreEqual(expected: 1, final.DoneData!.Parameters.Length);
		Assert.AreEqual(expected: "result", final.DoneData.Parameters[0].Name);
		Assert.IsNotNull(final.DoneData.Parameters[0].Expression);
		Assert.IsNotNull(final.DoneData.Content);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-018-EXISTING-045
	requirement_ids: [SCXML-PARSE-018]
	title: Existing SCXML-PARSE-018 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-018; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_018_Preserves_foreign_executable_action_namespace_name_and_outer_xml()
	{
		const string extensionNamespace = "urn:example:extension";
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" xmlns:ext=\"{extensionNamespace}\" version=\"1.0\"><state id=\"active\"><onentry><ext:record level=\"audit\"><ext:detail>value</ext:detail></ext:record></onentry></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		var custom = (ICustomAction)state.OnEntry[0].Action[0];
		Assert.AreEqual(extensionNamespace, custom.XmlNamespace);
		Assert.AreEqual(expected: "record", custom.XmlName);
		Assert.IsNotNull(custom.Xml);
		StringAssert.Contains(custom.Xml, substring: "level=\"audit\"");
		StringAssert.Contains(custom.Xml, substring: "detail");
		StringAssert.Contains(custom.Xml, substring: "value");
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-019-EXISTING-046
	requirement_ids: [SCXML-PARSE-019]
	title: Existing SCXML-PARSE-019 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-019; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	//[Ignore("Product defect DEF-SCXML-PARSE-021: malformed XML can return a partial model.")]
	public async Task SCXML_PARSE_019_Tokenizes_initial_target_event_and_namelist_on_xml_whitespace()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\" initial=\" first\tsecond\r\nthird \"><state id=\"first\"><transition event=\"alpha.beta\tgamma\r\ndelta\" target=\"one\ttwo\r\nthree\" /></state><state id=\"second\"><onentry><send namelist=\"left\tright\r\ncenter\" /></onentry></state><state id=\"third\" /></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		CollectionAssert.AreEqual(new[] { "first", "second", "third" }, result.Model!.Initial!.Transition!.Target.Array.Select(static item => item.Value).ToArray());
		var first = (IState)result.Model.States[0];
		CollectionAssert.AreEqual(new[] { "alpha.beta", "gamma", "delta" }, first.Transitions[0].EventDescriptors.Array.Select(static item => item.Value).ToArray());
		CollectionAssert.AreEqual(new[] { "one", "two", "three" }, first.Transitions[0].Target.Array.Select(static item => item.Value).ToArray());
		var second = (IState)result.Model.States[1];
		var send = (ISend)second.OnEntry[0].Action[0];
		Assert.AreEqual(expected: 3, send.NameList.Length);
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-019-EXISTING-047
	requirement_ids: [SCXML-PARSE-019]
	title: Existing SCXML-PARSE-019 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-019; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_019_Does_not_split_identifier_lists_on_non_breaking_space()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\"><transition event=\"alpha&#xA0;beta\" /></state></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		var state = (IState)result.Model!.States[0];
		CollectionAssert.AreEqual(new[] { "alpha\u00A0beta" }, state.Transitions[0].EventDescriptors.Array.Select(static item => item.Value).ToArray());
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-019-EXISTING-048
	requirement_ids: [SCXML-PARSE-019]
	title: Existing SCXML-PARSE-019 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-019; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_019_Preserves_duplicate_event_tokens_in_document_order()
	{
		var result = await ScxmlParserHarness.ParseAsync($"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"state\"><transition event=\"repeat repeat final\" /></state></scxml>");

		Assert.IsTrue(result.Accepted, Describe(result));
		var state = (IState)result.Model!.States[0];
		CollectionAssert.AreEqual(new[] { "repeat", "repeat", "final" }, state.Transitions[0].EventDescriptors.Array.Select(static item => item.Value).ToArray());
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-011-EXISTING-049
	requirement_ids: [SCXML-PARSE-011]
	title: Existing SCXML-PARSE-011 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-011; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_011_Preserves_empty_onentry_and_ordered_onexit_executable_content()
	{
		var result = await ScxmlParserHarness.ParseAsync(
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\"><state id=\"active\"><onentry /><onexit><log label=\"leaving\" /><raise event=\"exited\" /></onexit></state></scxml>");
		Assert.IsTrue(result.Accepted, Describe(result));

		var state = (IState)result.Model!.States[0];
		Assert.AreEqual(expected: 1, state.OnEntry.Length);
		Assert.IsTrue(state.OnEntry[0].Action.IsDefaultOrEmpty);
		Assert.AreEqual(expected: 1, state.OnExit.Length);
		Assert.AreEqual(expected: 2, state.OnExit[0].Action.Length);
		Assert.IsInstanceOfType<ILog>(state.OnExit[0].Action[0]);
		Assert.IsInstanceOfType<IRaise>(state.OnExit[0].Action[1]);
		Assert.AreEqual(expected: "leaving", ((ILog)state.OnExit[0].Action[0]).Label);
		Assert.AreEqual(expected: "exited", ((IRaise)state.OnExit[0].Action[1]).OutgoingEvent!.Name.ToString());
	}

	/*
	TEST-METADATA
	test_id: SCXML-PARSE-025-EXISTING-050
	requirement_ids: [SCXML-PARSE-025]
	title: Existing SCXML-PARSE-025 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-PARSE-025; incorrect behavior is distinguished by the method's explicit assertions.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: assigned requirement, citation_or_rule: authority-derived planned scenario }
	phase: 1
	feature: phase-1-existing-witness
	target_components: [existing-test-target]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [case-specific]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [existing-covered-partition]
	dimensions: { fixture: existing-method-specific }
	preconditions: [existing method fixture]
	dependencies: [existing exhaustive harness]
	arrange: Existing method's concrete setup.
	stimulus: Existing method's concrete operation.
	expected: Existing method's exact assertions.
	expected_exception_or_event: existing-method-specific
	forbidden: [outcome contradicted by existing assertions]
	edge_cases: [existing-method-specific]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: existing-method-bound }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [existing method cleanup]
	resource_risk: existing-method-specific
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public async Task SCXML_PARSE_025_Accepted_xml_and_serialized_public_model_have_matching_semantics()
	{
		const string source =
			$"<scxml xmlns=\"{ScxmlNamespace}\" version=\"1.0\" name=\"parity\" initial=\"idle\"><state id=\"idle\"><transition event=\"go\" target=\"done\" /></state><final id=\"done\" /></scxml>";
		var parsed = await ScxmlParserHarness.ParseAsync(source);
		Assert.IsTrue(parsed.Accepted, Describe(parsed));

		var output = new StringBuilder();

		// ReSharper disable once UseAwaitUsing
		using (var writer = XmlWriter.Create(output, new XmlWriterSettings { OmitXmlDeclaration = true}))
		{
			new ScxmlSerializerWriter(writer).Serialize(parsed.Model!);
		}

		var reparsed = await ScxmlParserHarness.ParseAsync(output.ToString());
		Assert.IsTrue(reparsed.Accepted, Describe(reparsed));
		Assert.AreEqual(parsed.Model!.Name, reparsed.Model!.Name);
		Assert.AreEqual(parsed.Model.Initial!.Transition!.Target.Array[0].Value, reparsed.Model.Initial!.Transition!.Target.Array[0].Value);
		CollectionAssert.AreEqual(parsed.Model.States.Select(static state => state.Id!.Value).ToArray(), reparsed.Model.States.Select(static state => state.Id!.Value).ToArray());
		var originalState = (IState)parsed.Model.States[0];
		var roundTripState = (IState)reparsed.Model.States[0];
		CollectionAssert.AreEqual(
			originalState.Transitions[0].EventDescriptors.Array.Select(static item => item.Value).ToArray(),
			roundTripState.Transitions[0].EventDescriptors.Array.Select(static item => item.Value).ToArray());
	}

	private static string Describe(ScxmlParserHarness.ParseResult result) =>
		$"Accepted={result.Accepted}; Diagnostics=[{string.Join(separator: " | ", result.Diagnostics)}]; Exception={result.Exception}";

	private sealed class OneByteReadStream(byte[] bytes) : MemoryStream(bytes, writable: false)
	{
		public int ReadCalls { get; private set; }

		public override bool CanSeek => false;

		public override int Read(byte[] buffer, int offset, int count)
		{
			ReadCalls++;

			return base.Read(buffer, offset, Math.Min(count, val2: 1));
		}

		public override Task<int> ReadAsync(byte[] buffer,
											int offset,
											int count,
											CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(Read(buffer, offset, count));
		}

		public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();
	}

	private sealed class FailingReadStream(byte[] bytes) : MemoryStream(bytes, writable: false)
	{
		private int _reads;

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (++_reads > 1) throw new IOException("deterministic read failure");

			return base.Read(buffer, offset, Math.Min(count, val2: 4));
		}

		public override Task<int> ReadAsync(byte[] buffer,
											int offset,
											int count,
											CancellationToken cancellationToken) =>
			Task.FromResult(Read(buffer, offset, count));
	}
}
