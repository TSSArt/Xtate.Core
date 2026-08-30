# Xtate.Core exhaustive SCXML test program

## Purpose

This package is the implementation brief for a new, independent test suite for `Xtate.Core`. The suite must validate the complete observable behavior of the SCXML engine, all supported data models (with especially deep XPath coverage), parser/serializer behavior, hosting and I/O, persistence and resume, security boundaries, resource ownership, cancellation, concurrency, reliability, and scalability.

The word **exhaustive** is used here in a test-engineering sense. Arbitrary XML, expressions, event streams, and state-machine graphs form infinite input spaces, so completeness is not a claim that every input string has been enumerated. The suite is complete only when all of the following closures have been achieved:

1. Every applicable normative SCXML, XPath data-model, XPath 1.0, XML, and XInclude requirement has one or more positive, negative, and error-path tests.
2. Every production parser policy, validation rule, public option, enum value, execution branch, externally visible exception family, and resource lifecycle is covered.
3. Every relevant boundary partition has below/at/above tests, including empty, singleton, many, maximum, overflow, malformed, cancelled, timed-out, and disposed forms.
4. High-risk interacting dimensions are tested as full cross-products; lower-risk dimensions use a generated pairwise or higher-strength covering array.
5. Unbounded structures are covered by reference-model, property-based, grammar-based fuzz, mutation, stress, soak, and leak tests.
6. Every discovered defect receives a minimal reproducer and a permanent regression test without weakening the standard-derived oracle.

This is a greenfield suite. Existing test source files, snapshots, expected values, and helper implementations must not be used to decide what correct behavior is.

## Current campaign mode: code generation only

The immediate goal is not a compilable or passing suite. Each phase generates C# source for all remaining unit tests and adds complete descriptions/metadata to existing and new tests. Phase agents do not compile, execute, repair, diagnose, edit production code, or update planning files. Document 06 is authoritative whenever later validation-oriented language in the backlog describes how the generated tests will eventually run.

## Handoff documents

- [01_SCXML_CONFORMANCE_MATRIX.md](01_SCXML_CONFORMANCE_MATRIX.md) specifies parsing, validation, interpreter semantics, executable content, events, invoke/send, and serialization.
- [02_XPATH_AND_DATAMODELS.md](02_XPATH_AND_DATAMODELS.md) specifies `DataModelValue`, null/runtime data models, the complete XPath surface, XML navigation, conversion, and assignment.
- [03_HOST_PERSISTENCE_IO_AND_SECURITY.md](03_HOST_PERSISTENCE_IO_AND_SECURITY.md) specifies host lifecycle, schedulers, SCXML/HTTP/named-pipe I/O, resources, external services, persistence, resume, and security.
- [04_ROBUSTNESS_RELIABILITY_AND_SCALE.md](04_ROBUSTNESS_RELIABILITY_AND_SCALE.md) specifies model/property/fuzz testing, fault injection, leak detection, stress, soak, performance, and scalability.
- [05_EXECUTION_TRACKER.md](05_EXECUTION_TRACKER.md) is a read-only backlog/progress snapshot during generation mode.
- [06_TOKEN_EFFICIENT_AGENT_RUNBOOK.md](06_TOKEN_EFFICIENT_AGENT_RUNBOOK.md) is the mandatory code-only generation contract, metadata schema, and Goal prompt.
- [07_REMAINING_TEST_SOURCE_GENERATION.md](07_REMAINING_TEST_SOURCE_GENERATION.md) is the audited remaining-work backlog and defines which current metadata and generated shells are not yet sufficient.

Agents must not load all planning files for every phase. Start with document 06, the assigned requirement sections, relevant production declarations, and the in-scope existing exhaustive tests. Document 05 is read-only historical context.

## Correctness authorities and precedence

Use the following authorities in descending order. A lower source must not silently override a higher one.

1. [W3C State Chart XML (SCXML) 1.0 Recommendation](https://www.w3.org/TR/scxml/) for normative SCXML behavior.
2. [W3C XPath Data Model for SCXML 1.0 Working Group Note](https://www.w3.org/TR/scxml-xpath-dm/) for Xtate's XPath data-model contract. The final SCXML Recommendation removed XPath because it lacked sufficient interoperable implementations; Xtate nevertheless exposes this data model, so the Note is the explicit behavioral authority.
3. [W3C XML Path Language (XPath) 1.0 Recommendation](https://www.w3.org/TR/xpath-10/) for expression, conversion, node-set, and function semantics.
4. [W3C XML Inclusions (XInclude) 1.0 Recommendation](https://www.w3.org/TR/xinclude/) for enabled XInclude behavior.
5. Public Xtate API contracts, XML documentation, repository documentation, and explicit project decisions for extensions or areas not defined by the standards.
6. Clearly documented compatibility behavior, but only where it does not conflict with authorities 1–5.

Current implementation behavior is not an oracle. Preserve the authority-derived expectation in generated code without executing it. Phase agents do not confirm product defects. If behavior is implementation-defined, encode the best-supported expectation and mark the test metadata `generated-review-required` with the alternatives in `compile_notes`.

The [W3C SCXML Implementation Report test suite](https://www.w3.org/Voice/2013/scxml-irp/) and [W3C XInclude conformance tests](https://www.w3.org/XML/Test/XInclude/) are mandatory imported baselines, not substitutes for the matrices in this package.

## Existing-test usage during code generation

Existing exhaustive tests must now be inspected and annotated so duplicate generation can be avoided.

- Existing tests may be read, mapped to requirement IDs, and given complete TEST-METADATA/CASE-METADATA.
- Existing expected values are not correctness authorities; derive every oracle from the authority precedence above.
- Do not delete or semantically weaken existing tests during generation.
- Add missing assertions only when needed to express the planned oracle; compilation is not required.
- Generate new tests under `test/Xtate.Core.Test/Exhaustive/`.
- Record duplicate coverage in source metadata instead of creating a separate ledger.
## Required test layers

Each requirement must be assigned to the cheapest layer that proves it, while semantic requirements must also have at least one end-to-end SCXML witness.

| Layer | Purpose | Examples |
|---|---|---|
| Component unit | One production class or small collaboration | `EventName`, XML converters, XPath location action, option guards |
| Parser/validator | XML-to-public-model and public-model validation | all attributes, cardinalities, conflicts, diagnostics |
| Compiled-model | public graph to interpreter graph | ID mapping, target resolution, ordering, compiled expressions |
| Algorithm | deterministic interpreter semantics | transition selection, exit/entry sets, queue priorities, history |
| End-to-end | complete machine through the public host | observable final state, data, events, invokes, disposal |
| Conformance | one normative assertion per fixture | W3C SCXML/XInclude cases plus local cases |
| Property/model | broad generated state space against an independent oracle | legal configurations, transition conflict resolution, round trips |
| Fault-injection | every awaited boundary and side effect | resource failure, storage crash, dispatcher failure, cancellation |
| Performance/reliability | behavior over time and at scale | load, stress, soak, leak, contention, recovery |

Do not call a mock-verification-only test a semantic test. Verify externally meaningful effects: active configuration, action order, data values and access modes, queued/raised/sent events, invocation state, persisted state, logs or diagnostics where contractual, and disposal/retention.

## Proposed suite layout

```text
test/Xtate.Core.Test/Exhaustive/
  Infrastructure/
    DeterministicRuntime/
    Assertions/
    Builders/
    Faults/
    Generators/
    Oracles/
    ResourceTracking/
  Conformance/
    W3cScxml/
    W3cXInclude/
    LocalNormative/
  Parsing/
  Validation/
  Interpreter/
  DataModels/
    Common/
    Null/
    Runtime/
    XPath/
  Serialization/
  Hosting/
  IoProcessors/
  Persistence/
  Security/
  Properties/
  Fuzzing/
  Performance/
  Reliability/
  TestData/
```

Use MSTest as required by the repository. Keep all mutable global state out of tests or protect it with explicit non-parallel categories. Test names should include the requirement ID, for example `SCXML_TRANS_014_DescendantTransitionPreemptsAncestor`.

## Planned deterministic test vocabulary

Generated test source should reference or declare the following test-side concepts. Their implementation and compilation are deferred; missing helpers must not stop generation and must be listed in metadata `compile_notes`:

1. **Virtual clock and scheduler.** Control delayed `<send>`, cancellation, idle destruction, timeout, persisted fire times, and retries without wall-clock sleeps. Expose pending timers and assert that none survive teardown.
2. **Single-step interpreter driver.** Start, deliver one external event, drain one internal event, run one microstep or macrostep, suspend at a named hook, and inspect a stable snapshot. The driver must set a bounded operation count and cancellation deadline.
3. **Structured trace recorder.** Record sequence-numbered callbacks for initialization, data/script evaluation, condition checks, exits, transition content, entries, internal/external queue operations, invoke start/cancel/finalize/autoforward, sends/cancels, persistence checkpoints, errors, and disposal.
4. **State snapshot assertion.** Canonically compare active atomic and ancestor state IDs, history values, data graph including access flags and metadata, `_event`, internal/external queues, active invokes, pending sends, and interpreter lifecycle state.
5. **Instrumented doubles.** Provide asynchronous, blockable, faulting, cancelling, and disposal-tracking implementations for evaluators, event routers, queues, resource loaders, streams, XML readers/writers, storage, external services, loggers, task monitors, and ID generators.
6. **Independent reference models.** Implement small, deliberately simple models for SCXML legal configuration/transition selection, XPath assignment tree mutation, event descriptor matching, delayed-event scheduling, and persistence journal replay. Do not reuse production algorithms.
7. **Data generators and shrinkers.** Generate legal and deliberately illegal public models, SCXML XML, state topologies, event streams, XPath XML trees/expressions, and data graphs. Shrink failures to the smallest topology, event sequence, and data value.
8. **Resource ledger.** Count and identify live state-machine scopes, tasks, timers, cancellation sources, streams, readers, writers, HTTP objects, named pipes, external services, pooled arrays, navigators/iterators, storage transactions, and weak-reference sentinels.

Never use an unbounded `Task.Delay`, arbitrary sleep, race-dependent ordering assertion, public network endpoint, fixed machine-specific port, real system time, or shared writable fixture. Real transport tests must bind ephemeral local endpoints and remain separately categorized.

## Test-source metadata contract

Every existing and new test method must carry the complete TEST-METADATA block defined in document 06. Every independently reported parameterized/generated case must carry CASE-METADATA or equivalent C# case-record fields.

Metadata must include a scenario-specific description, requirement IDs, authority, layer, phase, target components, test/oracle kind, risk/priority, frameworks/platforms, dependencies, partitions/dimensions, arrange/stimulus, exact expected exception/event and other expected/forbidden outcomes, determinism bound, isolation, cleanup/resource intent, tier/tags, related tests, known issue, compile notes, and generation status.

The metadata and test body together must let a later compilation agent repair API names and helpers without redesigning the scenario. Missing syntax or APIs may be deferred in `compile_notes`; missing semantic oracles may not.

For every error scenario, generate assertions for the semantic error class and owner/context, platform event behavior, continue/terminate policy, completed and skipped mutations/actions, forbidden side effects, and cleanup. Do not run the scenario during this campaign.
## Coverage dimensions and combination policy

The common dimensions below apply across documents. Do not blindly test them one at a time.

| Dimension | Values |
|---|---|
| Construction route | SCXML text, async reader/stream, public object model, fluent/builder API, serialized round trip |
| Data model | absent/default, null, runtime, XPath, unknown, custom provider success/failure |
| Binding | early, late, omitted/default, invalid |
| State topology | root-only edge, atomic, compound, nested compound, orthogonal parallel, mixed deep tree, history, final |
| Transition | targetless, self, internal, external, ancestor/descendant, multi-target, history target, eventless |
| Event source | eventless, internal raise, external host, delayed send, invoked child, HTTP, named pipe, restored queue |
| Payload | undefined, null, boolean, number variants, date/time variants, string, empty/mixed/keyed/unkeyed/deep/cyclic list, XML |
| Execution outcome | success, false/no-op, synchronous failure, asynchronous failure, cancellation, timeout, disposal race |
| Lifecycle | before start, starting, running unstable, stable idle, finishing, completed, destroying, destroyed, suspending, resumed |
| Persistence | none, stable-state, event, transition, executable-action |
| Runtime target | every supported target framework and relevant OS/transport capability |
| Scale | 0, 1, 2, typical, boundary−1, boundary, boundary+1, large, resource limit |

Use full cross-products when an interaction can change semantics, notably:

- state topology × transition type × source/target relationship;
- event source × queue priority × finalize/autoforward × failure;
- XPath assignment action × target node kind/cardinality × value kind × tree position;
- binding mode × initialization source × re-entry × failure;
- persistence level × suspension point × side-effect type × recovery outcome;
- lifecycle state × host operation × cancellation/disposal race;
- transport content type × size boundary × partial I/O × cancellation.

“Full cross-product” means complete generated case coverage, not one hand-written method or pasted fixture per cell. Use declarative cases, generation, and shared oracles so the source and agent context remain bounded. For remaining combinations, generate at least pairwise coverage and strength-3 coverage for high-risk modules. Store the generated covering array and seed so the exact campaign is reproducible.

## Test tiers

| Tier | When | Contents | Target duration |
|---|---|---|---|
| PR-fast | each change | deterministic unit, parser, validator, core algorithm, regressions | minutes |
| PR-complete | required before merge | all deterministic tests, imported conformance, property smoke, persistence crash matrix smoke | bounded CI job |
| Nightly | daily | large property campaigns, fuzz corpus, transport integration, stress, benchmark comparison, leak loops | hours |
| Weekly | scheduled | extended fuzzing, 8-hour soak, high-concurrency and large-graph scale, all target frameworks/OS lanes | long-running |
| Release | candidate gate | all above plus clean-host recovery, compatibility matrix, zero known critical mutations | release window |

Tag tests with stable categories such as `Exhaustive.Fast`, `Exhaustive.Conformance`, `Exhaustive.Property`, `Exhaustive.Fuzz`, `Exhaustive.Transport`, `Exhaustive.Persistence`, `Exhaustive.Performance`, `Exhaustive.Leak`, and `Exhaustive.Soak`.

## Standard corpus acquisition and provenance

Vendor exact, immutable copies of allowed W3C test inputs and metadata into the test-data tree or fetch them in a separate reproducible preparation step. Record source URL, upstream revision/date, license, checksum, local adaptations, and expected result. Do not make normal test execution depend on internet availability.

For each imported SCXML test:

- retain its normative assertion ID;
- generate test cases for all applicable construction routes and data models;
- adapt only the test harness protocol needed to map pass/fail final states into MSTest assertions;
- never change the expected outcome merely to match Xtate;
- mark inapplicable optional features with a written reason, not a silent skip.

The upstream SCXML conformance suite does not cover performance, resource use, or every implementation-specific extension. Those remain mandatory in this package.

## Code-generation completion gates

These gates apply now. Runtime coverage, mutation score, conformance execution, fuzzing, leaks, performance, and portability are deferred.

1. Every planned requirement ID and partition has annotated C# test source.
2. Every test method has complete TEST-METADATA and a unique stable test ID.
3. Every DataRow/DynamicData/generated case has a unique case ID and description.
4. Every description states what behavior is proved and how incorrect behavior is observed.
5. Every expected and forbidden result is explicit and authority-derived.
6. Every body expresses complete Arrange/Act/Assert intent, even if helper/API symbols do not exist.
7. Every unresolved compile dependency is listed in `compile_notes`.
8. Existing in-scope exhaustive tests are annotated and mapped before new duplicate methods are created.
9. Phase agents changed only C# test code.
10. No build, test, coverage, mutation, fuzz, benchmark, leak, stress, or formatting command was run.
## Code-generation phase order

Each phase has one end goal: generate all remaining annotated unit-test source for its scope.

1. Phase 1: annotate and generate parser, XInclude, validation, serializer, and directly related test-infrastructure cases.
2. Phase 2: annotate and generate lifecycle, state, transition, history, event, and executable-content cases.
3. Phase 3: annotate and generate common values, null/runtime data models, and all XPath cases.
4. Phase 4: annotate and generate host, scheduler, I/O, resources, external services, security, persistence, recovery, and platform cases.
5. Phase 5: generate property, fuzz, fault, leak, resource-budget, race, stress, scale, benchmark, soak, and crash test source.
6. Phase 6 is a source-only completeness review that generates any missing tests/metadata; it does not compile or execute them.

A phase may use multiple agents only with disjoint requirement families and test files. Phase agents do not update this documentation or the tracker.
## Ambiguity and missing infrastructure

Do not stop source generation because code would not compile, a helper is absent, a product feature appears broken, or a runtime environment is unavailable.

- Use plausible test-side APIs/helpers and list unresolved symbols in `compile_notes`.
- Keep the authority-derived expected result explicit.
- For genuine authority ambiguity, choose the best-supported oracle, set `generation_status: generated-review-required`, and explain the alternatives in metadata.
- Generate process-isolated, timing, transport, fuzz, leak, and scalability test source even when the runner/harness does not yet exist.
- Never convert uncertainty into a vague assertion or empty test body.
## Deliverables from the current generation campaign

The campaign delivers only:

- annotated existing exhaustive C# unit tests;
- newly generated C# unit tests for every remaining requirement/partition;
- C# test-side case records, fixtures, fakes, builders, or hypothetical helpers needed to express those tests;
- complete TEST-METADATA and CASE-METADATA beside the code;
- explicit `compile_notes` for unresolved source dependencies.

It does not deliver compiled binaries, passing runs, production fixes, project/package changes, Markdown/YAML ledgers, coverage or mutation reports, benchmark results, fuzz corpora, defect confirmation, or runtime evidence. Those belong to a later campaign.
