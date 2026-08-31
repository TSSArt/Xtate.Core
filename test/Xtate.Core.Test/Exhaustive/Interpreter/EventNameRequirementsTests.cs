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

using Xtate.StateMachine;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

[TestClass]
[TestCategory("Exhaustive.Fast")]
public sealed class EventNameRequirementsTests
{
	/*
	TEST-METADATA
	test_id: SCXML-EVENT-001-EXISTING-001
	requirement_ids: [SCXML-EVENT-001]
	title: Event name segment preservation
	description: Constructs default and segmented event names and distinguishes preservation from normalization or loss of empty segments.
	authority: { source: W3C SCXML 1.0, section: 5.10.1 Events, citation_or_rule: Event names are dot-separated identifiers represented without implicit normalization. }
	phase: 2
	feature: event-name
	target_components: [EventName]
	test_kind: unit
	oracle_type: exact-value
	risk: high
	priority: high
	construction_routes: [public-api]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,boundary,unicode]
	dimensions: { name_shape: default-single-multi-empty-segments-unicode }
	preconditions: [none]
	dependencies: [none]
	arrange: Construct EventName from each declared source string.
	stimulus: Query default flag, segment count, and string form.
	expected: Each DataRow has its exact expected flag, count, and round-trip value.
	expected_exception_or_event: none
	forbidden: Implicit case folding, Unicode normalization, or empty-segment loss.
	edge_cases: Leading, trailing and consecutive dots.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 8 rows }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [No retained resource]
	resource_risk: none
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-TRANS-002-EXISTING-001]
	known_issue: DEF-SCXML-EVENT-001 for default Count query
	compile_notes: none
	generation_status: existing-annotated
	*/
	/* CASE-METADATA: DataRows SCXML-EVENT-001-EXISTING-001-CASE-001..008 map in declaration order; each has exact source/isDefault/count/string expected values and the same positive/boundary/unicode partitions. */
	[TestMethod]
	[DataRow(null, true, 0, null)]
	[DataRow("", false, 0, "")]
	[DataRow("alpha", false, 1, "alpha")]
	[DataRow("alpha.beta.gamma", false, 3, "alpha.beta.gamma")]
	[DataRow(".alpha.", false, 3, ".alpha.")]
	[DataRow("alpha..beta", false, 3, "alpha..beta")]
	[DataRow("cafe\u0301", false, 1, "cafe\u0301")]
	[DataRow("CAFÉ", false, 1, "CAFÉ")]
	public void SCXML_EVENT_001_Event_name_preserves_the_specified_segment_sequence(string? source,
																					bool isDefault,
																					int count,
																					string expected)
	{
		var name = EventName.FromString(source);

		Assert.AreEqual(isDefault, name.IsDefault);
		Assert.AreEqual(count, name.Count);
		Assert.AreEqual(expected, name.ToString());
	}

	/*
	TEST-METADATA
	test_id: SCXML-TRANS-002-EXISTING-001
	requirement_ids: [SCXML-TRANS-002]
	title: Hierarchical event descriptor matching
	description: Compares each descriptor against an event name to distinguish SCXML prefix/wildcard rules from near-prefix, case, and Unicode false matches.
	authority: { source: W3C SCXML 1.0, section: 3.12.1 event matching, citation_or_rule: Descriptor tokens match exact names or dot-boundary descendants. }
	phase: 2
	feature: transition-selection
	target_components: [EventName]
	test_kind: unit
	oracle_type: exact-boolean-matrix
	risk: high
	priority: high
	construction_routes: [public-api]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive,negative,unicode,case]
	dimensions: { descriptor: exact-prefix-wildcard-empty }
	preconditions: [none]
	dependencies: [none]
	arrange: Construct the case event name and descriptor.
	stimulus: Call IsMatchedToEventDescriptor.
	expected: Every DynamicData case equals its declared boolean.
	expected_exception_or_event: none
	forbidden: foo matching foobar, case folding, or Unicode normalization.
	edge_cases: Wildcard and whitespace descriptors.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 14 rows }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [No retained resource]
	resource_risk: none
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-PHASE2-REMAINING-CASE-TABLE]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	/* CASE-METADATA: EventDescriptorCases supplies SCXML-TRANS-002-EXISTING-001-CASE-001..014 in method order; each row's input eventName/descriptor and expected boolean is its exact case oracle. */
	[TestMethod]
	[DynamicData(nameof(EventDescriptorCases))]
	public void SCXML_TRANS_002_Event_descriptor_matching_follows_the_SCXML_hierarchical_name_rules(string eventName, string descriptor, bool expected)
	{
		var actual = EventName.FromString(eventName).IsMatchedToEventDescriptor(descriptor);

		Assert.AreEqual(expected, actual, $"event={eventName}; descriptor={descriptor}");
	}

	/*
	TEST-METADATA
	test_id: SCXML-TRANS-002-EXISTING-002
	requirement_ids: [SCXML-TRANS-002]
	title: Default event rejects external descriptors
	description: Proves that an absent event name cannot be treated as a wildcard or named external event.
	authority: { source: W3C SCXML 1.0, section: 3.12.1 event matching, citation_or_rule: No default event name matches an external descriptor. }
	phase: 2
	feature: transition-selection
	target_components: [EventName]
	test_kind: unit
	oracle_type: exact-boolean
	risk: medium
	priority: high
	construction_routes: [default-value]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [negative]
	dimensions: { event: default, descriptor: wildcard-and-literal }
	preconditions: [none]
	dependencies: [none]
	arrange: Use default EventName.
	stimulus: Match wildcard and literal descriptors.
	expected: Both matches are false.
	expected_exception_or_event: none
	forbidden: Wildcard selection of a default event.
	edge_cases: Default value rather than empty string.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 2 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [No retained resource]
	resource_risk: none
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-TRANS-002-EXISTING-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_TRANS_002_Default_event_does_not_match_an_external_event_descriptor()
	{
		Assert.IsFalse(default(EventName).IsMatchedToEventDescriptor("*"));
		Assert.IsFalse(default(EventName).IsMatchedToEventDescriptor("alpha"));
	}

	/*
	TEST-METADATA
	test_id: SCXML-EVENT-002-EXISTING-001
	requirement_ids: [SCXML-EVENT-002]
	title: Canonical platform error event names
	description: Distinguishes execution, communication and platform error event identities required for runtime error routing.
	authority: { source: W3C SCXML 1.0, section: 5.10.1 Events, citation_or_rule: Platform error categories have distinct canonical names. }
	phase: 2
	feature: event-model
	target_components: [EventName]
	test_kind: unit
	oracle_type: exact-value-and-inequality
	risk: medium
	priority: high
	construction_routes: [static-members]
	data_models: [none]
	target_frameworks: [all-project-targets]
	platforms: [platform-independent]
	partitions: [positive]
	dimensions: { error_kind: execution-communication-platform }
	preconditions: [none]
	dependencies: [none]
	arrange: Obtain static canonical error names.
	stimulus: Format and compare names.
	expected: Exact three strings and pairwise distinct values.
	expected_exception_or_event: none
	forbidden: Alias of two error categories.
	edge_cases: Similar error prefixes.
	determinism: { clock: not-applicable, scheduling: deterministic, timeout_or_step_bound: 6 operations }
	isolation: { parallel_safe: true, shared_state: none }
	cleanup: [No retained resource]
	resource_risk: none
	tier: fast
	tags: [Exhaustive,SCXML]
	related_tests: [SCXML-ERROR-001-CASE-001]
	known_issue: none
	compile_notes: none
	generation_status: existing-annotated
	*/
	[TestMethod]
	public void SCXML_EVENT_002_Platform_error_event_names_are_distinct_and_canonical()
	{
		Assert.AreEqual(expected: "error.execution", EventName.ErrorExecution.ToString());
		Assert.AreEqual(expected: "error.communication", EventName.ErrorCommunication.ToString());
		Assert.AreEqual(expected: "error.platform", EventName.ErrorPlatform.ToString());
		Assert.AreNotEqual(EventName.ErrorExecution, EventName.ErrorCommunication);
		Assert.AreNotEqual(EventName.ErrorCommunication, EventName.ErrorPlatform);
	}

	public static IEnumerable<object[]> EventDescriptorCases() =>
	[
		Case(eventName: "alpha", descriptors: "alpha", expected: true),
		Case(eventName: "alpha.beta", descriptors: "alpha", expected: true),
		Case(eventName: "alpha.beta", descriptors: "alpha.beta", expected: true),
		Case(eventName: "alpha.beta.gamma", descriptors: "alpha.beta", expected: true),
		Case(eventName: "alpha.beta.gamma", descriptors: "alpha.*", expected: true),
		Case(eventName: "alpha.beta.gamma", descriptors: "alpha.", expected: true),
		Case(eventName: "alpha.beta.gamma", descriptors: "*", expected: true),
		Case(eventName: "alpha.beta", descriptors: "alpha.gamma", expected: false),
		Case(eventName: "alpha", descriptors: "alphabet", expected: false),
		Case(eventName: "alpha.beta", descriptors: "Alpha", expected: false),
		Case(eventName: "CAFÉ", descriptors: "café", expected: false),
		Case(eventName: "cafe\u0301", descriptors: "café", expected: false),
		Case(eventName: "alpha", descriptors: "", expected: false),
		Case(eventName: "alpha", descriptors: " ", expected: false)
	];

	private static object[] Case(string eventName, string descriptors, bool expected) => [eventName, descriptors, expected];
}
