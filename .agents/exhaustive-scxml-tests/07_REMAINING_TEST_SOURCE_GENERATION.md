# Remaining unit-test source generation

## Purpose

This document is the authoritative completion backlog for the current code-only campaign. It was produced from a read-only inspection of `test/Xtate.Core.Test/Exhaustive/`; no tests were compiled or executed.

The goal is not to mention every requirement ID. The goal is to represent every materially different fixture, stimulus, oracle, forbidden outcome, boundary, error path, and cleanup/resource scenario as explicit C# test source with complete metadata.

## Audit snapshot

Snapshot date: 2026-08-30.

| Finding | Current observation | Required completion |
|---|---:|---|
| Test methods (`[TestMethod]` or `[DataTestMethod]`) | 186 | Keep all unless an exact duplicate is mapped in metadata |
| TEST-METADATA markers | 139 | Every one of the 186 methods must have one associated complete block |
| Methods without associated TEST-METADATA | 47 | Add scenario-specific metadata directly beside each method |
| Generic existing-test descriptions | 111 | Replace with descriptions derived from the actual fixture, stimulus, assertions, and authority |
| Generic aggregate parameter descriptions | 10 | Give every independently reported row/case a unique ID, input, description, and exact oracle |
| Generic `GeneratedRequirementCase.For(id)` factories | 3 | Replace with explicit cases; an ID alone cannot generate its semantic partitions |
| `[Ignore(...)]` attributes | 18 | Preserve during generation unless the test itself is invalid; ensure metadata explains the known issue. Do not add new ignores merely for non-compilation |

Counts are an inventory aid, not a completion oracle. Files may change after this snapshot; the generation agent must recalculate the source-only inventory before editing.

## What does not count as generated coverage

None of the following satisfies a requirement or partition by itself:

- a requirement ID appearing in a string array;
- a range such as `XPATH-EXPR-001..014` in method-level metadata;
- `RequirementIds.Select(id => GeneratedRequirementCase.For(id))`;
- one case whose fixture says “generator selects every documented partition”;
- one expected value saying “matches the document oracle” without stating that oracle;
- one case claiming an entire cross-product without explicit dimension values;
- a generic description such as “existing authority witness” or “incorrect behavior is distinguished by the method assertions”;
- one CASE-METADATA entry representing all DataRows without one stable case ID and exact result per row;
- metadata fields containing `existing-method-specific`, `case-specific`, “all partitions,” or equivalent placeholders;
- an empty or comment-only body that does not express executable assertion intent.

A declarative C# case table is valid only when each record independently contains concrete input/fixture, stimulus, expected exception/event, other expected results, forbidden results, partitions, dimension values, risk, platform/framework applicability, and compile notes.

## Workstream A — attach missing metadata

The following source-associated omissions were observed:

| File | Methods missing associated TEST-METADATA |
|---|---:|
| `Infrastructure/DeterministicInfrastructureTests.cs` | 8 |
| `Interpreter/EventQueueRequirementsTests.cs` | 2 |
| `Interpreter/InterpreterLifecycleRequirementsTests.cs` | 31 |
| `Parsing/ScxmlParserRequirementsTests.cs` | 6 |
| **Total** | **47** |

For each method, derive metadata from its actual code and the relevant planning row. Do not paste a generic family template. Metadata must identify the exact fixture and observable discriminator of that method.

## Workstream B — replace generic metadata

The following generic descriptions were observed:

| File | Generic test descriptions | Generic aggregate case descriptions |
|---|---:|---:|
| `Parsing/ScxmlParserRequirementsTests.cs` | 50 | 7 |
| `Parsing/XIncludeRequirementsTests.cs` | 19 | 1 |
| `Serialization/ScxmlSerializerRequirementsTests.cs` | 14 | 0 |
| `Validation/StateMachineValidationRequirementsTests.cs` | 28 | 2 |
| **Total** | **111** | **10** |

Replace each description and every placeholder metadata field with method/case-specific facts. For DataRow/DynamicData methods, enumerate each row with a unique case ID and exact expected result. Metadata may remain in comments or move to a strongly typed C# case record.

## Workstream C — expand requirement-level shells

All generated files below require a semantic expansion audit. Keep useful test bodies and case-record types, but do not count broad requirement-level shells as complete.

| File | Current shape | Remaining action |
|---|---|---|
| `Generated/Phase1InfrastructureAndConformanceGeneratedTests.cs` | 1 data test; 2 broad explicit case IDs | Generate explicit cases for every infrastructure capability and every W3C corpus mapping/provenance/disposition scenario |
| `Phase1/Phase1RemainingRequirementsGeneratedTests.cs` | 4 data tests; 51 case IDs | Compare every case to document 01; split cases wherever the row contains different fixtures, outcomes, boundaries, routes, ownership, or cleanup oracles |
| `Interpreter/Phase2RemainingRequirementsGeneratedTests.cs` | 1 data test; 78 one-per-requirement cases | Expand every document-01 requirement into its explicit positive/negative/boundary/error/cancellation/concurrency/cleanup cases; one case per requirement is generally insufficient |
| `Generated/Phase3DataModelXPathRequirementsGeneratedTests.cs` | generic factory from each ID | Replace `For(id)` with explicit document-02 cases and exact XPath/value/tree/error oracles |
| `Generated/Phase3HighRiskPartitionGeneratedTests.cs` | 10 broad cases covering ranges | Split into explicit value kinds, list shapes, XPath axes/operators/functions, assignment actions/node kinds/cardinality, foreach/content, atomicity, isolation, and property cases |
| `Generated/Phase4HostPersistenceIoSecurityRequirementsGeneratedTests.cs` | generic factory from each ID | Replace `For(id)` with explicit document-03 lifecycle, fault, race, protocol, security, checkpoint, corruption, and recovery cases |
| `Generated/Phase4HighRiskPartitionGeneratedTests.cs` | 6 broad cases covering ranges | Expand exact host races, HTTP/pipe boundaries, ownership, authorization, persistence crash points, replay, and platform cases |
| `Generated/Phase5RobustnessReliabilityRequirementsGeneratedTests.cs` | generic factory from each ID | Replace `For(id)` with explicit document-04 adversarial inputs, seeds, schedules, limits, fault points, metrics, and acceptance criteria |
| `Generated/Phase5HighRiskPartitionGeneratedTests.cs` | 6 broad cases covering ranges | Expand per-resource leak targets, bounded resource attacks, linearizable race histories, scale points, soak phases, and crash/recovery outcomes |
| `Generated/Phase6CoverageMutationCompatibilityGeneratedTests.cs` | 6 broad audit cases | Generate explicit source-audit, coverage, mutation, framework/platform, compatibility, exception, and final-evidence cases with exact inputs and pass criteria |

## Workstream D — close every planning-row partition

For every row in documents 01–04:

1. Split the prose at each materially different fixture, stimulus, lifecycle point, failure point, boundary, route, data model, topology, or oracle.
2. Map each resulting scenario to an existing explicit test/case or generate a new one.
3. Apply the README cross-dimensions. Use an explicit covering-array case table for lower-risk combinations and explicit full cross-product records for semantic interactions.
4. Give every generated record a literal stable case ID and scenario-specific description.
5. State exact expected and forbidden observations. Do not delegate semantic definition to a future generator or harness.
6. Reference hypothetical helpers where useful and list them in `compile_notes`; do not implement a standalone harness campaign.
7. Keep all changes in C# files under `test/Xtate.Core.Test/Exhaustive/`.

## Required order

1. Recalculate method/metadata/generic-shell counts.
2. Complete Workstream A.
3. Complete Workstream B.
4. Expand Phase 1 and Phase 2 shells against document 01.
5. Expand Phase 3 against document 02.
6. Expand Phase 4 against document 03.
7. Expand Phase 5 and Phase 6 against document 04 and README final gates.
8. Perform a source-only completeness scan for missing metadata, duplicate IDs, generic placeholders, broad ranges without explicit child cases, and planning rows without explicit C# cases.
9. Stop without compiling, running, formatting, or updating Markdown/YAML/project/production files.

## Source-only completion criteria

The generation work is complete only when:

- zero test methods lack associated TEST-METADATA;
- zero metadata descriptions are generic or merely refer to existing assertions;
- every parameter row/generated record has its own stable case ID and exact oracle;
- no `GeneratedRequirementCase.For(id)` or equivalent semantic placeholder remains;
- every broad requirement range has explicit child cases for all planning-row scenarios;
- every planning-row partition is mapped to concrete C# source;
- no expected result says only “matches plan/document/authority”;
- unresolved compile dependencies are listed in `compile_notes`, while semantic expectations are fully resolved;
- only C# test files changed during the generation Goal;
- no build or execution command was run.

## Recommended Goal configuration

- Model: `gpt-5.6-terra`.
- Reasoning effort: `medium`.
- Speed: `standard`.
- Use one Goal for the complete remaining campaign only if the output budget is large enough for substantial C# generation. Otherwise resume by workstream/phase without changing the rules.
- For a separate metadata-only remediation pass, `gpt-5.6-luna` at `low` effort is sufficient.
- Use `gpt-5.6-sol` at `medium` only for a narrow standards ambiguity that Terra cannot resolve; do not use it for bulk source generation.

## Completion Goal prompt

```text
Complete all remaining Xtate.Core exhaustive unit-test source generation.

This is a source-generation task only. Non-compiling C# is acceptable and expected. Do not compile or execute anything.

Read completely before editing:
- .agents/exhaustive-scxml-tests/06_TOKEN_EFFICIENT_AGENT_RUNBOOK.md
- .agents/exhaustive-scxml-tests/07_REMAINING_TEST_SOURCE_GENERATION.md
- .agents/exhaustive-scxml-tests/README.md

Then read the applicable requirements in documents 01–04 and inspect all C# files under test/Xtate.Core.Test/Exhaustive/.

Allowed changes:
- C# test files under test/Xtate.Core.Test/Exhaustive/ only.
- Add complete TEST-METADATA to existing tests.
- Add complete per-case CASE-METADATA or equivalent strongly typed C# case records.
- Generate new test methods, explicit case tables, and minimal same-file/nested hypothetical test helpers needed to express the tests.

Forbidden changes/actions:
- Do not edit production code, project files, packages, Markdown, YAML, ledgers, or trackers.
- Do not run build, test, restore, formatting, coverage, mutation, fuzz, benchmark, leak, stress, soak, or scalability commands.
- Do not repair compilation errors.
- Do not diagnose or fix product defects.
- Do not add Ignore merely because code may not compile or behavior may fail.

First recalculate the source-only inventory from document 07. Then complete all four workstreams in its required order:
1. Add metadata to every test method that lacks it.
2. Replace every generic existing-test and aggregate DataRow description with scenario-specific metadata and unique per-case IDs.
3. Replace or expand every generic requirement-level generated shell.
4. Map every materially different scenario and cross-dimension from planning documents 01–04 to explicit C# test source.

Strict coverage rules:
- A requirement ID in an array or metadata range is not coverage.
- GeneratedRequirementCase.For(id), Ids.Select(...), and equivalent ID-only factories are not coverage.
- “Generator selects every partition,” “matches document/plan/authority,” “existing assertion,” “case-specific,” “existing-method-specific,” and similar placeholders are not valid fixtures, descriptions, or oracles.
- One case per requirement is insufficient whenever the planning row contains materially different fixtures, stimuli, expected results, failure points, boundaries, construction routes, data models, schedules, cleanup results, or resource risks.
- Every independently reported DataRow, DynamicData item, generated record, boundary, failure point, and materially different oracle needs a literal stable case ID and scenario-specific description.
- Declarative C# case tables are preferred over thousands of duplicated methods, but every record must explicitly state its input/fixture, stimulus, expected exception/event, exact other expected results, forbidden results, partitions, dimensions, risk, platforms/frameworks, and compile notes.
- Every test body must express concrete Arrange/Act/Assert intent. Hypothetical unresolved helpers are allowed and must be named in compile_notes. Semantic expectations may not be deferred.

Completion requires:
- zero test methods without associated TEST-METADATA;
- zero generic descriptions or placeholder metadata fields;
- zero generic ID-only case factories;
- unique test and case IDs;
- explicit child cases for every planning-row partition and required cross-dimension;
- exact authority-derived expected and forbidden outcomes;
- only C# test files changed;
- no compilation or execution performed.

Before stopping, perform a read-only source scan for missing metadata, duplicate IDs, generic phrases/factories, broad ranges without explicit child cases, and planning rows without concrete C# cases. Fix every source-generation gap found.

Final response must contain only:
- C# files created/modified;
- existing methods annotated;
- generic metadata entries replaced;
- explicit test methods and case records generated;
- requirement IDs and partitions covered;
- unresolved compile_notes;
- generated-review-required authority questions.
```
