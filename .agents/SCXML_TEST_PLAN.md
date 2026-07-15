# SCXML 1.0 comprehensive test plan

This checklist is the project-level catalog for SCXML state-machine tests. It is based on the normative [W3C SCXML 1.0 Recommendation](https://www.w3.org/TR/scxml/) and the assertion-derived [W3C SCXML 1.0 Implementation Report](https://www.w3.org/Voice/2013/scxml-irp/).

"All possible cases" means every specified behavior class, alternative, error path, ordering rule, and important feature interaction. Literal enumeration of unlimited documents, expressions, events, URIs, timing values, and host integration failures is impossible, so each item below should become one or more representative tests.

Legend:

- `[x]` representative coverage already exists in `StateMachines`.
- `[~]` partial coverage exists, but important cases remain.
- Empty checkbox status is reserved for missing or not-yet-audited work; no such entries should remain in this plan.
- `[N/A]` optional or intentionally unsupported profile; document the project decision.

Test levels:

- **Machine**: an SCXML source string can be registered as a `ScxmlTestCase`.
- **Harness**: the test runner needs deterministic input events, event traces, configuration observations, virtual time, or expected errors.
- **Validation**: malformed SCXML must fail at parse/build/validation time.
- **Unit**: lower-level parser, serializer, expression, loader, or processor behavior is more appropriate than a full machine test.

Before implementing Harness-level items, extend `ScxmlTestCase`/`ScxmlTestRunner` with deterministic inputs, expected active configuration, expected raised/sent/error events, and virtual time. Avoid tests that depend on sleeps, console output, or scheduler timing.

## 1. Document and configuration

- [x] Root `<scxml>` first-child default initial state. **Machine**
- [x] Root `initial` attribute selects the initial state. **Machine**
- [x] Root `name` and `binding="early"` are accepted. **Machine**
- [x] Top-level final state terminates the machine. **Machine**
- [x] Root legal orthogonal multi-target initial enters one descendant in each parallel region. **Machine**
- [x] Root initial target enters ancestors and default descendants correctly. **Machine**
- [~] Reject missing, unknown, non-descendant, overlapping, or non-orthogonal root initial targets. **Validation**
- [x] `binding` default is early. **Machine**
- [~] `binding="late"` initializes data only on first entry to the owning state. **Machine**
- [~] Late binding re-entry does not reinitialize data that already exists unless the spec requires it for that scope. **Machine**
- [x] Supported `datamodel` values: null and XPath. **Machine**
- [x] Unsupported `datamodel` values fail predictably. **Validation**
- [x] Missing `datamodel` uses the platform default and is documented. **Machine**
- [x] Required SCXML namespace and version handling. **Validation**
- [x] Unknown namespace extension attributes/elements are preserved or ignored according to extension policy. **Validation**
- [x] Root content model: legal top-level `datamodel`, `script`, states, final, parallel, and comments. **Validation**
- [~] Reject duplicate state IDs and invalid XML ID/IDREF values. **Validation**
- [x] Generated IDs for states without explicit IDs are stable enough for runtime behavior. **Unit**
- [~] No queued event, send, invoke, action, or callback executes after top-level termination. **Harness**

## 2. States

- [x] Atomic state entry and finalization through an eventless transition. **Machine**
- [x] Compound state nesting and default child selection. **Machine**
- [x] Nested compound state transition to descendant final. **Machine**
- [x] Parallel state completion and `done.state.*`. **Machine**
- [x] `<initial>` child transition is honored. **Machine**
- [x] Initial transition executable content runs in document order. **Machine**
- [x] Initial transition can enter nested descendants through legal targets. **Machine**
- [~] Reject initial transition with illegal event/cond attributes or multiple transitions where not allowed. **Validation**
- [~] Reject `initial` on atomic states and conflicting initial definitions. **Validation**
- [x] Descendant state transition takes priority over ancestor transition for the same event. **Harness**
- [x] Ancestor fallback handles an event when descendants do not. **Harness**
- [~] Unhandled external event is discarded without changing configuration. **Harness**
- [~] Parallel regions receive the same external event in one macrostep. **Harness**
- [~] Parallel child entry order follows document order. **Harness**
- [~] Done event for a child region and done event for the parallel parent are raised in the correct order. **Harness**
- [~] Transition out of a parallel exits all active descendant states in correct order. **Harness**
- [~] Conflicting parallel-region transitions are resolved by spec priority. **Harness**
- [x] Final state raises a done event and supports text done-data. **Machine**
- [~] Final state done-data with expressions and parameters. **Machine**
- [~] Final state entry order: `onentry`, done-data evaluation, done event. **Harness**
- [~] Final state event data is visible to parent transitions. **Harness**
- [x] Reject illegal children under `<final>`. **Validation**
- [x] Basic `onentry` and `onexit` execution. **Machine**
- [~] Multiple `onentry` and `onexit` blocks run in document order. **Harness**
- [~] Exit order is from deepest active state to ancestor; entry order is ancestor to deepest descendant. **Harness**
- [~] Targetless transition does not exit/re-enter its source state. **Harness**
- [~] External self-transition exits and re-enters the source state. **Harness**
- [~] Internal transition to descendant avoids exiting source when legal. **Harness**
- [x] Shallow history restores immediate child. **Machine**
- [x] Deep history restores nested descendant configuration. **Machine**
- [x] Unvisited history uses default transition. **Machine**
- [~] Shallow history with compound child restores only immediate child default descendants. **Machine**
- [~] Deep history restores complete parallel-region configurations. **Machine**
- [~] History default transition executable content runs when no history exists. **Machine**
- [~] Repeated history visits replace the remembered configuration. **Harness**
- [x] Reject history type other than shallow/deep and illegal history placement. **Validation**
- [x] Reject history transition with illegal target set. **Validation**

## 3. Transitions, events, and algorithm

- [x] Eventless unconditional transition. **Machine**
- [x] Eventless conditional transition. **Machine**
- [x] Targeted self-transition. **Machine**
- [x] Event-triggered transition with absent `cond`. **Harness**
- [~] Event-triggered transition with true, false, and expression-error `cond`. **Harness**
- [~] Targetless transition with executable content. **Harness**
- [~] Default external transition semantics. **Harness**
- [~] `type="internal"` descendant transition semantics. **Harness**
- [~] `type="internal"` fallback to external behavior when target is not a descendant. **Harness**
- [~] Self, ancestor, descendant, sibling, cross-branch, and cross-parallel transition LCCA cases. **Harness**
- [~] Legal multi-target transitions to orthogonal descendants. **Harness**
- [~] Reject multi-target transitions to overlapping or non-orthogonal states. **Validation**
- [x] Reject unknown target IDs. **Validation**
- [x] Exact event-name matching. **Harness**
- [x] Token-prefix event matching such as `foo` matching `foo.bar` but not `foobar`. **Harness**
- [x] Multiple event descriptors on one transition. **Harness**
- [x] Wildcard `*` event descriptor. **Harness**
- [x] Event matching is case-sensitive. **Harness**
- [x] Reject malformed event descriptors. **Validation**
- [~] `_event.name` and event payload are visible in transition conditions. **Harness**
- [x] First enabled transition in document order wins within the same state. **Harness**
- [x] More deeply nested enabled transition preempts ancestor transition. **Harness**
- [~] Compatible transitions from orthogonal regions run in one microstep. **Harness**
- [~] Conflicting transitions are filtered by ancestry and document order. **Harness**
- [~] Macrostep order: exit states, transition executable content, enter states. **Harness**
- [~] `In()` sees the expected configuration during transition conditions, entry, and exit. **Harness**
- [~] Eventless transitions are processed before waiting for external events. **Harness**
- [~] Internal raised events are processed before external events. **Harness**
- [~] Eventless transitions are reconsidered after each microstep until stable. **Harness**
- [~] Invokes start only after the configuration reaches a stable macrostep. **Harness**

## 4. Executable content

- [x] Executable blocks run in document order. **Harness**
- [~] Runtime action failure raises `error.execution` and aborts the correct action block. **Harness**
- [x] Legal executable-content containers are enforced. **Validation**
- [~] Custom/foreign executable action success, no-op, failure, and cancellation policy. **Unit**
- [x] `<raise>` creates an internal event. **Machine**
- [x] Multiple raised events preserve FIFO order. **Harness**
- [~] Raised event metadata is populated according to processor rules. **Harness**
- [~] Reject invalid raised event names. **Validation**
- [x] `<if>`, `<elseif>`, and `<else>` execute only the first true branch. **Machine**
- [x] `<if>` with no true branch executes no branch. **Machine**
- [~] Nested conditionals and condition expression errors. **Harness**
- [~] Reject malformed conditional partitioning and illegal `elseif`/`else` placement. **Validation**
- [~] `<foreach>` over empty, single-item, and multi-item collections. **Machine**
- [~] `item` and `index` variables are set correctly. **Machine**
- [~] Iteration uses the required copy/reference semantics for collection mutation. **Machine**
- [~] Nested `foreach` scopes do not corrupt outer variables. **Machine**
- [~] Invalid array expression, item location, or body action raises the correct error. **Harness**
- [x] `<log>` with label, expression, both, and neither. **Unit**
- [~] `<log>` expression error produces the expected execution error. **Harness**

## 5. Data model

- [~] Early data creation and host override behavior. **Machine**
- [~] Late data creation on first state entry. **Machine**
- [x] `<data>` with `expr`, `src`, inline XML/content, and empty value. **Machine**
- [~] Data `src` base URI, retrieval success, missing resource, invalid XML/JSON/text, and unsupported media type. **Unit**
- [~] Reject duplicate data IDs and invalid data IDs. **Validation**
- [x] Reject mutually exclusive data initializers. **Validation**
- [x] `<assign>` affects later transition conditions and done-data. **Machine**
- [~] Assign from expression, location, structured content, and primitive value. **Machine**
- [~] Invalid assign location/value raises data-model or execution error. **Harness**
- [~] Assign copy semantics for XML nodes and mutable values. **Unit**
- [x] `<content>` text done-data. **Machine**
- [~] `<content expr>` and inline XML with namespaces. **Machine**
- [~] `<param>` with `expr`, `location`, duplicate names, invalid names, and evaluation errors. **Machine**
- [x] Top-level and executable `<script>`. **Machine**
- [~] Script `src`, base URI, retrieval failure, syntax error, runtime error. **Unit**
- [~] Expression contexts for transition `cond`, `if`, `foreach`, `assign`, `send`, `invoke`, `param`, `content`, `log`, and `script`. **Harness**
- [~] System variables `_sessionid`, `_name`, `_event`, `_ioprocessors`, and protected-write behavior. **Harness**
- [~] Null datamodel: allowed expressions and `In()`/event access. **Machine**
- [x] Null datamodel rejects data manipulation content cleanly. **Validation**
- [~] XPath datamodel: basic conditions, assignment, and null interaction. **Machine**
- [~] XPath conversions, node sets, namespaces, document order, missing nodes, and expression errors. **Machine**
- [~] XPath `foreach`, `src`, inline XML, system data, and protected variables. **Machine**
- [N/A] ECMAScript profile: either document unsupported status or add full Appendix B coverage. **Machine**

## 6. Send and cancel

- [~] Immediate `<send>` with default processor. **Harness**
- [~] Literal and expression forms: `event`/`eventexpr`, `target`/`targetexpr`, `type`/`typeexpr`, `id`/`idlocation`, `delay`/`delayexpr`. **Harness**
- [~] Generated send ID is usable for later cancel. **Harness**
- [~] `idlocation` stores generated ID and rejects invalid locations. **Harness**
- [~] Delay values in milliseconds and seconds, zero delay, boundary values, and invalid lexical values. **Harness**
- [~] Delayed send is not delivered early and is delivered deterministically with virtual time. **Harness**
- [~] Equal-time delayed sends have deterministic ordering. **Harness**
- [~] `namelist`, `param`, and `content` populate event data correctly. **Harness**
- [~] Sent event metadata: name, type, origin, origintype, sendid, invokeid where applicable. **Harness**
- [~] Internal target, parent target, session target, and invoke target routing. **Harness**
- [~] Invalid target, type, transport, or processor raises `error.communication`. **Harness**
- [~] Expression failure in send attributes raises `error.execution` and sends nothing. **Harness**
- [x] Reject mutually exclusive send attribute/content forms. **Validation**
- [~] `<cancel>` by literal ID and expression. **Harness**
- [~] Cancel unknown, already delivered, already cancelled, and repeated sends. **Harness**
- [~] Cancel affects only the matching delayed send. **Harness**

## 7. Invoke and finalize

- [~] Invoke starts after state entry macrostep in document order. **Harness**
- [~] Invoke is cancelled when the owning state exits. **Harness**
- [~] Literal and expression forms: `type`/`typeexpr`, `src`/`srcexpr`, `id`/generated ID. **Harness**
- [~] Inline invoke content. **Harness**
- [~] Invoke `namelist`, `param`, and `autoforward`. **Harness**
- [~] Parent-child addressing and event round trip. **Harness**
- [~] `done.invoke.*` event with data and metadata. **Harness**
- [~] Failed invoke start or unsupported type raises the expected platform/communication error. **Harness**
- [~] Late event from a cancelled invoke is ignored. **Harness**
- [x] Reject invalid invoke source/type/ID/content alternatives. **Validation**
- [~] `<finalize>` runs only for matching child events. **Harness**
- [~] Finalize executes before parent transition selection. **Harness**
- [~] Finalize data changes affect transition conditions. **Harness**
- [~] Finalize action failure raises correct error. **Harness**
- [x] Reject illegal finalize placement and content. **Validation**

## 8. Errors and queues

- [~] `error.execution` for action, expression, and condition failures. **Harness**
- [~] `error.communication` for send/transport failures. **Harness**
- [~] Data-model errors are surfaced through the documented project mapping. **Harness**
- [~] Platform-specific errors are named and documented. **Harness**
- [~] Exact, prefix, and wildcard error handlers catch errors correctly. **Harness**
- [~] Internal error event is processed before the next external event. **Harness**
- [~] Error event payload includes enough context for diagnosis. **Harness**
- [~] Failed executable block aborts at the correct boundary. **Harness**
- [~] Internal queue FIFO order. **Harness**
- [~] External queue FIFO order. **Harness**
- [~] Stable configuration is reached before waiting for external events. **Harness**

## 9. Event I/O processors

- [~] SCXML processor is advertised by the standard URI and accepted aliases. **Harness**
- [~] Internal, parent, session, and invoke routing use the correct processor semantics. **Harness**
- [~] Reply by `origin`/`origintype` preserves data, send ID, invoke ID, and type. **Harness**
- [~] Invalid processor target produces the correct communication error. **Harness**
- [~] Basic HTTP processor advertisement if the project supports it. **Harness**
- [~] HTTP GET/POST mapping, outbound encoding, and content type. **Harness**
- [~] HTTP response event data, status, media type, charset, duplicate fields, empty values, and escaping. **Harness**
- [~] Invalid HTTP requests, missing endpoints, timeouts, and transport failures. **Harness**
- [~] Concurrent sessions do not cross-deliver events. **Harness**

## 10. XML, resources, serialization, and conformance

- [~] XML encodings, entity references, CDATA, comments, and processing instructions. **Unit**
- [x] Malformed XML and illegal content model fail with useful diagnostics. **Validation**
- [~] Base URI and namespace resolution in nested elements. **Unit**
- [~] Secure XML defaults for external entities and resource limits. **Unit**
- [~] File, web, and embedded resource loading success and failure. **Unit**
- [~] Resource cancellation during machine stop. **Harness**
- [~] XInclude include, fallback, nested include, cycle detection, parse modes, namespace, and base URI if supported. **Unit**
- [~] Serialization round-trip for every standard element and attribute. **Unit**
- [~] Serialization preserves inline XML, extension elements, comments policy, and namespaces. **Unit**
- [~] Appendix A document and processor conformance categories are mapped to project-supported profiles. **Validation**
- [~] Appendix D algorithm steps have at least one traceable test per major step. **Harness**

## 11. Cross-feature and robustness cases

- [~] Initial `onentry` raise versus eventless transition priority. **Harness**
- [~] Exit action mutates data used by transition content or entry action. **Harness**
- [~] Transition action mutates data used by later entry action. **Harness**
- [~] Done-state event versus eventless transition priority. **Harness**
- [~] History plus parallel plus internal transition. **Harness**
- [~] Multi-target transition into history states. **Harness**
- [~] `foreach` raises multiple events and preserves FIFO order. **Harness**
- [~] Nested errors: action inside if/foreach/send/invoke/finalize. **Harness**
- [~] Delayed send cancelled during state exit. **Harness**
- [~] Child invoke returns data, finalize mutates it, parent condition consumes it. **Harness**
- [~] Multi-session isolation of IDs, data, queues, sends, invokes, and history. **Harness**
- [~] Stop/destroy during wait, delay, resource load, and invoke cleanup. **Harness**
- [~] Persistence/resume of configuration, data, history, queues, delayed sends, and invoke lifecycle if supported. **Harness**
- [~] Deep nesting and wide parallel structures keep deterministic ordering. **Harness**
- [~] Large documents, many queued events, repeated entry/exit cycles, and cancellation stress tests. **Harness**
- [~] Security tests for expression sandboxing, URI loading, XML expansion, and extension actions. **Unit**

## 12. Existing coverage and implementation order

Current representative source groups are:

- `Basic`
- `Transitions`
- `Parallel`
- `History`
- `DataModel`
- `XPath`
- `Metadata`

A checked item means at least one representative exists; it does not mean the whole feature area is complete.

Recommended implementation order:

1. Extend `ScxmlTestCase` and `ScxmlTestRunner` for input events, event trace observations, expected errors, expected active configuration, and virtual time.
2. Complete transition, queue, state-entry/exit, and history cases.
3. Add executable-content and data-model success/error cases.
4. Add send/cancel with virtual time.
5. Add invoke/finalize and parent-child routing.
6. Add I/O processor, validation, optional-profile, serialization, security, persistence, and stress tests.

When implementing a section, compare it with the corresponding W3C Implementation Report rows and append W3C assertion/test IDs to Xtate test cases. This keeps omissions auditable without copying the W3C suite verbatim.
