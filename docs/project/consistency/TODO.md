# Xtate.Core consistency TODO

Current request (2026-10-30): revalidate C47 and audit all remaining criteria across the solution. The user selected **agree missing rules first, then perform the full audit**, and approved all nine recommendations. CONS-009 through CONS-017 are complete policy decisions adopted into convention revision 61; they do not certify code compliance. The full audit is now in progress. Their [input snapshot](AUDIT.policy-inputs.json) enumerates 783 library and 258 test C# files, including one generated resource designer. It is an inventory and namespace-discovery input, not semantic audit coverage. The audit pins convention revision **61**, digest `15FB78A6A7CD38564792BA8AEDECA97B127AF14C4AC608FFB3C0C53D99325A46`.

The previous approved audit covered **asynchronous execution (C47) across hosting, scheduling and task monitoring**: the 76 production files enumerated in [AUDIT.inventory.json](AUDIT.inventory.json), including persistent scheduling, the dedicated I/O scheduler, and task forwarding helpers. That historical audit was pinned to convention revision **51**, digest `2701F65161D3762E23EEFC83DA28FB487901CBD9EE62D9C20E7A4F608BC3F382`.

All eight original tasks are complete: CONS-007 supplies the [selected policy](CONVENTIONS.md#cons-007-policy), and the seven runtime repairs are implemented and regression-tested. The six remaining repairs were completed on 2026-10-28 against convention revision 52. The original static C47 audit remains pinned to revision 51; its affected policy/source checks remain stale until separately revalidated. Its five source-linked reproductions and original member spans remain historical evidence. See [coverage](COVERAGE.md), [exact original file dispositions and member spans](AUDIT.coverage.json), [structural evidence](AUDIT.structure.json), [reproductions](AUDIT.probes.json), and [progress](TODO.progress.json).

The operative target of the historical audit is [C47](CONVENTIONS.md#c47): preserve ValueTask/context conventions, avoid blocking async runtime paths, and retain observation and terminal completion of owned operations. The selected failure-precedence and diagnostic-fallback policy guides those repairs; it does not count as a full audit of C12/C13/C15/C16/C46. Seven remediation tasks addressed nine deviating files; the completed decision task supplied their selected policy. The new solution-wide audit is in progress; see the current coverage checkpoint.

## Policy agreement before the solution-wide audit

All nine decisions were approved by the user on 2026-10-30 and documented in convention revision 61. Their original recommendations and alternatives below are retained as decision history; adoption does not certify existing implementations or authorize implementing future audit findings. The original eight completion records remain intact.

<a id="cons-009"></a>

- [x] **CONS-009 [P1] Select scheduler cancellation and admission semantics.**

  **Criterion / blocked checks:** [C12](CONVENTIONS.md#c12); scheduler admission, persistence and cancellation boundaries. Exact initial inputs: [IEventScheduler](../../../src/Xtate.Core/StateMachineHost/Abstractions/IEventScheduler.cs), [InProcEventScheduler](../../../src/Xtate.Core/StateMachineHost/Services/InProcEventScheduler.cs), [PersistentEventScheduler](../../../src/Xtate.Core/Persistence/Services/PersistentEventScheduler.cs), [ScheduledEvent](../../../src/Xtate.Core/StateMachineHost/Model/ScheduledEvent.cs), their callers and tests; the full eligible set will be derived from every CancellationToken declaration/call boundary during audit.

  **Evidence / uncertainty:** ScheduleEvent currently adds the event and starts monitored delivery without reading its caller token. Delivery instead links owner-disposal and event-lifetime tokens. Current scheduler tests predominantly pass CancellationToken.None; they do not select the caller-token contract.

  **Recommended target:** A caller token controls admission and cancellable work before the operation commits, not the lifetime of an admitted scheduled event. Check a pre-cancelled token before registration or other externally visible effects. Define the commit point for both in-memory and persistent scheduling; failed/cancelled admission must leave neither an orphan delivery nor an orphan persisted record. Once admission succeeds, delivery is owned by the scheduler and is cancelled through CancelEvent/event cancellation or owner shutdown, not by later cancellation of the scheduling caller. CancelEvent also rejects pre-cancellation before mutation; once removal/cancellation is committed, finish that cancellation without undoing it. A wait-only cancellation API must explicitly preserve observation of the underlying operation and its original cancellation token. Existing stream token-linking and exception/token identity rules remain in force.

  **Alternative / tradeoff:** Link the scheduling caller token to the entire delayed delivery lifetime. This makes a short-lived caller able to cancel an already accepted event and changes the independent scheduling contract. Another explicit alternative is admission-only tokens with no pre-commit asynchronous cancellation; this is simpler but makes persistence waits less responsive.

  **Dependencies / risk / acceptance:** Coordinate the commit/shutdown boundary with CONS-010/011. Medium-to-high behavioral risk for a future repair; public signatures stay stable. After approval, document the selected commit semantics, inventory every token boundary and require gated tests for pre-cancelled admission, cancellation during persistence, cancellation after commit, explicit event cancellation and preserved result/token identity. Selecting this policy is complete only when the user decision is recorded and the convention digest advances.

<a id="cons-010"></a>

- [x] **CONS-010 [P1] Select cleanup and Resource borrowing guarantees.**

  **Criterion / blocked checks:** [C13](CONVENTIONS.md#c13); all owning disposal/cleanup implementations and returned stream/buffer boundaries. Initial evidence: [Resource](../../../src/Xtate.Core/ResourceLoaders/Abstractions/Resource.cs), [ResourceCoverageTest](../../../test/Xtate.Core.Test/UnitTests/ResourceLoaders/ResourceCoverageTest.cs), both scope managers, InProcEventScheduler and [Disposer](../../../src/Xtate.Core/Common/Helpers/Disposer.cs).

  **Evidence / uncertainty:** Resource.GetStream(true) can return its original stream, whereas cached paths return wrappers; GetBytes returns a shared cached array. An existing test explicitly requires original-stream identity. Cleanup loops elsewhere differ in whether one throwing resource prevents later cleanup.

  **Recommended target:** Owners attempt cleanup of every resource they own, even after an ordinary cleanup exception. Retain one failure as the same exception object; aggregate multiple independent failures in attempt order, without flattening nested aggregates. Keep the existing CONS-007 operation-plus-cleanup precedence at its named boundaries. Resource retains ownership of its input stream. A returned original stream is borrowed: callers must not dispose it or use it after Resource disposal. A newly created stream wrapper is caller-owned. GetBytes remains a shared buffer which callers must treat as read-only; a defensive copy is an explicit future API/behavior change. Cached returned arrays remain ordinary managed values after owner disposal, while further Resource access is rejected. Concurrent Resource reads, or reads overlapping disposal, require external serialization; no undocumented general thread-safety promise is inferred.

  **Alternative / tradeoff:** Introduce explicit ownership-transfer/copying APIs for every returned stream and buffer. This provides stronger isolation but requires migration and can change allocation and aliasing behavior. For cleanup, fail-fast traversal is an alternative only if explicitly selected as a narrower owner contract; it leaves later resources unreleased.

  **Dependencies / risk / acceptance:** Coordinate concurrent shutdown with CONS-011 and contract documentation with CONS-014. Preserve public signatures and the tested stream identity. After approval, enumerate owning versus borrowed references and require throwing-cleanup tests that show later resources are attempted, ordered failure retention, repeated disposal, and distinct lifetime tests for borrowed streams, owned wrappers and shared arrays. Existing happy-path tests alone are insufficient.

<a id="cons-011"></a>

- [x] **CONS-011 [P1] Select host admission, shutdown and reentrancy guarantees.**

  **Criterion / blocked checks:** [C46](CONVENTIONS.md#c46); all shared host/scheduler lifecycle boundaries, including both scope managers, InProcEventScheduler, ScheduledEvent and their composition/lifetime dependencies. Resource follows the separately selected external-serialization contract in CONS-010; builder-local mutable models do not become globally thread-safe by this rule.

  **Evidence / uncertainty:** ConcurrentDictionary use protects individual dictionary operations, but fields are also read/replaced/cleared across lifecycle calls. Existing ordinary lifecycle tests do not define Start-versus-Dispose admission, duplicate cleanup or reentrant callbacks.

  **Recommended target:** Host/scheduler owners define one observable shutdown admission boundary. Work admitted before it retains an owner and reaches a terminal result; new Start/Schedule work after it fails with ObjectDisposedException before registration. Terminal Cancel/Destroy commands remain idempotent where their existing contracts allow an already absent item. Each owned scope/event is released once even when completion races shutdown. Repeated/concurrent DisposeAsync calls share shutdown completion and its selected failure outcome. DisposeAsync waits for owned runtime work and cleanup, not arbitrary diagnostic-provider completion; a retained observer still owns pending diagnostics under CONS-007. Synchronous Dispose initiates the supported synchronous shutdown path and transfers any remaining async observation explicitly without sync-over-async. Do not call external callbacks while holding an ownership/admission lock. Reentrant callbacks must not wait on their own unfinished operation; document such restrictions and use controlled handoff rather than a recursive lock or async-void callback.

  **Alternative / tradeoff:** Require callers to serialize all host/scheduler Start, Cancel and Dispose calls. This is a narrower API guarantee and transfers shutdown race prevention to every caller. Full safe admission/shutdown coordination costs implementation complexity but gives host owners one enforceable lifetime contract.

  **Dependencies / risk / acceptance:** High implementation/lifetime risk if the audit finds deviations; no blanket locking rewrite is selected. Record exact admission and ownership transitions. Require barrier-controlled Start-versus-Dispose, completion-versus-cleanup, repeated sync/async disposal and reentrant-callback tests with bounded hang guards. Verify preserved public results and no duplicate release; do not use passing parallel tests or a concurrent collection as proof of the whole contract.

<a id="cons-012"></a>

- [x] **CONS-012 [P1] Select diagnostic payload and event-identity rules.**

  **Criterion / blocked checks:** [C16](CONVENTIONS.md#c16); every library emission, entity parser/enricher and log-provider boundary. Initial evidence: [Logger](../../../src/Xtate.Core/Common/Logging/Services/Logger.cs), [logging contracts](../../../src/Xtate.Core/Common/Logging/Abstractions/ILogger.cs), [StateMachineInterpreterModule](../../../src/Xtate.Core/Interpreter/DependencyInjection/StateMachineInterpreterModule.cs) and scheduler error reporting.

  **Evidence / uncertainty:** Typed categories and structured enrichers already exist. EventId 1 appears in different categories, which is compatible with category-scoped identity. The framework accepts arbitrary message/entity values; there is no current approved payload classification or redaction policy.

  **Recommended target:** Identify an event by (source category, eventId), with one stable meaning per pair and no renumbering as a cleanup. Default framework diagnostics may emit logical session/send/invoke/state identifiers, source/category, severity, error type and operational metadata. Treat identifiers as correlation data, not proof that arbitrary values are safe. Do not emit credentials, authorization headers, connection secrets, arbitrary data-model values or request/document bodies by default. Treat URI user-info/query and exception messages/custom exception data as potentially sensitive; sanitize or use an explicitly opt-in diagnostic policy before exporting them. Keep useful exception type/stack context where permitted. Preserve structured fields and lazy level checks so disabled diagnostics do not serialize arbitrary entities. Any payload-enabled mode must state the fields it adds and its redaction responsibility. Retain the selected nonrecursive terminal fallback unchanged.

  **Alternative / tradeoff:** Preserve unrestricted payload-rich diagnostics and make all redaction the provider/host's responsibility. This retains current debugging detail but requires every host to provide effective redaction and makes built-in default output unsuitable for some consumers. A global event-ID registry is another alternative; category-scoped IDs avoid unnecessary renumbering.

  **Dependencies / risk / acceptance:** Coordinate public logging documentation with CONS-014. Audit fields at producers, enrichers and providers, not just logger call text. Require enabled/disabled-level tests, structured category/event identity tests and recognizable sentinel values proving default secret/body fields are absent. The policy does not authorize changing persisted formats or silently dropping all exception context.

<a id="cons-013"></a>

- [x] **CONS-013 [P2] Select namespace mapping while retaining public compatibility.**

  **Criterion / blocked checks:** [C06](CONVENTIONS.md#c06); every declared namespace, organizational-folder mapping, project RootNamespace and namespace-related IDE settings. The exact enumerated namespace inputs are in [AUDIT.policy-inputs.json](AUDIT.policy-inputs.json). The extraction is textual and will be structurally verified during audit.

  **Evidence / uncertainty:** RootNamespace is Xtate. Organizational Abstractions/Model/Common folders do not uniformly contribute namespace segments, while .editorconfig currently requests folder matching. Examples include StateMachine/Model/Structures -> Xtate.StateMachine and Interpreter/ModelBuilder/Model/Nodes -> Xtate.Interpreter.Model.

  **Recommended target:** Preserve existing externally visible fully qualified type names for source/binary compatibility. Namespaces express subsystem/API ownership rather than every physical folder. Keep Common/Helpers, Common/Extensions and Common/Exceptions at Xtate; omit Common before named subsystems such as Logging or TaskMonitor. Abstractions itself is organizational: retain the feature namespace, preserve meaningful children such as Logging.Provider, and flatten the existing Interfaces/Enums/Evaluators organizational cohorts. StateMachine and Persistence public model categories retain their feature namespaces. Interpreter/ModelBuilder does not add a ModelBuilder namespace segment; compiled Model/Nodes remains Xtate.Interpreter.Model. DataModel/Handlers does not add a Handlers segment. Preserve semantic Services, DependencyInjection, Internal, Extensions and Provider segments where used by their peer cohort. Framework polyfills keep the required System namespaces. Internal declarations follow the documented role/cohort map; anomalous mappings require a recorded compatibility/framework/role reason, not acceptance by frequency. Align IDE namespace checks with that map instead of moving public namespaces to silence the folder rule.

  **Alternative / tradeoff:** Make namespaces exactly follow all physical folders, including organizational folders. This is easier to generate mechanically but changes many public fully qualified names and needs an explicit versioned migration. Another alternative is moving physical files to fit the compatible public namespace map; that is a source-layout change, not permission to flatten responsibilities.

  **Dependencies / risk / acceptance:** Low runtime risk for selecting the compatible rule; high compatibility risk for public renames. During audit, structurally enumerate types/namespaces and effective accessibility, inspect public consumers, derive an explicit per-cohort folder map and flag settings conflicts. Every remaining exception needs its concrete reason. No code move, namespace rename or IDE-setting edit is part of this agreement step.

<a id="cons-014"></a>

- [x] **CONS-014 [P2] Select public XML documentation coverage and enforcement.**

  **Criterion / blocked checks:** [C36](CONVENTIONS.md#c36); every effectively public/protected library type and member, package documentation settings and inherited documentation. Initial evidence: the [library project](../../../src/Xtate.Core/Xtate.Core.csproj), Resource, model interfaces and EventName. The project currently does not explicitly enable GenerateDocumentationFile.

  **Recommended target:** Require XML documentation for all handwritten effectively public/protected library APIs, including public DI/service types that remain externally visible. Generated framework/resource members are assessed through their generator or inherited contract rather than hand-edited. Provide a meaningful summary, type/parameter documentation and return/value semantics where applicable. Document null/default/empty distinctions, cancellation timing, mutation/ownership, thread-safety/lifetime constraints and contractual exceptions when relevant. An override or implementation may use inheritdoc when its contract is unchanged; changed behavior needs an explicit supplement. Enable package XML generation and enforce missing-doc diagnostics for the required surface once documentation is supplied. Record current gaps as remediation work; do not silence warnings merely to pass the audit.

  **Alternative / tradeoff:** Require docs only for a named set of consumer entry points, leaving other public implementation types outside the mandatory surface. This is less work, but needs an exact approved API allowlist and does not give consumers complete coverage of exported APIs. Requiring all public/protected APIs has a larger initial documentation cost.

  **Dependencies / risk / acceptance:** Ownership, cancellation, diagnostics and concurrency documentation depends on CONS-009/010/011/012. Audit effective accessibility and inherited XML resolution rather than raw public-keyword counts. Verify generated package XML and the analyzer/build rule, then compare documented contracts with signatures and tests. Documentation coverage alone does not certify semantic correctness.

<a id="cons-015"></a>

- [x] **CONS-015 [P2] Adopt the proposed shared-helper placement rule.**

  **Criterion / blocked checks:** [C22](CONVENTIONS.md#c22); all helper/extension/compatibility types and their actual cross-feature callers. Initial examples are Infra in Common, SegmentedName in StateMachine/Internal and InjectedCancellationStream in ResourceLoaders/Internal.

  **Recommended target:** Keep feature-semantic helpers within the owning feature. Use Common for infrastructure whose responsibility is shared across independent features; caller count alone does not prove independence. Use DataTypes for SCXML data-value semantics and precisely guarded Polyfills for framework compatibility. Before proposing a move, name the responsibility, all affected callers, the allowed dependency direction and the compatible destination namespace. Shared code must not introduce a dependency from lower-level infrastructure to a feature merely to avoid duplication.

  **Alternative / tradeoff:** Keep placement case-by-case without an enforceable boundary, or centralize every reused helper under Common. The former limits the audit's conclusions; the latter can turn Common into a feature-dependent catch-all. The recommendation adopts the existing documented proposal rather than introducing a new architecture framework.

  **Dependencies / risk / acceptance:** Coordinate namespaces with CONS-013. Future moves require source/import, link and public-compatibility checks. Audit every helper's actual callers and semantic ownership; record justified feature-local reuse and flag only evidenced responsibility/dependency mismatches. Adoption is a policy change, not approval for source relocation.

<a id="cons-016"></a>

- [x] **CONS-016 [P2] Adopt comment and suppression-rationale requirements.**

  **Criterion / blocked checks:** [C37](CONVENTIONS.md#c37); handwritten implementation comments, pragma/attribute/IDE/analyzer suppressions and compatibility explanations in library and tests. Generated code and required AGPL boilerplate have separate roles.

  **Recommended target:** Explain non-obvious intent, ownership, invariants and compatibility constraints; avoid comments that merely restate syntax. Every maintained suppression names its precise rule, uses the narrowest practical scope and gives a concrete local reason or an exact shared-policy reference. File/project-wide suppression needs a reason for that breadth. Pragma disable/restore boundaries must balance where the scope permits. Apply the rule to existing maintained suppressions as well as new ones, recording legacy rationale gaps as remediation rather than declaring them acceptable by age. Preserve useful comments and license headers.

  **Alternative / tradeoff:** Require reasons only for suppressions introduced after the adoption checkpoint, preserving older suppressions as a fixed baseline. This reduces initial work but leaves older exemptions less reviewable and requires an exact immutable baseline. The recommendation avoids treating age alone as a justification.

  **Dependencies / risk / acceptance:** No behavior change is needed to select the rule. Enumerate actual comment/suppression syntax, verify each scope/rule/reason and flag stale explanations against current control flow. Repository-wide rationale-only tasks can be grouped only with their exact affected set and a common verification method. Do not rewrite generated code or invent reasons for existing suppressions.

<a id="cons-017"></a>

- [x] **CONS-017 [P2] Adopt responsibility-based complexity review.**

  **Criterion / blocked checks:** [C45](CONVENTIONS.md#c45); all maintained handwritten types and methods, comparing orchestration, contracts, representation and test roles separately.

  **Recommended target:** Assess cohesion, identifiable stages, ownership transitions and dependency direction. A deviation must name concrete responsibilities that conflict or change independently, show the affected invariants/callers and propose a coherent boundary that preserves behavior. Method length, branch count and sample averages are investigation signals, not mandatory limits. Do not require extraction merely because a method is large, and do not collapse a meaningful contract/composition boundary to reduce file count. Suspected hotspots without enough semantic evidence become investigation tasks, not mandatory rewrites. Performance claims require workload-specific measurement.

  **Alternative / tradeoff:** Introduce fixed method-size/cyclomatic-complexity ceilings. These are easier to measure but currently lack an approved baseline and can force artificial splits across valid orchestration or algorithmic stages. Another alternative is leaving responsibility review advisory, which prevents a full auditable target for C45.

  **Dependencies / risk / acceptance:** Coordinate helper ownership with CONS-015 and the existing component-boundary criteria. For each finding, provide the exact type/method, conflicting responsibilities, proposed destination and compatibility/lifetime regression checks. An audit record must explain its semantic judgment; a numeric report alone is not coverage.

## P1 — Completion and observation

<a id="cons-007"></a>

- [x] **CONS-007 [P1] Decide failure precedence and the terminal reporting fallback for these repairs.**

  **Target / affected:** [C47](CONVENTIONS.md#c47), with existing unresolved context in [C15](CONVENTIONS.md#c15) and [C16](CONVENTIONS.md#c16). The affected boundaries are the two scope managers' `WaitAndCleanup`, the task monitor's `MonitorTaskCompletion`/`TaskFailed`/`TaskCancelled`, and the idle timer's error-reporting callback. Owner: library maintainers; no individual owner is recorded.

  **Selected decision (2026-10-18):** Adopt the [six-row outcome matrix and terminal-observer policy](CONVENTIONS.md#cons-007-policy). Keep ordinary single-fault identity and operation-cancellation tokens. If operation and cleanup both fail, fault the result with an ordered aggregate (operation or cancellation first, cleanup second). Cleanup-only cancellation is wrapped in a single-inner aggregate to preserve fault semantics through async forwarding. Complete the caller result before diagnostic callbacks. Observe reporting failures once through an internal atomic latest-failure slot with no callbacks/I/O/background task. Preserve both `LazyTask` constructors and continue accepting a null monitor with a cancellable token; late faults use the supplied monitor or that fallback. The bounded in-memory record trades durable diagnostic history for a terminal path that cannot recurse into a failing logger.

  **Dependencies / risk:** Policy prerequisite satisfied for CONS-001, CONS-004, CONS-005 and the reporting portion of CONS-006. Runtime implementation and behavioral tests remain in those tasks. Double failures become faulted aggregates, including cancellation plus cleanup failure; ordinary single-failure behavior is retained. No public API, package or persistence migration is selected.

  **Acceptance satisfied:** The existing convention set now records every success/fault/cancellation × cleanup-success/failure row, the synchronous/asynchronous reporting-failure rule, bounded fallback retention and null-monitor compatibility. C15/C16/C47 references and progress are reconciled. Verification for this decision is document/link/matrix consistency; it does not assert passing runtime repair tests. No separate backlog or competing standard was created.

<a id="cons-001"></a>

- [x] **CONS-001 [P1] Complete scope-manager results when cleanup fails.**

  **Target / affected:** [C47](CONVENTIONS.md#c47). [`StateMachineScopeManager.WaitAndCleanup`, `Run`, `Cleanup`](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineScopeManager.cs) and [`ExternalServiceScopeManager.WaitAndCleanup`, `Start`, `Cleanup`](../../../src/Xtate.Core/StateMachineHost/Services/ExternalServiceScopeManager.cs); their existing `StateMachineScopeManagerCoverageTest.cs` and `ExternalServiceScopeManagerCoverageTest.cs` under `test/Xtate.Core.Test/UnitTests/StateMachineHost`.

  **Current / required:** Both routines capture operation failure, await `Cleanup`, then set the result completion source. If unregistering or scope disposal throws, none of those result setters executes. The monitored background routine can fault while `StateMachineResult.GetResult()` or `ExternalServiceResult.WaitForCompletion()` stays pending. Require terminal completion of the separately published caller result on every such exit. Merely sending the background failure to a monitor does not complete that result. This is established directly by both source control flows; the existing tests exercise ordinary cleanup and do not prove throwing-cleanup behavior.

  **Dependencies / risk:** CONS-007 selects failure precedence. Medium implementation risk: preserve registration/removal order, scope ownership, original exception identity and cancellation token in the single-failure cases. Preserve public signatures, persisted formats and the distinction between caller and background tasks. No file moves or renames.

  **Acceptance:** For each manager, gate controller completion so `Start` first returns its result. Make unregistering and async scope disposal fail separately; cover successful, faulted and cancelled controllers. Assert that the returned result reaches the selected terminal state and the background task is observed, with no wait left pending. Include cleanup failure before an asynchronous suspension and after one. Assert original error/token preservation when cleanup succeeds. Use a timeout only as a hang guard and capture both tasks explicitly. Then run `dotnet test test/Xtate.Core.Test/Xtate.Core.Test.csproj -f net10.0 --filter "FullyQualifiedName~StateMachineScopeManagerCoverageTest|FullyQualifiedName~ExternalServiceScopeManagerCoverageTest"`; build relevant legacy targets when implementing shared-path changes.

  **Implemented and verified:** Both `WaitAndCleanup` routines now capture cleanup errors and complete the published result, retaining the original operation/cleanup exceptions in the selected order. Cleanup-only cancellation is wrapped so `Execute` remains faulted. The worker completes after publishing its result, including when everything completes synchronously, so the monitor cannot prevent `Start` from returning the result. Existing registration/removal order and public signatures are preserved.

  [`ScopeManagerCompletionTest.cs`](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/ScopeManagerCompletionTest.cs) supplies 40 paired-manager matrix/edge cases and an `Execute` forwarding regression. The original 40-case matrix failed 32 cases before the production fix; all 41 final regressions pass on net10.0. Broader hosting/persistence tests pass (204), focused scope-manager/persistence tests pass on net462 (71), and the library builds on all six target frameworks with zero warnings/errors. Detailed commands, source hashes and TRX fingerprints are recorded under `task_resolutions.CONS-001` in [TODO.progress.json](TODO.progress.json). Other runtime tasks and the historical audit's freshness status are unchanged by this targeted repair.

<a id="cons-002"></a>

- [x] **CONS-002 [P1] Retain underlying-operation observation when a monitored wait is already cancelled.**

  **Target / affected:** [C47](CONVENTIONS.md#c47). All four `WaitAsync` overloads and both `WaitAndMonitor` helpers in [`TaskMonitor.cs`](../../../src/Xtate.Core/Common/TaskMonitor/Services/TaskMonitor.cs); [`TaskMonitorCoverageTest.cs`](../../../test/Xtate.Core.Test/UnitTests/Common/TaskMonitorCoverageTest.cs). Host context: `ExternalServiceScopeManager.Start` transfers resolution through this monitor.

  **Current / required:** With an incomplete operation and an already-cancelled caller token, each overload returns a cancelled replacement without attaching the monitor to the original. For a source-backed `ValueTask`, this bypasses both `AsTask` and consumption. The `pre_cancelled_wait_abandons_source` reproduction records zero continuation registrations and zero consumptions. The later-cancellation branch also only monitors when `!task.IsCompleted` in `finally`; review the race in which the cancelled wait wins but the original faults before that test. Require continuous observation whenever waiting stops without consuming the original outcome. The existing completed-operation fast path is legitimate and must remain nonblocking.

  **Dependencies / risk:** No prerequisite. Medium risk: preserve cancellation of the wait rather than unexpectedly cancelling the owned underlying operation; do not await a source-backed `ValueTask` twice or change completed-operation precedence. Preserve all four signatures and token identity.

  **Acceptance:** For Task, Task<T>, ValueTask and ValueTask<T>, pass an incomplete operation and a pre-cancelled token, observe cancellation of the returned wait, then release/fault/cancel the original and assert its outcome reaches the monitor. Include a custom `IValueTaskSource` and assert exactly one eventual `GetResult`. Add a controlled cancellation-versus-completion race; no GC timing or `UnobservedTaskException` event is needed as the assertion mechanism. Retain normal/completed-operation tests. Run the `TaskMonitorCoverageTest` filter on net10.0 and compatibility builds for the shared implementation.

  **Implemented and verified (2026-10-28):** All four wait shapes transfer the original operation to a contained completion observer when caller cancellation wins, including pre-cancellation and an original operation that completes before the cancelled await resumes. Completed-operation precedence remains unchanged. Source-backed values are consumed once. Legacy targets use an async consumption adapter to preserve the underlying cancellation token, which the legacy source-backed `ValueTask.AsTask()` conversion otherwise loses. [TaskObservationRegressionTest](../../../test/Xtate.Core.Test/UnitTests/Common/TaskObservationRegressionTest.cs) covers all shapes, late success/failure/cancellation, a controlled ExecutionContext-restoration race and cancellation returning while diagnostics remain pending.

<a id="cons-004"></a>

- [x] **CONS-004 [P1] Give task-monitor reporting failures a terminal owner.**

  **Target / affected:** [C47](CONVENTIONS.md#c47). The three `MonitorTaskCompletion` overloads, their callers, and overridable `TaskFailed`/`TaskCancelled` in [`TaskMonitor.cs`](../../../src/Xtate.Core/Common/TaskMonitor/Services/TaskMonitor.cs); `TaskMonitorCoverageTest.cs`.

  **Current / required:** `Forget` and `WaitAndMonitor` discard the Task returned by `MonitorTaskCompletion`. Its handlers await overridable reporting callbacks, which can throw or return faulted ValueTasks. Those secondary exceptions fault the discarded observer. `reporter_failure_escapes_observer` reproduces that outcome using the linked production class and a faulting reporting override. The original operation is observed, but observation is not terminal if its reporter produces another unowned fault. Require a bounded final observation/reporting path.

  **Dependencies / risk:** CONS-007 selects that terminal fallback. Medium risk: avoid recursive reporting, blocking waits and an async-void escape. Preserve the existing tested immediate-fault behavior of `Forget`: already faulted/cancelled inputs currently throw synchronously; that timing difference is not itself flagged as inconsistent.

  **Acceptance:** Exercise Task, ValueTask and generic ValueTask observers for original faults and cancellations; make reporting throw synchronously and fault after a controlled suspension. Verify the selected fallback receives the secondary failure once, no observer fault is discarded, and neither normal callers nor ThreadPool callbacks receive an unhandled secondary exception. Preserve existing immediate-completion and logging tests. Run `TaskMonitorCoverageTest` and the source-linked reproduction when implementing.

  **Implemented and verified (2026-10-28):** The three completion observers contain synchronous, asynchronous and cancelled reporting failures through `ReportFailure`. [TaskFailureSink](../../../src/Xtate.Core/Common/Helpers/TaskFailureSink.cs) atomically retains one original/reporting failure record, deduplicates identical exception references and provides an internal snapshot without logging or callbacks. Existing immediate fault/cancellation propagation from `Forget` remains synchronous. Regression matrices cover original failure/cancellation, all three observer shapes, both reporter-failure timings, reporter cancellation and record replacement. The audit reproductions retain their original historical role; product tests provide the current verification.

<a id="cons-005"></a>

- [x] **CONS-005 [P1] Observe late factory failures after cancellation of an unmonitored LazyTask.**

  **Target / affected:** [C47](CONVENTIONS.md#c47). [`LazyTask<T>.Task`, `Execute`, and its optional-monitor constructor](../../../src/Xtate.Core/Common/Helpers/LazyTask.cs), the parameterless `Forget` overloads in [`TaskExtensions.cs`](../../../src/Xtate.Core/Common/Extensions/TaskExtensions.cs) and [`ValueTaskExtensions.cs`](../../../src/Xtate.Core/Common/Extensions/ValueTaskExtensions.cs), plus `LazyTaskCoverageTest.cs`, `TaskExtensionsTest.cs` and `ValueTaskExtensionsTest.cs` in `test/Xtate.Core.Test/UnitTests/Common`.

  **Current / required:** Cancellation can complete the public lazy result before the factory finishes. If the factory then faults, `Execute` cannot fault the already-cancelled result and rethrows. With a null monitor, the returned ValueTask goes through parameterless `Forget`; `Preserve()` retains a representation but supplies no observer. The Task variant is an empty method. The source-linked probe confirms the public result is already cancelled, and source tracing identifies the unobserved late-failure branch. A complete production-source caller search finds `LazyTask.Task` as the only parameterless-Forget call; test calls also exist. Preserve completion observation through that cancellation branch instead of relying on preserving a discarded value.

  **Dependencies / risk:** CONS-007 chooses an optional-monitor-compatible terminal sink or an explicitly approved constructor contract change. CONS-004 is a prerequisite if the repaired monitor becomes the fallback. Medium risk: preserve factory-once behavior, shared Task identity, early caller cancellation and single ValueTask consumption. The no-token constructor normally publishes factory errors to its result; do not demand duplicate reporting for those ordinary paths.

  **Acceptance:** Gate the factory, cancel the lazy result, then fault the released factory and assert a designated observer receives that late error exactly once. Cover initially cancelled and subsequently cancelled tokens, null and real monitors, source-backed ValueTasks and success-after-cancellation. Reconcile both parameterless helper APIs and their callers: either restrict their valid use to independently observed work or supply observation; never add a blocking wait. Preserve constructor signatures unless the approved policy expressly changes them. Run the LazyTask, TaskExtensions, ValueTaskExtensions and AsyncEnumerableAndValueTaskCoverageTest filters on net10.0 and build legacy targets.

  **Implemented and verified (2026-10-28):** Parameterless Task and ValueTask `Forget` overloads now supply nonthrowing completion observers backed by the terminal sink. LazyTask converts its execution once to a reusable Task and transfers observation safely if a supplied monitor throws, preserving both original and distinct reporting failures. Its public result remains cancelled. Regression matrices cover initially/later cancelled callers, source-backed factories, late success/failure, null/real/throwing monitors, failing reporters and synchronous factory failure after pre-cancellation. Ordinary published factory faults do not generate duplicate reporting; factory-once execution and shared result identity remain intact.

## P2 — ValueTask consumption and timer reporting

<a id="cons-003"></a>

- [x] **CONS-003 [P2] Consume successful source-backed ValueTasks in Forget fast paths.**

  **Target / affected:** [C47](CONVENTIONS.md#c47). `Forget(ValueTask)` and `Forget<TResult>(ValueTask<TResult>)` in [`TaskMonitor.cs`](../../../src/Xtate.Core/Common/TaskMonitor/Services/TaskMonitor.cs), plus `TaskMonitorCoverageTest.cs`.

  **Current / required:** Both successful-completion branches return after checking `IsCompletedSuccessfully` without calling `GetResult`. Checking status is not consumption of a source-backed value. The linked-source `successful_valuetask_forget_skips_getresult` reproduction records zero `GetResult` calls. Require exactly one consumption so a custom/pooled source can finish its consumer lifecycle; discarding a plain inline result remains cheap. The Task success fast path is a different representation and is not included in this repair.

  **Dependencies / risk:** None; can be implemented alongside CONS-002/004 but is a separate root cause. Low-to-medium risk: do not introduce `AsTask` allocations unnecessarily, reuse a consumed value, or synchronously wait for an incomplete one. Preserve current already-faulted input behavior.

  **Acceptance:** Supply completed successful non-generic and generic `IValueTaskSource` instances that count `GetResult`; assert one call for each, plus normal inline/Task-backed success. Cover pending, faulted and cancelled paths to prevent double consumption. Run `TaskMonitorCoverageTest` on a modern target and compile all supported library targets.

  **Implemented and verified (2026-10-28):** Both successful ValueTask fast paths call `GetResult` once after the completed-success check. No conversion to Task or incomplete-operation wait is introduced in those fast paths. Counting-source regressions cover generic/non-generic, completed/pending and success/fault/cancellation outcomes, alongside the existing inline and Task-backed cases.

<a id="cons-006"></a>

- [x] **CONS-006 [P2] Observe asynchronous logging started by the idle timer callback.**

  **Target / affected:** [C47](CONVENTIONS.md#c47). [`StateMachineDestroyOnIdle.StateTracker.DestroyOnIdle`](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineDestroyOnIdle.cs) and [`StateMachineDestroyOnIdleCoverageTest.DestroySignalFailureIsLoggedByTimerCallback`](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineDestroyOnIdleCoverageTest.cs).

  **Current / required:** The callback catches a destroy-signal exception and invokes `owner.Logger.Write(...)` as a statement, discarding its ValueTask. `ILogger` explicitly returns ValueTask, and the actual logger awaits enrichers/providers; asynchronous completion cannot be assumed. The existing timer test returns `ValueTask.CompletedTask` from its mock and therefore does not check this boundary. Require an owner for asynchronous log completion, and a contained path for synchronous and asynchronous reporting failures.

  **Dependencies / risk:** Use the terminal reporting policy from CONS-007 and, if routed through ITaskMonitor, the repaired observer from CONS-004. Medium risk around Timer callbacks and DI: preserve optional idle tracking, timer disposal, and existing async context behavior. No `.Wait()`, `.Result`, or async-void workaround. Keep logs' current fields and privacy behavior; those policies are not audited here.

  **Acceptance:** Make logging return a gated incomplete ValueTask and then fail; verify that its designated owner observes completion/failure. Separately make logging throw before returning. Assert neither failure escapes the timer callback and no wait blocks the timer thread. Preserve existing arm/disarm and destroy-signal tests. Run `StateMachineDestroyOnIdleCoverageTest` on net10.0 and check the conditional legacy Timer disposal branch when implementing.

  **Implemented and verified (2026-10-28):** The timer transfers logging completion to a contained observer carrying the original destroy failure. Both a synchronous logger throw and a later logger fault reach the terminal sink with both exception identities retained. [StateMachineDestroyOnIdleCoverageTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/StateMachineDestroyOnIdleCoverageTest.cs) verifies real timer disposal finishes while logging is still pending, successful/failed logging is consumed once and synchronous logger failure stays inside the actual callback. These tests also pass through the legacy disposal branch without changing DI or timer lifetimes.

<a id="cons-008"></a>

- [x] **CONS-008 [P2] Retain ownership of completion proxies when controller initialization fails.**

  **Target / affected:** [C47](CONVENTIONS.md#c47). [`StateMachineControllerBase.Start`, `ExecuteAsync`, `GetResult`](../../../src/Xtate.Core/StateMachineHost/Services/StateMachineControllerBase.cs), [`StateMachineStatus.WhenAccepted`, `ForceFailed`, `ForceCancelled`](../../../src/Xtate.Core/StateMachineHost/Abstractions/StateMachineStatus.cs), and `StateMachineControllerBaseCoverageTest.cs` / `StateMachineHostModelCoverageTest.cs` under `test/Xtate.Core.Test/UnitTests/StateMachineHost`.

  **Current / required:** `ExecuteAsync` faults `_completedTcs` and rethrows into the monitor; the status can independently fault `_acceptedTcs`. When startup fails before a controller handle is returned, callers receive the startup error but no result proxy has been exported. `GetResult` is additionally initialization-guarded. If `ExecuteAsync` has already failed when the production monitor's `Forget` is called, that call throws synchronously before `WhenAccepted` is awaited, leaving the readiness proxy unobserved as well. Observing the interpreter/background error does not consume these separate faulted proxy Tasks. Require an identifiable owner for the private proxies when startup fails or a handle is never exported. The `synchronous_startup_failure_leaves_separate_faulted_proxies` probe links the actual `Start`/`ExecuteAsync`/status sources and confirms two faulted proxies with zero readiness waits; it does not exercise IoC initialization or reproduce a process crash.

  **Dependencies / risk:** Coordinate with CONS-001/004; no new policy is needed to establish proxy observation. Medium risk: preserve readiness/result separation, the tested synchronous-fault behavior of `Forget`, and fault/cancellation identity; avoid duplicate user-facing reporting merely to consume private proxies. Do not expose `GetResult` before initialization just to silence a diagnostic.

  **Acceptance:** Use the real `StateMachineStatus` and production monitor, not only an always-accepted status double. Make interpreter execution fail before its first suspension and after a controlled suspension before acceptance. Assert the returned initialization failure, and verify that both private readiness/result faults have a designated observer even when no controller is returned. Cover cancellation and success/acceptance to retain existing behavior. Deterministically capture the observation paths; do not rely on GC timing. Run the controller and host-model test filters plus compatibility builds when implementing.

  **Implemented and verified (2026-10-28):** Start obtains the readiness proxy before detaching execution and, on startup failure, retains a silent combined observer for readiness and result proxies. This consumes their outcomes without duplicate logging or exposing GetResult before initialization. [ControllerInitializationRegressionTest](../../../test/Xtate.Core.Test/UnitTests/StateMachineHost/ControllerInitializationRegressionTest.cs) uses the real status and production monitor for immediate/delayed startup faults and cancellation, verifies the retained observer completes successfully, and preserves early acceptance with a pending result and completion before acceptance.

## Verification boundary

**Implementation verification (2026-10-28):** All 4,090 tests pass on net10.0. The Common, StateMachineHost and Persistence unit-test subsets pass on net462 (446 tests). The library builds on net11.0, net10.0, net9.0, net8.0, netstandard2.0 and net462 with zero warnings/errors. The first 28 new common-observation cases produced 21 failures before the repairs; the expanded regressions include the controlled cancellation/completion race and the legacy cancellation-token regression. Commands, source/test hashes and TRX hashes are recorded under the six `task_resolutions` and `current_implementation` entries in [TODO.progress.json](TODO.progress.json). Sink-state tests use `DoNotParallelize`. This completes the existing repairs; the historical audit remains stale.

**Original audit boundary (historical):** No production or test source was changed to create the original plan. Five audit reproductions compiled actual linked TaskMonitor, LazyTask, extension, controller and status sources against .NET 10 with minimal audit-only collaborators. The controller probe calls the actual protected `Start` through an audit subclass; IoC behavior is not modeled by its compile-time stubs. The lazy probe proves the cancelled result state; the subsequent unmonitored rethrow is additionally established by source control flow, not by nondeterministic GC events. The scope-result and idle-timer findings were established by direct source/control-flow inspection and the stated limits of the tests then available. The original audit did not use a full solution test/build as evidence of C47 completeness.

Antigravity supplied a second opinion on the previously approved sanitized lifecycle description. Its review was independently checked. Its claim that cleanup faults complete the caller result was rejected: the actual result stays pending. Its claims that nonblocking rules require draining every scope and that a caller scheduler token becomes the dispatch token were also rejected as unsupported. The CLI warned that plan mode had no effect with slash-command expansion disabled; the invocation nevertheless used the required flags and no-tools/no-files instructions, from an isolated empty directory with no repository attached. No additional disclosure or delegation was needed for this narrowed audit. See [record](AUDIT.antigravity.json).
