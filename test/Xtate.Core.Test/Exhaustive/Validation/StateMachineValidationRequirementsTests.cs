using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.StateMachine;
using Xtate.Core.Test.Exhaustive.Interpreter;

namespace Xtate.Core.Test.Exhaustive.Validation;

[TestClass]
[TestCategory("Exhaustive.Fast")]
public sealed class StateMachineValidationRequirementsTests
{
	/*
	TEST-METADATA
	test_id: SCXML-VALID-001-EXISTING-070
	requirement_ids: [SCXML-VALID-001]
	title: Assign without location and expression reports both required-property diagnostics
	description: An otherwise empty AssignEntity omits both required location and expression properties; correct validation returns exactly two diagnostics, while an incomplete validator reports one, accepts the executable content, or emits unrelated diagnostics.
	authority: { source: W3C SCXML 1.0, section: 3.12.1 assign, citation_or_rule: assign requires a location and an expression/value source under the public model contract }
	phase: 1
	feature: executable-content-validation
	target_components: [StateMachineValidator,AssignEntity]
	test_kind: validator-unit
	oracle_type: exact-diagnostic-count
	risk: high
	priority: high
	construction_routes: [public-object-model]
	data_models: [not-applicable]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative,missing-location,missing-expression]
	dimensions: { executable: assign, location: absent, expression: absent }
	preconditions: [model contains one otherwise-default AssignEntity]
	dependencies: [StateMachineValidationHarness]
	arrange: Construct a model containing a new AssignEntity with no assigned properties.
	stimulus: Validate the public object model once.
	expected: Exactly two diagnostics, one for missing location and one for missing expression/value.
	expected_exception_or_event: none
	forbidden: [zero diagnostics, one merged diagnostic, accepted assign entity, unrelated diagnostic]
	edge_cases: [both independently required properties absent together]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one validation pass' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [validation result and model have no external resources]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Validation, Existing]
	related_tests: [SCXML-VALID-002-EXISTING-075,SCXML-VALID-002-EXISTING-076]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_VALID_001_Reports_each_independent_missing_required_assign_property()
	{
		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(new AssignEntity()));

		Assert.AreEqual(2, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-071
	requirement_ids: [SCXML-VALID-002]
	title: Send rejects simultaneous literal event and event expression
	description: A SendEntity specifies both EventName=event.literal and EventExpression=eventExpression; correct validation produces exactly one mutually-exclusive-field diagnostic, while a wrong validator accepts both or reports unrelated errors.
	authority: { source: W3C SCXML 1.0, section: 6.2 send, citation_or_rule: event and eventexpr are mutually exclusive send attributes }
	phase: 1
	feature: send-validation
	target_components: [StateMachineValidator,SendEntity]
	test_kind: validator-unit
	oracle_type: exact-diagnostic-count
	risk: high
	priority: high
	construction_routes: [public-object-model]
	data_models: [not-applicable]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative,mutually-exclusive-fields]
	dimensions: { event: literal, eventexpr: value-expression }
	preconditions: [otherwise-default SendEntity]
	dependencies: [StateMachineValidationHarness]
	arrange: Construct SendEntity with literal event name and non-null EventExpression.
	stimulus: Validate the public object model once.
	expected: Exactly one diagnostic reports the event/eventexpr conflict.
	expected_exception_or_event: none
	forbidden: [zero diagnostics, more than one unrelated diagnostic, accepted conflicting send fields]
	edge_cases: [nonempty literal alongside syntactically valid expression]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one validation pass' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [validation result and model have no external resources]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_VALID_002_Rejects_send_event_literal_and_expression_together()
	{
		var send = new SendEntity
		{
			EventName = "event.literal",
			EventExpression = new ValueExpression { Expression = "eventExpression" }
		};

		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(send));
		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-072
	requirement_ids: [SCXML-VALID-002]
	title: Cancel rejects an absent literal and expression send ID
	description: A CancelEntity supplies neither sendid nor sendidexpr; correct validation returns exactly one missing-ID diagnostic, while a wrong validator accepts an unspecified cancellation target or emits unrelated diagnostics.
	authority: { source: W3C SCXML 1.0, section: 6.3 cancel, citation_or_rule: cancel identifies the scheduled send through sendid or sendidexpr }
	phase: 1
	feature: cancel-validation
	target_components: [StateMachineValidator,CancelEntity]
	test_kind: validator-unit
	oracle_type: exact-diagnostic-count
	risk: high
	priority: high
	construction_routes: [public-object-model]
	data_models: [not-applicable]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative,missing-send-id]
	dimensions: { sendid: absent, sendidexpr: absent }
	preconditions: [otherwise-default CancelEntity]
	dependencies: [StateMachineValidationHarness]
	arrange: Construct CancelEntity without a literal or expression ID.
	stimulus: Validate the public object model once.
	expected: Exactly one diagnostic reports the missing cancellation send ID.
	expected_exception_or_event: none
	forbidden: [zero diagnostics, accepted unscoped cancellation, unrelated diagnostic]
	edge_cases: [both alternative ID fields absent]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one validation pass' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [validation result and model have no external resources]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_VALID_002_Rejects_cancel_with_neither_literal_nor_expression_id()
	{
		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(new CancelEntity()));
		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-073
	requirement_ids: [SCXML-VALID-002]
	title: Send accepts exactly one literal event field
	description: A SendEntity specifies EventName=event.literal and no EventExpression; correct validation returns no diagnostics, while an over-restrictive validator rejects the legal literal form or mutates the value.
	authority: { source: W3C SCXML 1.0, section: 6.2 send, citation_or_rule: literal event is a valid alternative to eventexpr }
	phase: 1
	feature: send-validation
	target_components: [StateMachineValidator,SendEntity]
	test_kind: validator-unit
	oracle_type: zero-diagnostic-acceptance
	risk: high
	priority: high
	construction_routes: [public-object-model]
	data_models: [not-applicable]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,literal-event]
	dimensions: { event: literal-nonempty, eventexpr: absent }
	preconditions: [otherwise-default SendEntity]
	dependencies: [StateMachineValidationHarness]
	arrange: Construct SendEntity with EventName event.literal only.
	stimulus: Validate the public object model once.
	expected: Zero validation diagnostics.
	expected_exception_or_event: none
	forbidden: [diagnostic, event-name mutation, rejected legal send]
	edge_cases: [literal route without expression]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one validation pass' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [validation result and model have no external resources]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_VALID_002_Accepts_send_with_only_a_literal_event()
	{
		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(new SendEntity { EventName = "event.literal" }));
		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-074
	requirement_ids: [SCXML-VALID-002]
	title: Cancel accepts exactly one literal send ID
	description: A CancelEntity specifies SendId=send-1 and no SendIdExpression; correct validation returns no diagnostics, while an over-restrictive validator rejects the legal literal route or changes its identifier.
	authority: { source: W3C SCXML 1.0, section: 6.3 cancel, citation_or_rule: literal sendid is a valid cancellation identifier }
	phase: 1
	feature: cancel-validation
	target_components: [StateMachineValidator,CancelEntity]
	test_kind: validator-unit
	oracle_type: zero-diagnostic-acceptance
	risk: high
	priority: high
	construction_routes: [public-object-model]
	data_models: [not-applicable]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,literal-send-id]
	dimensions: { sendid: literal-nonempty, sendidexpr: absent }
	preconditions: [otherwise-default CancelEntity]
	dependencies: [StateMachineValidationHarness]
	arrange: Construct CancelEntity with SendId send-1 only.
	stimulus: Validate the public object model once.
	expected: Zero validation diagnostics.
	expected_exception_or_event: none
	forbidden: [diagnostic, send-ID mutation, rejected legal cancel]
	edge_cases: [literal cancellation route without expression]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one validation pass' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [validation result and model have no external resources]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_VALID_002_Accepts_cancel_with_only_a_literal_id()
	{
		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(new CancelEntity { SendId = "send-1" }));
		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-075
	requirement_ids: [SCXML-VALID-002]
	title: Cancel rejects simultaneous literal and expression send IDs
	description: A CancelEntity contains SendId=send-1 and SendIdExpression=sendIdExpression; correct validation returns exactly one mutually-exclusive-ID diagnostic, while a wrong validator accepts ambiguous cancellation targeting.
	authority: { source: W3C SCXML 1.0, section: 6.3 cancel, citation_or_rule: sendid and sendidexpr are mutually exclusive cancellation identifiers }
	phase: 1
	feature: cancel-validation
	target_components: [StateMachineValidator,CancelEntity]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [not-applicable]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative,mutually-exclusive-fields]
	dimensions: { sendid: literal, sendidexpr: value-expression }
	preconditions: [otherwise-default CancelEntity]
	dependencies: [StateMachineValidationHarness]
	arrange: Construct CancelEntity with both alternative identifier fields.
	stimulus: Validate the public object model once.
	expected: Exactly one diagnostic reports the sendid/sendidexpr conflict.
	expected_exception_or_event: none
	forbidden: [zero diagnostics, accepted ambiguous cancellation, unrelated diagnostic]
	edge_cases: [two nonempty valid alternative IDs]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one validation pass' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [validation result and model have no external resources]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_VALID_002_Rejects_cancel_with_both_literal_and_expression_ids()
	{
		var cancel = new CancelEntity
		{
			SendId = "send-1",
			SendIdExpression = new ValueExpression { Expression = "sendIdExpression" }
		};

		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(cancel));
		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-076
	requirement_ids: [SCXML-VALID-002]
	title: Invoke rejects simultaneous literal and expression type fields
	description: An InvokeEntity declares a literal URI type and a TypeExpression; correct validation returns exactly one mutually-exclusive-type diagnostic, while a wrong validator accepts ambiguous provider selection.
	authority: { source: W3C SCXML 1.0, section: 6.4 invoke, citation_or_rule: type and typeexpr are mutually exclusive invocation attributes }
	phase: 1
	feature: invoke-validation
	target_components: [StateMachineValidator,InvokeEntity]
	test_kind: unit
	oracle_type: exact-existing-assertion
	risk: high
	priority: high
	construction_routes: [existing-fixture-route]
	data_models: [not-applicable]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative,mutually-exclusive-fields]
	dimensions: { type: literal-uri, typeexpr: value-expression }
	preconditions: [invoke is contained by one valid state]
	dependencies: [StateMachineValidationHarness]
	arrange: Construct a state with InvokeEntity that has both Type and TypeExpression.
	stimulus: Validate the public object model once.
	expected: Exactly one diagnostic reports the type/typeexpr conflict.
	expected_exception_or_event: none
	forbidden: [zero diagnostics, accepted ambiguous invoke provider, unrelated diagnostic]
	edge_cases: [valid literal URI and syntactically valid expression]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one validation pass' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [validation result and model have no external resources]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_VALID_002_Rejects_invoke_with_both_literal_and_expression_types()
	{
		var invoke = new InvokeEntity
		{
			Type = new FullUri("urn:example:invoke"),
			TypeExpression = new ValueExpression { Expression = "invokeType" }
		};

		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity
		{
			States = [new StateEntity { Id = (Identifier)"state", Invoke = [invoke] }]
		});

		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-077
	requirement_ids: [SCXML-VALID-002]
	title: Existing SCXML-VALID-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_002_Rejects_invoke_with_namelist_and_parameters_together()
	{
		var invoke = new InvokeEntity
		{
			Type = new FullUri("urn:example:invoke"),
			NameList = [new LocationExpression { Expression = "input" }],
			Parameters = [new ParamEntity { Name = "other", Expression = new ValueExpression { Expression = "value" } }]
		};

		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity
		{
			States = [new StateEntity { Id = (Identifier)"state", Invoke = [invoke] }]
		});

		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	[TestMethod]
	[DataRow(true)]
	[DataRow(false)]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-VALID-002-EXISTING-PARAM-009-CASE-001
	    requirement_ids: [SCXML-VALID-002]
	    description: True row supplies an invoke namelist as the only payload form and is accepted.
	    partition: positive-namelist
	    input: useNameList=true
	    stimulus: Validate the constructed public model once.
	    expected: Zero diagnostics with a namelist-only invoke payload.
	    expected_exception_or_event: none
	    forbidden: Rejection, parameters added alongside namelist, or unrelated diagnostic.
	    dimensions: { payload_form: namelist, useNameList: true }
	    risk: medium
	    target_frameworks_platforms: all-project-targets/platform-independent
	    compile_notes: none
	  - case_id: SCXML-VALID-002-EXISTING-PARAM-009-CASE-002
	    requirement_ids: [SCXML-VALID-002]
	    description: False row supplies invoke parameters as the only payload form and is accepted.
	    partition: positive-parameters
	    input: useNameList=false
	    stimulus: Validate the constructed public model once.
	    expected: Zero diagnostics with a parameters-only invoke payload.
	    expected_exception_or_event: none
	    forbidden: Rejection, namelist added alongside parameters, or unrelated diagnostic.
	    dimensions: { payload_form: parameters, useNameList: false }
	    risk: medium
	    target_frameworks_platforms: all-project-targets/platform-independent
	    compile_notes: none
	*/
	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-PARAM-009
	requirement_ids: [SCXML-VALID-002]
	title: Invoke accepts one payload form at a time
	description: The true row supplies only namelist and the false row supplies only parameters; correct validation accepts both legal alternatives, while an incorrect validator rejects either form or treats the forms as an invalid simultaneous payload.
	authority: { source: W3C SCXML 1.0, section: 6.4 invoke, citation_or_rule: namelist and params are individually legal invocation payload forms }
	phase: 1
	feature: invoke-validation
	target_components: [StateMachineValidator,InvokeEntity]
	test_kind: unit
	oracle_type: exact-row-result
	risk: high
	priority: high
	construction_routes: [public-object-model]
	data_models: [not-applicable]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,namelist,parameters]
	dimensions: { input: useNameList-true-or-false }
	preconditions: [invoke contains exactly one payload alternative]
	dependencies: [StateMachineValidationHarness]
	arrange: Construct the row-selected namelist-only or parameters-only invoke.
	stimulus: Validate the public object model once.
	expected: Zero diagnostics for each explicit row.
	expected_exception_or_event: none
	forbidden: [diagnostic, combined payload fields, mutated payload selection]
	edge_cases: [boolean selection of mutually distinct legal routes]
	determinism: { clock: not-applicable, scheduling: synchronous, timeout_or_step_bound: 'one validation pass' }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [validation result and model have no external resources]
	resource_risk: none
	tier: fast
	tags: [Exhaustive, SCXML, Existing, Parameterized]
	related_tests: []
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	public void SCXML_VALID_002_Accepts_each_single_invoke_payload_form(bool useNameList)
	{
		var invoke = new InvokeEntity { Type = new FullUri("urn:example:invoke") };
		if (useNameList)
			invoke.NameList = [new LocationExpression { Expression = "input" }];
		else
			invoke.Parameters = [new ParamEntity { Name = "input", Expression = new ValueExpression { Expression = "value" } }];

		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity
		{
			States = [new StateEntity { Id = (Identifier)"state", Invoke = [invoke] }]
		});

		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-001-EXISTING-078
	requirement_ids: [SCXML-VALID-001]
	title: Existing SCXML-VALID-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-001; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_001_Repeated_validation_preserves_diagnostic_order()
	{
		var model = ModelWith(new AssignEntity());
		var first = StateMachineValidationHarness.Validate(model);
		var second = StateMachineValidationHarness.Validate(model);

		CollectionAssert.AreEqual(first.ToArray(), second.ToArray());
		Assert.AreEqual(2, first.Count);
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-001-EXISTING-079
	requirement_ids: [SCXML-VALID-001]
	title: Existing SCXML-VALID-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-001; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_001_Rejects_param_without_required_name()
	{
		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(new SendEntity
		{
			Parameters = [new ParamEntity { Expression = new ValueExpression { Expression = "value" } }]
		}));

		Assert.IsTrue(diagnostics.Count >= 1, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-080
	requirement_ids: [SCXML-VALID-002]
	title: Existing SCXML-VALID-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_002_Accepts_param_with_only_expression()
	{
		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(new SendEntity
		{
			EventName = "event",
			Parameters = [new ParamEntity { Name = "value", Expression = new ValueExpression { Expression = "source" } }]
		}));

		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-081
	requirement_ids: [SCXML-VALID-002]
	title: Existing SCXML-VALID-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_002_Rejects_content_with_both_expression_and_inline_body()
	{
		var send = new SendEntity
		{
			Content = new ContentEntity
			{
				Expression = new ValueExpression { Expression = "payload" },
				Body = new ContentBody { Value = "inline payload" }
			}
		};

		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(send));
		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-082
	requirement_ids: [SCXML-VALID-002]
	title: Existing SCXML-VALID-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_002_Rejects_content_with_neither_expression_nor_body()
	{
		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(new SendEntity
		{
			EventName = "event",
			Content = new ContentEntity()
		}));

		Assert.AreEqual(2, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-083
	requirement_ids: [SCXML-VALID-002]
	title: Existing SCXML-VALID-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_002_Accepts_content_with_only_inline_body()
	{
		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(new SendEntity
		{
			Content = new ContentEntity { Body = new ContentBody { Value = "payload" } }
		}));

		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-002-EXISTING-084
	requirement_ids: [SCXML-VALID-002]
	title: Existing SCXML-VALID-002 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-002; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_002_Rejects_param_with_both_expression_and_location()
	{
		var send = new SendEntity
		{
			EventName = "event",
			Parameters =
			[
				new ParamEntity
				{
					Name = "payload",
					Expression = new ValueExpression { Expression = "value" },
					Location = new LocationExpression { Expression = "source" }
				}
			]
		};

		var diagnostics = StateMachineValidationHarness.Validate(ModelWith(send));
		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-003-EXISTING-085
	requirement_ids: [SCXML-VALID-003]
	title: Existing SCXML-VALID-003 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-003; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_003_Rejects_root_initial_when_no_root_states_exist()
	{
		var model = new StateMachineEntity
		{
			Initial = new InitialEntity { Transition = new TransitionEntity { Target = [(Identifier)"missing-state"] } }
		};

		var diagnostics = StateMachineValidationHarness.Validate(model);
		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-004-EXISTING-086
	requirement_ids: [SCXML-VALID-004]
	title: Existing SCXML-VALID-004 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-004; incorrect behavior is distinguished by the method's explicit assertions.
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
		[Ignore("Product defect DEF-SCXML-VALID-004: duplicate state identifiers are not diagnosed.")]
	public void SCXML_VALID_004_Rejects_duplicate_state_identifiers()
	{
		var model = new StateMachineEntity
		{
			States =
			[
				new StateEntity { Id = (Identifier)"duplicate" },
				new StateEntity { Id = (Identifier)"duplicate" }
			]
		};

		var diagnostics = StateMachineValidationHarness.Validate(model);
		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-001-EXISTING-087
	requirement_ids: [SCXML-VALID-001]
	title: Existing SCXML-VALID-001 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-001; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_001_Rejects_an_out_of_range_root_binding_enum_value()
	{
		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity { Binding = (BindingType)99 });
		Assert.AreEqual(1, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-006-EXISTING-088
	requirement_ids: [SCXML-VALID-006]
	title: Existing SCXML-VALID-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-006; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_006_Rejects_history_without_default_transition_and_invalid_type()
	{
		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity
		{
			States = [new StateEntity { Id = (Identifier)"compound", HistoryStates = [new HistoryEntity { Type = (HistoryType)99 }] }]
		});

		Assert.AreEqual(2, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-006-EXISTING-089
	requirement_ids: [SCXML-VALID-006]
	title: Existing SCXML-VALID-006 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-006; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_006_Accepts_shallow_history_with_one_default_target()
	{
		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity
		{
			States =
			[
				new StateEntity
				{
					Id = (Identifier)"compound",
					States = [new StateEntity { Id = (Identifier)"child" }],
					HistoryStates =
					[
						new HistoryEntity
						{
							Id = (Identifier)"resume",
							Type = HistoryType.Shallow,
							Transition = new TransitionEntity { Target = [(Identifier)"child"] }
						}
					]
				}
			]
		});

		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-008-EXISTING-090
	requirement_ids: [SCXML-VALID-008]
	title: Existing SCXML-VALID-008 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-008; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_008_Rejects_raise_and_send_inside_finalize()
	{
		var finalize = new FinalizeEntity
		{
			Action = [new RaiseEntity { OutgoingEvent = new EventEntity("forbidden") }, new SendEntity { EventName = "also-forbidden" }]
		};
		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity
		{
			States = [new StateEntity { Id = (Identifier)"state", Invoke = [new InvokeEntity { Type = new FullUri("urn:test"), Finalize = finalize }] }]
		});

		Assert.AreEqual(2, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-007-EXISTING-091
	requirement_ids: [SCXML-VALID-007]
	title: Existing SCXML-VALID-007 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-007; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_007_Accepts_a_final_state_with_valid_done_data()
	{
		var final = new FinalEntity
		{
			Id = (Identifier)"complete",
			DoneData = new DoneDataEntity
			{
				Parameters = [new ParamEntity { Name = "result", Expression = new ValueExpression { Expression = "value" } }]
			}
		};
		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity { States = [final] });

		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-005-EXISTING-092
	requirement_ids: [SCXML-VALID-005]
	title: Existing SCXML-VALID-005 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-005; incorrect behavior is distinguished by the method's explicit assertions.
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
	public void SCXML_VALID_005_Accepts_distinct_children_as_orthogonal_parallel_regions()
	{
		var diagnostics = StateMachineValidationHarness.Validate(new StateMachineEntity
		{
			States =
			[
				new ParallelEntity
				{
					Id = (Identifier)"parallel",
					States =
					[
						new StateEntity { Id = (Identifier)"left" },
						new StateEntity { Id = (Identifier)"right" }
					]
				}
			]
		});

		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-011-EXISTING-093
	requirement_ids: [SCXML-VALID-011]
	title: Existing SCXML-VALID-011 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-011; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_VALID_011_Repeated_and_concurrent_build_validation_is_stable_and_does_not_mutate_the_public_graph()
	{
		var model = new StateMachineEntity
		{
			Initial = new InitialEntity { Transition = new TransitionEntity { Target = [(Identifier)"ready"] } },
			States = [new StateEntity { Id = (Identifier)"ready" }]
		};
		var before = (model.Initial!.Transition!.Target!, model.States![0].Id);

		var results = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => StateMachineValidationHarness.Validate(model))));

		foreach (var result in results)
			CollectionAssert.AreEqual(Array.Empty<string>(), result.ToArray());
		Assert.AreEqual(before.Item1[0], model.Initial.Transition.Target[0]);
		Assert.AreEqual(before.Item2, model.States[0].Id);
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-011-EXISTING-094
	requirement_ids: [SCXML-VALID-011]
	title: Existing SCXML-VALID-011 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-011; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_VALID_011_A_failed_validation_does_not_poison_a_later_valid_build()
	{
		var invalid = new StateMachineEntity
		{
			States = [new StateEntity { Id = (Identifier)"same" }, new StateEntity { Id = (Identifier)"same" }]
		};
		var valid = new StateMachineEntity { States = [new StateEntity { Id = (Identifier)"fresh" }] };

		_ = StateMachineValidationHarness.Validate(invalid);
		var diagnostics = await Task.Run(() => StateMachineValidationHarness.Validate(valid));

		Assert.AreEqual(0, diagnostics.Count, string.Join(" | ", diagnostics));
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-012-EXISTING-095
	requirement_ids: [SCXML-VALID-012]
	title: Existing SCXML-VALID-012 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-012; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_VALID_012_Xml_parse_and_direct_public_model_validation_agree_on_a_valid_scenario()
	{
		const string xml = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" initial=\"ready\"><state id=\"ready\" /></scxml>";
		var parsed = await Xtate.Core.Test.Exhaustive.Parsing.ScxmlParserHarness.ParseAsync(xml);
		var direct = new StateMachineEntity
		{
			Initial = new InitialEntity { Transition = new TransitionEntity { Target = [(Identifier)"ready"] } },
			States = [new StateEntity { Id = (Identifier)"ready" }]
		};

		Assert.IsTrue(parsed.Accepted, string.Join(" | ", parsed.Diagnostics));
		var directDiagnostics = StateMachineValidationHarness.Validate(direct);
		var parsedDiagnostics = StateMachineValidationHarness.Validate(parsed.Model!);
		CollectionAssert.AreEqual(directDiagnostics.ToArray(), parsedDiagnostics.ToArray());
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-009-EXISTING-096
	requirement_ids: [SCXML-VALID-009]
	title: Existing SCXML-VALID-009 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-009; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_VALID_009_Unknown_data_model_type_fails_during_model_execution()
	{
		const string xml = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" datamodel=\"urn:unsupported:datamodel\"><final id=\"done\" /></scxml>";
		var failed = false;

		try
		{
			await ScxmlRuntimeHarness.ExecuteAsync(xml);
		}
		catch (Exception)
		{
			failed = true;
		}

		Assert.IsTrue(failed, "SCXML-VALID-009: an unsupported data-model type must not execute as if supported.");
	}

	[TestMethod]
	[Ignore("Product defect DEF-SCXML-VALID-009: the built-in SCXML data-model identifiers fail runtime handler construction.")]
	[DataRow("scxml")]
	[DataRow("http://www.w3.org/TR/scxml/")]
	/*
	CASE-METADATA
	cases:
	  - case_id: SCXML-VALID-009-EXISTING-PARAM-010-ROWS
	    description: Each declared DataRow is an independently reported lexical or configuration partition for SCXML-VALID-009.
	    partition: parameterized-existing
	    input: The exact DataRow arguments immediately above this method.
	    expected: Each row satisfies the method's explicit expected-result assertion.
	*/
	/*
	TEST-METADATA
	test_id: SCXML-VALID-009-EXISTING-PARAM-010
	requirement_ids: [SCXML-VALID-009]
	title: Existing parameterized SCXML-VALID-009 authority witness
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
	public async Task SCXML_VALID_009_Supported_data_model_type_and_alias_execute(string dataModelType)
	{
		var xml = $"<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\" datamodel=\"{dataModelType}\"><final id=\"done\" /></scxml>";

		try
		{
			await ScxmlRuntimeHarness.ExecuteAsync(xml);
		}
		catch (Exception exception)
		{
			Assert.Fail($"SCXML-VALID-009: supported data-model '{dataModelType}' was rejected: {exception.GetType().Name}: {exception.Message}");
		}
	}

	/*
	TEST-METADATA
	test_id: SCXML-VALID-010-EXISTING-097
	requirement_ids: [SCXML-VALID-010]
	title: Existing SCXML-VALID-010 authority witness
	description: Existing exhaustive witness retains its concrete assertion and maps it to SCXML-VALID-010; incorrect behavior is distinguished by the method's explicit assertions.
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
	public async Task SCXML_VALID_010_Invalid_expression_fails_during_compilation_instead_of_executing_partially()
	{
		const string xml = "<scxml xmlns=\"http://www.w3.org/2005/07/scxml\" version=\"1.0\"><state id=\"state\"><onentry><log expr=\"(\" /></onentry></state></scxml>";
		var failed = false;

		try
		{
			await ScxmlRuntimeHarness.ExecuteAsync(xml);
		}
		catch (Exception)
		{
			failed = true;
		}

		Assert.IsTrue(failed, "SCXML-VALID-010: an invalid expression must fail compilation/execution.");
	}

	private static IStateMachine ModelWith(IExecutableEntity action) => new StateMachineEntity
	{
		States =
		[
			new StateEntity
			{
				Id = (Identifier)"state",
				OnEntry = [new OnEntryEntity { Action = [action] }]
			}
		]
	};
}
