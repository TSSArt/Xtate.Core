using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Generated;

[TestClass]
public sealed class Phase1ParsingValidationSerializationExplicitGeneratedTests
{
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
    [DataTestMethod]
    [DynamicData(nameof(Cases), DynamicDataSourceType.Method)]
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

    public static readonly ScxmlSourceCase[] ExplicitCases =
    [
        new("SCXML-PARSE-001-CASE-001", "SCXML-PARSE-001", "Canonical SCXML namespace and version 1.0 parse to a root model with no diagnostics.", "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'><final id='done'/></scxml>", "Parse from one-byte async stream.", "Model root namespace is canonical, version is 1.0, final id is done, diagnostics empty.", "Accepted missing/wrong namespace or version defaulting; reader remains open."),
        new("SCXML-PARSE-004-CASE-001", "SCXML-PARSE-004", "A root with duplicate state IDs is rejected before it yields a usable compiled model.", "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'><state id='x'/><final id='x'/></scxml>", "Parse then validate.", "Duplicate-ID validation diagnostic names x and no compiled model is returned.", "Partial compiled graph, target resolution by arbitrary duplicate, or silent ID renaming."),
        new("SCXML-PARSE-009-CASE-001", "SCXML-PARSE-009", "Leading whitespace in closed lexical binding token is rejected rather than trimmed into late binding.", "<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0' binding=' late'><final id='done'/></scxml>", "Parse root attributes.", "Binding lexical diagnostic and no usable model.", "BindingType.Late result, whitespace normalization, or later execution."),
        new("XINCLUDE-001-CASE-001", "XINCLUDE-001", "A permitted local XML XInclude replaces the include element with the included element in document order.", "Main SCXML contains xi:include href='child.xml'; resolver maps child.xml to <state id='included'/>", "Parse with XInclude enabled and bounded resolver.", "Model contains state included at the include position; resolver is called once for child.xml.", "Unexpanded xi:include, duplicate insertion, resolver call to another URI, or retained child reader."),
        new("XINCLUDE-004-CASE-001", "XINCLUDE-004", "An XInclude file URI outside the allowed resolver policy is denied before opening a stream.", "xi:include href='file:///outside/secret.xml' with resolver policy local-test-root only.", "Parse with XInclude enabled.", "Controlled include/security diagnostic and resolver open-call count zero.", "File access, fallback to empty content, partially included model, or leaked resolver scope."),
        new("SCXML-SER-002-CASE-001", "SCXML-SER-001|SCXML-SER-002", "Serializer preserves root name/binding and escapes attribute/text special characters without changing semantic values.", "Public model name='a&b\"c', binding=Late, log label='<tag>&'.", "Serialize then parse serialized XML.", "XML contains escaped lexical forms; reparsed model equals name, binding, and label values exactly.", "Unescaped ampersand/quote/angle bracket, omitted binding/name, culture-specific escaping, or semantic loss."),
        new("SCXML-SER-005-CASE-001", "SCXML-SER-005", "Writer failure during serialization reports failure and does not expose a misleading completed SCXML document.", "Valid model and writer that throws after the root start tag but before first child.", "Serialize through faulting writer.", "Controlled writer/serialization failure; exposed output is empty or explicitly marked incomplete by contract; writer disposed once.", "Successful-looking closing scxml document, swallowed error, partially reusable writer, or retained model graph.")
    ];

    public sealed record ScxmlSourceCase(string CaseId, string RequirementIds, string Description, string Input, string Operation, string Expected, string Forbidden);
}
