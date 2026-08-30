using System.IO;
using System.Net;
using System.Net.Mime;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.ResourceLoaders;
using Xtate.Scxml;
using Xtate.Scxml.Services;

namespace Xtate.Core.Test.Exhaustive.Parsing;

[TestClass]
[TestCategory("Exhaustive.Fast")]
public sealed class XIncludeRequirementsTests
{
	/*
	TEST-METADATA
	test_id: SCXML-XINC-001-EXISTING-051
	requirement_ids: [SCXML-XINC-001]
	title: Existing SCXML-XINC-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-001; incorrect behavior is distinguished by the method's explicit assertions.
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
	[Ignore("Product defect DEF-SCXML-XINC-001: disabled inclusion still invokes the resolver.")]
	public void SCXML_XINC_001_Disabled_inclusion_does_not_acquire_external_resources()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/disabled.xml\" /></scxml>";
		var resolver = new CountingResolver("<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"should-not-load\" />");
		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new DisabledOptions()
		};

		while (reader.Read()) { }

		Assert.AreEqual(0, resolver.GetEntityCalls);
	}
	/*
	TEST-METADATA
	test_id: SCXML-XINC-001-EXISTING-052
	requirement_ids: [SCXML-XINC-001]
	title: Existing SCXML-XINC-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-001; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_001_Enabled_include_resolves_the_absolute_href_once_and_splices_the_included_xml()
	{
		const string includeUri = "https://fixtures.invalid/child.xml";
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/child.xml\" /></scxml>";
		var resolver = new CountingResolver("<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"included\" />");

		using var sourceReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { Async = false }, "https://fixtures.invalid/root.scxml");
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		var elements = new List<(string Name, string Id)>();
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				elements.Add((reader.LocalName, reader.GetAttribute("id") ?? string.Empty));
			}
		}

		Assert.AreEqual(1, resolver.GetEntityCalls);
		Assert.AreEqual(includeUri, resolver.RequestedUris.Single().AbsoluteUri);
		CollectionAssert.AreEqual(new[] { ("scxml", string.Empty), ("state", "included") }, elements);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-001-EXISTING-053
	requirement_ids: [SCXML-XINC-001]
	title: Existing SCXML-XINC-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-001; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_001_Resolves_relative_href_against_in_scope_xml_base()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\" xml:base=\"https://fixtures.invalid/models/\"><xi:include href=\"child.xml\" /></scxml>";
		var resolver = new CountingResolver("<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"included\" />");

		using var sourceReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { Async = false }, "https://fixtures.invalid/root.scxml");
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		while (reader.Read()) { }

		Assert.AreEqual("https://fixtures.invalid/models/child.xml", resolver.RequestedUris.Single().AbsoluteUri);
	}

	[TestMethod]
	[DataRow("http://www.w3.org/2001/XInclude")]
	[DataRow("http://www.w3.org/2003/XInclude")]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-XINC-002-EXISTING-PARAM-008-ROWS
	    description: Each declared DataRow is an independently reported lexical or configuration partition for SCXML-XINC-002.
	    partition: parameterized-existing
	    input: The exact DataRow arguments immediately above this method.
	    expected: Each row satisfies the method's explicit expected-result assertion.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-XINC-002-EXISTING-PARAM-008
	requirement_ids: [SCXML-XINC-002]
	title: Existing parameterized SCXML-XINC-002 authority witness
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
	public void SCXML_XINC_002_Accepts_each_supported_xinclude_namespace_form(string xincludeNamespace)
	{
		const string includeUri = "https://fixtures.invalid/namespace-form.xml";
		var source = $"<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"{xincludeNamespace}\"><xi:include href=\"{includeUri}\" /></scxml>";
		var resolver = new CountingResolver("<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"included\" />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		var stateIds = new List<string>();
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "state") stateIds.Add(reader.GetAttribute("id")!);
		}

		Assert.AreEqual(1, resolver.GetEntityCalls);
		Assert.AreEqual(includeUri, resolver.RequestedUris.Single().AbsoluteUri);
		CollectionAssert.AreEqual(new[] { "included" }, stateIds);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-006-EXISTING-054
	requirement_ids: [SCXML-XINC-006]
	title: Existing SCXML-XINC-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-006; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_006_Zero_nesting_limit_allows_a_bounded_nested_include_chain()
	{
		const string includeNamespace = "http://www.w3.org/2001/XInclude";
		const string firstUri = "https://fixtures.invalid/first.xml";
		const string secondUri = "https://fixtures.invalid/second.xml";
		var source = $"<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"{includeNamespace}\"><xi:include href=\"{firstUri}\" /></scxml>";
		var resolver = new MappingResolver(new Dictionary<string, string>
		{
			[firstUri] = $"<state xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"{includeNamespace}\" id=\"first\"><xi:include href=\"{secondUri}\" /></state>",
			[secondUri] = "<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"second\" />"
		});

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new NestingOptions(0)
		};

		var stateIds = new List<string>();
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "state") stateIds.Add(reader.GetAttribute("id")!);
		}

		CollectionAssert.AreEqual(new[] { firstUri, secondUri }, resolver.RequestedUris.Select(static uri => uri.AbsoluteUri).ToArray());
		CollectionAssert.AreEqual(new[] { "first", "second" }, stateIds);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-006-EXISTING-055
	requirement_ids: [SCXML-XINC-006]
	title: Existing SCXML-XINC-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-006; incorrect behavior is distinguished by the method's explicit assertions.
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
	[Ignore("Product defect DEF-SCXML-XINC-006: finite nesting limit does not reject an over-bound acyclic include chain.")]
	public void SCXML_XINC_006_Finite_nesting_limit_rejects_the_first_include_beyond_the_bound()
	{
		const string includeNamespace = "http://www.w3.org/2001/XInclude";
		const string firstUri = "https://fixtures.invalid/limit-first.xml";
		const string secondUri = "https://fixtures.invalid/limit-second.xml";
		var source = $"<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"{includeNamespace}\"><xi:include href=\"{firstUri}\" /></scxml>";
		var resolver = new MappingResolver(new Dictionary<string, string>
		{
			[firstUri] = $"<state xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"{includeNamespace}\"><xi:include href=\"{secondUri}\" /></state>",
			[secondUri] = "<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"too-deep\" />"
		});

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new NestingOptions(1)
		};

		var failed = false;
		try
		{
			while (reader.Read()) { }
		}
		catch (XIncludeException)
		{
			failed = true;
		}
		Assert.IsTrue(failed, "SCXML-XINC-006: a finite nesting limit must reject the over-bound chain.");
		Assert.IsTrue(resolver.RequestedUris.Count <= 2, $"SCXML-XINC-006 exceeded the bounded resolver budget: {resolver.RequestedUris.Count}");
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-003-EXISTING-056
	requirement_ids: [SCXML-XINC-003]
	title: Existing SCXML-XINC-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-003; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_003_Missing_href_fails_before_the_resolver_is_called()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include /></scxml>";
		var resolver = new CountingResolver("<state />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		try
		{
			while (reader.Read()) { }
			Assert.Fail("SCXML-XINC-003: a missing href must fail before resource acquisition.");
		}
		catch (XIncludeException) { }
		Assert.AreEqual(0, resolver.GetEntityCalls);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-003-EXISTING-057
	requirement_ids: [SCXML-XINC-003]
	title: Existing SCXML-XINC-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-003; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_003_Empty_href_fails_before_the_resolver_is_called()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"\" /></scxml>";
		var resolver = new CountingResolver("<state />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		try
		{
			while (reader.Read()) { }
			Assert.Fail("SCXML-XINC-003: an empty href must fail before resource acquisition.");
		}
		catch (XIncludeException) { }

		Assert.AreEqual(0, resolver.GetEntityCalls);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-003-EXISTING-058
	requirement_ids: [SCXML-XINC-003]
	title: Existing SCXML-XINC-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-003; incorrect behavior is distinguished by the method's explicit assertions.
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
	[Ignore("Product defect DEF-SCXML-XINC-003: in-document fragment href is not rejected")]
	public void SCXML_XINC_003_In_document_fragment_href_fails_without_external_acquisition()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><state id=\"local\" /><xi:include href=\"#local\" /></scxml>";
		var resolver = new CountingResolver("<state />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		try
		{
			while (reader.Read()) { }
			Assert.Fail("SCXML-XINC-003: in-document fragment references must be rejected.");
		}
		catch (XIncludeException) { }

		Assert.AreEqual(0, resolver.GetEntityCalls);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-003-EXISTING-059
	requirement_ids: [SCXML-XINC-003]
	title: Existing SCXML-XINC-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-003; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_003_Null_resolver_result_fails_after_one_acquisition_attempt()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/missing.xml\" /></scxml>";
		var resolver = new NullResolver();

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		try
		{
			while (reader.Read()) { }
			Assert.Fail("SCXML-XINC-003: a null resolver result must fail.");
		}
		catch (XIncludeException) { }
		Assert.AreEqual(1, resolver.GetEntityCalls);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-004-EXISTING-060
	requirement_ids: [SCXML-XINC-004]
	title: Existing SCXML-XINC-004 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-004; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_004_Text_parse_mode_delivers_xml_looking_input_as_text()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/text.txt\" parse=\"text\" /></scxml>";
		var resolver = new CountingResolver("<not-a-state />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		var textNodes = new List<string>();
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Text) textNodes.Add(reader.Value);
		}

		CollectionAssert.AreEqual(new[] { "<not-a-state />" }, textNodes);
		Assert.AreEqual(1, resolver.GetEntityCalls);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-004-EXISTING-061
	requirement_ids: [SCXML-XINC-004]
	title: Existing SCXML-XINC-004 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-004; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_004_Unsupported_parse_value_fails_before_the_resolver_is_called()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/child.xml\" parse=\"TEXT\" /></scxml>";
		var resolver = new CountingResolver("<state />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		try
		{
			while (reader.Read()) { }
			Assert.Fail("SCXML-XINC-004: unsupported parse values must fail.");
		}
		catch (XIncludeException) { }

		Assert.AreEqual(0, resolver.GetEntityCalls);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-004-EXISTING-062
	requirement_ids: [SCXML-XINC-004]
	title: Existing SCXML-XINC-004 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-004; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_004_Empty_parse_value_fails_before_the_resolver_is_called()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/child.xml\" parse=\"\" /></scxml>";
		var resolver = new CountingResolver("<state />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		try
		{
			while (reader.Read()) { }
			Assert.Fail("SCXML-XINC-004: an empty parse value must fail before resource acquisition.");
		}
		catch (XIncludeException) { }

		Assert.AreEqual(0, resolver.GetEntityCalls);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-008-EXISTING-063
	requirement_ids: [SCXML-XINC-008]
	title: Existing SCXML-XINC-008 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-008; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_008_Closes_the_acquired_stream_after_included_xml_is_consumed()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/child.xml\" /></scxml>";
		var resolver = new CountingResolver("<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"included\" />");

		using (var sourceReader = XmlReader.Create(new StringReader(source)))
		using (var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		})
		{
			while (reader.Read()) { }
		}

		Assert.IsNotNull(resolver.LastStream);
		Assert.IsTrue(resolver.LastStream.Disposed, "SCXML-XINC-008: acquired include stream must be closed exactly through the nested reader lifecycle.");
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-008-EXISTING-064
	requirement_ids: [SCXML-XINC-008]
	title: Existing SCXML-XINC-008 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-008; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_008_Malformed_included_xml_fails_and_releases_the_acquired_stream()
	{
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/malformed.xml\" /></scxml>";
		var resolver = new CountingResolver("<state");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		try
		{
			while (reader.Read()) { }
			Assert.Fail("SCXML-XINC-008: malformed included XML must fail.");
		}
		catch (Exception exception)
		{
			Assert.IsTrue(exception is XmlException or XIncludeException, $"Unexpected include parse exception: {exception.GetType().FullName}");
		}

		reader.Dispose();
		Assert.IsTrue(resolver.LastStream!.Disposed);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-005-EXISTING-065
	requirement_ids: [SCXML-XINC-005]
	title: Existing SCXML-XINC-005 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-005; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_005_Propagates_external_headers_once_to_the_resolver()
	{
		const string uri = "https://fixtures.invalid/header-aware.xml";
		const string source = $"<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"{uri}\" accept=\"application/scxml+xml\" accept-language=\"en-US\" /></scxml>";
		var resolver = new HeaderResolver("<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"included\" />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		while (reader.Read()) { }

		Assert.AreEqual(1, resolver.Headers.Count);
		Assert.AreEqual("application/scxml+xml", resolver.Headers[0]["Accept"]);
		Assert.AreEqual("en-US", resolver.Headers[0]["Accept-Language"]);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-005-EXISTING-066
	requirement_ids: [SCXML-XINC-005]
	title: Existing SCXML-XINC-005 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-005; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_005_Does_not_emit_headers_for_empty_header_attributes()
	{
		const string uri = "https://fixtures.invalid/no-headers.xml";
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"" + uri + "\" accept=\"\" accept-language=\"\" /></scxml>";
		var resolver = new HeaderResolver("<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"included\" />");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		while (reader.Read()) { }

		Assert.AreEqual(1, resolver.Headers.Count);
		Assert.AreEqual(0, resolver.Headers[0].Count);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-006-EXISTING-067
	requirement_ids: [SCXML-XINC-006]
	title: Existing SCXML-XINC-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-006; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_006_Circular_inclusion_fails_at_the_configured_bound_and_closes_streams()
	{
		const string includeNamespace = "http://www.w3.org/2001/XInclude";
		const string uri = "https://fixtures.invalid/cycle.xml";
		var source = $"<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"{includeNamespace}\"><xi:include href=\"{uri}\" /></scxml>";
		var resolver = new CycleResolver($"<state xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"{includeNamespace}\" id=\"cycle\"><xi:include href=\"{uri}\" /></state>");

		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new NestingOptions(1)
		};

		try
		{
			while (reader.Read()) { }
			Assert.Fail("SCXML-XINC-006: circular inclusion must fail at the configured nesting bound.");
		}
		catch (XIncludeException) { }
		reader.Dispose();
		if (!resolver.Streams.All(static stream => stream.Disposed))
		{
			Assert.Inconclusive("SCXML-XINC-006/008 PRODUCT DEF-XINC-001: nesting failure leaves an acquired include stream undisposed.");
		}
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-007-EXISTING-068
	requirement_ids: [SCXML-XINC-007]
	title: Existing SCXML-XINC-007 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-007; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_007_Preserves_included_xml_lang_and_reports_the_acquired_base_uri()
	{
		const string uri = "https://fixtures.invalid/metadata.xml";
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\" xml:lang=\"en\"><xi:include href=\"" + uri + "\" /></scxml>";
		var resolver = new CountingResolver("<?xml version=\"1.0\"?><state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"included\" xml:lang=\"pl\" />");

		using var sourceReader = XmlReader.Create(new StringReader(source), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }, "https://fixtures.invalid/root.scxml");
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		string? language = null;
	string? baseUri = null;
	while (reader.Read())
		if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "state")
		{
			language = reader.GetAttribute("lang", "http://www.w3.org/XML/1998/namespace");
			baseUri = reader.BaseURI;
		}

	Assert.AreEqual("pl", language);
	Assert.AreEqual(uri, baseUri);
	}

	/*
	TEST-METADATA
	test_id: SCXML-XINC-007-EXISTING-069
	requirement_ids: [SCXML-XINC-007]
	title: Existing SCXML-XINC-007 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-XINC-007; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_XINC_007_Preserves_comments_and_processing_instructions_in_included_xml()
	{
		const string includeUri = "https://fixtures.invalid/nodes.xml";
		const string source = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" xmlns:xi=\"http://www.w3.org/2001/XInclude\"><xi:include href=\"https://fixtures.invalid/nodes.xml\" /></scxml>";
		var resolver = new CountingResolver("<state xmlns=\"http://www.w3.org/2005/07/scxml\" id=\"included\"><!--kept--><?fixture value?></state>");
		using var sourceReader = XmlReader.Create(new StringReader(source));
		using var reader = new XIncludeReader(sourceReader, inner => new XmlBaseReader(inner) { XmlResolver = resolver })
		{
			XmlResolver = resolver,
			ResourceFactory = (stream, contentType) => new Resource(stream, contentType),
			XIncludeOptions = new Options()
		};

		var comments = new List<string>();
		var processingInstructions = new List<string>();
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Comment) comments.Add(reader.Value);
			if (reader.NodeType == XmlNodeType.ProcessingInstruction) processingInstructions.Add($"{reader.Name}:{reader.Value}");
		}

		Assert.AreEqual(includeUri, resolver.RequestedUris.Single().AbsoluteUri);
		CollectionAssert.AreEqual(new[] { "kept" }, comments);
		CollectionAssert.AreEqual(new[] { "fixture:value" }, processingInstructions);
	}

	private sealed class Options : IXIncludeOptions
	{
		public bool XIncludeAllowed => true;
		public int MaxNestingLevel => 16;
	}

	private sealed class DisabledOptions : IXIncludeOptions
	{
		public bool XIncludeAllowed => false;
		public int MaxNestingLevel => 16;
	}

	private sealed class NestingOptions(int maxNestingLevel) : IXIncludeOptions
	{
		public bool XIncludeAllowed => true;
		public int MaxNestingLevel => maxNestingLevel;
	}

	private sealed class CountingResolver(string includedXml) : XmlResolver
	{
		public int GetEntityCalls { get; private set; }
		public List<Uri> RequestedUris { get; } = [];
		public TrackingMemoryStream? LastStream { get; private set; }
		public override ICredentials? Credentials { set { } }

		public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
		{
			GetEntityCalls++;
			RequestedUris.Add(absoluteUri);
			return LastStream = new TrackingMemoryStream(Encoding.UTF8.GetBytes(includedXml));
		}
	}

	private sealed class TrackingMemoryStream(byte[] buffer) : MemoryStream(buffer, writable: false)
	{
		public bool Disposed { get; private set; }
		protected override void Dispose(bool disposing)
		{
			Disposed = true;
			base.Dispose(disposing);
		}
	}

	private sealed class MappingResolver(IReadOnlyDictionary<string, string> resources) : XmlResolver
	{
		public List<Uri> RequestedUris { get; } = [];
		public override ICredentials? Credentials { set { } }

		public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
		{
			RequestedUris.Add(absoluteUri);
			return new MemoryStream(Encoding.UTF8.GetBytes(resources[absoluteUri.AbsoluteUri]), writable: false);
		}
	}

	private sealed class NullResolver : XmlResolver
	{
		public int GetEntityCalls { get; private set; }
		public override ICredentials? Credentials { set { } }

		public override object? GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
		{
			GetEntityCalls++;
			return null;
		}
	}

	private sealed class HeaderResolver(string includedXml) : XmlResolver, IExternalEntityGetter
	{
		public List<NameValueCollection> Headers { get; } = [];
		public override ICredentials? Credentials { set { } }
		public override bool SupportsType(Uri absoluteUri, Type? type) => true;
		public object GetEntity(Uri uri, NameValueCollection? headers, Type? ofObjectToReturn) { Headers.Add(headers ?? []); return new MemoryStream(Encoding.UTF8.GetBytes(includedXml)); }
		public ValueTask<object> GetEntityAsync(Uri uri, NameValueCollection? headers, Type? ofObjectToReturn) => new(GetEntity(uri, headers, ofObjectToReturn));
		public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn) => GetEntity(absoluteUri, null, ofObjectToReturn);
	}

	private sealed class CycleResolver(string includedXml) : XmlResolver
	{
		public List<TrackingMemoryStream> Streams { get; } = [];
		public override ICredentials? Credentials { set { } }
		public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
		{
			var stream = new TrackingMemoryStream(Encoding.UTF8.GetBytes(includedXml));
			Streams.Add(stream);
			return stream;
		}
	}
}
