using System.Text;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.Scxml.Services;
using Xtate.StateMachine;
using Xtate.Core.Test.Exhaustive.Parsing;
using Xtate.Core.Test.Exhaustive.Interpreter;
using Xtate.StateMachine.Builder.Services;

namespace Xtate.Core.Test.Exhaustive.Serialization;

[TestClass]
[TestCategory("Exhaustive.Fast")]
public sealed class ScxmlSerializerRequirementsTests
{
	/*
	TEST-METADATA
	test_id: SCXML-SER-001-EXISTING-098
	requirement_ids: [SCXML-SER-001]
	title: Existing SCXML-SER-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-001; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_SER_001_Emits_the_required_scxml_namespace_and_version()
	{
		var document = Serialize(new StateMachineEntity { States = [new StateEntity { Id = (Identifier)"idle" }] });
		var root = document.DocumentElement!;

		Assert.AreEqual("http://www.w3.org/2005/07/scxml", root.NamespaceURI);
		Assert.AreEqual("1.0", root.GetAttribute("version"));
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-001-EXISTING-099
	requirement_ids: [SCXML-SER-001]
	title: Existing SCXML-SER-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-001; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_SER_001_Serializes_root_name_binding_and_ordered_state_children()
	{
		var machine = new StateMachineEntity
		{
			Name = "fixture",
			Binding = BindingType.Late,
			States = [new StateEntity { Id = (Identifier)"idle" }]
		};

		var document = Serialize(machine);
		var root = document.DocumentElement!;

		Assert.AreEqual("scxml", root.LocalName);
		Assert.AreEqual("fixture", root.GetAttribute("name"));
		Assert.AreEqual("late", root.GetAttribute("binding"));
		Assert.AreEqual("idle", ((XmlElement)root.FirstChild!).GetAttribute("id"));
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-001-EXISTING-100
	requirement_ids: [SCXML-SER-001]
	title: Existing SCXML-SER-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-001; incorrect behavior is distinguished by the method's explicit assertions.
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
		[Ignore("Product defect DEF-SCXML-SER-001: serializer omits public model properties.")]
	public void SCXML_SER_001_Preserves_every_public_assign_property_including_xpath_type_and_attr()
	{
		var assign = new AssignEntity
		{
			Location = new LocationExpression { Expression = "item/@name" },
			Expression = new ValueExpression { Expression = "'Ada'" },
			Type = "replacechildren",
			Attribute = "xml:lang"
		};
		var machine = new StateMachineEntity
		{
			States =
			[
				new StateEntity
				{
					Id = (Identifier)"idle",
					OnEntry = [new OnEntryEntity { Action = [assign] }]
				}
			]
		};

		var assignElement = (XmlElement)Serialize(machine).SelectSingleNode("/*[local-name()='scxml']/*[local-name()='state']/*[local-name()='onentry']/*[local-name()='assign']")!;

		Assert.AreEqual("item/@name", assignElement.GetAttribute("location"));
		Assert.AreEqual("'Ada'", assignElement.GetAttribute("expr"));
		Assert.AreEqual("replacechildren", assignElement.GetAttribute("type"), "SCXML-SER-001: serializer must not lose the XPath assignment action.");
		Assert.AreEqual("xml:lang", assignElement.GetAttribute("attr"), "SCXML-SER-001: serializer must not lose the XPath assignment attribute.");
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-002-EXISTING-101
	requirement_ids: [SCXML-SER-002]
	title: Existing SCXML-SER-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_SER_002_Escapes_attribute_values_and_produces_well_formed_xml()
	{
		var machine = new StateMachineEntity
		{
			States =
			[
				new StateEntity
				{
					Id = (Identifier)"state",
					OnEntry = [new OnEntryEntity { Action = [new LogEntity { Label = "A & B < C \"quoted\"" }] }]
				}
			]
		};

		var document = Serialize(machine);
		var log = (XmlElement)document.SelectSingleNode("/*[local-name()='scxml']/*[local-name()='state']/*[local-name()='onentry']/*[local-name()='log']")!;
		Assert.AreEqual("A & B < C \"quoted\"", log.GetAttribute("label"));
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-002-EXISTING-102
	requirement_ids: [SCXML-SER-002]
	title: Existing SCXML-SER-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	[Ignore("Product defect DEF-SCXML-SER-002: inline payload text is not XML-escaped")]
	public void SCXML_SER_002_Preserves_xml_significant_inline_payload_text()
	{
		var machine = new StateMachineEntity
		{
			States =
			[
				new StateEntity
				{
					Id = (Identifier)"state",
					OnEntry = [new OnEntryEntity { Action = [new SendEntity { EventName = "payload", Content = new ContentEntity { Body = new ContentBody { Value = "A & B < C" } } }] }]
				}
			]
		};

		var document = Serialize(machine);
		var content = (XmlElement)document.SelectSingleNode("/*[local-name()='scxml']/*[local-name()='state']/*[local-name()='onentry']/*[local-name()='send']/*[local-name()='content']")!;
		Assert.AreEqual("A & B < C", content.InnerText);
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-002-EXISTING-103
	requirement_ids: [SCXML-SER-002]
	title: Existing SCXML-SER-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_SER_002_Formats_delays_deterministically_in_seconds_or_milliseconds()
	{
		var machine = new StateMachineEntity
		{
			States =
			[
				new StateEntity
				{
					Id = (Identifier)"state",
					OnEntry =
					[
						new OnEntryEntity { Action = [new SendEntity { EventName = "one", DelayMs = 2000 }, new SendEntity { EventName = "two", DelayMs = 125 }] }
					]
				}
			]
		};

		var sends = Serialize(machine).SelectNodes("/*[local-name()='scxml']/*[local-name()='state']/*[local-name()='onentry']/*[local-name()='send']")!;
		Assert.AreEqual("2s", ((XmlElement)sends[0]!).GetAttribute("delay"));
		Assert.AreEqual("125ms", ((XmlElement)sends[1]!).GetAttribute("delay"));
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-002-EXISTING-104
	requirement_ids: [SCXML-SER-002]
	title: Existing SCXML-SER-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_SER_002_Preserves_send_uri_and_namelist_attributes()
	{
		var machine = new StateMachineEntity
		{
			States =
			[
				new StateEntity
				{
					Id = (Identifier)"state",
					OnEntry =
					[
						new OnEntryEntity
						{
							Action =
							[
								new SendEntity
								{
									EventName = "notice",
									Target = new FullUri("#_parent"),
									Type = new FullUri("urn:example:event"),
									NameList = [new LocationExpression { Expression = "first" }, new LocationExpression { Expression = "second" }]
								}
							]
						}
					]
				}
			]
		};

		var send = (XmlElement)Serialize(machine).SelectSingleNode("/*[local-name()='scxml']/*[local-name()='state']/*[local-name()='onentry']/*[local-name()='send']")!;
		Assert.AreEqual("#_parent", send.GetAttribute("target"));
		Assert.AreEqual("urn:example:event", send.GetAttribute("type"));
		Assert.AreEqual("first second", send.GetAttribute("namelist"));
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-002-EXISTING-105
	requirement_ids: [SCXML-SER-002]
	title: Existing SCXML-SER-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_SER_002_Preserves_enabled_invoke_autoforward()
	{
		var machine = new StateMachineEntity
		{
			States = [new StateEntity { Id = (Identifier)"state", Invoke = [new InvokeEntity { AutoForward = true }] }]
		};

		var invoke = (XmlElement)Serialize(machine).SelectSingleNode("/*[local-name()='scxml']/*[local-name()='state']/*[local-name()='invoke']")!;
		Assert.AreEqual("true", invoke.GetAttribute("autoforward"));
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-003-EXISTING-106
	requirement_ids: [SCXML-SER-003]
	title: Existing SCXML-SER-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-003; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_SER_003_Round_trips_a_minimal_state_identity_through_the_public_parser_seam()
	{
		var machine = new StateMachineEntity
		{
			States = [new StateEntity { Id = (Identifier)"idle" }]
		};

		var result = await ScxmlParserHarness.ParseAsync(SerializeText(machine));

		Assert.IsTrue(result.Accepted, string.Join(" | ", result.Diagnostics));
		var model = result.Model!;
		Assert.AreEqual(1, model.States.Count());
		Assert.AreEqual("idle", model.States[0].Id!.Value);
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-003-EXISTING-107
	requirement_ids: [SCXML-SER-003]
	title: Existing SCXML-SER-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-003; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_SER_003_Round_trips_transition_events_targets_and_internal_type()
	{
		var machine = new StateMachineEntity
		{
			States =
			[
				new StateEntity
				{
					Id = (Identifier)"source",
					Transitions =
					[
						new TransitionEntity
						{
							EventDescriptors = [(EventDescriptor)"go", (EventDescriptor)"retry"],
							Target = [(Identifier)"first", (Identifier)"second"],
							Type = TransitionType.Internal
						}
					]
				}
			]
		};

		var result = await ScxmlParserHarness.ParseAsync(SerializeText(machine));

		Assert.IsTrue(result.Accepted, string.Join(" | ", result.Diagnostics));
		var state = (IState)result.Model!.States[0];
		var transition = state.Transitions[0];
		Assert.AreEqual(TransitionType.Internal, transition.Type);
		CollectionAssert.AreEqual(new[] { "go", "retry" }, transition.EventDescriptors.Select(descriptor => descriptor.Value).ToArray());
		CollectionAssert.AreEqual(new[] { "first", "second" }, transition.Target.Select(identifier => identifier.Value).ToArray());
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-003-EXISTING-108
	requirement_ids: [SCXML-SER-003]
	title: Existing SCXML-SER-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-003; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_SER_003_Serializing_a_parsed_model_is_idempotent_for_semantic_attributes()
	{
		const string xml = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" name=\"fixture\"><state id=\"idle\"><transition event=\"go\" target=\"done\" /></state><final id=\"done\" /></scxml>";
		var first = await ScxmlParserHarness.ParseAsync(xml);
		Assert.IsTrue(first.Accepted, string.Join(" | ", first.Diagnostics));

		var second = await ScxmlParserHarness.ParseAsync(SerializeText(first.Model!));
		Assert.IsTrue(second.Accepted, string.Join(" | ", second.Diagnostics));
		Assert.AreEqual(first.Model!.Name, second.Model!.Name);
		Assert.AreEqual(first.Model.States.Count(), second.Model.States.Count());
		CollectionAssert.AreEqual(first.Model.States.Select(static state => state.Id!.Value).ToArray(), second.Model.States.Select(static state => state.Id!.Value).ToArray());
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-004-EXISTING-109
	requirement_ids: [SCXML-SER-004]
	title: Existing SCXML-SER-004 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-004; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_SER_004_Executing_a_round_tripped_model_matches_the_original_result()
	{
		const string xml = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" initial=\"start\"><state id=\"start\"><transition target=\"done\" /></state><final id=\"done\" /></scxml>";
		var parsed = await ScxmlParserHarness.ParseAsync(xml);
		Assert.IsTrue(parsed.Accepted, string.Join(" | ", parsed.Diagnostics));

		var originalResult = await ScxmlRuntimeHarness.ExecuteAsync(xml);
		var roundTrippedResult = await ScxmlRuntimeHarness.ExecuteAsync(SerializeText(parsed.Model!));

		Assert.AreEqual(originalResult, roundTrippedResult, "SCXML-SER-004: execution result changed after serialization.");
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-005-EXISTING-110
	requirement_ids: [SCXML-SER-005]
	title: Existing SCXML-SER-005 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-005; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_SER_005_Propagates_a_writer_failure_instead_of_reporting_success()
	{
		var machine = new StateMachineEntity { States = [new StateEntity { Id = (Identifier)"state" }] };
		using var stream = new FailingWriteStream();
		using var writer = XmlWriter.Create(stream, new XmlWriterSettings { OmitXmlDeclaration = true });

		var failed = false;
		try
		{
			new ScxmlSerializerWriter(writer).Serialize(machine);
			writer.Flush();
		}
		catch (IOException)
		{
			failed = true;
		}

		Assert.IsTrue(failed, "SCXML-SER-005: a writer failure must be observable to the caller.");
	}

	/*
	TEST-METADATA
	test_id: SCXML-SER-006-EXISTING-111
	requirement_ids: [SCXML-SER-006]
	title: Existing SCXML-SER-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-SER-006; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_SER_006_Builder_route_matches_text_route_for_a_minimal_graph()
	{
		var stateBuilder = new StateBuilder();
		stateBuilder.SetId((Identifier)"ready");
		var machineBuilder = new StateMachineBuilder();
		machineBuilder.SetInitial([(Identifier)"ready"]);
		machineBuilder.AddState(stateBuilder.Build());
		var builderModel = machineBuilder.Build();

		var text = SerializeText(builderModel);
		var parsed = await ScxmlParserHarness.ParseAsync(text);

		Assert.IsTrue(parsed.Accepted, string.Join(" | ", parsed.Diagnostics));
		Assert.AreEqual(builderModel.Initial!.Transition!.Target[0], parsed.Model!.Initial!.Transition!.Target[0]);
		Assert.AreEqual(builderModel.States[0].Id, parsed.Model.States[0].Id);
	}

	private static XmlDocument Serialize(IStateMachine machine)
	{
		var document = new XmlDocument();
		document.LoadXml(SerializeText(machine));
		return document;
	}

	private static string SerializeText(IStateMachine machine)
	{
		var output = new StringBuilder();
		using (var writer = XmlWriter.Create(output, new XmlWriterSettings { OmitXmlDeclaration = true }))
		{
			new ScxmlSerializerWriter(writer).Serialize(machine);
		}

		return output.ToString();
	}

	private sealed class FailingWriteStream : MemoryStream
	{
		public override void Write(byte[] buffer, int offset, int count) => throw new IOException("deterministic serializer failure");
	}
}
