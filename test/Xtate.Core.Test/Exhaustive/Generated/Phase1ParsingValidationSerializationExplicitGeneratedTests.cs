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
public sealed class Phase1ParsingValidationSerializationExplicitGeneratedTests
{
	private static readonly ScxmlSourceCase[] ExplicitCases =
	[
		new(
			CaseId: "SCXML-PARSE-001-CASE-001", RequirementIds: "SCXML-PARSE-001", Description: "Canonical SCXML namespace and version 1.0 parse to a root model with no diagnostics.",
			Input: "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'><final id='done'/></scxml>", Operation: "Parse from one-byte async stream.",
			Expected: "Model root namespace is canonical, version is 1.0, final id is done, diagnostics empty.",
			Forbidden: "Accepted missing/wrong namespace or version defaulting; reader remains open."),
		new(
			CaseId: "SCXML-PARSE-004-CASE-001", RequirementIds: "SCXML-PARSE-004", Description: "A root with duplicate state IDs is rejected before it yields a usable compiled model.",
			Input: "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'><state id='x'/><final id='x'/></scxml>", Operation: "Parse then validate.",
			Expected: "Duplicate-ID validation diagnostic names x and no compiled model is returned.",
			Forbidden: "Partial compiled graph, target resolution by arbitrary duplicate, or silent ID renaming."),
		new(
			CaseId: "SCXML-PARSE-009-CASE-001", RequirementIds: "SCXML-PARSE-009", Description: "Leading whitespace in closed lexical binding token is rejected rather than trimmed into late binding.",
			Input: "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' binding=' late'><final id='done'/></scxml>", Operation: "Parse root attributes.",
			Expected: "Binding lexical diagnostic and no usable model.", Forbidden: "BindingType.Late result, whitespace normalization, or later execution."),
		new(
			CaseId: "XINCLUDE-001-CASE-001", RequirementIds: "XINCLUDE-001", Description: "A permitted local XML XInclude replaces the include element with the included element in document order.",
			Input: "Main SCXML contains xi:include href='child.xml'; resolver maps child.xml to <state id='included'/>", Operation: "Parse with XInclude enabled and bounded resolver.",
			Expected: "Model contains state included at the include position; resolver is called once for child.xml.",
			Forbidden: "Unexpanded xi:include, duplicate insertion, resolver call to another URI, or retained child reader."),
		new(
			CaseId: "XINCLUDE-004-CASE-001", RequirementIds: "XINCLUDE-004", Description: "An XInclude file URI outside the allowed resolver policy is denied before opening a stream.",
			Input: "xi:include href='file:///outside/secret.xml' with resolver policy local-test-root only.", Operation: "Parse with XInclude enabled.",
			Expected: "Controlled include/security diagnostic and resolver open-call count zero.",
			Forbidden: "File access, fallback to empty content, partially included model, or leaked resolver scope."),
		new(
			CaseId: "SCXML-SER-002-CASE-001", RequirementIds: "SCXML-SER-001|SCXML-SER-002",
			Description: "Serializer preserves root name/binding and escapes attribute/text special characters without changing semantic values.",
			Input: "Public model name='a&b\"c', binding=Late, log label='<tag>&'.", Operation: "Serialize then parse serialized XML.",
			Expected: "XML contains escaped lexical forms; reparsed model equals name, binding, and label values exactly.",
			Forbidden: "Unescaped ampersand/quote/angle bracket, omitted binding/name, culture-specific escaping, or semantic loss."),
		new(
			CaseId: "SCXML-SER-005-CASE-001", RequirementIds: "SCXML-SER-005",
			Description: "Writer failure during serialization reports failure and does not expose a misleading completed SCXML document.",
			Input: "Valid model and writer that throws after the root start tag but before first child.", Operation: "Serialize through faulting writer.",
			Expected: "Controlled writer/serialization failure; exposed output is empty or explicitly marked incomplete by contract; writer disposed once.",
			Forbidden: "Successful-looking closing scxml document, swallowed error, partially reusable writer, or retained model graph.")
	];

	/*
	TEST-METADATA
	test_id: PHASE1-PARSE-VALIDATE-SERIALIZE-EXPLICIT-001
	requirement_ids: [SCXML-PARSE-001,SCXML-PARSE-004,SCXML-PARSE-009,XINCLUDE-001,XINCLUDE-004,SCXML-VALID-001,SCXML-VALID-004,SCXML-SER-001,SCXML-SER-002,SCXML-SER-005]
	title: Parser validation XInclude and serializer requirements have explicit edge fixtures
	description: Each record contains literal XML or public-model input plus a complete parse/validation/output oracle, distinguishing rejection stage, inclusion behavior, and non-partial serialization from a generic requirement reference.
	authority: { source: W3C SCXML 1.0 and W3C XInclude 1.0; exhaustive plan document 01, section: parsing validation and serializer construction routes, citation_or_rule: XML namespace/version/cardinality and serialization preservation rules are exact per record }
	phase: 1
	feature: parsing-validation-serialization
	target_components: [ScxmlParser,StateMachineValidator,ScxmlSerializer,XIncludeResolver]
	test_kind: parser-validator-roundtrip
	oracle_type: exact-diagnostic-model-and-xml-output
	risk: critical
	priority: critical
	construction_routes: [scxml-text,async-reader,stream,public-object-model]
	data_models: [null,xpath]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,lexical-negative,namespace,include-fault,validation,escaping,writer-fault]
	dimensions: { records: seven-literal-inputs }
	preconditions: [isolated parser options and instrumented resolver/writer]
	dependencies: [ExplicitScxmlSourceHarness,RecordingXIncludeResolver,FaultingXmlWriter]
	arrange: Build the literal record input and capture resolver calls, model snapshot, and writer bytes.
	stimulus: Parse, validate, include, or serialize once through the declared route.
	expected: [record-specific model, diagnostic, output bytes, and cleanup result]
	expected_exception_or_event: record-specific parse/validation/IO error or none
	forbidden: [silent defaulting, resolver escape, partially usable model, lost semantic property, partial document]
	edge_cases: [one-byte stream, duplicate ID, forbidden XInclude scheme, Unicode escaping]
	determinism: { clock: not-applicable, scheduling: bounded-reader-and-writer-steps, timeout_or_step_bound: '512 operations' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [all reader writer and resolver handles disposed exactly once]
	resource_risk: XML-reader-writer-retention
	tier: fast
	tags: [Exhaustive,Parsing,Validation,Serialization,XInclude]
	related_tests: []
	known_issue: none
	compile_notes: ExplicitScxmlSourceHarness, RecordingXIncludeResolver, and FaultingXmlWriter are planned test-side helpers.
	generation_status: generated-uncompiled
	*/
	[TestMethod]
	[DynamicData(nameof(Cases))]
	public async Task Parsing_validation_and_serialization_case_has_exact_outcome(ScxmlSourceCase testCase)
	{
		// Arrange
		await using var harness = await ExplicitScxmlSourceHarness.CreateAsync(testCase);
		var before = await harness.CaptureSnapshotAsync();

		// Act
		var outcome = await harness.ExecuteAsync(testCase.Operation);

		// Assert
		await harness.AssertExactOutcomeAsync(testCase.Expected, outcome);
		await harness.AssertForbiddenEffectsAbsentAsync(testCase.Forbidden, before);
		await harness.AssertCleanupAsync();
	}

	public static IEnumerable<object[]> Cases() => ExplicitCases.Select(testCase => new object[] { testCase });

	public sealed record ScxmlSourceCase(
		string CaseId,
		string RequirementIds,
		string Description,
		string Input,
		string Operation,
		string Expected,
		string Forbidden);
}
