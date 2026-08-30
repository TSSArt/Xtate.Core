using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Phase1;

// This is intentionally a source-generation artifact.  PhaseOneContractHarness is a
// future test-side adapter; the table is the authority-derived executable test plan.
[TestClass]
[TestCategory("Exhaustive.Generated")]
public sealed class Phase1RemainingRequirementsGeneratedTests
{
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-PARSE-001-CASE-101
	    description: Exact namespace/root acceptance and all namespace, local-name, case, prefix-lookalike, nested-root, base-URI and line/column rejection partitions.
	    partition: positive; namespace-negative; diagnostic-location
	    input: SCXML documents varying root QName and location.
	    expected: Exact SCXML root is accepted; every other root has no model and a location-bearing diagnostic.
	  - case_id: SCXML-PARSE-002-CASE-101
	    description: Required version lexical matrix covers exact, absent, empty, padded, numeric, Unicode-digit, case, duplicate and foreign-qualified forms.
	    partition: required-property; lexical-negative; duplicate
	    input: Root version attribute matrix.
	    expected: Only unqualified exact 1.0 is accepted; every other form returns no usable model.
	  - case_id: SCXML-PARSE-003-CASE-101
	    description: Root attribute matrix independently varies initial, datamodel, binding, name, version, xml:base, declarations, unknown names, Unicode, empty and duplicate forms.
	    partition: defaults; lexical-negative; namespace; duplicate
	    input: Legal and illegal root attribute combinations.
	    expected: Legal values preserve exact public properties; illegal or unknown attributes reject without normalization.
	  - case_id: SCXML-PARSE-004-CASE-101
	    description: Root-child cardinality/order matrix covers state, parallel, final, datamodel, script, text, CDATA, comment, PI, foreign and executable children.
	    partition: cardinality; ordering; structural-negative
	    input: Root child sequences with 0/1/many instances.
	    expected: Legal ordered children are preserved; forbidden content rejects with no model.
	  - case_id: SCXML-PARSE-005-CASE-101
	    description: Parser-policy enumeration covers every SCXML element attribute and child at absent, empty, valid, lexical-invalid, semantic-invalid, duplicate, unknown and foreign-qualified partitions.
	    partition: policy-generated; boundary; structural-negative
	    input: Parser policy metadata cross-product.
	    expected: Only policy-authorized forms construct a model; each forbidden form produces a diagnostic and no model.
	  - case_id: SCXML-PARSE-006-CASE-101
	    description: State matrix covers atomic, compound and nested compound state children, singleton/repeated constructs, order, text and foreign content.
	    partition: topology; cardinality; ordering
	    input: State fixtures containing every permitted and forbidden nested construct.
	    expected: Legal state shape is retained in document order; invalid shape rejects.
	  - case_id: SCXML-PARSE-007-CASE-101
	    description: Parallel matrix covers 0/1/2/many regions and all permitted children, including forbidden initial attribute and element forms.
	    partition: cardinality; topology; forbidden-attribute
	    input: Parallel region and child matrix.
	    expected: Legal regions parse; initial forms and illegal children reject.
	  - case_id: SCXML-PARSE-008-CASE-101
	    description: Final-state matrix covers id, entry/exit, done-data cardinality and all forbidden nested structural constructs in each placement.
	    partition: cardinality; placement; structural-negative
	    input: Root, compound and parallel final fixtures.
	    expected: One legal done-data is preserved; repeated/forbidden children reject.
	  - case_id: SCXML-PARSE-009-CASE-101
	    description: Initial/history matrix covers transition cardinality, history type lexical forms, ids and prohibited default-transition attributes.
	    partition: default; lexical-negative; cardinality
	    input: Initial and history element fixtures.
	    expected: Only conforming transition/type forms parse; invalid forms reject.
	  - case_id: SCXML-PARSE-010-CASE-101
	    description: Transition matrix covers event/cond/target/type defaults, XML whitespace tokenization, Unicode, wildcards, duplicates and executable versus structural children.
	    partition: tokenization; defaults; lexical-negative
	    input: Transition attribute and child matrix.
	    expected: Exact tokens and legal actions are retained; illegal types/children reject.
	  - case_id: SCXML-PARSE-011-CASE-101
	    description: Entry/exit matrix preserves exact executable order across empty, repeated, nested conditional/loop and foreign/structural content partitions.
	    partition: ordering; cardinality; structural-negative
	    input: onentry/onexit action sequences.
	    expected: Legal ordered actions are retained; foreign/structural children reject.
	  - case_id: SCXML-PARSE-012-CASE-101
	    description: Raise/log/cancel/assign lexical and exclusivity matrix includes missing data, raw content and foreign-content partitions.
	    partition: required-property; exclusive; lexical-negative
	    input: Executable action attribute matrix.
	    expected: Valid action properties survive exactly; invalid/exclusive forms reject.
	  - case_id: SCXML-PARSE-013-CASE-101
	    description: If/elseif/else branch-marker ordering matrix includes nested and empty branches plus markers outside if.
	    partition: ordering; nesting; structural-negative
	    input: Conditional action sequences.
	    expected: Legal branch order parses; absent/misordered/repeated/outside markers reject.
	  - case_id: SCXML-PARSE-014-CASE-101
	    description: Foreach attribute, variable-edge, executable-child, nesting and illegal-marker matrix.
	    partition: required-property; nesting; structural-negative
	    input: Foreach fixtures.
	    expected: Valid foreach is retained; missing/empty/illegal forms reject.
	  - case_id: SCXML-PARSE-015-CASE-101
	    description: Send literal/expression, id/idlocation, payload, parameter, content and cardinality conflict matrix.
	    partition: exclusive; payload; malformed
	    input: Send attribute and child matrix.
	    expected: Exactly one legal representation per exclusive group is accepted; conflicts reject.
	  - case_id: SCXML-PARSE-016-CASE-101
	    description: Invoke type/src/id/autoforward/payload/finalize matrix with repetition, boolean lexical and illegal-content partitions.
	    partition: exclusive; boolean-lexical; cardinality
	    input: Invoke fixtures.
	    expected: Legal invoke properties retain exact values; invalid/conflicting forms reject.
	  - case_id: SCXML-PARSE-017-CASE-101
	    description: Param/content/donedata/data/datamodel/script matrix covers required, exclusive, repeated, raw XML, whitespace, CDATA, entity and mixed content.
	    partition: required-property; content; cardinality
	    input: Declaration and payload fixtures.
	    expected: Conforming declaration survives; malformed/conflicting forms reject.
	  - case_id: SCXML-PARSE-018-CASE-101
	    description: Foreign executable content losslessness matrix covers namespaces, attributes, nested changes, comments, CDATA, lookalikes, malformed subtree and unsupported providers.
	    partition: extension; lossless; malformed
	    input: Foreign-namespace executable fragments.
	    expected: Supported fragments preserve outer XML/namespaces exactly; malformed or unsupported fragments fail explicitly.
	  - case_id: SCXML-PARSE-019-CASE-101
	    description: Identifier-list tokenization matrix covers all XML whitespace, NBSP, empties, duplicates, URI-like values and Unicode scalar boundaries for initial/target/event/namelist.
	    partition: tokenization; Unicode; boundary
	    input: Identifier-list attributes.
	    expected: XML whitespace separates tokens only; NBSP and invalid/empty lexical forms do not silently change tokens.
	  - case_id: SCXML-PARSE-020-CASE-101
	    description: Delay matrix covers zero, units, sign, leading zero, decimal, whitespace, case, Int32/Int64 limits, overflow and expression form.
	    partition: arithmetic-boundary; lexical-negative; overflow
	    input: delay and delayexpr values.
	    expected: Valid non-overflow delay maps exactly to milliseconds; invalid/overflow never wraps and rejects.
	  - case_id: SCXML-PARSE-021-CASE-101
	    description: Malformed XML and byte/chunk failure matrix covers all structural positions, entity/prefix/encoding failures and incomplete multibyte input.
	    partition: malformed; stream-failure; encoding
	    input: Invalid documents and deterministic failing streams.
	    expected: Diagnostic and no partial model for every failure.
	  - case_id: SCXML-PARSE-022-CASE-101
	    description: Encoding, normalization, newline, control-character, scalar, surrogate and culture-independence matrix.
	    partition: encoding; Unicode; culture
	    input: UTF-8/UTF-16 BOM/declaration and Unicode fixtures.
	    expected: Valid encodings preserve identifiers independently of culture; invalid controls/surrogates reject.
	  - case_id: SCXML-PARSE-023-CASE-101
	    description: Sync/async stream and reader ownership matrix covers chunks, non-seekable/delayed reads, cancellation points, read/dispose faults and retry.
	    partition: cancellation; cleanup; ownership
	    input: Instrumented readers and streams.
	    expected: Correct result or propagated cancellation/fault, exactly-once owned-resource disposal, caller stream retained and no partial model.
	  - case_id: SCXML-PARSE-024-CASE-101
	    description: XML security matrix covers DTD, external entity, XXE disclosure, bounded expansion and cyclic/billion-laughs payloads.
	    partition: security; resource-bound; hostile-input
	    input: Entity and DTD payload corpus.
	    expected: Unsafe expansion/acquisition is blocked with no disclosure or model.
	  - case_id: SCXML-PARSE-025-CASE-101
	    description: XML parser versus equivalent builder graph differential matrix including accepted semantic parity and rejected diagnostic category parity.
	    partition: differential; positive; negative
	    input: Paired XML and public-model fixtures.
	    expected: Canonical semantic models/diagnostic categories match; no route-specific acceptance.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-PARSE-MATRIX-101
	requirement_ids: [SCXML-PARSE-001..025]
	title: Complete parser and XML-policy partition matrix
	description: Exercises every authority-planned parser partition through declarative cases so legal XML produces the exact public model and every invalid/security/stream case produces no partial model.
	authority: { source: W3C SCXML 1.0 and XML 1.0, section: 3.2 and parser matrix 01, citation_or_rule: XML and SCXML lexical/structural conformance }
	phase: 1
	feature: parser-and-xml-policy
	target_components: [ScxmlDirector, ScxmlDeserializer]
	test_kind: matrix
	oracle_type: model-or-diagnostic-and-resource-ledger
	risk: critical
	priority: critical
	construction_routes: [scxml-text, stream, async-reader, public-model]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive, negative, boundary, malformed, cancellation, cleanup, resource, security, reliability]
	dimensions: { input: XML and stream, locale: invariant and non-invariant, encoding: UTF-8/UTF-16 }
	preconditions: [deterministic parser fixture and diagnostic collector]
	dependencies: [PhaseOneContractHarness, instrumented streams, parser-policy metadata]
	arrange: Select one fully specified CASE-METADATA parser fixture.
	stimulus: Parse synchronously or asynchronously at the named cancellation/failure boundary.
	expected: Every case table expectation holds exactly.
	expected_exception_or_event: Case-specific diagnostic or cancellation only.
	forbidden: [partial model, silent lexical normalization, external disclosure, leaked owned resource]
	edge_cases: [Unicode scalar boundaries, overflow, duplicate attributes, one-byte reads]
	determinism: { clock: virtual-or-not-applicable, scheduling: deterministic, timeout_or_step_bound: 256 reads }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [dispose test-owned reader/stream once, assert resource ledger zero]
	resource_risk: stream-and-entity-expansion
	tier: fast
	tags: [Exhaustive, SCXML, Parser, Generated]
	related_tests: [ScxmlParserRequirementsTests]
	known_issue: historical parser defects retained without changing oracle
	compile_notes: PhaseOneContractHarness and parser-policy fixture generator are intentionally unresolved test-side helpers.
	generation_status: generated-uncompiled
	*/
	[DataTestMethod]
	[DynamicData(nameof(ParseCases))]
	public async Task SCXML_PARSE_Matrix_Executes_authority_case(PhaseOneCase @case)
	{
		// Arrange
		var fixture = PhaseOneContractHarness.CreateParserFixture(@case);
		// Act
		var outcome = await PhaseOneContractHarness.ParseAsync(fixture, CancellationToken.None);
		// Assert
		PhaseOneContractHarness.AssertExactOutcome(@case, outcome);
		PhaseOneContractHarness.AssertForbiddenEffectsAbsent(@case, outcome);
		PhaseOneContractHarness.AssertCleanup(@case, fixture);
	}

	public static IEnumerable<object[]> ParseCases => PhaseOneCases.For("SCXML-PARSE");

	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-XINC-001-CASE-101
	    description: Disabled inclusion prohibits direct and indirect resolver acquisition; enabled inclusion resolves relative/absolute href against document URI and nested xml:base.
	    partition: disabled; enabled; nested-base
	    input: XInclude documents and counting resolver.
	    expected: Disabled resolver count is zero; enabled requests exact resolved URIs.
	  - case_id: SCXML-XINC-002-CASE-101
	    description: XInclude 2001/2003 namespace, empty/non-empty/fallback, wrong QName/case and nested include matrix.
	    partition: namespace; fallback; structural-negative
	    input: Include element forms.
	    expected: Supported standard form is processed; unsupported form fails explicitly, never silently passes.
	  - case_id: SCXML-XINC-003-CASE-101
	    description: Href matrix covers absent, empty, fragment, relative/absolute, escaped, malformed, scheme, redirect, absent base and all resolver return shapes.
	    partition: URI; resolver-failure; cleanup
	    input: Include href and instrumented resolver matrix.
	    expected: Invalid/unavailable resource emits wrapped diagnostic and releases acquired resource.
	  - case_id: SCXML-XINC-004-CASE-101
	    description: Parse and text-encoding matrix covers xml/text/omitted/invalid values, charset precedence, BOM, UTF, invalid charset and XML-looking text.
	    partition: lexical-negative; encoding; text
	    input: Include parse/encoding/resource matrix.
	    expected: XML is parsed and text remains text according to contract; unsupported values fail before acquisition.
	  - case_id: SCXML-XINC-005-CASE-101
	    description: Header propagation matrix covers exact-once, empty, repeated, Unicode, injection-like, unsupported resolver and nested headers.
	    partition: headers; nesting; security
	    input: Accept and accept-language values with capable/non-capable resolver.
	    expected: Capable resolver receives exact safe headers once per include; unsupported resolver has no fabricated header side effect.
	  - case_id: SCXML-XINC-006-CASE-101
	    description: Limit/cycle matrix covers limit minus one, equal, plus one, long chains, fan-out, repeated URI and negative/zero/one/default/max values.
	    partition: boundary; cycle; scalability; cleanup
	    input: Bounded include graphs.
	    expected: Excess/cycle fails within bound and every nested reader/stream is closed once.
	  - case_id: SCXML-XINC-007-CASE-101
	    description: Included declaration/doctype/node/fragment/comment/PI/root/namespace/xml:lang/base/depth matrix.
	    partition: node-kind; metadata; namespace
	    input: Included XML corpus.
	    expected: Contractually preserved nodes and metadata retain exact order, language, base URI and depth.
	  - case_id: SCXML-XINC-008-CASE-101
	    description: Acquisition/read/parse/cancel/dispose fault injection at every nesting depth.
	    partition: fault; cancellation; cleanup; resource
	    input: Instrumented nested resolver/reader stack.
	    expected: Original reader remains owned by caller, every acquired resource closes once, cache is empty and no partial model escapes.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-XINC-MATRIX-101
	requirement_ids: [SCXML-XINC-001..008]
	title: Complete XInclude and xml-base partition matrix
	description: Executes all inclusion, URI, header, limit, metadata, fault and cleanup partitions with an instrumented reader stack.
	authority: { source: W3C XInclude and planning matrix 01, section: 2, citation_or_rule: acquisition and inclusion semantics }
	phase: 1
	feature: xinclude
	target_components: [XIncludeReader, XmlBaseReader]
	test_kind: matrix
	oracle_type: reader-trace-diagnostic-resource-ledger
	risk: critical
	priority: critical
	construction_routes: [xml-reader, resolver, stream]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive, negative, boundary, cancellation, cleanup, resource, security, reliability, scalability]
	dimensions: { nesting: finite/default/unlimited, resource: resolver-stream, mode: disabled/enabled }
	preconditions: [instrumented resolver and resource ledger]
	dependencies: [PhaseOneContractHarness, virtual cancellation scheduler]
	arrange: Select one CASE-METADATA include graph and resolver behavior.
	stimulus: Read through inclusion or cancel/fault at the named operation.
	expected: Exact case reader trace, diagnostic, URI/header and resource result.
	expected_exception_or_event: Case-specific XInclude diagnostic or cancellation.
	forbidden: [resolver invocation when disabled, unbounded cycle, partial model, retained resource]
	edge_cases: [MaxInt nesting, fragment href, injection-like headers]
	determinism: { clock: virtual, scheduling: deterministic, timeout_or_step_bound: 512 reads }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [close each acquired resource exactly once, retain caller reader ownership]
	resource_risk: nested-reader-stack
	tier: fast
	tags: [Exhaustive, SCXML, XInclude, Generated]
	related_tests: [XIncludeRequirementsTests]
	known_issue: historical XInclude defects retained without changing oracle
	compile_notes: PhaseOneContractHarness provides future instrumented XInclude adapter and resource ledger.
	generation_status: generated-uncompiled
	*/
	[DataTestMethod]
	[DynamicData(nameof(XIncludeCases))]
	public async Task SCXML_XINC_Matrix_Executes_authority_case(PhaseOneCase @case)
	{
		// Arrange
		var fixture = PhaseOneContractHarness.CreateXIncludeFixture(@case);
		// Act
		var outcome = await PhaseOneContractHarness.ReadIncludeAsync(fixture, CancellationToken.None);
		// Assert
		PhaseOneContractHarness.AssertExactOutcome(@case, outcome);
		PhaseOneContractHarness.AssertForbiddenEffectsAbsent(@case, outcome);
		PhaseOneContractHarness.AssertCleanup(@case, fixture);
	}

	public static IEnumerable<object[]> XIncludeCases => PhaseOneCases.For("SCXML-XINC");

	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-VALID-001-CASE-101
	    description: Required-property matrix for every public-model type covers null, empty, default and valid values plus deterministic aggregation.
	    partition: required-property; aggregate; determinism
	    input: Incomplete and complete public graphs.
	    expected: Every independent omission has a validation diagnostic; no incidental exception replaces it.
	  - case_id: SCXML-VALID-002-CASE-101
	    description: Every mutually-exclusive property group is tested as neither, legal singleton, each pair and all-present.
	    partition: exclusive; cardinality
	    input: Assign/cancel/content/data/invoke/param/script/send conflict matrix.
	    expected: Exactly legal singleton configurations validate; neither/conflict configurations diagnose.
	  - case_id: SCXML-VALID-003-CASE-101
	    description: Root and compound initial selection matrix covers defaults, explicit initial conflict, target count/existence and descendant legality.
	    partition: default; target-resolution; conflict
	    input: Initial and state graph matrix.
	    expected: Valid initial selection resolves exactly; illegal/missing targets diagnose before execution.
	  - case_id: SCXML-VALID-004-CASE-101
	    description: ID matrix covers state/history uniqueness, generated ID stability, empty/Unicode/collisions, references and duplicate target tokens.
	    partition: identifier; generated-value; target-resolution
	    input: Public graphs with explicit/generated IDs.
	    expected: Illegal collision/reference diagnoses; generated identifiers remain stable across builds and serialization.
	  - case_id: SCXML-VALID-005-CASE-101
	    description: Multi-target matrix covers every pair relation and 3+ mixtures across compound and orthogonal parallel regions.
	    partition: topology; multi-target; boundary
	    input: Transition target sets.
	    expected: Only legal orthogonal configuration validates; overlap and conflicting descendants diagnose.
	  - case_id: SCXML-VALID-006-CASE-101
	    description: History placement/default/type/target relation/history-reference matrix.
	    partition: placement; cardinality; lexical-negative
	    input: History nodes and default transitions.
	    expected: Only conforming history relation validates; all forbidden transition/property forms diagnose.
	  - case_id: SCXML-VALID-007-CASE-101
	    description: Final-state, done-data and parent-completion matrix covers placement/cardinality/top-level/outgoing-transition restrictions.
	    partition: placement; cardinality; structural-negative
	    input: Final-state graph matrix.
	    expected: Legal final completion validates; prohibited children/transitions diagnose.
	  - case_id: SCXML-VALID-008-CASE-101
	    description: Executable placement matrix includes finalize raise/send prohibition, branch markers and custom action placement.
	    partition: placement; structural-negative; extension
	    input: Executable-content graph matrix.
	    expected: Only allowed placement validates; forbidden action placement diagnoses.
	  - case_id: SCXML-VALID-009-CASE-101
	    description: Data-model matrix covers omitted, known, alias, unknown, case variant, handler construction, expression/script/custom support and external media types.
	    partition: selection; handler-failure; extension
	    input: Model data-model and handler registry matrix.
	    expected: Supported identifier builds; unknown/unsupported handler/type fails before execution with diagnostic.
	  - case_id: SCXML-VALID-010-CASE-101
	    description: Expression compilation matrix covers every expression-bearing property, lexical namespaces, ownership, failure isolation, cancellation and external acquisition cleanup.
	    partition: compilation; namespace; cancellation; cleanup
	    input: Expressions and instrumented external script/data resources.
	    expected: Correct lexical context compiles; any failure returns no partial compiled model and releases resources.
	  - case_id: SCXML-VALID-011-CASE-101
	    description: Repeated/concurrent build matrix proves stable ordering/IDs, immutable public graph, context isolation and failed-build recovery.
	    partition: concurrency; determinism; isolation
	    input: Shared valid/invalid semantic graphs under deterministic schedules.
	    expected: Equal successful outputs, no mutation/context bleed, and later valid build unaffected by failure.
	  - case_id: SCXML-VALID-012-CASE-101
	    description: XML parse-validate-build versus direct public-model route differential matrix.
	    partition: differential; positive; negative
	    input: Paired semantic graph fixtures.
	    expected: Acceptance, diagnostics and compiled semantics match unless an explicit contract says otherwise.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-VALID-MATRIX-101
	requirement_ids: [SCXML-VALID-001..012]
	title: Complete public-model validation and compiled-model matrix
	description: Covers required, exclusive, graph, expression, handler, concurrent and differential model-build partitions with explicit diagnostics and no-partial-build oracle.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: 3, citation_or_rule: public model validation and model construction }
	phase: 1
	feature: validation-and-build
	target_components: [StateMachineValidator, StateMachineBuilder]
	test_kind: matrix
	oracle_type: diagnostics-compiled-model-differential
	risk: critical
	priority: critical
	construction_routes: [public-object, scxml-text, builder]
	data_models: [null, scxml, custom]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive, negative, boundary, error, cancellation, concurrency, cleanup, resource, reliability]
	dimensions: { graph: atomic/compound/parallel, build: single/repeated/concurrent, expression: namespace-scoped }
	preconditions: [deterministic validator/build harness]
	dependencies: [PhaseOneContractHarness, data-model handler registry, resource ledger]
	arrange: Select one CASE-METADATA graph and handler/resource configuration.
	stimulus: Validate and build once or under the named deterministic concurrent schedule.
	expected: Exact diagnostic set/order or equivalent immutable compiled model.
	expected_exception_or_event: Case-specific validation diagnostic or cancellation.
	forbidden: [null-reference replacement, partial compiled model, graph mutation, context cross-talk, retained resource]
	edge_cases: [Unicode IDs, duplicate tokens, generated IDs, failed concurrent build]
	determinism: { clock: virtual, scheduling: deterministic, timeout_or_step_bound: 64 builds }
	isolation: { parallel_safe: true, shared_state: handler registry reset per case }
	cleanup: [dispose acquired resources and reset handler registry]
	resource_risk: external-script-and-data-acquisition
	tier: fast
	tags: [Exhaustive, SCXML, Validation, Generated]
	related_tests: [StateMachineValidationRequirementsTests]
	known_issue: historical validation defects retained without changing oracle
	compile_notes: PhaseOneContractHarness supplies future compiled-model comparison and data-model registry adapter.
	generation_status: generated-uncompiled
	*/
	[DataTestMethod]
	[DynamicData(nameof(ValidationCases))]
	public async Task SCXML_VALID_Matrix_Executes_authority_case(PhaseOneCase @case)
	{
		// Arrange
		var fixture = PhaseOneContractHarness.CreateValidationFixture(@case);
		// Act
		var outcome = await PhaseOneContractHarness.ValidateAndBuildAsync(fixture, CancellationToken.None);
		// Assert
		PhaseOneContractHarness.AssertExactOutcome(@case, outcome);
		PhaseOneContractHarness.AssertForbiddenEffectsAbsent(@case, outcome);
		PhaseOneContractHarness.AssertCleanup(@case, fixture);
	}

	public static IEnumerable<object[]> ValidationCases => PhaseOneCases.For("SCXML-VALID");

	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-SER-001-CASE-101
	    description: Every model element, attribute, default, optional field, executable child, raw content, namespace and extension is serialized, including root name/binding and assign type/attr.
	    partition: complete-field-coverage; defaults; extensions
	    input: Generated public-model field corpus.
	    expected: Each semantic field is represented in output; omitted name/binding/type/attr is forbidden.
	  - case_id: SCXML-SER-002-CASE-101
	    description: Escaping matrix covers XML-significant text/attributes/raw content, prefixes/rebindings, Unicode, whitespace, CDATA boundaries, comments, URI/token lists, enums and delays.
	    partition: escaping; Unicode; culture; well-formedness
	    input: XML-sensitive serialization values.
	    expected: Output is well formed, culture-independent and reparses to exactly equivalent values without unescaped XML significance.
	  - case_id: SCXML-SER-003-CASE-101
	    description: Semantic model-to-XML-to-model matrix covers valid/generated fixture corpus, ordered children and canonical second serialization.
	    partition: round-trip; ordering; idempotence
	    input: Valid public graphs.
	    expected: Every semantic property and child order matches; second output is canonically identical except documented formatting/prefix freedom.
	  - case_id: SCXML-SER-004-CASE-101
	    description: Imported W3C and generated SCXML execution differential compares original and round-tripped traces, data, outputs and result for identical event streams.
	    partition: execution-differential; corpus
	    input: Imported/generated models and deterministic event streams.
	    expected: Exact equivalent trace/data/output/result; serialization must not change executable semantics.
	  - case_id: SCXML-SER-005-CASE-101
	    description: Invalid/incomplete model, cancellation, writer failure and disposal failure matrix prevents misleading partial documents.
	    partition: error; cancellation; cleanup; writer-failure
	    input: Invalid models and instrumented writer/stream faults.
	    expected: Explicit exception/contractual rejection; no reported success or misleading partial valid document; owned writer closes once.
	  - case_id: SCXML-SER-006-CASE-101
	    description: Text, object and fluent/builder route equivalence matrix covers representative and generated graphs through validate, compile, execute and serialize.
	    partition: route-differential; generated-graph
	    input: Three construction routes for each semantic graph.
	    expected: All routes validate, compile, execute and serialize equivalently.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-SER-MATRIX-101
	requirement_ids: [SCXML-SER-001..006]
	title: Complete serializer and construction-route equivalence matrix
	description: Covers field presence, escaping, round trip, execution equivalence, failure cleanup and construction-route differential partitions with exact semantic output or failure oracle.
	authority: { source: W3C SCXML 1.0 and planning matrix 01, section: 14, citation_or_rule: serialization and route equivalence }
	phase: 1
	feature: serialization
	target_components: [ScxmlSerializerWriter, ScxmlSerializer]
	test_kind: matrix
	oracle_type: well-formed-xml-semantic-differential-and-resource-ledger
	risk: critical
	priority: critical
	construction_routes: [scxml-text, public-object, fluent-builder]
	data_models: [none, xpath]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive, negative, boundary, error, cancellation, cleanup, resource, reliability]
	dimensions: { output: text/xml-writer/stream, content: raw/escaped/extension, route: three-way }
	preconditions: [deterministic XML canonicalizer and execution trace recorder]
	dependencies: [PhaseOneContractHarness, instrumented writer, parser and runtime adapter]
	arrange: Select one CASE-METADATA model, route and output-fault fixture.
	stimulus: Serialize, optionally parse/execute/serialize again, or fault/cancel at named writer operation.
	expected: Exact semantic output, round-trip trace, or explicit failure and cleanup stated by case.
	expected_exception_or_event: Case-specific writer exception or cancellation.
	forbidden: [missing semantic field, malformed XML, unescaped payload, partial-success report, resource leak]
	edge_cases: [CDATA terminator, namespace rebind, Unicode scalar, disposal failure]
	determinism: { clock: virtual, scheduling: deterministic, timeout_or_step_bound: 128 writes }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [flush/dispose owned writer once and assert no retained stream/session]
	resource_risk: writer-and-runtime-session
	tier: fast
	tags: [Exhaustive, SCXML, Serialization, Generated]
	related_tests: [ScxmlSerializerRequirementsTests]
	known_issue: historical serializer defects retained without changing oracle
	compile_notes: PhaseOneContractHarness provides future route-equivalence canonicalizer and faulting writer adapter.
	generation_status: generated-uncompiled
	*/
	[DataTestMethod]
	[DynamicData(nameof(SerializationCases))]
	public async Task SCXML_SER_Matrix_Executes_authority_case(PhaseOneCase @case)
	{
		// Arrange
		var fixture = PhaseOneContractHarness.CreateSerializationFixture(@case);
		// Act
		var outcome = await PhaseOneContractHarness.SerializeAndCompareAsync(fixture, CancellationToken.None);
		// Assert
		PhaseOneContractHarness.AssertExactOutcome(@case, outcome);
		PhaseOneContractHarness.AssertForbiddenEffectsAbsent(@case, outcome);
		PhaseOneContractHarness.AssertCleanup(@case, fixture);
	}

	public static IEnumerable<object[]> SerializationCases => PhaseOneCases.For("SCXML-SER");
}

public sealed record PhaseOneCase(string CaseId, string RequirementId);

internal static class PhaseOneCases
{
	private static readonly PhaseOneCase[] All =
	[
		new("SCXML-PARSE-001-CASE-101", "SCXML-PARSE-001"), new("SCXML-PARSE-002-CASE-101", "SCXML-PARSE-002"), new("SCXML-PARSE-003-CASE-101", "SCXML-PARSE-003"), new("SCXML-PARSE-004-CASE-101", "SCXML-PARSE-004"), new("SCXML-PARSE-005-CASE-101", "SCXML-PARSE-005"),
		new("SCXML-PARSE-006-CASE-101", "SCXML-PARSE-006"), new("SCXML-PARSE-007-CASE-101", "SCXML-PARSE-007"), new("SCXML-PARSE-008-CASE-101", "SCXML-PARSE-008"), new("SCXML-PARSE-009-CASE-101", "SCXML-PARSE-009"), new("SCXML-PARSE-010-CASE-101", "SCXML-PARSE-010"),
		new("SCXML-PARSE-011-CASE-101", "SCXML-PARSE-011"), new("SCXML-PARSE-012-CASE-101", "SCXML-PARSE-012"), new("SCXML-PARSE-013-CASE-101", "SCXML-PARSE-013"), new("SCXML-PARSE-014-CASE-101", "SCXML-PARSE-014"), new("SCXML-PARSE-015-CASE-101", "SCXML-PARSE-015"),
		new("SCXML-PARSE-016-CASE-101", "SCXML-PARSE-016"), new("SCXML-PARSE-017-CASE-101", "SCXML-PARSE-017"), new("SCXML-PARSE-018-CASE-101", "SCXML-PARSE-018"), new("SCXML-PARSE-019-CASE-101", "SCXML-PARSE-019"), new("SCXML-PARSE-020-CASE-101", "SCXML-PARSE-020"),
		new("SCXML-PARSE-021-CASE-101", "SCXML-PARSE-021"), new("SCXML-PARSE-022-CASE-101", "SCXML-PARSE-022"), new("SCXML-PARSE-023-CASE-101", "SCXML-PARSE-023"), new("SCXML-PARSE-024-CASE-101", "SCXML-PARSE-024"), new("SCXML-PARSE-025-CASE-101", "SCXML-PARSE-025"),
		new("SCXML-XINC-001-CASE-101", "SCXML-XINC-001"), new("SCXML-XINC-002-CASE-101", "SCXML-XINC-002"), new("SCXML-XINC-003-CASE-101", "SCXML-XINC-003"), new("SCXML-XINC-004-CASE-101", "SCXML-XINC-004"), new("SCXML-XINC-005-CASE-101", "SCXML-XINC-005"), new("SCXML-XINC-006-CASE-101", "SCXML-XINC-006"), new("SCXML-XINC-007-CASE-101", "SCXML-XINC-007"), new("SCXML-XINC-008-CASE-101", "SCXML-XINC-008"),
		new("SCXML-VALID-001-CASE-101", "SCXML-VALID-001"), new("SCXML-VALID-002-CASE-101", "SCXML-VALID-002"), new("SCXML-VALID-003-CASE-101", "SCXML-VALID-003"), new("SCXML-VALID-004-CASE-101", "SCXML-VALID-004"), new("SCXML-VALID-005-CASE-101", "SCXML-VALID-005"), new("SCXML-VALID-006-CASE-101", "SCXML-VALID-006"), new("SCXML-VALID-007-CASE-101", "SCXML-VALID-007"), new("SCXML-VALID-008-CASE-101", "SCXML-VALID-008"), new("SCXML-VALID-009-CASE-101", "SCXML-VALID-009"), new("SCXML-VALID-010-CASE-101", "SCXML-VALID-010"), new("SCXML-VALID-011-CASE-101", "SCXML-VALID-011"), new("SCXML-VALID-012-CASE-101", "SCXML-VALID-012"),
		new("SCXML-SER-001-CASE-101", "SCXML-SER-001"), new("SCXML-SER-002-CASE-101", "SCXML-SER-002"), new("SCXML-SER-003-CASE-101", "SCXML-SER-003"), new("SCXML-SER-004-CASE-101", "SCXML-SER-004"), new("SCXML-SER-005-CASE-101", "SCXML-SER-005"), new("SCXML-SER-006-CASE-101", "SCXML-SER-006")
	];

	public static IEnumerable<object[]> For(string family) => All.Where(@case => @case.RequirementId.StartsWith(family, System.StringComparison.Ordinal)).Select(@case => new object[] { @case });
}
