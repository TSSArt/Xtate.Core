# Code-only unit-test generation runbook

## Mission

The immediate campaign deliverable is **source code for every remaining unit test**. Phase agents generate or annotate test code only. Compilation, execution, product fixes, harness completion, defect confirmation, coverage measurement, and performance campaigns happen later in a separate implementation/validation campaign.

Non-compiling test code is acceptable. Missing APIs, helpers, fixtures, packages, or production behavior must not stop generation when the intended test and oracle can be expressed precisely.

Before continuing an existing campaign, read `07_REMAINING_TEST_SOURCE_GENERATION.md`. A requirement ID mention or generic requirement-level case is not completion.

## Allowed workspace changes

A phase agent may change only:

- C# test files under `test/Xtate.Core.Test/Exhaustive/`;
- existing exhaustive unit tests to add the mandatory description/metadata;
- new exhaustive unit-test files;
- minimal nested or same-file test-side helper, builder, fake, oracle, or fixture declarations only when needed to express generated unit tests. Do not start a standalone infrastructure implementation.

A phase agent must not:

- edit production code;
- edit these Markdown planning files or the tracker;
- run build, test, coverage, mutation, benchmark, fuzz, leak, stress, or formatting commands;
- repair compilation errors;
- install packages or change project files;
- diagnose runtime failures or create execution-derived defect claims;
- reduce or change an authority-derived expected result to match current behavior;
- spend the phase building general infrastructure instead of generating the planned test bodies.

Read-only search and source inspection are allowed. Existing tests may be inspected to avoid duplicates and may be annotated, but they are evidence of existing coverage, not the authority for expected behavior.

## Phase completion

A phase is complete when every in-scope requirement and required partition is represented by test source with complete metadata. Completion does not require compilation or a passing run.

The workspace delta from a phase must contain only test code. The final response may summarize counts and unresolved compile notes, but the agent must not update any Markdown/YAML tracker or report file.

Statuses used inside metadata:

- `existing-annotated` — an existing test was given complete metadata;
- `generated-uncompiled` — new test source was generated and not compiled;
- `generated-review-required` — the oracle is explicit but an authority/API mapping needs human review;
- `duplicate-covered` — a planned case is explicitly mapped to another test/case and no duplicate method is generated.

## Mandatory metadata for every test

Place this block immediately above each test method. Do not use a shortened form.

```csharp
/*
TEST-METADATA
test_id: SCXML-TRANS-004-CASE-003
requirement_ids:
  - SCXML-TRANS-004
title: Descendant transition preempts a conflicting ancestor transition
description: >
  Explains the behavior being proved, why the scenario matters, and the
  observable distinction that would reveal an incorrect implementation.
authority:
  source: W3C SCXML 1.0
  section: 3.13 Selecting and Executing Transitions
  citation_or_rule: Descendant-source transition has priority over ancestor-source transition.
phase: 2
feature: transition-selection
target_components:
  - StateMachineInterpreter
test_kind: algorithm
oracle_type: ordered-trace-and-final-configuration
risk: high
priority: critical
construction_routes:
  - scxml-text
data_models:
  - none
target_frameworks:
  - all-project-targets
platforms:
  - platform-independent
partitions:
  - positive
  - conflict
dimensions:
  topology: nested-compound
  event_source: external
  transition_type: external
preconditions:
  - parent and child are active
dependencies:
  - deterministic interpreter driver
  - ordered trace recorder
arrange: Exact fixture, graph, options, fakes, and initial state.
stimulus: Exact event, call, cancellation point, or concurrent schedule.
expected:
  - Exact ordered trace, state, value, event, exception, or resource result.
expected_exception_or_event: none
forbidden:
  - Ancestor transition action must not execute.
edge_cases:
  - Same event descriptor on both transitions.
determinism:
  clock: virtual-or-not-applicable
  scheduling: deterministic
  timeout_or_step_bound: 100 operations
isolation:
  parallel_safe: true
  shared_state: none
cleanup:
  - No pending event, timer, task, stream, scope, or retained session.
resource_risk: none
tier: fast
tags:
  - Exhaustive
  - SCXML
related_tests: []
known_issue: none
compile_notes: none
generation_status: generated-uncompiled
*/
[TestMethod]
public async Task SCXML_TRANS_004_CASE_003_DescendantPreemptsAncestor()
{
    // Arrange
    // Act
    // Assert
}
```

Metadata rules:

- `test_id` is globally unique and stable.
- `description` is mandatory and scenario-specific; it must explain what distinguishes correct from incorrect behavior.
- `authority` must identify the standard section, public contract, or explicit planning requirement. Never cite current implementation behavior as the oracle.
- `target_components`, dependencies, frameworks/platforms, risk/priority, oracle type, isolation, `arrange`, `stimulus`, `expected`, and `forbidden` must be concrete enough for a later agent to repair compilation without redesigning the test.
- `compile_notes` names intentionally unresolved types, members, helpers, or packages. It must not contain unresolved semantic questions.
- If authority is genuinely ambiguous, keep an explicit expected result, set `generation_status: generated-review-required`, and explain the ambiguity in `compile_notes`.
- Do not use TODO, “verify appropriately,” “expected behavior,” or other placeholder language for the oracle.
- Metadata is required on existing tests as well as newly generated tests.

## Parameterized and generated cases

Every independently reported case needs a unique case ID and description. A parameterized method has one method-level TEST-METADATA block plus a CASE-METADATA block immediately above it:

```csharp
/*
CASE-METADATA
cases:
  - case_id: SCXML-PARSE-003-CASE-001
    description: Exact late binding token is accepted.
    partition: positive
    input: binding="late"
    expected: BindingType.Late with no diagnostic.
  - case_id: SCXML-PARSE-003-CASE-002
    description: Leading XML whitespace does not silently alter a closed lexical token.
    partition: lexical-negative
    input: binding=" late"
    expected: Binding diagnostic and no usable model.
*/
```

A large generated matrix may store case metadata in a strongly named C# case record or collection instead of a comment list, provided every record includes:

```text
case_id, requirement_ids, description, input/fixture, stimulus,
expected, expected_exception_or_event, forbidden, partitions, dimensions,
risk, target_frameworks/platforms, compile_notes
```

Do not collapse cases with materially different oracles into one description. Do not generate thousands of pasted methods when a declarative C# case table and one test body express the same tests.

The following are prohibited semantic shortcuts: generating a case from only a requirement ID; using ranges without explicit child records; saying a fixture generator selects every partition; saying the result matches a planning document without spelling out the result; or using one aggregate metadata row for all DataRows. Every case record must be literal and independently reviewable.

## Test body requirements

Even when code is intentionally non-compiling:

- Emit a complete Arrange/Act/Assert-shaped body, not an empty method.
- Express the exact assertion intent using plausible MSTest APIs and named test helpers.
- Prefer readable hypothetical helper names over large duplicated setup.
- Put unresolved helper/API names in `compile_notes`.
- Include negative assertions and cleanup/resource assertions where required.
- Include deterministic schedules, seeds, operation bounds, and cancellation points in code or metadata.
- Use comments only to explain intent; comments must not replace all executable assertion code.
- Do not generate production fixes.
- Do not add `[Ignore]` merely because the source is expected not to compile or the current product may fail.

## Existing-test annotation pass

For each phase:

1. Inspect only the in-scope existing exhaustive test files.
2. Map each existing method and each parameterized case to planned requirement IDs.
3. Add full TEST-METADATA and CASE-METADATA without changing the existing semantic assertion unless it is plainly missing.
4. Generate code for every uncovered planned partition.
5. If an existing test covers several planned cases, list them explicitly in case metadata.
6. If two existing tests duplicate one case, annotate both and use `related_tests`; do not delete tests during generation.
7. Mark a suspected invalid oracle as `generated-review-required`; do not diagnose or fix production behavior.

## Generation workflow

A phase agent performs exactly this workflow:

1. Read this runbook.
2. Read only the assigned phase/family rows from documents 01–04.
3. Read production declarations needed to name APIs and construct plausible fixtures.
4. Read the in-scope existing exhaustive tests and build an in-memory coverage map.
5. Annotate existing tests.
6. Generate all remaining test methods/case tables for the assigned phase.
7. Perform a source-only completeness review: every planned ID/partition is represented, metadata fields are present, IDs are unique, and no oracle is vague.
8. Stop. Do not compile, run, repair, update trackers, or begin another phase.

The phase may be large because the task is code generation rather than validation. Split only to avoid context loss or file conflicts. Parallel agents must own disjoint test files and requirement families.

## File organization

Prefer one file per tight requirement family:

```text
Parsing/ScxmlParser_<range>_GeneratedTests.cs
Validation/StateMachineValidation_<range>_GeneratedTests.cs
Interpreter/Lifecycle_GeneratedTests.cs
Interpreter/Transitions_<range>_GeneratedTests.cs
DataModels/XPath/Assignment_<actions>_GeneratedTests.cs
...
```

Keep metadata with the method it describes. Do not create a separate Markdown or YAML metadata ledger. Shared C# metadata case records may live in the same feature directory.

## Goal prompt

```text
Generate source code only for all remaining unit tests in phase <N>: <families/IDs>.

Read 06_TOKEN_EFFICIENT_AGENT_RUNBOOK.md, 07_REMAINING_TEST_SOURCE_GENERATION.md, and only the assigned requirement sections. Inspect production declarations and the in-scope existing exhaustive tests. Existing tests may be annotated and used to avoid duplicate cases, but not as correctness authorities.

Workspace changes must be C# test code under test/Xtate.Core.Test/Exhaustive/ only. Add the complete TEST-METADATA block to every existing and new test. Add per-case CASE-METADATA for DataRow/DynamicData/generated cases. Generate complete Arrange/Act/Assert-shaped bodies with explicit expected and forbidden outcomes. Non-compiling code and hypothetical test helpers are allowed; record unresolved symbols in compile_notes.

Do not run builds/tests, fix compilation, edit production/project/Markdown/YAML files, diagnose product defects, or stop because infrastructure is missing. Do not count generic metadata, ID arrays, range-only cases, `For(id)` factories, or “matches the plan” oracles as coverage. Finish when every assigned requirement and partition is represented by explicit annotated test source, then return only a compact count/file summary.
```

## Model and usage guidance

This work is dominated by structured source generation:

- Use GPT-5.6 Luna at low effort for annotation and regular parser/value/matrix generation.
- Use GPT-5.6 Terra at low or medium effort for interpreter, XPath, persistence, concurrency, and resource semantics.
- Use Sol only when an authority-derived oracle cannot be formulated by Terra.
- Use standard speed.
- Give each run one phase or disjoint family set and a finite output-file scope.
- Spend tokens on test bodies and metadata, not command output, compilation repair, tracker prose, or repeated repository summaries.
