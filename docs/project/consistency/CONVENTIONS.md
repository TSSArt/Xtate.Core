# Xtate.Core conventions

This document describes conventions for `Xtate.Core.sln`: the production library, its MSTest project, configuration, and maintained guides. It does not audit conformance or authorize implementation changes. Repository instructions and explicit configuration remain authoritative; proposals below are not silently promoted to existing rules.

The canonical workspace is `docs/project/consistency/`. [Manifest](consistency.json), [documenting progress](DOCUMENTING.progress.json), [input inventory](INVENTORY.json), and [evidence registry](EVIDENCE.json) preserve the inputs and discovery scope. Xtate.IoC has a separate solution and manifest with its own source root; its documentation and stale audit are outside this scope.

Policy update on 2026-10-30: the user adopted the complete recommendations CONS-009 through CONS-017. Their targets are normative for the continuing full audit. Existing implementations are assessed separately; the original discovery baseline and observations remain historical.

## Basis and limits

Baseline: parent Git revision `88d74702ce1685dd4544cfe68abbd776f6cc44a1`. `Global.Packages.props` was already modified; its working-tree bytes, not an assumed clean revision, are fingerprinted. The source inventory contains 1,083 maintained files: 781 library C# files (including one generated resource designer), 254 test C# files, eight test assets, and 40 configuration/guide files. Generated build properties remain configuration evidence. Generated output (`bin`, `obj`, `TestResults`), IDE metadata, and Git internals are excluded. Maintained `Legacy` tests are included; their name is not grounds for exclusion.

All 50 catalog criteria apply. The inventory is exhaustive for enumerated maintained paths; convention discovery is deliberately sampled. Each record gives the exact evidence-file sample size and named symbols or settings. Counts describe inspected files, not inferred prevalence across all methods or types. The full potential C# discovery population is 780 handwritten library files and 254 test files; no whole-solution behavioral verification or code-coverage measurement is claimed. Configuration evidence is distinguished from runtime, model, composition, and test peers.

Existing rules have priority over observed frequency. **Established pattern** means supported by the named sample, not a measured solution-wide majority. **Proposed target** identifies an auditable recommendation awaiting adoption. **Decision required** means the record is complete as documentation but lacks a full auditable target until the stated policy question is resolved. Confidence concerns support for the description, not the correctness of every implementation.

Discovery uses local source and settings, not external framework or SCXML specification claims. The union of criterion evidence includes 24 of 1,034 handwritten C# files and 19 configuration/guide files; files were read for the cited symbols, control flows and settings, not certified in their entirety. Build/test commands are repository entry points; no tests were run for this documentation-only change. In particular, the actual test project includes `net11.0`, while existing agent guidance lists older test targets; project files control the recorded target list.

## Reading order

Contracts and lifecycle (C09–C16) and component boundaries (C17–C22) come first, followed by naming (C01–C08), tests/documentation (C32–C38), local structure (C23–C31), configuration (C39–C44), and cross-cutting behavior (C45–C50). See the final reconciliation for unresolved decisions.

<a id="c09"></a>

## C09 — Failure representation

**Applicability and comparison unit:** Applicable. Library operations: distinguish optional lookup, invalid arguments, lifecycle failure and processor failure; compare methods with the same outcome semantics.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (5 C# files); [loader](../../../src/Xtate.Core/ResourceLoaders/Services/ResourceLoaderService.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [infra](../../../src/Xtate.Core/Common/Helpers/Infra.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** ResourceLoaderService.Request skips a provider returning no loader, then throws ProcessorException if none can serve the URI. Resource checks disposal; Infra distinguishes argument errors from InvalidOperationException invariants. EventName.FromString(null) returns default. Scope tests preserve the original execution exception.

**Selected target:** **Established pattern**. Confidence: **Medium**. Keep absence distinct from a fault: optional provider discovery may return null; required resource acquisition fails with ProcessorException; caller contract violations use argument exceptions and disposed access uses ObjectDisposedException. Preserve default EventName as absence. This follows the sampled outcome contracts, not one universal error type.

**Inspection and compliance / open decisions:** Trace success, absence and failure branches and inspect callers and matching tests. Verify exception type and parameter name where exposed. Do not change a nullable lookup into a throwing lookup as a naming cleanup. No additional decision for these sampled contracts.


<a id="c10"></a>

## C10 — Nullability and absence

**Applicability and comparison unit:** Applicable. Library model and service signatures; compare optional model values separately from required injected collaborators.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (4 C# files); [build](../../../Directory.Build.props), [state](../../../src/Xtate.Core/StateMachine/Abstractions/Interfaces/IState.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [loader](../../../src/Xtate.Core/ResourceLoaders/Services/ResourceLoaderService.cs), [library](../../../src/Xtate.Core/Xtate.Core.csproj). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Nullable is enabled in shared build properties, with nullable warnings suppressed for legacy library targets. IState has nullable optional entities and ImmutableArray collections. EventName distinguishes default and empty. ResourceLoaderService has a required nullable location dependency.

**Selected target:** **Existing rule**. Confidence: **High**. Preserve nullable annotations and the distinction between an optional value and a required injection slot. Do not equate default ImmutableArray or default EventName with an empty value without contract evidence. Legacy warning suppression does not change runtime null contracts.

**Inspection and compliance / open decisions:** Inspect signatures, default constructors, annotations and consuming branches. Validate that optional members remain nullable and required injection is initialized. Do not infer element immutability from ImmutableArray. No unresolved target for annotation preservation; per-type absence semantics remain type-specific.


<a id="c11"></a>

## C11 — Argument validation

**Applicability and comparison unit:** Applicable. Public inputs, internal invariants and IoC-provided properties are separate comparison cohorts.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (4 C# files); [layout](../../../Xtate.Core.sln.DotSettings), [infra](../../../src/Xtate.Core/Common/Helpers/Infra.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [xml](../../../src/Xtate.Core/Scxml/Services/ScxmlDeserializer.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** The IDE custom null-check statement is Infra.Requires. Infra.Requires uses CallerArgumentExpression and throws ArgumentNullException; assertions throw InvalidOperationException even outside a DEBUG guard. Resource uses a throw expression in initialization. SCXML validates the constructed model.

**Selected target:** **Established pattern**. Confidence: **High**. Use Infra.Requires and typed Infra range/enum helpers for ordinary argument guards; preserve throw expressions needed during field initialization and specialized decoding errors. Use Infra.Assert/NotNull for proven internal invariants, not to misclassify caller errors. Preserve validation before externally visible work.

**Inspection and compliance / open decisions:** Inspect control flow and helper implementation, not just guard names. Check exception type, parameter name and failure timing; do not mechanically add guards to every IoC init-property. A universal guard-at-every-method policy is not established or selected.


<a id="c12"></a>

## C12 — Cancellation

**Applicability and comparison unit:** Applicable. Stream adapters, scheduled work and host results; compare explicit per-call tokens separately from lifetime cancellation.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (6 C# files); [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [scheduled](../../../src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [streamTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/InjectedCancellationStreamCoverageTest.cs), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** InjectedCancellationStream forwards one token or creates/disposes a linked source for two distinct cancelable tokens. Scheduler dispatch links disposal and scheduled-event tokens. ScheduleEvent accepts a token without consuming it in the inspected body. Scope results distinguish OperationCanceledException from faults.

**Selected target:** **Existing rule** (explicit user adoption of CONS-009 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. A caller token controls admission and cancellable work before the operation commits, not the lifetime of an admitted scheduled event. Check a pre-cancelled token before registration or other externally visible effects. Define the commit point for both in-memory and persistent scheduling; failed/cancelled admission must leave neither an orphan delivery nor an orphan persisted record. Once admission succeeds, delivery is owned by the scheduler and is cancelled through CancelEvent/event cancellation or owner shutdown, not by later cancellation of the scheduling caller. CancelEvent also rejects pre-cancellation before mutation; once removal/cancellation is committed, finish that cancellation without undoing it. A wait-only cancellation API must explicitly preserve observation of the underlying operation and its original cancellation token. Existing stream token-linking and exception/token identity rules remain in force.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. Coordinate the commit/shutdown boundary with CONS-010/011. Medium-to-high behavioral risk for a future repair; public signatures stay stable. After approval, document the selected commit semantics, inventory every token boundary and require gated tests for pre-cancelled admission, cancellation during persistence, cancellation after commit, explicit event cancellation and preserved result/token identity. Selecting this policy is complete only when the user decision is recorded and the convention digest advances.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-009). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c13"></a>

## C13 — Resource ownership and disposal

**Applicability and comparison unit:** Applicable. Owning resource wrappers, host scopes and scheduled events; compare acquisition, normal completion, disposal and partial failure.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (6 C# files); [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [scheduled](../../../src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Resource owns and disposes its stream; GetStream(true) can return the original stream, and GetBytes can return the cached array. ScopeManager removes and disposes owned scopes. Scheduler sync disposal collects cancellation exceptions, while ScopeManager loops without aggregation. Tests cover ordinary cleanup and repeated disposal.

**Selected target:** **Existing rule** (explicit user adoption of CONS-010 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. Owners attempt cleanup of every resource they own, even after an ordinary cleanup exception. Retain one failure as the same exception object; aggregate multiple independent failures in attempt order, without flattening nested aggregates. Keep the existing CONS-007 operation-plus-cleanup precedence at its named boundaries. Resource retains ownership of its input stream. A returned original stream is borrowed: callers must not dispose it or use it after Resource disposal. A newly created stream wrapper is caller-owned. GetBytes remains a shared buffer which callers must treat as read-only; a defensive copy is an explicit future API/behavior change. Cached returned arrays remain ordinary managed values after owner disposal, while further Resource access is rejected. Concurrent Resource reads, or reads overlapping disposal, require external serialization; no undocumented general thread-safety promise is inferred.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. Coordinate concurrent shutdown with CONS-011 and contract documentation with CONS-014. Preserve public signatures and the tested stream identity. After approval, enumerate owning versus borrowed references and require throwing-cleanup tests that show later resources are attempted, ordered failure retention, repeated disposal, and distinct lifetime tests for borrowed streams, owned wrappers and shared arrays. Existing happy-path tests alone are insufficient.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-010). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c14"></a>

## C14 — Mutation and collection contracts

**Applicability and comparison unit:** Applicable. Public model collections, mutable construction entities and resource buffers are distinct units.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (6 C# files); [state](../../../src/Xtate.Core/StateMachine/Abstractions/Interfaces/IState.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** IState exposes ImmutableArray values; StateEntity has setters and copies interface values; StateNode projects compiled-node arrays through explicit interface implementations. Resource returns a cached byte array directly, while EventName wraps immutable segments and copies Create input.

**Selected target:** **Established pattern**. Confidence: **Medium**. Use ImmutableArray for the existing public graph collection contracts and preserve mutable builder/entity roles. Do not assume deep immutability of elements or defensive copying of Resource buffers. Preserve ordering and default/empty distinctions; changing aliasing is a contract change rather than uniformity work.

**Inspection and compliance / open decisions:** Trace copies and aliases from input to output; compare equality and ordering with consumers. Verify mutations only where the API explicitly allows them. Resource buffer lifetime/ownership policy is unresolved under C13; this record selects the model-shape rule only.


<a id="c15"></a>

## C15 — Error translation at boundaries

**Applicability and comparison unit:** Applicable. Resource selection, SCXML validation and asynchronous host completion; compare domain boundaries with cleanup boundaries.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (5 C# files); [loader](../../../src/Xtate.Core/ResourceLoaders/Services/ResourceLoaderService.cs), [xml](../../../src/Xtate.Core/Scxml/Services/ScxmlDeserializer.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** No resource provider becomes ProcessorException; deserialization validates the constructed model. ScopeManager captures an execution failure, awaits Cleanup, then completes its result source. Scheduler logs dispatch failures and cleans up in finally. Scope tests preserve execution faults when cleanup succeeds.

**Selected target:** **Existing rule** (policy selected by CONS-007). Confidence: **High**. Keep domain-specific exceptions and SCXML validation boundaries. For the two scope managers, apply the [selected failure matrix](#cons-007-policy): retain single failures, aggregate independent operation/cleanup failures in that order, and always complete the published result. Detached reporting follows the same policy's terminal-observer rule. This is a selected contract for the pending repairs, not a description of behavior already implemented.

**Inspection and compliance / open decisions:** Verify every row of the selected matrix for both managers, including unregister/disposal failure before and after an asynchronous suspension. Assert task state, original exception identity, cancellation token and ordered aggregate contents. Logger failure must not replace a published outcome or create an unowned observer failure. CONS-001 and CONS-004 implement these obligations; CONS-007 resolves the policy only. Retry policy and general cleanup-draining guarantees are not introduced by this decision.


<a id="c16"></a>

## C16 — Diagnostics

**Applicability and comparison unit:** Applicable. Runtime logger calls and exception resources; compare emission fields and levels separately from test output.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (3 C# files); [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [module](../../../src/Xtate.Core/Interpreter/DependencyInjection/StateMachineInterpreterModule.cs), [infra](../../../src/Xtate.Core/Common/Helpers/Infra.cs), [testRules](../../../.github/instructions/tests.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Scheduler checks Logger.IsEnabled(Level.Error), writes eventId 1 with SendId and exception, and awaits logging. Interpreter module registers info/debug/verbose enrichers and entity parsers. Exceptions use Resources and Res.Format. Tests are instructed to assert structured logs.

**Selected target:** **Existing rule** (explicit user adoption of CONS-012 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. Identify an event by (source category, eventId), with one stable meaning per pair and no renumbering as a cleanup. Default framework diagnostics may emit logical session/send/invoke/state identifiers, source/category, severity, error type and operational metadata. Treat identifiers as correlation data, not proof that arbitrary values are safe. Do not emit credentials, authorization headers, connection secrets, arbitrary data-model values or request/document bodies by default. Treat URI user-info/query and exception messages/custom exception data as potentially sensitive; sanitize or use an explicitly opt-in diagnostic policy before exporting them. Keep useful exception type/stack context where permitted. Preserve structured fields and lazy level checks so disabled diagnostics do not serialize arbitrary entities. Any payload-enabled mode must state the fields it adds and its redaction responsibility. Retain the selected nonrecursive terminal fallback unchanged.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. Coordinate public logging documentation with CONS-014. Audit fields at producers, enrichers and providers, not just logger call text. Require enabled/disabled-level tests, structured category/event identity tests and recognizable sentinel values proving default secret/body fields are absent. The policy does not authorize changing persisted formats or silently dropping all exception context.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-012). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c17"></a>

## C17 — Contracts versus implementations

**Applicability and comparison unit:** Applicable. Public graph, compiled graph, infrastructure abstractions and composition modules.

**Evidence and discovery coverage:** 7/7 selected evidence files inspected for this criterion (5 C# files); [guide](../../../.agents/AGENTS.md), [csharp](../../../.github/instructions/csharp.instructions.md), [state](../../../src/Xtate.Core/StateMachine/Abstractions/Interfaces/IState.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [loader](../../../src/Xtate.Core/ResourceLoaders/Services/ResourceLoaderService.cs), [module](../../../src/Xtate.Core/Interpreter/DependencyInjection/StateMachineInterpreterModule.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** IState defines the public graph, StateEntity is mutable model data, and StateNode wraps source state with compiled children. Runtime services implement contracts; module registrations select implementations.

**Selected target:** **Existing rule**. Confidence: **High**. Keep public/source models separate from compiled interpreter state and host orchestration. Place adapter details behind their owning abstraction; preserve existing intentional graph bridging and explicit interface projections. The repository guide explicitly requires this separation.

**Inspection and compliance / open decisions:** Inspect member types and dependencies, then trace parser to validator to compiled model to runtime and persistence when a model changes. A name or folder alone is not proof of a boundary violation. No unresolved target.


<a id="c18"></a>

## C18 — Dependency direction

**Applicability and comparison unit:** Applicable. Solution project references and logical subsystem dependencies; distinguish these two levels.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (2 C# files); [library](../../../src/Xtate.Core/Xtate.Core.csproj), [testProject](../../../test/Xtate.Core.Test/Xtate.Core.Test.csproj), [catalog](../../../.github/instructions/repo-catalog.instructions.md), [module](../../../src/Xtate.Core/Interpreter/DependencyInjection/StateMachineInterpreterModule.cs), [xml](../../../src/Xtate.Core/Scxml/Services/ScxmlDeserializer.cs), [guide](../../../.agents/AGENTS.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Both project files were inspected: tests reference the library; the library has package dependencies and no test reference. SCXML delegates to model construction/validation; the interpreter module composes model builder, data-model handlers and IoC. Hosting is above interpretation in the repository map.

**Selected target:** **Existing rule**. Confidence: **High**. Keep production independent of tests. Follow the documented subsystem direction: SCXML to builders/validation/resource loaders; interpreter to model/data-model abstractions; host to interpreter; persistence integrates with runtime state. Do not impose extra assemblies just to simulate layers.

**Inspection and compliance / open decisions:** Inspect project references and imports, then actual type and module dependencies. Check changes for reverse dependencies or cycles at the relevant subsystem level. This is a documented target, not a completed cycle audit.


<a id="c19"></a>

## C19 — Dependency construction

**Applicability and comparison unit:** Applicable. IoC service collaborators versus constructor-supplied value/model inputs.

**Evidence and discovery coverage:** 7/7 selected evidence files inspected for this criterion (5 C# files); [diRules](../../../.github/instructions/dependency-injection.instructions.md), [csharp](../../../.github/instructions/csharp.instructions.md), [loader](../../../src/Xtate.Core/ResourceLoaders/Services/ResourceLoaderService.cs), [xml](../../../src/Xtate.Core/Scxml/Services/ScxmlDeserializer.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [optionsProvider](../../../src/Xtate.Core/StateMachineOptions/Services/StateMachineOptionsProvider.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Loader and deserializer use required init-properties with SetByIoC. Resource uses primary constructor values; StateNode constructs compiled projections from constructor inputs; options provider accepts IOptions. These are legitimate role differences.

**Selected target:** **Existing rule**. Confidence: **High**. Preserve Xtate.IoC property injection and async factories for service collaborators. Use constructor inputs for value/model initialization where established. Do not convert the service graph to Microsoft DI constructor conventions or add service location to domain models.

**Inspection and compliance / open decisions:** Inspect the closest module, injection attributes and factory shape together. Verify new required properties are supplied by the intended construction path; preserve synchronous versus asynchronous factory semantics. No unresolved target.


<a id="c20"></a>

## C20 — Registration and composition

**Applicability and comparison unit:** Applicable. DependencyInjection module classes and registration entries, compared within a feature.

**Evidence and discovery coverage:** 3/3 selected evidence files inspected for this criterion (1 C# files); [diRules](../../../.github/instructions/dependency-injection.instructions.md), [module](../../../src/Xtate.Core/Interpreter/DependencyInjection/StateMachineInterpreterModule.cs), [guide](../../../.agents/AGENTS.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** StateMachineInterpreterModule derives from a generic Module dependency chain. It uses SharedWithin.Scope, IfNotRegistered defaults and one shared EventQueue forwarded to two interfaces. Registration order is explicitly behavioral in the guide.

**Selected target:** **Existing rule**. Confidence: **High**. Keep feature wiring in Module subclasses and preserve dependency order, defaults, sharing boundaries and forwarding identity. Add business behavior to services rather than AddServices. Use the existing Xtate APIs and resolve services asynchronously in DI tests.

**Inspection and compliance / open decisions:** Trace each registration to consumers, inspect scope/disposal ownership, and test replacement and same-instance forwarding where changed. Do not alphabetize registration statements without semantic evidence. No unresolved target.


<a id="c21"></a>

## C21 — Configuration organization

**Applicability and comparison unit:** Applicable. Options data and narrow option-provider interfaces, compared separately from external serialization keys.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (3 C# files); [options](../../../src/Xtate.Core/StateMachineOptions/Options/StateMachineOptions.cs), [optionsProvider](../../../src/Xtate.Core/StateMachineOptions/Services/StateMachineOptionsProvider.cs), [xml](../../../src/Xtate.Core/Scxml/Services/ScxmlDeserializer.cs), [guide](../../../.agents/AGENTS.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** StateMachineOptions defines PersistenceLevel.None, DestroyStateMachine, infinite idle timeout, XInclude disabled and nesting limit 16. Provider retains options.Value and exposes narrow interfaces. Deserializer gates includes with XIncludeAllowed == true.

**Selected target:** **Established pattern**. Confidence: **Medium**. Keep defaults with the owning options class and expose only the options a consumer needs. Preserve the existing defaults and XInclude opt-in gate. Do not claim immutable snapshots or reload semantics: the provider retains a reference to mutable options.

**Inspection and compliance / open decisions:** Inspect setters, provider capture and consumer timing. For a configuration change, verify default and explicitly configured behavior. Reload and mutation guarantees are not established; require a separate contract if adding either, rather than assuming Microsoft options semantics.


<a id="c22"></a>

## C22 — Shared functionality placement

**Applicability and comparison unit:** Applicable. Feature-local helpers, cross-feature infrastructure and compatibility shims.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (3 C# files); [catalog](../../../.github/instructions/repo-catalog.instructions.md), [guide](../../../.agents/AGENTS.md), [infra](../../../src/Xtate.Core/Common/Helpers/Infra.cs), [segmented](../../../src/Xtate.Core/StateMachine/Internal/SegmentedName.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [polyRules](../../../.github/instructions/polyfills.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Infra is in Common/Helpers with namespace Xtate; SegmentedName is local to StateMachine/Internal; stream adaptation is ResourceLoaders/Internal. The catalog explicitly identifies unclear Common/DataTypes/root helper boundaries.

**Selected target:** **Existing rule** (explicit user adoption of CONS-015 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. Keep feature-semantic helpers within the owning feature. Use Common for infrastructure whose responsibility is shared across independent features; caller count alone does not prove independence. Use DataTypes for SCXML data-value semantics and precisely guarded Polyfills for framework compatibility. Before proposing a move, name the responsibility, all affected callers, the allowed dependency direction and the compatible destination namespace. Shared code must not introduce a dependency from lower-level infrastructure to a feature merely to avoid duplication.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. Coordinate namespaces with CONS-013. Future moves require source/import, link and public-compatibility checks. Audit every helper's actual callers and semantic ownership; record justified feature-local reuse and flag only evidenced responsibility/dependency mismatches. Adoption is a policy change, not approval for source relocation.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-015). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c01"></a>

## C01 — Domain terminology

**Applicability and comparison unit:** Applicable. Public graph, compiled graph, host identity and event terminology; compare domain symbols and guide vocabulary.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (5 C# files); [guide](../../../.agents/AGENTS.md), [state](../../../src/Xtate.Core/StateMachine/Abstractions/Interfaces/IState.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** The sampled graph cohort contains IState, StateEntity and StateNode, representing contract, mutable source data and compiled node respectively. Host code uses SessionId; EventName defines done/error/invoke segments and platform/execution/communication errors.

**Selected target:** **Established pattern**. Confidence: **High**. Use state machine, state, transition, interpreter, session, invoke, send, data model and SCXML with their existing meanings. Retain Entity for source-model entities and Node for compiled representations where that distinction already exists. Do not rename protocol spellings through terminology cleanup.

**Inspection and compliance / open decisions:** Compare new names with the nearest contract and its compiled/persisted counterpart. Verify externally serialized names separately under C48. No unresolved target.


<a id="c02"></a>

## C02 — Type naming by responsibility

**Applicability and comparison unit:** Applicable. Contracts, services, modules, providers, entities, nodes and test doubles are separate naming cohorts.

**Evidence and discovery coverage:** 8/8 selected evidence files inspected for this criterion (8 C# files); [state](../../../src/Xtate.Core/StateMachine/Abstractions/Interfaces/IState.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [loader](../../../src/Xtate.Core/ResourceLoaders/Services/ResourceLoaderService.cs), [module](../../../src/Xtate.Core/Interpreter/DependencyInjection/StateMachineInterpreterModule.cs), [optionsProvider](../../../src/Xtate.Core/StateMachineOptions/Services/StateMachineOptionsProvider.cs), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** The named peers use IState, StateEntity, StateNode, ResourceLoaderService, StateMachineInterpreterModule and StateMachineOptionsProvider. Tests use ResourceCoverageTest and TestStateMachineScopeManager; not every implementation has a Service suffix.

**Selected target:** **Established pattern**. Confidence: **Medium**. Use I-prefixed interfaces and responsibility-bearing suffixes where the role matches: Module for composition, Provider for selection/provision, Entity and Node for their respective graphs. Tests describe the subject/behavior and use local descriptive doubles. Preserve unsuffixed domain types such as Resource.

**Inspection and compliance / open decisions:** Compare same-role peers and confirm the suffix denotes actual responsibility. Do not add Service to every public class. No measured whole-solution naming frequency is claimed.


<a id="c03"></a>

## C03 — Method and property naming

**Applicability and comparison unit:** Applicable. Domain operations and predicates versus framework override/interop names.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (5 C# files); [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [scheduled](../../../src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Resource exposes async GetBytes/GetContent/GetStream without Async suffix; scope operations include Start/Execute/Destroy. Framework stream methods and DisposeAsync retain Async. EventName has IsDefault and IsError(), and options use nouns/predicates.

**Selected target:** **Established pattern**. Confidence: **High**. Use verbs for operations, nouns for values and Is/Can-style predicates where they fit. Preserve the library's established unsuffixed ValueTask domain APIs; retain Async where required by a framework contract or paired sync/async API. Async suffix is not a universal repository requirement.

**Inspection and compliance / open decisions:** Compare declarations and their callers/overrides. Check that renaming would not change a public contract. Do not infer blocking behavior from a missing Async suffix. No unresolved target.


<a id="c04"></a>

## C04 — Field, parameter and local naming

**Applicability and comparison unit:** Applicable. Handwritten service fields, constants, method parameters and short algorithm locals.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (5 C# files); [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Resource, scope manager and EventName use underscore camelCase private fields; constants and static named values use PascalCase. Encode uses value/span/uValue; EventName uses i/pos in local segmentation. StateEntity's internal Ancestor field is a model-role variant.

**Selected target:** **Established pattern**. Confidence: **Medium**. Use underscore camelCase for private instance storage, camelCase for parameters/locals and PascalCase for exposed members and named constants. Keep short indices inside bounded algorithms and descriptive lifecycle variables. Preserve interface/framework parameter alignment; model storage is not automatically a private-field peer.

**Inspection and compliance / open decisions:** Inspect declarations in role context and ensure locals remain clear across their lifetime. No blanket acronym-expansion or internal-field rename is established; public compatibility takes precedence.


<a id="c05"></a>

## C05 — Units and qualifiers

**Applicability and comparison unit:** Applicable. Duration, encoding, identity and representation-bearing APIs.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (4 C# files); [options](../../../src/Xtate.Core/StateMachineOptions/Options/StateMachineOptions.cs), [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [persistedEvent](../../../src/Xtate.Core/Persistence/Model/PersistedScheduledEvent.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Idle timeout is TimeSpan; scheduled delays are DelayMs integers; persisted FireOn is derived from UTC time plus milliseconds. Resource distinguishes bytes, content, streams, ContentType and Encoding.

**Selected target:** **Established pattern**. Confidence: **High**. Use a unit-bearing type such as TimeSpan or explicit suffix such as Ms for numeric durations. Distinguish SessionId, SendId and InvokeId; keep bytes/text/stream and encoding qualifiers where representation matters. Preserve persisted units and field meanings.

**Inspection and compliance / open decisions:** Trace arithmetic and conversions, especially Task.Delay and persisted FireOn. Verify values are neither renamed nor converted without their consumers. No universal clock abstraction is inferred; see C49.


<a id="c06"></a>

## C06 — Namespace and module structure

**Applicability and comparison unit:** Applicable. Public API namespaces versus organizational folders and internal/service namespaces.

**Evidence and discovery coverage:** 8/8 selected evidence files inspected for this criterion (4 C# files); [editor](../../../.editorconfig), [layout](../../../Xtate.Core.sln.DotSettings), [library](../../../src/Xtate.Core/Xtate.Core.csproj), [state](../../../src/Xtate.Core/StateMachine/Abstractions/Interfaces/IState.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [loader](../../../src/Xtate.Core/ResourceLoaders/Services/ResourceLoaderService.cs), [polyRules](../../../.github/instructions/polyfills.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** RootNamespace is Xtate. IState and StateEntity live in deep organizational folders but use Xtate.StateMachine; StateNode uses Xtate.Interpreter.Model; loader uses its Services namespace. Editorconfig requests folder matching. Polyfills explicitly permit required System namespaces.

**Selected target:** **Existing rule** (explicit user adoption of CONS-013 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. Preserve existing externally visible fully qualified type names for source/binary compatibility. Namespaces express subsystem/API ownership rather than every physical folder. Keep Common/Helpers, Common/Extensions and Common/Exceptions at Xtate; omit Common before named subsystems such as Logging or TaskMonitor. Abstractions itself is organizational: retain the feature namespace, preserve meaningful children such as Logging.Provider, and flatten the existing Interfaces/Enums/Evaluators organizational cohorts. StateMachine and Persistence public model categories retain their feature namespaces. Interpreter/ModelBuilder does not add a ModelBuilder namespace segment; compiled Model/Nodes remains Xtate.Interpreter.Model. DataModel/Handlers does not add a Handlers segment. Preserve semantic Services, DependencyInjection, Internal, Extensions and Provider segments where used by their peer cohort. Framework polyfills keep the required System namespaces. Internal declarations follow the documented role/cohort map; anomalous mappings require a recorded compatibility/framework/role reason, not acceptance by frequency. Align IDE namespace checks with that map instead of moving public namespaces to silence the folder rule.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. Low runtime risk for selecting the compatible rule; high compatibility risk for public renames. During audit, structurally enumerate types/namespaces and effective accessibility, inspect public consumers, derive an explicit per-cohort folder map and flag settings conflicts. Every remaining exception needs its concrete reason. No code move, namespace rename or IDE-setting edit is part of this agreement step.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-013). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c07"></a>

## C07 — Folder responsibilities

**Applicability and comparison unit:** Applicable. Library feature folders and the single test project, compared by responsibility rather than size.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (0 C# files); [guide](../../../.agents/AGENTS.md), [catalog](../../../.github/instructions/repo-catalog.instructions.md), [diRules](../../../.github/instructions/dependency-injection.instructions.md), [testRules](../../../.github/instructions/tests.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** The guide maps parser, public model, compiler/interpreter, hosting, transports and persistence. Sampled features separate Abstractions, Services, Internal, Model, Options and DependencyInjection. Tests use both root files and feature areas.

**Selected target:** **Existing rule**. Confidence: **High**. Place contracts, services, internal helpers and composition in the corresponding existing feature roles; keep parser, model, interpreter, host and persistence responsibilities distinct. Put new tests in the closest feature area. Small size alone does not justify bypassing an established same-role folder.

**Inspection and compliance / open decisions:** Inspect the complete path inventory and actual responsibilities; do not treat every feature as required to have every folder. No empty folders or README placeholders are required. Namespace policy remains C06.


<a id="c08"></a>

## C08 — File naming and contents

**Applicability and comparison unit:** Applicable. Principal type files, nested test doubles, global imports and assembly metadata.

**Evidence and discovery coverage:** 8/8 selected evidence files inspected for this criterion (8 C# files); [state](../../../src/Xtate.Core/StateMachine/Abstractions/Interfaces/IState.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs), [usings](../../../src/Xtate.Core/Properties/GlobalUsings.cs), [parallel](../../../test/Xtate.Core.Test/ParallelizeAttribute.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** The six inspected principal production/test type files match their main type names. Resource tests and scope tests contain related nested doubles. GlobalUsings and ParallelizeAttribute are intentional non-principal-type files.

**Selected target:** **Established pattern**. Confidence: **Medium**. Name ordinary source files after their principal type; keep tightly coupled private doubles/nested types with their owner. Retain dedicated global-using and assembly-attribute files. Do not split a nested helper merely to satisfy one-type-per-file.

**Inspection and compliance / open decisions:** Inspect declarations structurally, accounting for generic names, nested types and partial declarations rather than counting the word class. No exhaustive declaration/file mismatch audit was performed.


<a id="c32"></a>

## C32 — Test placement and naming

**Applicability and comparison unit:** Applicable. MSTest feature and behavioral test files, including maintained root and Legacy tests.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (3 C# files); [testRules](../../../.github/instructions/tests.instructions.md), [testProject](../../../test/Xtate.Core.Test/Xtate.Core.Test.csproj), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [streamTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/InjectedCancellationStreamCoverageTest.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Three inspected behavior test classes use TestClass/TestMethod and subject-plus-CoverageTest names in UnitTests feature folders. Test methods describe outcomes. The project explicitly embeds selected SCXML fixtures, including Legacy/test.scxml.

**Selected target:** **Existing rule**. Confidence: **High**. Use MSTest and the closest existing feature area (DI, Interpreter, HostedTests, Scxml, StateMachines, UnitTests, Persistence or Legacy). Name methods for the behavior/outcome; keep fixture names specific. Preserve maintained root and legacy tests instead of excluding them from review.

**Inspection and compliance / open decisions:** Map changed behavior to the corresponding test cohort. Confirm fixture inclusion in the project when resource lookup requires it. No universal requirement to rename existing Test/Tests/CoverageTest variants is established.


<a id="c33"></a>

## C33 — Test organization

**Applicability and comparison unit:** Applicable. Unit tests and asynchronous behavioral tests; compare assertion and setup shapes.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (4 C# files); [testRules](../../../.github/instructions/tests.instructions.md), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [streamTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/InjectedCancellationStreamCoverageTest.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs), [parallel](../../../test/Xtate.Core.Test/ParallelizeAttribute.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Three inspected test files create inputs, invoke operations and assert observable results with MSTest. Async tests return Task and await exceptions. Scope tests use completion sources; assembly configuration enables method-level parallelism.

**Selected target:** **Existing rule**. Confidence: **High**. Keep tests deterministic, independent and safe under parallel execution. Await asynchronous assertions; assert state, events, returned values and failure identity. Arrange/act/assert separation may be expressed by blank lines without mandatory comments. Use controlled completion rather than arbitrary sleeps.

**Inspection and compliance / open decisions:** Read test control flow, ensure every async operation is observed and shared state is isolated. A timeout guarding a hang is different from using elapsed time as correctness evidence. No test execution is claimed here.


<a id="c34"></a>

## C34 — Fixtures and doubles

**Applicability and comparison unit:** Applicable. Local adapters, mock collaborators, reusable host fixtures and embedded SCXML resources.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (2 C# files); [testRules](../../../.github/instructions/tests.instructions.md), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs), [testProject](../../../test/Xtate.Core.Test/Xtate.Core.Test.csproj). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Resource tests define local non-seekable/token-observing streams. Scope tests use Moq and a CapturingTaskMonitor plus a subclass exposing lifecycle hooks. Repository instructions point to HostedTestBase and shared service helpers.

**Selected target:** **Existing rule**. Confidence: **High**. Keep narrowly scoped doubles local and reuse established host/setup helpers for shared integration roles. Dispose owned test resources and restore global/environment changes. Keep embedded SCXML fixtures minimal. Do not replace a behavior assertion with a mock call count when result semantics matter.

**Inspection and compliance / open decisions:** Inspect setup/teardown and ownership under method-level parallelism. Check mock expectations against the contract, not incidental internal ordering. Shared fixture implementation was not exhaustively reviewed; no universal mocking policy is imposed.


<a id="c35"></a>

## C35 — Failure and boundary tests

**Applicability and comparison unit:** Applicable. Affected runtime, ownership, cancellation, serialization and registration behavior.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (3 C# files); [testRules](../../../.github/instructions/tests.instructions.md), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [streamTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/InjectedCancellationStreamCoverageTest.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** The sampled tests cover disposed reads, null Resource input, cached/direct streams, token combinations, duplicate sessions, ordinary failure cleanup and repeated disposal. Their presence does not prove cleanup-exception or race coverage.

**Selected target:** **Existing rule**. Confidence: **High**. For behavior changes, cover the affected graph/model, runtime, registration and persistence dimensions with focused regressions. Include relevant absence/default, cancellation, partial-failure and disposal boundaries. Match expected behavior to an explicit contract rather than merely exercising lines.

**Inspection and compliance / open decisions:** Inspect assertion semantics and adversarial paths. Compare repeated disposal with throwing disposal and concurrent disposal separately. Coverage gaps implicated by C12/C13/C15/C46 remain policy/evidence questions, not a claim that all existing tests pass or are sufficient.


<a id="c36"></a>

## C36 — Public-contract documentation

**Applicability and comparison unit:** Applicable. Public interfaces, resource APIs and public value types; compare required content separately from presence.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (4 C# files); [state](../../../src/Xtate.Core/StateMachine/Abstractions/Interfaces/IState.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [infra](../../../src/Xtate.Core/Common/Helpers/Infra.cs), [library](../../../src/Xtate.Core/Xtate.Core.csproj). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** IState and Resource have no XML summaries in the inspected files, while EventName has a type summary and Infra documents several guards. The library project does not itself enable GenerateDocumentationFile. Existing source therefore does not establish a uniform required-documentation surface.

**Selected target:** **Existing rule** (explicit user adoption of CONS-014 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. Require XML documentation for all handwritten effectively public/protected library APIs, including public DI/service types that remain externally visible. Generated framework/resource members are assessed through their generator or inherited contract rather than hand-edited. Provide a meaningful summary, type/parameter documentation and return/value semantics where applicable. Document null/default/empty distinctions, cancellation timing, mutation/ownership, thread-safety/lifetime constraints and contractual exceptions when relevant. An override or implementation may use inheritdoc when its contract is unchanged; changed behavior needs an explicit supplement. Enable package XML generation and enforce missing-doc diagnostics for the required surface once documentation is supplied. Record current gaps as remediation work; do not silence warnings merely to pass the audit.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. Ownership, cancellation, diagnostics and concurrency documentation depends on CONS-009/010/011/012. Audit effective accessibility and inherited XML resolution rather than raw public-keyword counts. Verify generated package XML and the analyzer/build rule, then compare documented contracts with signatures and tests. Documentation coverage alone does not certify semantic correctness.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-014). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c37"></a>

## C37 — Implementation comments

**Applicability and comparison unit:** Applicable. Handwritten logic, invariants, suppressions and compatibility explanations.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (4 C# files); [infra](../../../src/Xtate.Core/Common/Helpers/Infra.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [csharp](../../../.github/instructions/csharp.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Infra comments explain guard outcomes; Encode and EventName contain scoped IDE suppressions; resource tests contain file-level ReSharper suppressions. These examples establish forms but not a uniform requirement to explain every suppression.

**Selected target:** **Existing rule** (explicit user adoption of CONS-016 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. Explain non-obvious intent, ownership, invariants and compatibility constraints; avoid comments that merely restate syntax. Every maintained suppression names its precise rule, uses the narrowest practical scope and gives a concrete local reason or an exact shared-policy reference. File/project-wide suppression needs a reason for that breadth. Pragma disable/restore boundaries must balance where the scope permits. Apply the rule to existing maintained suppressions as well as new ones, recording legacy rationale gaps as remediation rather than declaring them acceptable by age. Preserve useful comments and license headers.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. No behavior change is needed to select the rule. Enumerate actual comment/suppression syntax, verify each scope/rule/reason and flag stale explanations against current control flow. Repository-wide rationale-only tasks can be grouped only with their exact affected set and a common verification method. Do not rewrite generated code or invent reasons for existing suppressions.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-016). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c38"></a>

## C38 — Component documentation structure

**Applicability and comparison unit:** Applicable. Central usage/readme, repository architecture guidance and feature-specific rules; compare placement independently of content.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (0 C# files); [readme](../../../README.md), [guide](../../../.agents/AGENTS.md), [catalog](../../../.github/instructions/repo-catalog.instructions.md), [csharp](../../../.github/instructions/csharp.instructions.md), [diRules](../../../.github/instructions/dependency-injection.instructions.md), [testRules](../../../.github/instructions/tests.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Maintained guidance is centralized in README, .agents/AGENTS.md and .github/instructions rather than a README per feature. The catalog records module responsibility, dependencies, key files and unresolved integration areas. The inventory found no pre-existing Core consistency workspace.

**Selected target:** **Established pattern**. Confidence: **High**. Keep user entry, installation and usage in README; architecture/navigation in the repository guide/catalog; path-specific rules in instructions. Document feature responsibility, composition/configuration, lifecycle and limits when relevant. Add substantive feature guides only when they improve navigation; do not create empty README symmetry.

**Inspection and compliance / open decisions:** Check links and consistency with project files separately from content adequacy. The guide's test-target list omits net11.0 relative to the current project; this document records the authoritative project state without rewriting unrelated guidance. No component-guide-per-folder mandate is selected.


<a id="c23"></a>

## C23 — Member ordering

**Applicability and comparison unit:** Applicable. Ordinary handwritten types and the IDE layout's explicit non-reorderable exceptions.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (5 C# files); [layout](../../../Xtate.Core.sln.DotSettings), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** The decoded Default Pattern orders public delegates, public enums, public Provider-named nested classes, static fields/constants, instance fields, constructors, properties/indexers, interface implementation regions, other members and nested types. Sampled types visibly use fields before properties and interface regions. Explicit interface properties may be captured by higher-priority interface grouping.

**Selected target:** **Existing rule**. Confidence: **High**. Use the configured matcher priorities: delegate/enum entries and interface regions have priority 100. Constants precede static fields; instance fields sort by readonly and name; constructors use the configured Static key. Interface members group by interface and sort by interface/kind/access. Other methods/events/helpers stay in the residual group; do not invent a public/private order. COM/NoReorder types are exempt.

**Inspection and compliance / open decisions:** Inspect the decoded XML, applicable matcher and declaration order, preserving initializer dependencies and layout/interop contracts. The xUnit/NUnit templates are present but do not prescribe MSTest ordering. Never reorder fields whose initialization depends on another field solely to satisfy visual sorting.


<a id="c24"></a>

## C24 — Related-member grouping

**Applicability and comparison unit:** Applicable. Interface implementations, overload families and lifecycle/helper methods.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (5 C# files); [layout](../../../Xtate.Core.sln.DotSettings), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [entity](../../../src/Xtate.Core/StateMachine/Model/Structures/StateEntity.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [optionsProvider](../../../src/Xtate.Core/StateMachineOptions/Services/StateMachineOptionsProvider.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Resource, entities, node and options provider group members into Interface regions; layout explicitly creates those groups. Stream overrides are paired with corresponding private async helpers rather than collected into one abstract access-level sequence.

**Selected target:** **Existing rule**. Confidence: **High**. Preserve configured interface grouping and keep related overloads/helpers readable within the applicable residual group. Lifecycle methods may be separated by interface regions as configured. Do not require regions for every conceptual group or destroy a useful override/helper pairing.

**Inspection and compliance / open decisions:** Check interface membership and the region/group matcher, then follow the operation's control flow across helpers. Existing groups and initializer dependencies take priority over an invented global alphabetical method order. No unresolved target.


<a id="c25"></a>

## C25 — Constructor organization

**Applicability and comparison unit:** Applicable. Value-owning constructors, compiled-node initialization and service injection are different cohorts.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (5 C# files); [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [scheduled](../../../src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs), [optionsProvider](../../../src/Xtate.Core/StateMachineOptions/Services/StateMachineOptionsProvider.cs), [loader](../../../src/Xtate.Core/ResourceLoaders/Services/ResourceLoaderService.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Resource guards its constructor stream in field initialization. StateNode calls the base constructor, projects child nodes, registers them and publishes properties. ScheduledEvent delegates to its base; options provider captures options.Value. Loader receives collaborators through init-properties.

**Selected target:** **Established pattern**. Confidence: **Medium**. Keep constructor inputs in domain/base-dependency order, validate before publishing invalid state, and preserve registration and initializer dependencies. Allow primary constructors for simple capture and explicit bodies for staged model initialization. Service injection remains governed by C19.

**Inspection and compliance / open decisions:** Trace base construction, guards, assignments and any side effects; verify failed construction cannot publish incomplete state. No universal alphabetical parameter order or ban on constructor work is established.


<a id="c26"></a>

## C26 — Accessibility

**Applicability and comparison unit:** Applicable. Public extension points, explicit interface projections and internal implementation helpers.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (4 C# files); [editor](../../../.editorconfig), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [library](../../../src/Xtate.Core/Xtate.Core.csproj). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Editorconfig prefers explicit access modifiers except interface members. Resource and StateNode are public and extensible; InjectedCancellationStream is internal sealed and Encode internal static. The project grants InternalsVisibleTo to the test assembly.

**Selected target:** **Existing rule**. Confidence: **High**. Use explicit accessibility as configured and keep internal helpers internal. Preserve public/protected virtual extension points and explicit interface projections. Do not seal public service types or narrow accessibility merely to match a nearby internal helper.

**Inspection and compliance / open decisions:** Inspect the role, existing overrides and public compatibility before changing access or sealing. Test visibility is supported by the project and does not require widening production APIs. No universal public-surface minimization change is selected.


<a id="c27"></a>

## C27 — Field mutability

**Applicability and comparison unit:** Applicable. Stable references versus lifetime sentinels and lazy caches.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (5 C# files); [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [optionsProvider](../../../src/Xtate.Core/StateMachineOptions/Services/StateMachineOptionsProvider.cs), [scheduled](../../../src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [layout](../../../Xtate.Core.sln.DotSettings). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** StateNode and options provider retain readonly references; ScheduledEvent owns a readonly token source. Resource's stream/cache fields and scope manager's dictionary field are mutable because disposal/cache state changes. Cleanup profile enables making eligible fields readonly.

**Selected target:** **Established pattern**. Confidence: **High**. Use readonly for references that do not change after construction; preserve mutable fields that encode cache or lifecycle transitions. Readonly does not imply immutable referenced objects or thread safety. Do not convert init-injected properties into fields without composition evidence.

**Inspection and compliance / open decisions:** Trace all assignments including disposal and conditional paths; distinguish reference replacement from object mutation. Check initializer order and publication. No unresolved target.


<a id="c28"></a>

## C28 — Constants and defaults

**Applicability and comparison unit:** Applicable. Protocol constants, per-feature defaults and lifecycle sentinels.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (4 C# files); [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [options](../../../src/Xtate.Core/StateMachineOptions/Options/StateMachineOptions.cs), [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** EventName owns separator and protocol identifiers; options own explicit defaults; scheduler owns EmptySendId. Encode uses bit masks and length bounds as part of the representation algorithm.

**Selected target:** **Established pattern**. Confidence: **High**. Keep defaults with the owning options type and named protocol identities with the responsible domain type. Preserve sentinel values and initializer dependency order. Bit masks that express the algorithm need not be extracted into unrelated shared constants simply because they are numeric.

**Inspection and compliance / open decisions:** Inspect repeated values for shared meaning, not textual equality; compare persisted/wire values under C48 before changing them. Never derive complexity or performance thresholds from these constants. No unresolved target.


<a id="c29"></a>

## C29 — Guards and main-path layout

**Applicability and comparison unit:** Applicable. Resource access, decoding and lifecycle orchestration methods.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (4 C# files); [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Resource checks disposal then uses cache early returns; stream token selection exits through simple branches. ScopeManager.Run separates acquisition/start and cleanup ownership using try/finally. Encode uses short range/prefix branches.

**Selected target:** **Established pattern**. Confidence: **Medium**. Use preflight guards and early returns for local absence/cache cases; keep lifecycle ownership transitions explicit in try/finally or using. Preserve the order of publication, registration and cleanup rather than flattening every method mechanically.

**Inspection and compliance / open decisions:** Read all exits and exception paths. Verify no early return skips required cleanup or leaves a result incomplete. Existing cleanup-failure gaps are decisions under C13/C15, not approved examples to reproduce.


<a id="c30"></a>

## C30 — Expression decomposition

**Applicability and comparison unit:** Applicable. Model initialization, binary encoding and token routing, compared by semantic complexity.

**Evidence and discovery coverage:** 4/4 selected evidence files inspected for this criterion (4 C# files); [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** StateNode names initial/states/historyStates/transitions before registration and assignment. Stream uses IsCombinedTokenRequired plus async helper methods. Encode keeps bit operations together because they express an encoding layout.

**Selected target:** **Established pattern**. Confidence: **Medium**. Name intermediate domain stages and predicates when they explain state or ownership. Extract repeated semantic operations, not arbitrary line-count chunks. Keep compact bitwise/representation expressions with their algorithm; do not invent numeric complexity limits.

**Inspection and compliance / open decisions:** Trace values and side effects and verify helper extraction preserves evaluation order, overflow and exception timing. No universal maximum expression length or method size is established.


<a id="c31"></a>

## C31 — Property and method presentation

**Applicability and comparison unit:** Applicable. Simple projections/delegations versus multi-step asynchronous or lifecycle work.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (5 C# files); [editor](../../../.editorconfig), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [optionsProvider](../../../src/Xtate.Core/StateMachineOptions/Services/StateMachineOptionsProvider.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Options provider uses expression-bodied projections; stream uses expression delegation plus block async helpers. Resource and scope disposal use block bodies. Editorconfig preserves single-line blocks/statements but does not mandate every member be expression-bodied.

**Selected target:** **Existing rule**. Confidence: **Medium**. Follow explicit formatter settings; use concise expressions for direct projections/delegation as seen in peers and blocks for staged validation, awaiting and cleanup. Preserve framework-required accessor forms and private-get/init injection properties.

**Inspection and compliance / open decisions:** Inspect generated control flow and readability rather than forcing a single body form. Formatter preferences do not authorize changing evaluation, exception or disposal semantics. No unresolved target.


<a id="c39"></a>

## C39 — Formatting

**Applicability and comparison unit:** Applicable. Handwritten C# formatting and headers; generated inputs are not independent style peers.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (2 C# files); [editor](../../../.editorconfig), [layout](../../../Xtate.Core.sln.DotSettings), [csharp](../../../.github/instructions/csharp.instructions.md), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Editorconfig uses hard tabs with width four, keyword spacing, System-first imports, no cast-following space and preserved single-line blocks/statements. Resource shows normal multiline braces; Encode retains compact branch lines. Source instructions require the neighboring AGPL header style.

**Selected target:** **Existing rule**. Confidence: **High**. Apply .editorconfig and the repository cleanup profile where applicable, preserving headers and precise conditional compilation. Do not infer universal line-ending or encoding rules absent from those settings, and do not count generated resource boilerplate as independent style evidence.

**Inspection and compliance / open decisions:** Use configured formatting and inspect the diff for semantic/conditional changes. Compare same-role source; do not reformat generated files or the solution as part of documentation. No formatter run is claimed.


<a id="c40"></a>

## C40 — Imports and usings

**Applicability and comparison unit:** Applicable. Production/test global imports, local feature imports and semantic aliases.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (4 C# files); [editor](../../../.editorconfig), [usings](../../../src/Xtate.Core/Properties/GlobalUsings.cs), [testUsings](../../../test/Xtate.Core.Test/GlobalUsings.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Editorconfig sorts System directives first. Library/test projects have separate global-using files. Library aliases IServiceProvider to Xtate.IoC.IServiceProvider, NotNullAttribute to the BCL attribute, and Empty to ValueTuple. Local files add feature-specific namespaces.

**Selected target:** **Existing rule**. Confidence: **High**. Use global imports for the established common set and local imports for feature dependencies. Preserve semantic aliases, especially IServiceProvider; organize local imports with the configured System-first preference. Do not replace Xtate service-provider references with the BCL interface inadvertently.

**Inspection and compliance / open decisions:** Resolve ambiguous names before removing imports or aliases. Check global declarations and conditional directives as well as local text. Existing global lists are not evidence of a fully enforced alphabetical order.


<a id="c41"></a>

## C41 — Equivalent language syntax

**Applicability and comparison unit:** Applicable. Handwritten modern syntax across all supported target frameworks.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (3 C# files); [editor](../../../.editorconfig), [build](../../../Directory.Build.props), [csharp](../../../.github/instructions/csharp.instructions.md), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Build properties select preview language version. Editorconfig suggests var, predefined type keywords, inline out variables, simple default, object initializers and pattern matching. Samples use primary constructors, collection expressions and property patterns.

**Selected target:** **Existing rule**. Confidence: **High**. Apply the configured preferences when behavior-equivalent and compatible with every target; preserve explicit types when needed for overload resolution, representation or clarity. Modern syntax support does not establish availability of modern runtime APIs on legacy targets.

**Inspection and compliance / open decisions:** Compare inferred type, binding and evaluation behavior before a stylistic rewrite. Check polyfills and target guards for runtime dependencies. No new syntax migration is authorized by documenting these rules.


<a id="c42"></a>

## C42 — Project and build organization

**Applicability and comparison unit:** Applicable. Library, test project, solution, imported properties and publishing workflow.

**Evidence and discovery coverage:** 7/7 selected evidence files inspected for this criterion (0 C# files); [library](../../../src/Xtate.Core/Xtate.Core.csproj), [testProject](../../../test/Xtate.Core.Test/Xtate.Core.Test.csproj), [sln](../../../Xtate.Core.sln), [build](../../../Directory.Build.props), [guide](../../../.agents/AGENTS.md), [publish](../../../.github/workflows/publish.yml), [ciRules](../../../.github/instructions/github-actions.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Both project files and solution membership were inspected. Library targets net11.0/net10.0/net9.0/net8.0/netstandard2.0/net462; tests target net11.0/net10.0/net9.0/net8.0 plus optional net462. Shared build props import parent props when present. Publish runs restore/build/test/pack and separate Windows tests.

**Selected target:** **Existing rule**. Confidence: **High**. Keep role-specific targets and preserve modern/legacy compatibility. Treat generated build properties as inputs, not hand-edit targets. Resolve parent imports when building this monorepo checkout; a standalone Core checkout can have different effective imports. Use SkipNetFrameworkTests only for the test target gate.

**Inspection and compliance / open decisions:** Inspect evaluated imports and project membership rather than relying only on README target lists. Verify workflow ordering and no-restore/no-build assumptions when changing build configuration. This documentation records configuration, not a successful build.


<a id="c43"></a>

## C43 — Analyzers and suppressions

**Applicability and comparison unit:** Applicable. Build-wide analyzer enablement, target-specific suppressions and local pragmas.

**Evidence and discovery coverage:** 7/7 selected evidence files inspected for this criterion (3 C# files); [build](../../../Directory.Build.props), [library](../../../src/Xtate.Core/Xtate.Core.csproj), [layout](../../../Xtate.Core.sln.DotSettings), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [csharp](../../../.github/instructions/csharp.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Build props enable NET analyzers and code-style enforcement. Library references ConfigureAwaitChecker.Analyzer with PrivateAssets all and suppresses nullable warnings for two legacy targets. Encode/EventName use scoped IDE pragmas; tests use ReSharper directives.

**Selected target:** **Existing rule**. Confidence: **High**. Keep enabled analyzers and preserve scoped compatibility suppressions. Review individual suppression intent before removal; do not broaden a local exception to solution-wide disabling. Apply the proposed rationale rule in C37 once adopted. IDE layout and inspection settings are not equivalent to build diagnostics.

**Inspection and compliance / open decisions:** Inspect effective target properties, package assets and pragma boundaries. Distinguish IDE suggestions from build failures. No clean-analyzer result is claimed, and absence of diagnostics in this documentation run proves nothing about conformance.


<a id="c44"></a>

## C44 — Dependency declarations

**Applicability and comparison unit:** Applicable. Package versions, asset scopes, project references and package-source mapping.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (0 C# files); [versions](../../../Directory.Packages.props), [globalVersions](../../../Global.Packages.props), [library](../../../src/Xtate.Core/Xtate.Core.csproj), [testProject](../../../test/Xtate.Core.Test/Xtate.Core.Test.csproj), [nuget](../../../NuGet.Config), [guide](../../../.agents/AGENTS.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Local central versions name Xtate.IoC 1.9.0 and Channels 10.0.11; generated global versions supply shared packages. Both project files omit package versions. Analyzer/shared-content and coverage assets use PrivateAssets all; NuGet maps Xtate.* to the configured GitHub source with a general NuGet source.

**Selected target:** **Existing rule**. Confidence: **High**. Keep package versions centralized and PackageReference entries versionless. Keep test-only tools in tests and private build/analyzer assets private. Preserve target-conditional compatibility packages and source mapping. Edit the authoritative dependency inputs rather than generated global files.

**Inspection and compliance / open decisions:** Resolve imported version definitions and asset flow before changes. No lockfile/restore-audit policy is inferred solely from package lists; adding one is a separate policy choice. The pre-existing Global.Packages.props modification is preserved and fingerprinted.


<a id="c45"></a>

## C45 — Complexity and responsibility

**Applicability and comparison unit:** Applicable. Feature services, orchestration and representation helpers; comparison is semantic responsibility, not average size.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (3 C# files); [catalog](../../../.github/instructions/repo-catalog.instructions.md), [node](../../../src/Xtate.Core/Interpreter/ModelBuilder/Model/Nodes/StateNode.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [editor](../../../.editorconfig), [layout](../../../Xtate.Core.sln.DotSettings). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** The inspected services separate model projection, scope orchestration and byte encoding; helper methods expose stages. No explicit numeric method-size or complexity limit was found in the inspected formatter/IDE settings. Large or branch-heavy code is not by itself a violation.

**Selected target:** **Existing rule** (explicit user adoption of CONS-017 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. Assess cohesion, identifiable stages, ownership transitions and dependency direction. A deviation must name concrete responsibilities that conflict or change independently, show the affected invariants/callers and propose a coherent boundary that preserves behavior. Method length, branch count and sample averages are investigation signals, not mandatory limits. Do not require extraction merely because a method is large, and do not collapse a meaningful contract/composition boundary to reduce file count. Suspected hotspots without enough semantic evidence become investigation tasks, not mandatory rewrites. Performance claims require workload-specific measurement.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. Coordinate helper ownership with CONS-015 and the existing component-boundary criteria. For each finding, provide the exact type/method, conflicting responsibilities, proposed destination and compatibility/lifetime regression checks. An audit record must explain its semantic judgment; a numeric report alone is not coverage.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-017). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c46"></a>

## C46 — Concurrency and synchronization

**Applicability and comparison unit:** Applicable. Shared host/scheduler state, disposal races and completion publication.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (5 C# files); [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [scheduled](../../../src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs), [parallel](../../../test/Xtate.Core.Test/ParallelizeAttribute.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** ScopeManager uses ConcurrentDictionary but also reads/clears the owning field across lifecycle methods. Scheduler has background delayed work and cancellation. Scope tests use controlled completion, but the inspected tests do not establish a concurrent Start-versus-Dispose contract. Test parallelism is a separate property.

**Selected target:** **Existing rule** (explicit user adoption of CONS-011 on 2026-10-30). Confidence: **High** for the selected policy; existing code is not presumed compliant. Host/scheduler owners define one observable shutdown admission boundary. Work admitted before it retains an owner and reaches a terminal result; new Start/Schedule work after it fails with ObjectDisposedException before registration. Terminal Cancel/Destroy commands remain idempotent where their existing contracts allow an already absent item. Each owned scope/event is released once even when completion races shutdown. Repeated/concurrent DisposeAsync calls share shutdown completion and its selected failure outcome. DisposeAsync waits for owned runtime work and cleanup, not arbitrary diagnostic-provider completion; a retained observer still owns pending diagnostics under CONS-007. Synchronous Dispose initiates the supported synchronous shutdown path and transfers any remaining async observation explicitly without sync-over-async. Do not call external callbacks while holding an ownership/admission lock. Reentrant callbacks must not wait on their own unfinished operation; document such restrictions and use controlled handoff rather than a recursive lock or async-void callback.

**Inspection and compliance / open decisions:** No remaining policy decision for this criterion. High implementation/lifetime risk if the audit finds deviations; no blanket locking rewrite is selected. Record exact admission and ownership transitions. Require barrier-controlled Start-versus-Dispose, completion-versus-cleanup, repeated sync/async disposal and reentrant-callback tests with bounded hang guards. Verify preserved public results and no duplicate release; do not use passing parallel tests or a concurrent collection as proof of the whole contract.

**Approval and policy evidence:** The user approved all nine complete recommendations in [the decision record](TODO.md#cons-011). The [policy input snapshot](AUDIT.policy-inputs.json) records exact source/settings fingerprints and declared-namespace discovery. Original observed/evidence paragraphs above remain historical sampled discovery; this adoption does not claim fresh semantic audit coverage or implemented remediation.

<a id="c47"></a>

## C47 — Asynchronous execution

**Applicability and comparison unit:** Applicable. Library operations, framework adapters and explicitly detached monitored work.

**Evidence and discovery coverage:** 7/7 selected evidence files inspected for this criterion (5 C# files); [csharp](../../../.github/instructions/csharp.instructions.md), [guide](../../../.agents/AGENTS.md), [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [scope](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs), [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Library examples use ValueTask and ConfigureAwait(false); framework stream overrides use Task/ValueTask as required. ScopeManager and scheduler detach via Forget(TaskMonitor), and tests capture and await monitored tasks. ScopeManager's cleanup-before-completion risk is separately recorded in C15.

**Selected target:** **Existing rule**. Confidence: **High**. Preserve ValueTask and ConfigureAwait(false) conventions and avoid blocking I/O/synchronous waits in async runtime paths. Await owned operations or explicitly transfer observation to ITaskMonitor; never detach by simply discarding a returned task. Preserve sync disposal as a separate supported contract.

**Inspection and compliance / open decisions:** Trace task ownership to observation and terminal completion. Inspect exception/cancellation paths and ValueTask consumption; do not assume task monitoring cures incomplete result publication. Apply the following selected CONS-007 policy to its named boundaries. Other resource-lifetime and concurrency decisions remain separate.

<a id="cons-007-policy"></a>

### CONS-007 — Selected failure and observation policy

Selected on 2026-10-18 under the user's request to fix CONS-007. This is a contract decision for CONS-001, CONS-004, CONS-005 and CONS-006; their runtime changes remain pending. It applies to `StateMachineScopeManager.WaitAndCleanup`, `ExternalServiceScopeManager.WaitAndCleanup`, task-monitor completion observers, the idle timer's error reporting, and `LazyTask`'s optional-monitor fallback. It does not select a new general retry, timeout, cleanup-draining or concurrent-disposal policy.

**Operation and cleanup outcomes.** Capture the operation outcome, attempt its existing cleanup, capture any cleanup error, and complete the separately published caller result before invoking diagnostic callbacks. Cleanup failure must not bypass the result setter. The outcome is independent of whether the failing operation completes synchronously or after suspension.

| Operation outcome | Cleanup outcome | Required caller result |
| --- | --- | --- |
| Success | Success | Successful result, with the original value where applicable |
| Fault | Success | Faulted with the same operation exception instance |
| Cancellation | Success | Cancelled with the original cancellation token, including a default token |
| Success | Failure | Faulted with the same cleanup exception instance; if that exception is an `OperationCanceledException`, faulted with a single-inner `AggregateException` retaining it |
| Fault | Failure | Faulted with `AggregateException(operationException, cleanupException)` |
| Cancellation | Failure | Faulted with `AggregateException(operationCancellation, cleanupException)`; retain the cancellation exception and its token as the first inner exception |

The two double-failure rows deliberately use a faulted result: cleanup failure makes shutdown unsuccessful even when the operation was cancelled. A cancelled result cannot also carry the cleanup fault. The aggregate retains both errors rather than mutating `Exception.Data` or losing one through logging. Preserve the original exception objects, their ordering and stack information; do not flatten an original aggregate. If cleanup already reports multiple errors as an aggregate, retain that cleanup aggregate as the second inner exception. A cleanup `OperationCanceledException` is a cleanup failure, not by itself cancellation of the operation's result. In the cleanup-only case, wrap it in a single-inner aggregate so async forwarding methods do not reclassify the propagated exception as operation cancellation. These new cleanup-failure outcomes replace an unfinished-result path; ordinary single-fault identity and operation-cancellation behavior are preserved.

**Reporting failure and terminal fallback.** The result owner publishes its outcome independently of diagnostic completion. A detached observer consumes its input exactly once, attempts its selected reporting callback once, and catches both a callback that throws before returning and an asynchronously faulted/cancelled reporting ValueTask. Reporting errors never replace the already selected caller result. No retry, recursive logger invocation, blocking wait or async-void escape is permitted.

The selected default terminal fallback is an internal process-local **latest-failure slot**, updated atomically with no locks, callbacks, formatting, serialization, I/O or new background Task. It retains one failure record containing the observed failure/cancellation and, when distinct, the reporting failure. Capture must not throw ordinary exceptions. Reporting a failure that is the same exception instance as the original must not duplicate it. Provide an internal atomic snapshot for diagnostics/tests; no public API or new dependency is required. A newer fallback record replaces the previous record and releases that reference. The bound is one retained record, not a promised byte limit for arbitrary exception graphs. This deliberately sacrifices historical/durable fallback reporting to keep the last-resort path nonblocking and nonrecursive; it is not a replacement for normal logging. Do not use `ILogger`, `Trace`, console/file/network output, events or user-provided delegates from this final sink, because those can themselves throw or block.

Ordinary logging still uses the injected logger. A reporting operation that has not completed remains explicitly owned by its observer; the decision does not invent a logging deadline or promise that an arbitrary logging provider completes. A caller's result and cancellation return must not wait for diagnostic completion. Synchronous `TaskMonitor.Forget` propagation for already faulted/cancelled inputs remains its existing contract; owners such as a void timer callback must contain that synchronous exception and use the final fallback. Failure in a diagnostic observer must never fault a second discarded observer Task.

**Optional monitor compatibility.** Both existing `LazyTask<T>` constructors remain valid, including a cancellable token with `taskMonitor: null`. Preserve factory-once execution, shared result Task identity and early caller cancellation. If the caller result was already cancelled and the factory subsequently faults, observe the factory failure once: use the supplied monitor when present, otherwise the internal latest-failure slot. If the supplied monitor throws or its internal reporter fails, retain that secondary error through the same terminal fallback. Never turn the already cancelled public Task into a faulted Task or discard a preserved ValueTask without an observer. Ordinary factory faults that are successfully published to the public result are not separately reported just to duplicate their consumption.

**Acceptance and compatibility.** CONS-001 tests the six outcome rows for each manager and verifies result completion before reporting, with original identity/token and ordered aggregates. CONS-004 tests throwing and asynchronously failing/cancelled reporters and verifies one bounded fallback capture without recursion or an unowned fault. CONS-005 tests null/provided monitors, initially/later cancelled tokens, late factory success/failure and exactly-once source-backed ValueTask consumption. CONS-006 tests both failure timings of timer logging without blocking the callback. Tests of the shared fallback must isolate or restore its state under the test suite's parallelism. Preserve public signatures, DI/property-injection lifetimes and persisted/wire formats; use APIs available on every current library target (`net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, `net462`). No new runtime/package dependency is selected.


<a id="c48"></a>

## C48 — Serialization and external representation

**Applicability and comparison unit:** Applicable. SCXML parsing, event names, persisted scheduling fields and binary encodings.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (4 C# files); [xmlRules](../../../.github/instructions/scxml.instructions.md), [xml](../../../src/Xtate.Core/Scxml/Services/ScxmlDeserializer.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [persistedEvent](../../../src/Xtate.Core/Persistence/Model/PersistedScheduledEvent.cs), [guide](../../../.agents/AGENTS.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Deserializer constructs then validates the public model. EventName uses dot-separated segments and ordinal descriptor matching. PersistedScheduledEvent stores TypeInfo, DelayMs and FireOn under fixed keys; Encode implements a specific multi-byte integer representation.

**Selected target:** **Existing rule**. Confidence: **High**. Preserve external names, numeric identities, units, namespace handling and persisted representation during source cleanup. Keep model/validator/compiler/persistence changes coordinated. Treat any wire/storage change as a compatibility decision with explicit versioning/migration evidence, not a naming fix.

**Inspection and compliance / open decisions:** Trace writer and reader as a pair and compare existing payload/fixture expectations. The sample establishes representation sensitivity, not exhaustive wire compatibility or an approved migration strategy. Do not reinterpret Encode as a generic text codec based on its prefix shapes.


<a id="c49"></a>

## C49 — Time, culture and determinism

**Applicability and comparison unit:** Applicable. Scheduler durations, persisted deadlines, protocol comparison and behavioral tests.

**Evidence and discovery coverage:** 5/5 selected evidence files inspected for this criterion (4 C# files); [persistedEvent](../../../src/Xtate.Core/Persistence/Model/PersistedScheduledEvent.cs), [scheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [scopeTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineScopeManagerCoverageTest.cs), [testRules](../../../.github/instructions/tests.instructions.md). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** PersistedScheduledEvent derives FireOn from DateTime.UtcNow plus DelayMs. Scheduler waits with Task.Delay. Event descriptors use StringComparison.Ordinal. Scope tests use controlled completion plus bounded wait cancellation rather than elapsed-time assertions.

**Selected target:** **Established pattern**. Confidence: **High**. Keep persisted deadlines in the existing UTC representation, preserve millisecond delays and use ordinal comparisons for protocol identifiers where established. Prefer controlled test completion. Do not introduce culture-sensitive protocol matching or assume wall-clock time is monotonic.

**Inspection and compliance / open decisions:** Inspect time/culture conversion boundaries and clock-dependent tests. A pluggable clock or universal TimeProvider migration is not established; require a separate design decision if deterministic scheduling simulation is needed. No wall-clock timing guarantees are inferred.


<a id="c50"></a>

## C50 — Allocation and performance conventions

**Applicability and comparison unit:** Applicable. Caching, spans, immutable-array construction and conditional token linking; compare by workload and ownership.

**Evidence and discovery coverage:** 6/6 selected evidence files inspected for this criterion (6 C# files); [resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [stream](../../../src/Xtate.Core/ResourceLoaders/Internal/InjectedCancellationStream.cs), [event](../../../src/Xtate.Core/StateMachine/Model/Types/EventName.cs), [encode](../../../src/Xtate.Core/Persistence/Internal/Encode.cs), [resourceTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), [streamTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/InjectedCancellationStreamCoverageTest.cs). This is a purposive discovery sample, not an exhaustive count of compliant declarations. Exact paths and hashes are in the tracker and inventory.

**Observed:** Resource caches content/bytes; InjectedCancellationStream avoids a linked source for one effective token. EventName builds a fresh array then wraps it with ImmutableCollectionsMarshal; Encode operates on spans. Tests validate some observable caching/token behavior, not performance.

**Selected target:** **Established pattern**. Confidence: **Medium**. Preserve deliberate caching and allocation-sensitive ownership patterns when behavior-equivalent. Do not replace array ownership transfer, spans or ValueTask with a visually uniform form without checking aliasing and workload cost. New optimizations require measurement and correctness evidence.

**Inspection and compliance / open decisions:** Trace ownership/escape and benchmark the relevant workload before claiming improvement. No allocation budget, benchmark result or universal pooling requirement is selected. Resource's public buffer ownership remains a decision under C13 rather than a performance excuse.



## Completion and approved policies

All 50 criteria have selected auditable targets: 32 Existing rule and 18 Established pattern records, with no N/A criteria or unresolved policy decisions. On 2026-10-30 the user explicitly adopted CONS-009 through CONS-017, resolving the six previous Decision required records and adopting the three previous Proposed target records. The exact approved recommendations and their alternatives remain in the single TODO as decision history.

The approved policies address cancellation/admission (C12), resource ownership and cleanup (C13), host concurrency/shutdown (C46), diagnostics (C16), namespace mapping (C06), public XML documentation (C36), helper ownership (C22), implementation comments/suppressions (C37) and qualitative responsibility review (C45). They are targets for audit, not claims that their future remediation is already complete. Existing public API, wire/persisted compatibility and the named CONS-007 failure policy remain constraints.

Original sampled discovery, its baseline, observations and fingerprints remain historical. The 2026-10-30 policy input snapshot enumerates 783 library and 258 test C# files and records namespace/configuration discovery; it does not certify every source file against every criterion. Existing CONS-001 through CONS-008 repairs were completed separately and must be revalidated against current inputs during audit.

The authorized next action is the full solution consistency audit, including C47 revalidation. It must pin this convention revision/digest, enumerate complete eligible sets by criterion/role, assess each unit and map every deviation to the existing common TODO. No production source change is performed by documentation or audit.