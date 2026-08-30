# SCXML parsing, validation, and execution matrix

## How to use this matrix

Every row below is a requirement family, not one test. Expand each row across the applicable common dimensions in the README and create one test per materially different oracle. Use the ID as the test-name prefix and add a numeric suffix when a family expands, for example `SCXML_PARSE_006_03`.

For the current campaign, follow documents 06 and 07 and generate source code for every remaining test in the assigned phase/family. Annotate existing tests first, then emit missing explicit tests and per-case metadata. Non-compiling code is acceptable. Generic requirement-level shells do not count. Do not run commands, repair compilation, diagnose failures, or update planning files.

For every valid machine, assert the complete ordered trace and stable configuration. For every invalid machine, test both the XML construction route and the equivalent public object model where possible. Distinguish parse diagnostics, model validation diagnostics, model-build failures, expression compilation failures, `error.execution`, platform errors, interpreter termination, and host destruction.

## 1. XML document and SCXML parser

| ID | Required scenarios and oracle |
|---|---|
| SCXML-PARSE-001 | Accept the exact SCXML namespace and root element; reject no namespace, wrong namespace, wrong local name, case variants, prefixed lookalikes, and a nested `<scxml>` used as the document root. Verify diagnostics retain base URI and line/column where available. |
| SCXML-PARSE-002 | Test `version`: exact supported value, absent, empty, whitespace-padded, numeric variants, culture-specific digits, wrong case, duplicate attribute, and foreign-namespace attribute. Verify required/unsupported behavior against SCXML §3.2. |
| SCXML-PARSE-003 | Test all root attributes independently and in legal/illegal combinations: `initial`, `datamodel`, `binding`, `name`, `version`, `xml:base`, namespace declarations, and unknown unqualified/qualified attributes. Cover omitted defaults, empty values, Unicode, and duplicate lexical attributes. |
| SCXML-PARSE-004 | Parse each legal root child (`state`, `parallel`, `final`, `datamodel`, `script`) at 0/1/many cardinality and all meaningful orderings. Test forbidden children, executable elements at root, text/CDATA, comments, processing instructions, and foreign elements. |
| SCXML-PARSE-005 | For every SCXML element, enumerate every supported attribute as absent, empty, valid, lexically invalid, semantically invalid, duplicate, unqualified unknown, and foreign-qualified. Enumerate every allowed child at 0/1/many and before/between/after other children. Build this from parser policy metadata, not hand-selected examples. |
| SCXML-PARSE-006 | Test `<state>` with `id`, `initial`, nested `state`/`parallel`/`final`, `initial`, `history`, `onentry`, `onexit`, `transition`, `invoke`, and `datamodel`: singleton, repeated, mixed order, text nodes, and foreign content. Repeat for atomic, compound, and nested compound forms. |
| SCXML-PARSE-007 | Test `<parallel>` with `id` and every permitted nested construct; 0, 1, 2, and many regions; final/history/invoke/transitions; nested parallel; and forbidden `initial` attribute/element forms. |
| SCXML-PARSE-008 | Test `<final>` with `id`, `onentry`, `onexit`, and `donedata`; repeated `donedata`; nested states/transitions/invokes that must be rejected; and final as root child, compound child, and parallel-region child. |
| SCXML-PARSE-009 | Test `<initial>` and `<history>` cardinality and content. Cover one/multiple/no transitions; `history type="shallow|deep"`, omitted default, casing/whitespace/unknown; IDs; and transition attributes forbidden by the standard even if the parser can represent them. |
| SCXML-PARSE-010 | Test `<transition>` attributes `event`, `cond`, `target`, and `type`, including omission/default, empty tokens, repeated spaces, tabs/newlines, mixed delimiters, duplicate targets, Unicode identifiers, wildcard descriptors, unknown type, and case variants. Test every legal executable child and illegal structural child. |
| SCXML-PARSE-011 | Test `<onentry>`/`<onexit>` empty and with each executable-content kind, repeated actions, nested conditionals/loops, foreign executable content, and forbidden structural content. Preserve document order exactly. |
| SCXML-PARSE-012 | Test `<raise event>`, `<log label expr>`, `<cancel sendid sendidexpr>`, and `<assign location expr type attr>` attribute lexical boundaries, mutually exclusive forms, missing required data, empty values, raw content, and foreign content. |
| SCXML-PARSE-013 | Test `<if cond>`, `<elseif cond>`, and `<else>`: no branch marker, every marker order, repeated else, actions before/between/after markers, nested `if`, empty branches, and markers illegally appearing outside `if`. |
| SCXML-PARSE-014 | Test `<foreach array item index>` with missing/empty/valid attributes, all executable children, nested loops, variable-name edge cases, and illegal branch markers or structural content. |
| SCXML-PARSE-015 | Test `<send>` with every attribute and child: literal/expression pairs for event/target/type/delay, `id`, `idlocation`, `namelist`, repeated/malformed `<param>`, `<content>`, no payload, and every conflict/cardinality combination. |
| SCXML-PARSE-016 | Test `<invoke>` with every attribute and child: `type/typeexpr`, `src/srcexpr`, `id/idlocation`, `namelist`, `autoforward`, params, content, finalize; repeated finalize/content; invalid boolean lexical forms; and illegal executable/structural content. |
| SCXML-PARSE-017 | Test `<param name expr location>`, `<content expr>`, `<donedata>`, `<data id src expr>`, `<datamodel>`, and `<script src>` for all required, exclusive, repeated, inline/raw XML, whitespace, CDATA, entity, and mixed-content forms. |
| SCXML-PARSE-018 | Test custom executable content in foreign namespaces: empty/nonempty, attributes, nested namespace changes, comments/CDATA, same local names as SCXML elements, malformed subtree, and unsupported custom-action provider. The captured outer XML and namespaces must be lossless where contractually promised. |
| SCXML-PARSE-019 | Test identifier-list tokenization separately for `initial`, `target`, `event`, and `namelist`: single/multiple ASCII spaces, tabs, CR/LF, leading/trailing whitespace, non-breaking whitespace, empty tokens, duplicates, dots, colons, URI-like values, combining characters, and surrogate pairs. Assert standard tokenization rather than current helper behavior. |
| SCXML-PARSE-020 | Test delay lexical forms and arithmetic boundaries: zero, positive integer milliseconds/seconds, omitted suffix, leading sign/zero, decimal/fraction, whitespace, uppercase suffix, negative, `Int32`/`Int64` boundaries, multiplication overflow, and expression form. Verify parsing never wraps to a negative or unrelated delay. |
| SCXML-PARSE-021 | Test malformed XML at every structural position: truncated start/end tag, bad entity, invalid character, duplicate attribute, unbound prefix, namespace rebinding, invalid encoding, incomplete multibyte character, and stream failure after each byte/chunk boundary. No partial model may escape. |
| SCXML-PARSE-022 | Test UTF-8/UTF-16 LE/BE, BOM/no BOM, XML declaration encodings, Unicode element content and identifiers, normalization forms, CR/LF normalization, NUL/control characters, maximum scalar, invalid surrogate, and culture-independent parsing under several process cultures. |
| SCXML-PARSE-023 | Test sync and async reader/stream construction with one-byte chunks, non-seekable streams, delayed reads, cancellation before/while/after read, read exception, disposal exception, and repeated deserialize attempt. Assert stream/reader ownership and exactly-once disposal. |
| SCXML-PARSE-024 | Test XML security defaults: DTD prohibited/controlled as intended, external entity blocked unless explicitly allowed, no XXE file/network disclosure, bounded entity expansion, and safe failure for entity cycles/billion-laughs inputs. |
| SCXML-PARSE-025 | Verify parser parity: each accepted XML document has the same public model as the equivalent builder graph; each rejected document maps to the intended diagnostic category. Canonicalize only non-semantic representation differences. |

## 2. XInclude and `xml:base`

XInclude tests must run with inclusion disabled, enabled with default nesting 16, custom finite limits, zero/unlimited semantics, and cancellation/disposal. Include the upstream W3C XInclude corpus where supported.

| ID | Required scenarios and oracle |
|---|---|
| SCXML-XINC-001 | Disabled inclusion rejects all external acquisition, including nested includes and resolver calls triggered indirectly. Prove the resolver/loader is not invoked. Enabled inclusion resolves relative and absolute references against document URI and nested `xml:base`. |
| SCXML-XINC-002 | Test XInclude 2001 and 2003 namespace forms, exact empty `<include/>`, non-empty include with fallback, wrong namespace/local-name/case, and nested include elements. Record unsupported standard features as conformance failures or explicit product decisions, never silent passes. |
| SCXML-XINC-003 | Test `href`: missing, empty/in-document reference, fragment, relative, absolute, escaped/unescaped, malformed, unsupported scheme, redirect, base URI absent, and resolver returning null/wrong type/stream/resource. Assert wrapped diagnostic and cleanup. |
| SCXML-XINC-004 | Test `parse`: omitted, `xml`, `text`, empty, wrong case, whitespace, unknown. For text, test explicit `encoding`, resource charset precedence, BOM precedence, UTF variants, invalid/unsupported charset, arbitrary text that resembles XML, and XML media types treated according to the project contract. |
| SCXML-XINC-005 | Test `accept` and `accept-language` propagation exactly once through capable resolvers; empty, repeated, non-ASCII, and injection-like header values; resolver without external-header support; and nested include headers. |
| SCXML-XINC-006 | Test nesting at limit−1/limit/limit+1 and long acyclic chains; direct/indirect include cycles; repeated include of the same URI; fan-out; and limit values negative/zero/one/default/max integer. A cycle or excessive nesting must fail in bounded time and release every reader/stream. |
| SCXML-XINC-007 | Test included XML declarations, document types, document/document-fragment nodes, comments, processing instructions, multiple roots, inherited namespaces, `xml:lang`, and base URI/depth reporting as control enters and leaves each included document. |
| SCXML-XINC-008 | Inject acquisition/read/parse/cancel/dispose failures at every nesting level. Verify the full reader stack is closed once, the original reader is not lost, no cached resource remains live, and no partial state-machine model is returned. |

## 3. Public-model validation and compiled model building

| ID | Required scenarios and oracle |
|---|---|
| SCXML-VALID-001 | For every required property on every public-model type, test null/empty/default/valid. Aggregate multiple independent validation errors deterministically where supported; never let an incidental null reference replace a validation result. |
| SCXML-VALID-002 | For every mutually exclusive pair or group, test neither, each legal singleton, each illegal pair, and all-present: assign expr/content; cancel literal/expression ID; content expr/body; data src/expr/content; invoke type/typeexpr, id/idlocation, src/srcexpr, namelist/params; param expr/location; script src/body; and all send literal/expression/payload conflicts. |
| SCXML-VALID-003 | Validate root initial requirements, compound-state initial restrictions, default initial selection, explicit `<initial>` versus `initial` attribute conflict, target count, target existence, and legal initial target descendants. |
| SCXML-VALID-004 | Validate unique IDs across all state/history nodes, generated IDs, empty/Unicode IDs, collisions between generated and explicit IDs, forward/backward target references, unresolved targets, duplicate target tokens, and stable generated-ID behavior across builds/serialization. |
| SCXML-VALID-005 | Validate legal multi-target configurations: targets must form a legal configuration, avoid ancestor/descendant overlap, and select at most one descendant per non-parallel region while permitting orthogonal regions. Cover every pair relation and 3+ target mixtures. |
| SCXML-VALID-006 | Validate history restrictions: placement, default transition, exactly one target as applicable, no event/condition on default history transition, shallow/deep type, target relation to parent, and nested/history-to-history references. |
| SCXML-VALID-007 | Validate final-state restrictions, donedata placement/cardinality, top-level final behavior, no outgoing transitions/children where prohibited, and parent completion constraints. |
| SCXML-VALID-008 | Validate executable-content placement, including the special `<finalize>` prohibition on `<raise>` and `<send>`, branch markers only inside `if`, and custom executable content only where executable content is allowed. |
| SCXML-VALID-009 | Validate data-model support at model-build time: omitted/known/alias/unknown/case variant URI/name, handler construction failure, expression type rejected by handler, script support, custom actions/functions, and external-data media type. |
| SCXML-VALID-010 | Compile every expression-bearing property. Assert namespace capture from the expression's lexical element, diagnostic ownership, failure isolation, no partial compiled model, and cancellation/resource cleanup during external script/data acquisition. |
| SCXML-VALID-011 | Build the same semantic graph repeatedly and concurrently. Assert immutable/public graph is not mutated, compiled ordering and IDs are stable, no expression context crosses machines, and one failed build cannot poison later builds. |
| SCXML-VALID-012 | Differentially compare XML parse+validate+build with direct public-model validate+build for every scenario. Any route-specific acceptance or semantic result requires an explicit contract. |

## 4. Interpreter startup, binding, and lifecycle

| ID | Required scenarios and oracle |
|---|---|
| SCXML-LIFE-001 | Verify lifecycle notification order from accepted through initialization, started, running/stable, completion, exit, cancellation, destruction, and disposal. Cover normal final completion, unhandled error, explicit destroy, queue close, host cancellation, and startup failure. |
| SCXML-LIFE-002 | Start with no root states, one state, multiple root states using the default first child, a valid root `initial` attribute, and invalid root `initial` targets. Verify exact entry trace. A child `<initial>` element is valid only inside a compound `<state>` and is invalid directly under `<scxml>`. |
| SCXML-LIFE-003 | Early binding initializes root and all nested data exactly once in document order before initial entry. Late binding creates/initializes each state's data only on the first entry to that state, including exit/re-entry, history re-entry, parallel regions, and failed initialization. |
| SCXML-LIFE-004 | Test initialization values from expr, inline content, external src, undefined, and startup arguments. Cover duplicate argument names, unknown argument names, access modes, conversion failure, source failure, and partial initialization. Startup arguments override only the intended declarations and must not overwrite system variables. |
| SCXML-LIFE-005 | Execute root global script at the required point relative to root data initialization and initial-state entry. Cover empty, successful, failing, asynchronous, cancelling, unsupported, and external script forms. |
| SCXML-LIFE-006 | Verify a machine reaches stable state only after all enabled eventless transitions and internal events are consumed and invokes scheduled as specified. Stable-state callbacks/checkpoints must not occur in intermediate illegal configurations. |
| SCXML-LIFE-007 | Operations before start, duplicate/concurrent start, event during start, destroy during start, start after complete/destroy, duplicate/concurrent destroy, dispose while blocked, and external queue close must be deterministic and bounded. |
| SCXML-LIFE-008 | Verify top-level final exits interpreter, produces root done data once, cancels remaining activity, stops event consumption, and completes host result once. Events racing with completion must be accepted/rejected according to a documented linearization point. |

## 5. State entry, exit, completion, and configuration

| ID | Required scenarios and oracle |
|---|---|
| SCXML-STATE-001 | Enter atomic, compound, parallel, and final states. Assert ancestors before descendants, parallel regions in document/entry order, active configuration legality, and no duplicate entry when multiple transition targets share ancestors. |
| SCXML-STATE-002 | Exit configurations deepest-first in defined exit order. Assert `onexit` occurs before transition content and state removal, invokes are cancelled at the specified point, and shared ancestors exit once. |
| SCXML-STATE-003 | For compound default entry, execute initial transition content between parent `onentry` and child entry. Cover explicit initial element, initial attribute, first-child default, and multi-target initial transition to parallel descendants. |
| SCXML-STATE-004 | Parallel completion occurs only when every region is in a final state. Test staggered completion, nested parallels, region re-exit, transition escaping parallel, and done event generation exactly once per completed activation. |
| SCXML-STATE-005 | Entering final produces `done.state.<parentId>` with correct done data after final entry actions and before parent completion processing. Cover root final, compound final, parallel region final, nested final, payload error, and parent exit in same macrostep. |
| SCXML-STATE-006 | Assert legal configuration invariants after every microstep in generated runs: only atomic/final basic states in configuration plus implied ancestors, one active child per compound state, every region represented for active parallel states, no exited state remains. |
| SCXML-STATE-007 | Test topology depth 1–6, exactly beyond the navigator's inline path capacity, 100+, and configured resource limit. Deep valid machines must not corrupt ordering; adversarial depth must fail by policy rather than stack overflow. |

## 6. Transition matching, selection, conflicts, and microsteps

| ID | Required scenarios and oracle |
|---|---|
| SCXML-TRANS-001 | Eventless transitions are considered before internal and external events at the required points. Cover eventless false/true/error conditions, newly enabled transitions, eventless chains, cycles, and eventless competition across parallel regions. |
| SCXML-TRANS-002 | Event descriptor matching: exact name; prefix hierarchy; descriptor ending `.*`; descriptor ending `.`; `*`; multiple descriptors; empty/default event; whitespace tokenization; consecutive dots/empty parts; case sensitivity; Unicode; and near-prefix nonmatch (`foo` vs `foobar`). |
| SCXML-TRANS-003 | Within a state, choose the first document-order transition whose event descriptor matches and condition evaluates true. Prove later conditions are not evaluated after selection and false/error conditions allow the next candidate as specified. |
| SCXML-TRANS-004 | Search from each active atomic state through ancestors. A descendant-source transition preempts a conflicting ancestor transition; non-conflicting transitions from orthogonal regions may co-execute. Enumerate same source, ancestor/descendant sources, siblings, cousins, and parallel regions. |
| SCXML-TRANS-005 | Build and independently compute exit sets and transition domains for targetless, self, internal, external, source-to-descendant, source-to-ancestor, sibling, cross-region, multi-target, history, and root transitions. |
| SCXML-TRANS-006 | Internal versus external transition semantics: compound source to descendant, atomic self, compound self, source to outside subtree, explicit type omitted/default/invalid. Assert exact states exited/re-entered and data/invoke reinitialization consequences. |
| SCXML-TRANS-007 | Targetless transitions execute content without state exit/entry. Test eventless and eventful, with/without condition, chained raised events, executable failure, and competition with targeted transitions. |
| SCXML-TRANS-008 | Multi-target transition into orthogonal descendants exits/enters shared ancestors once, orders targets by document order, executes content once, and ends in a legal configuration. Exhaust all legal/illegal target relation classes. |
| SCXML-TRANS-009 | Conflict resolution across 2, 3, and many enabled transitions: identical exit sets, intersecting exit sets, disjoint sets, descendant preemption, document-order ties, and conflicts created through shared parallel ancestors. Compare to independent reference model. |
| SCXML-TRANS-010 | A microstep order is exactly exit actions, transition executable content, then entry actions, including all nested states and multiple selected transitions. Inject a trace action at every position and assert a total order. |
| SCXML-TRANS-011 | A condition error queues the correct platform event and treats the condition as false without partial transition effects. Cross with internal/external/error events, alternative transitions, all unhandled-error behaviors, and errors raised while handling an error. |
| SCXML-TRANS-012 | Executable-content failure stops the remaining actions in that executable sequence, produces the specified error, and preserves already completed side effects. Test failures in exit, transition, entry, initial, finalize, foreach, if, and done-data content. |
| SCXML-TRANS-013 | Cancellation at every await boundary in selection, exit, actions, entry, invocation, and queue wait leaves either the before or after state at the documented linearization point—never a corrupt hybrid—and teardown completes. |
| SCXML-TRANS-014 | Generate small legal machines (all non-isomorphic trees up to a practical bound) and event streams; compare each macrostep configuration and trace with the independent SCXML reference model. Shrink every mismatch. |

## 7. History semantics

| ID | Required scenarios and oracle |
|---|---|
| SCXML-HIST-001 | Shallow history remembers active immediate children; deep history remembers active atomic descendants. Cover compound and parallel parents, nested parallels, multiple exits/re-entries, and histories captured by transitions that exit different subsets. |
| SCXML-HIST-002 | First entry through uninitialized history follows its default transition and executes default transition content once. Later entries use stored history and do not execute default content. |
| SCXML-HIST-003 | Re-enter through shallow history and then apply each remembered child's normal default entry; deep history restores descendants directly with required ancestor entries. Assert entry order and invoke/data initialization behavior. |
| SCXML-HIST-004 | Multiple history pseudostates in one parent remain independent. Cover shallow+deep, targeted by different transitions, overwritten captures, parent never active, nested history, and history reached in a multi-target transition. |
| SCXML-HIST-005 | History state IDs and targets survive serialization and persistence/resume. Corrupt/missing stored history must produce a controlled persistence/validation result, not an illegal configuration. |

## 8. Event model and queue ordering

| ID | Required scenarios and oracle |
|---|---|
| SCXML-EVENT-001 | Validate event name construction/matching for default/empty, one/many segments, leading/trailing/consecutive dots, whitespace, wildcard-like literals, long names, Unicode normalization, and case. Invalid names must fail at the correct boundary. |
| SCXML-EVENT-002 | Verify `_event` before start, while processing eventless logic, for each internal/external/platform/invoke event, between macrosteps, and after completion/resume. All fields (`name`, `type`, `sendid`, `origin`, `origintype`, `invokeid`, `data`) must have correct types and access. |
| SCXML-EVENT-003 | Internal events are FIFO and take priority over the next external event. Raised events from exit/transition/entry/finalize and platform errors join the queue at the specified time. Trace multiple producers in one macrostep. |
| SCXML-EVENT-004 | External events are FIFO across host dispatch, sends, invoked children, restored queues, HTTP, and named pipe. Define and test ordering when dispatch calls are concurrent; no loss, duplication, or torn payload. |
| SCXML-EVENT-005 | Events with stale or nonmatching invoke IDs are filtered as specified; matching events run finalize before transition selection. Cross with autoforward, done/error invoke events, cancellation, and resume. |
| SCXML-EVENT-006 | Closing/cancelling/faulting internal and external queues in every lifecycle phase produces the configured termination behavior and wakes blocked consumers. Pending event payloads and waiters must be released. |
| SCXML-EVENT-007 | Verify event payload snapshot/aliasing policy. Mutating source data after dispatch, in one transition, in finalize, or in another session must not produce accidental cross-event or cross-session mutation. |
| SCXML-EVENT-008 | Event storms, recursive raises, send-to-self, and eventless loops terminate via a documented livelock/resource policy or continue under an explicit bounded harness. Detection must not reject long but progressing machines. |

## 9. Executable content independent of data-model syntax

| ID | Required scenarios and oracle |
|---|---|
| SCXML-EXEC-001 | `<raise>` queues one internal event with exact name, no payload, and correct order. Test missing/invalid event through all construction routes and raise while handling error/finalize/exit. |
| SCXML-EXEC-002 | `<if>` evaluates conditions lazily and in order, executes exactly the first true branch or else, skips all other expressions/actions, and supports empty/nested branches. Inject false, true, evaluation error, cancellation, and side-effecting conditions. |
| SCXML-EXEC-003 | `<foreach>` determines its iteration collection once, visits the data-model-defined order, sets item/index correctly, scopes variables correctly, and always restores scope after success/error/cancel/nesting. Mutation of the source during iteration follows a documented snapshot/live policy. |
| SCXML-EXEC-004 | `<log>` evaluates label/expression according to logging enablement contract, formats all data kinds/culture-invariant values, performs no state mutation, propagates or converts evaluator/logger failure correctly, and does not retain large exception/data graphs. |
| SCXML-EXEC-005 | `<assign>` common orchestration evaluates the right side and location in the specified order, mutates atomically where required, and handles undefined/empty/multi-location/data-model-specific operations. Detailed XPath cases are in document 02. |
| SCXML-EXEC-006 | `<script>` inline/external/global/local success, no-op, async completion, error, cancellation, unsupported data model, resource failure, and repeated execution. Verify external script encoding/media/base URI and disposal. |
| SCXML-EXEC-007 | Custom actions resolve by namespace/name, receive namespaces/content/context, run in document order, and handle provider absent/multiple/failing/cancelling/disposed. Unknown custom content must not be silently ignored unless explicitly contracted. |
| SCXML-EXEC-008 | Empty executable blocks and very long blocks (1, 2, 255, 256, 1k, 100k actions) preserve order, are cancellable, do not recurse to stack overflow, and have linear resource behavior. |

## 10. Data, params, content, and done data

| ID | Required scenarios and oracle |
|---|---|
| SCXML-DATA-001 | Root/state data declarations from no source, expr, inline content, and external src. Cross with early/late binding, startup arguments, re-entry, source content type, empty documents, malformed data, and evaluator failure. |
| SCXML-DATA-002 | `<param>` from expr or location, undefined/null/scalar/list/XML, repeated names, empty names through public model, evaluation order, one failure among many, and aliasing/snapshot behavior. |
| SCXML-DATA-003 | `namelist` evaluates each location once in lexical order, uses the intended key name, rejects/handles duplicate names, and combines with params/content only according to SCXML rules. |
| SCXML-DATA-004 | `<content>` expression versus body: whitespace-only, text, XML, mixed content, namespace inheritance, special characters, large content, malformed content, evaluator failure, and repeat execution/cache behavior after both successful and failed parse. |
| SCXML-DATA-005 | `<donedata>` with none, params, content, all data kinds, error, and cancellation. Verify payload appears exactly once on the correct `done.state`/completed result and is not evaluated when final is never entered. |
| SCXML-DATA-006 | Data access flags—writable, read-only, constant—and metadata survive conversion, event construction, invocation, persistence, and resume. Every forbidden write must fail without partial change. |

## 11. Send, cancel, delayed events, and routing

| ID | Required scenarios and oracle |
|---|---|
| SCXML-SEND-001 | Resolve literal versus expression for event, target, type, and delay; evaluate each exactly once in specified order; cover undefined/null/wrong type/error/cancellation. No router call occurs after an earlier required-field failure. |
| SCXML-SEND-002 | Generate/propagate send ID, write `idlocation`, handle explicit empty/duplicate IDs, and ensure generated IDs are unique under concurrency and persistence/resume. If writing idlocation fails, define whether scheduling occurred and test compensating behavior. |
| SCXML-SEND-003 | Construct payload from namelist, params, content, or none. Assert conflicts are rejected, data is isolated, raw-string content remains raw where intended, and `_event`/system values can be passed without losing type. |
| SCXML-SEND-004 | Immediate/zero and delayed send at below/at/above time boundaries; cancel before schedule, before fire, during dispatch, after fire, unknown ID, same ID shared by many events, and concurrent cancel/fire. Exactly the permitted number of events dispatch. |
| SCXML-SEND-005 | Route with omitted/default SCXML type, canonical URI, alias, custom type, unknown type, internal target, current session, parent, explicit session, invoke target, malformed target, and unavailable service. Verify origin/origintype/sender/target IDs. |
| SCXML-SEND-006 | Router or scheduler throws synchronously/asynchronously, times out, is cancelled, or reports partial dispatch. Verify `error.communication`/platform behavior, no task is forgotten, and scheduled-event collection is cleaned. |
| SCXML-CANCEL-001 | Resolve literal/expression send ID, reject no ID/wrong type/empty as specified, cancel one/many/no matching events, and test repeated/concurrent cancellation. Cancellation cannot affect events with a distinct ID. |
| SCXML-CANCEL-002 | Dispose scheduler with 0/1/many pending events, cancellation failures, dispatch in progress, and simultaneous schedule/cancel. Sync and async disposal must aggregate/report errors consistently and leave no timers/tasks. |

## 12. Invoke, finalize, and autoforward

| ID | Required scenarios and oracle |
|---|---|
| SCXML-INVOKE-001 | Resolve `type/typeexpr`, `src/srcexpr`, content, `id/idlocation`, params, and namelist in the required order. Cover every valid source form and every conflict, undefined/wrong type, expression error, resource failure, and cancellation. |
| SCXML-INVOKE-002 | Generate unique invoke IDs; write idlocation; expose current invoke ID only during the intended start/cancel/finalize operations; isolate nested/concurrent invokes; and clear all ambient context after failure/cancel/completion. |
| SCXML-INVOKE-003 | Invokes start only after the state has completed its macrostep/entered stable point as required. If the state exits before start, it must not start. Test entry followed by immediate eventless exit, internal-event exit, and external-event exit. |
| SCXML-INVOKE-004 | One/many invokes per state, nested child machines, parallel invokes, same source, content source, external service, unknown provider, start failure, completion, service error, cancellation, and parent destruction. Assert active registry and exactly-once disposal. |
| SCXML-INVOKE-005 | On state exit, cancel all its active invokes in the required order relative to `onexit`. Test cancellation success/failure/hang, completion racing cancellation, duplicate exit signals, and teardown escalation. |
| SCXML-INVOKE-006 | Matching returned events execute `<finalize>` before transition selection. Nonmatching/stale invoke IDs do not finalize. Cover done, error, arbitrary child event, finalize failure/cancel, and event payload mutation. |
| SCXML-INVOKE-007 | `autoforward=true` forwards each external event exactly once to every applicable active invoke, with correct order relative to finalize and parent processing. Cover false/default, multiple invokes, child failure, parent exit, internal events, and loops. |
| SCXML-INVOKE-008 | Child SCXML source/content inherits only intended location, arguments, security context, parent session, invoke ID, type, and I/O targets. Child data/configuration/events cannot leak into sibling or parent except via defined events/results. |

## 13. Error handling and termination

| ID | Required scenarios and oracle |
|---|---|
| SCXML-ERROR-001 | Inject errors in every phase: parse, validate, build, data init, global script, condition, exit action, transition action, entry action, initial action, done data, send/cancel, invoke start/cancel/finalize/autoforward, queue, persistence, logging, and cleanup. Build a phase-to-error-event/result table. |
| SCXML-ERROR-002 | Run each runtime error under every `UnhandledErrorBehaviour`: ignore, destroy, terminate, and any current/future enum values. Verify lifecycle, exit actions, result/exception, queue contents, host collection membership, and disposal. |
| SCXML-ERROR-003 | An error while processing another error, repeated errors, and error-event loops must remain bounded and preserve the configured policy. No recursive stack overflow or silent background-task failure. |
| SCXML-ERROR-004 | Preserve exception data in the data-model error object: message, type, source, stack/text, null fields, inner/aggregate/custom exceptions, lazy evaluation, access flags, serialization/persistence, and release after event processing. Do not assert unstable stack formatting beyond contract. |
| SCXML-ERROR-005 | Livelock detection: shortest and long eventless cycles, internal self-raise cycles, cycles that mutate data/progress, high but finite chains around detector thresholds, parallel activity, and queue-size changes. No false positive for a finite machine and bounded termination for a true livelock. |
| SCXML-ERROR-006 | Destroy/terminate must close queues, stop schedulers/invokes, exit active states as specified, complete waiters, reject later operations, and aggregate cleanup failures without losing the primary failure. |

## 14. Serializer and construction-route equivalence

| ID | Required scenarios and oracle |
|---|---|
| SCXML-SER-001 | Serialize every model element, attribute, default, optional field, executable child, raw content, namespace, and extension. Explicitly include root `name`/`binding` and XPath assign `type`/`attr`. Missing output is a defect, not an accepted round-trip loss. |
| SCXML-SER-002 | Escape attribute/text/raw XML characters, namespace prefixes/rebindings, Unicode, whitespace, CDATA boundaries, comments where represented, URI values, event/target/namelist token lists, booleans, enums, and delays. Output must be well-formed and culture-independent. |
| SCXML-SER-003 | Semantic round trip `model -> XML -> model` for every valid fixture and generated model. Compare every semantic property and ordered child. Then serialize again and require canonical idempotence modulo documented formatting/prefix choices. |
| SCXML-SER-004 | Parse and serialize imported W3C plus generated SCXML; executing original and round-tripped models with the same event stream must yield identical trace, data, outputs, and result. |
| SCXML-SER-005 | Serializer behavior for invalid/incomplete public models: reject before output or produce only according to an explicit API contract; never emit a misleading partially valid document after a writer failure. Test cancellation and writer/disposal failure. |
| SCXML-SER-006 | Compare SCXML text, public object, and fluent/builder construction routes for representative and generated graphs. They must validate, compile, execute, and serialize equivalently. |

## 15. W3C conformance ledger requirements

Create a machine-readable ledger with at least these columns: upstream assertion/test ID, SCXML section, normative keyword, feature, datamodel applicability, local fixture, expected pass/fail state, local test method, status, deviation, and defect link.

At minimum, map and execute every applicable assertion in these SCXML areas:

- core element and attribute constraints;
- state types, legal configurations, document/state order;
- transition selection, preemption, domains, exit/entry sets, microstep and macrostep algorithm;
- initial, final, parallel completion, shallow/deep history;
- event naming/descriptors, internal/external queues, system events;
- executable content (`raise`, `if` family, `foreach`, `log`, assign, script, custom content);
- data initialization/binding, expressions, system variables, params/content/donedata;
- send/cancel, delayed events, SCXML I/O processor targets;
- invoke, finalize, autoforward, done/error invoke events;
- error handling and algorithm termination.

Run data-model-neutral conformance cases with every applicable data model. For a test written in a data-model-specific syntax, create an equivalent XPath form and runtime form when the semantics can be represented. Record genuinely inapplicable cases explicitly.
