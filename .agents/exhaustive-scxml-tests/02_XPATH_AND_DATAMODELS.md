# Data values, data models, and exhaustive XPath plan

## Scope and oracle rules

This document covers the common `DataModelValue` graph, null and runtime handlers, data-model selection/extensions, and the XPath data model. XPath expected behavior comes from the W3C SCXML XPath data-model Note and XPath 1.0, not from the current implementation.

For the current campaign, follow documents 06 and 07 and generate source for every remaining assigned data-model/XPath test. Annotate existing tests and every parameterized case with complete metadata. Use explicit declarative C# case tables for large matrices; an ID-driven generic factory does not count. Non-compiling helpers are acceptable; do not build, run, repair, or update planning files.

For XPath, test the same behavior at four layers:

1. expression compilation and static context;
2. evaluator result and dynamic context;
3. direct mutable XML/tree behavior;
4. complete SCXML machine behavior.

Each mutation test must snapshot the complete tree before and after, including order, keys, attributes, namespace declarations, metadata, access flags, and node identity/aliasing where observable. On an error, assert the required `error.execution` behavior and that the operation has **no effect** unless the standard explicitly permits partial effects.

## 1. Common `DataModelValue` and `DataModelList`

| ID | Required scenarios and oracle |
|---|---|
| DM-VALUE-001 | Construct and inspect every value kind: undefined, null, boolean false/true, number as `Int32`/`Int64`/`Double`/`Decimal`, date/time as `DateTime`/`DateTimeOffset`, string, list, and lazy. Include default struct value and each implicit/explicit conversion route. |
| DM-VALUE-002 | Number boundaries: min/max/zero/±1 for integral types; positive/negative fractions; decimal scale; ±0.0; epsilon/subnormal; max finite; ±infinity; NaN payloads. Assert kind preservation or documented normalization, equality, comparison, hashing, string/XML/JSON/persistence conversion, and culture independence. |
| DM-VALUE-003 | Date/time boundaries: min/max, UTC/local/unspecified, offset min/max, daylight-transition values, fractional ticks, round trip under non-Gregorian cultures/time zones, XML/JSON lexical forms, equality and persistence. Tests must not depend on local clock. |
| DM-VALUE-004 | Strings: null route versus empty, whitespace classes, CR/LF, NUL/control characters, quotes/XML/JSON escapes, BMP/non-BMP, invalid surrogate input where representable, normalization forms, 1/large strings, and strings equal to reserved metadata/type tokens. |
| DM-VALUE-005 | Access modes writable/read-only/constant on every kind and nested location. Test allowed update, prohibited replace/delete/child mutation/attribute mutation, propagation into nested values, clone/conversion/persistence, and failure atomicity. |
| DM-VALUE-006 | Lazy values: single evaluation, concurrent evaluation, sync/async exception if applicable, recursive/self-referential lazy, lazy returning lazy, all result kinds, access mode, equality/conversion, disposal/context retention, and serialization before/after evaluation. |
| DM-LIST-001 | Lists with 0/1/many unkeyed items, keyed items, and mixed keyed/unkeyed items. Cover empty key, duplicate key, null key, case variants, Unicode keys, reserved `_` and XPath metadata names, very long keys, insertion order, and lookup/update/remove semantics. |
| DM-LIST-002 | Index operations at −1, 0, last, count, count+1, max integer; key lookup absent/present/duplicate; insert first/middle/last; replace; remove; clear; enumeration during mutation; and versioning behavior. No integer overflow or wrong-item mutation. |
| DM-LIST-003 | Metadata: namespace URI/prefix, attributes, namespace declarations, element/list name, keyed child names, no-key marker, and empty-key marker. Test duplicate metadata entries, prefix rebinding, default namespace, reserved `xml`/`xmlns`, empty values, and preservation through every converter. |
| DM-LIST-004 | Deep and wide graphs: depth 0/1/6/7/100/large; width 0/1/255/256/1k/large. Include shared child instances, direct/indirect cycles, repeated references, and lazy cycles. Each operation must either preserve graph identity as contracted or reject cycles in bounded time without stack overflow. |
| DM-LIST-005 | Copy/clone/snapshot semantics. Mutate original/clone at root and deep child; verify intentional deep/shallow behavior, access flags, metadata, lazy instances, shared references, and event/invoke payload isolation. |
| DM-CONV-001 | Convert every value kind to/from CLR object, dynamic access, JSON, XML, XPath object, event data, invocation params, and persistence. Build a source-kind × destination × boundary matrix and assert exact loss/normalization or controlled rejection. |
| DM-CONV-002 | Equality and hashing across same/different numeric representations, undefined/null, date representations, lists with same content/different order/metadata/access, cyclic/shared graphs, and lazy values. Verify dictionary/set safety where supported. |
| DM-CONV-003 | Concurrent read/read, read/write, and write/write operations on shared values/lists. If types are not thread-safe, prove host/session isolation prevents unintended sharing; operations must not corrupt internal structure. |

## 2. Data-model discovery and handler lifecycle

| ID | Required scenarios and oracle |
|---|---|
| DM-HANDLER-001 | Select absent/default, null, runtime, XPath, each alias/full identifier, case variants, empty identifier, unknown identifier, and a custom handler. Assert deterministic precedence when multiple handlers claim a name. |
| DM-HANDLER-002 | Handler creation/initialization success, sync/async failure, cancellation, duplicate provider, disposed scope, and concurrent machines. One handler/context must never leak variables, namespaces, ambient runtime, compiled descriptors, or services into another. |
| DM-HANDLER-003 | For each expression role—condition, value, location, foreach array, script/action, inline content, external data, done data—test supported syntax/result kinds and deliberate unsupported kinds. Rejection must occur at the intended validation/build/runtime stage. |
| DM-HANDLER-004 | Custom variable/function/action providers: none, one, many with same key, precedence, lazy/asynchronous initialization, wrong return kind, exception, cancellation, and disposal. Provider state must be session-scoped unless explicitly registered globally. |

## 3. Null data model

The null data model still participates in state semantics and `In(...)` conditions. Everything else must be explicitly supported or rejected, never silently treated as a different data model.

| ID | Required scenarios and oracle |
|---|---|
| DM-NULL-001 | Omitted/explicit null data model with a machine containing no expressions or data. Exercise all state/transition/history/event/send/invoke semantics that do not require data. |
| DM-NULL-002 | `In(...)` with active/inactive state, ancestor/atomic/final/history IDs, empty/unknown/duplicate ID, generated ID, leading/trailing/internal whitespace, quotes/escapes, wrong case, malformed parenthesis, nested text, and long input. |
| DM-NULL-003 | Boolean condition constants or alternative textual forms are accepted only if contractually supported. Every other condition syntax must produce a precise build/runtime error and must not evaluate as truthy by accident. |
| DM-NULL-004 | Attempt data declaration, assign, foreach, value expression, content expression, done-data expression, script, params/namelist, and expression-driven send/invoke fields. Test XML and public-model routes; assert intended validation stage and no side effect. |
| DM-NULL-005 | Inline literal content/payload and literal send/invoke attributes that are data-model-neutral: explicitly prove which are allowed. Unsupported operations must not crash through a missing evaluator. |

## 4. Runtime data model

| ID | Required scenarios and oracle |
|---|---|
| DM-RUNTIME-001 | Runtime predicate false/true, synchronous/asynchronous, exception, cancellation, reentrancy, nested evaluation, and concurrent sessions. The ambient runtime context must be correct only during the callback and cleared/restored afterward. |
| DM-RUNTIME-002 | Runtime value callback returning every `DataModelValue` kind, delayed completion, exception/cancel, callback recursively invoking runtime APIs, and parallel callbacks. Verify data/context/session isolation. |
| DM-RUNTIME-003 | Runtime action success, async suspension, exception/cancel, nested action, action dispatching events/start/cancel invoke, and action after machine exit. Later actions stop on error as specified. |
| DM-RUNTIME-004 | Runtime APIs for current data model, arguments, `InState`, logging, send/cancel event, start/cancel invoke: valid calls in predicate/value/action; calls outside runtime callback; after await; from child task; after completion; and from a suppressed/restored execution context. |
| DM-RUNTIME-005 | Ambient-context retention probe: execute callbacks capturing a large sentinel object, complete/destroy machine, force full GC, and require sentinel collection. Repeat after exceptions, cancellation, nested callbacks, and thread-pool continuation to expose uncleared `AsyncLocal` state. |
| DM-RUNTIME-006 | Unsupported runtime datamodel declarations, locations/assignments, foreach syntax, scripts, and foreign expression objects fail at validation/build time. Passing a delegate/object of a near-but-wrong runtime type must not be dynamically accepted. |
| DM-RUNTIME-007 | Build one runtime expression and attempt to use it across machines/sessions concurrently. Verify expression object reuse does not bind permanently to the first context and side effects go only to the executing session. |

## 5. XPath XML data model representation

The required representation is a document whose root element is `<datamodel>` and whose child `<data id="...">` elements represent global variables. Test the representation directly and through SCXML execution.

| ID | Required scenarios and oracle |
|---|---|
| XPATH-TREE-001 | Initial empty data model; one/many `<data>` variables; declaration order; unique/duplicate/empty/Unicode IDs; values undefined/null/scalar/list/XML; and root/name/namespace metadata. Unknown-variable behavior must follow the declared Xtate extension and be documented. |
| XPATH-TREE-002 | Early binding creates and initializes all data variables before execution. Late binding exposes the required elements but assigns a state's value on first entry only. Test never-entered state, first entry, re-entry, history, parallel entry, failed first initialization, and same ID declarations in different states. |
| XPATH-TREE-003 | Scalar encoding for boolean, each number subtype including decimal/NaN/infinity, each date subtype, string, null, and undefined. Assert namespace/prefix/type attributes, lexical format, whitespace, and round-trip kind. |
| XPATH-TREE-004 | List encoding for keyed, unkeyed, empty-key, duplicate-key, mixed, nested, empty, and metadata-rich lists. Assert element names, `x:item`, `x:empty`, attributes, namespace declarations, child order, text-node aggregation, and round trip. |
| XPATH-TREE-005 | XML input with elements, text, CDATA, comments, processing instructions, attributes, namespace nodes, mixed content, whitespace-only nodes, entity expansion, and multiple top-level nodes. Assert exactly which node kinds survive conversion; unsupported input must fail predictably rather than truncate siblings. |
| XPATH-TREE-006 | Conversion from XML with reserved type namespace: every known `x:type`, absent type, unknown type, wrong prefix/right URI, right prefix/wrong URI, duplicate type attribute, malformed lexical value, and type marker on container nodes. |
| XPATH-TREE-007 | Namespace behavior: default namespace, multiple prefixes for one URI, prefix shadowing, undeclaration where legal, `xml` namespace, attribute namespaces, inherited declarations, and namespace-axis order. Moving through the navigator must report correct prefix/name/namespace/value. |
| XPATH-TREE-008 | Navigator axes and movement at every node kind: root, first/next/previous child, parent, first/next attribute, namespace scopes, same-node comparison, position, clone, root check, empty node, and `MoveToId`. Compare with a canonical XML navigator where applicable. |
| XPATH-TREE-009 | Mutable navigator operations at root/element/scalar/list positions: set value, append/prepend/insert/replace/delete child, create/delete attribute, normalize after mutation, and cursor position after operation. Cross access modes and invalid node types. |
| XPATH-TREE-010 | Path storage boundaries at depth 0–5, 6, 7, 255, 256, and deep limit to exercise inline-to-array growth and index widths. Clone/move/mutate each depth and ensure no stale path, wrong sibling, or retained oversized buffer. |
| XPATH-TREE-011 | XML serialize/deserialize every value and generated tree through sync/async, one-byte chunks, non-seekable streams, cancellation, malformed/truncated XML, reader failure, writer failure, and disposal failure. Output is deterministic and culture-independent. |
| XPATH-TREE-012 | Decimal and every other numeric subtype must serialize through all buffer/span/writer paths. Compare small versus large output-buffer paths to detect type cases accidentally omitted from one implementation. |
| XPATH-TREE-013 | Deep/wide XML conversion must be bounded, cancellable, and free of recursive stack overflow. Test shared/cyclic `DataModelList` graphs: faithful handling if supported or controlled cycle rejection. |

## 6. XPath static context, compilation, variables, and namespaces

| ID | Required scenarios and oracle |
|---|---|
| XPATH-COMP-001 | Compile empty/whitespace, literals, valid expressions, malformed tokens, incomplete operators, invalid QName, unknown prefix, unknown function, wrong arity, unsupported XPath version syntax, and extremely long/deep expressions. Assert validation diagnostics and bounded compile time. |
| XPATH-COMP-002 | Capture namespace bindings from the expression's lexical SCXML element and ancestors: default namespace, expression prefixes, shadowing, redeclaration after compile, same prefix in another machine, absent prefix, reserved prefixes, and custom-action nesting. XPath unprefixed names retain XPath 1.0 semantics. |
| XPATH-COMP-003 | Built-in variables and data variables: known, unknown, forward declaration, case variants, Unicode, reserved system name, namespaced variable (unsupported by design), variable in predicate/function argument, and variable rebound by foreach scope. |
| XPATH-COMP-004 | Variable descriptor initialization before/after first use, concurrent first use, initialization error/cancel, expression reused, and descriptor list release. No variable may appear as an empty iterator merely because initialization raced. |
| XPATH-COMP-005 | Built-in and custom functions: correct name/namespace/arity/argument/result types; overload collision; provider initialization; function exception/cancel; recursive call; and concurrent machine isolation. Unknown function must be a compile error. |
| XPATH-COMP-006 | Compile the same expression once/many/concurrently in one/many contexts. Verify compiled object/context lifecycle, namespace immutability, variable rebinding semantics, thread safety or explicit non-sharing, and collection after machine disposal. |
| XPATH-COMP-007 | Expression role constraints: value accepts every valid XPath result kind; condition accepts every XPath expression with required effective-boolean conversion; location resolves writable node sets; foreach requires node-set; script is rejected for XPath; assignment action/attr is validated. |

## 7. XPath 1.0 expression semantics

Build both table-driven examples and differential/property tests against an independent conforming XPath 1.0 engine over equivalent immutable XML. Exclude only Xtate-specific functions/variables and mutation.

| ID | Required scenarios and oracle |
|---|---|
| XPATH-EXPR-001 | Literals: empty/nonempty strings, both quote styles, embedded opposite quote, whitespace, Unicode; integers, decimals, leading/trailing decimal point if legal, negative via unary operator, large/overflowing numbers, NaN/infinity produced by operations. |
| XPATH-EXPR-002 | Arithmetic `+ - * div mod` and unary minus across number/string/boolean/node-set conversions; ±0, NaN, infinity, division/modulo by zero, precedence/associativity, parentheses, and culture. |
| XPATH-EXPR-003 | Boolean `and`/`or` short-circuit, `not`, nested expressions, conversion of every object kind, errors/side effects in skipped/evaluated operands, and precedence. |
| XPATH-EXPR-004 | Equality/inequality and relational operators for every pair of boolean, number, string, and node-set; empty/one/many nodes; any-pair node-set semantics; NaN; whitespace strings; document order; and conversion precedence. |
| XPATH-EXPR-005 | Location paths: absolute/relative, `.`, `..`, `/`, `//`, child/descendant/parent/ancestor/following-sibling/preceding-sibling/following/preceding/attribute/namespace/self/descendant-or-self/ancestor-or-self axes. Test root/boundaries and abbreviated syntax. |
| XPATH-EXPR-006 | Node tests: QName, `*`, prefix wildcard where legal, `node()`, `text()`, `comment()`, `processing-instruction()` with/without target; namespace/default-namespace cases and absent node kinds. |
| XPATH-EXPR-007 | Predicates: numeric position, boolean, string/number/node-set conversion, nested predicates, context position/size, reverse axes, `last()`, filtered empty/many sets, and predicates referencing data/system/foreach variables. |
| XPATH-EXPR-008 | Union `|`: disjoint/overlapping/identical/empty node sets, roots/attributes/namespaces where supported, duplicate elimination, and document order. Invalid non-node-set operands fail. |
| XPATH-EXPR-009 | Core node-set functions `last`, `position`, `count`, `local-name`, `namespace-uri`, `name`, `id`: all arities, omitted/current node, empty/many node sets, metadata namespaces, and `id()` behavior consistent with navigator ID support/declared limitation. |
| XPATH-EXPR-010 | Core string functions `string`, `concat`, `starts-with`, `contains`, `substring-before`, `substring-after`, `substring`, `string-length`, `normalize-space`, `translate`: empty/Unicode/whitespace, rounding boundaries, NaN/infinity indices, multiple arguments, and wrong arity. |
| XPATH-EXPR-011 | Core boolean functions `boolean`, `not`, `true`, `false`, `lang`: every object kind, nonempty node-set EBV, empty/nonempty string EBV, ±0/NaN number EBV, inherited `xml:lang`, and case-insensitive language matching. |
| XPATH-EXPR-012 | Core number functions `number`, `sum`, `floor`, `ceiling`, `round`: all object kinds, whitespace/invalid numeric strings, empty/mixed node sets, NaN/infinity, negative zero, halfway rounding, and wrong arity. |
| XPATH-EXPR-013 | Context conversion to Xtate value: root/element/text/attribute/namespace node; empty/one/many node set; string, boolean, integer-like and fractional number; object/unsupported return. Assert type, order, deep copy versus alias, and `stripRoots` behavior. |
| XPATH-EXPR-014 | Effective boolean value exactly per XPath/SCXML XPath data model: nonempty node-set true regardless of first node's text; empty node-set false; nonempty string true even `"false"`/`"0"`; empty string false; nonzero finite/infinite number true; ±0 and NaN false; booleans unchanged. |

## 8. XPath `In()` and SCXML system variables

| ID | Required scenarios and oracle |
|---|---|
| XPATH-SYS-001 | `In()` correct name/case/no namespace, exactly one argument, string conversion, active atomic/ancestor/final/unknown/generated ID, before initialization, during exit/entry, and after completion. |
| XPATH-SYS-002 | `In()` node-set argument: empty, one active, one inactive, mixed active/inactive, duplicate nodes, text/element nodes, and document order. Derive exact semantics from the XPath data-model Note (`In(string(...))` conversion) and flag any all-elements extension discrepancy. |
| XPATH-SYS-003 | `_name`, `_sessionid`, `_event`, and `_ioprocessors` XML shape, types, values, and access modes before/during/after events; omitted optional fields; every event/data kind; one/many I/O processors; processor aliases and locations. |
| XPATH-SYS-004 | Attempts to assign/replace/delete/add children or attributes on system variables and their descendants. Read-only/constant semantics must hold with no partial mutation; errors have correct owner/event. |
| XPATH-SYS-005 | `_event.data`: key/value list becomes the required child representation; XML payload remains XML; other payload becomes the required normalized string representation. Test duplicate/empty keys, unkeyed values, nested lists, whitespace text, and unsupported node kinds. |
| XPATH-SYS-006 | Xtate extension `_x` fields (`args`, `configuration`, `datamodel`, `host`) across all lifecycle phases, data models, metadata, absent host/configuration, arguments, and access attempts. Undefined placeholders must be stable and documented. |
| XPATH-SYS-007 | System-variable name collision in user `<data>` or foreach variable, unknown variable auto-creation extension, and assignment to reserved names. Define and test precedence and failure behavior explicitly. |

## 9. XPath locations and all assignment actions

The W3C XPath data model defines `replacechildren` (default), `firstchild`, `lastchild`, `previoussibling`, `nextsibling`, `replace`, `delete`, and `addattribute`. `attr` is required only for `addattribute`. For a node-set location, evaluate the value expression once and apply the operation to every selected target; a failure must enqueue `error.execution` and leave the data model unchanged.

Build a generated full cross-product over:

- 8 actions;
- target cardinality 0/1/2/many;
- target kind root/element/text/attribute/namespace/scalar/list item;
- target position first/middle/last/only/root;
- value empty node set, one/many nodes, root/element/text/attribute/namespace, string, boolean, each number boundary, null, undefined, list/XML;
- destination access writable/read-only/constant;
- same-tree source before/inside/after target, source equals target, ancestor/descendant source, and cross-tree source;
- 1 and many targets with overlapping/ancestor-related selections;
- success, expression error, conversion error, mutation error, cancellation.

| ID | Required scenarios and oracle |
|---|---|
| XPATH-ASSIGN-001 | Parse assignment action with omitted/default, every exact value, case/whitespace variants, unknown/empty. Parse/validate `attr` absent/empty/valid QName/invalid QName/prefixed/unbound/reserved and its legality on non-addattribute actions. |
| XPATH-ASSIGN-002 | `replacechildren`: remove all existing children and insert a deep copy of the value representation in order. Test no children, mixed content, scalar normalization, self/ancestor source, and root/system/access restrictions. |
| XPATH-ASSIGN-003 | `firstchild` and `lastchild`: insert before/after existing children, handle empty parent, preserve existing order, normalize scalar/list representation correctly, and reject targets that cannot have children. |
| XPATH-ASSIGN-004 | `previoussibling` and `nextsibling`: insert at first/middle/last, reject/no-op at root according to standard, preserve parent/order, handle multiple inserted nodes, and avoid cursor drift across multiple targets. |
| XPATH-ASSIGN-005 | `replace`: replace the target node itself with deep-copied value node(s), including first/middle/last/only child, scalar/list transitions, overlapping targets, source equal/related to target, and target root/attribute/namespace restrictions. |
| XPATH-ASSIGN-006 | `delete`: delete each selected target, including siblings selected together, ancestor+descendant overlap, duplicate iterator positions, only child causing normalization, attribute, root/system variables, and empty location. Iteration cannot skip or delete the wrong node as positions shift. |
| XPATH-ASSIGN-007 | `addattribute`: add exact name/value to each target element; unprefixed/prefixed name and namespace resolution; duplicate existing attribute; namespace declaration/reserved name; empty/multi-node value string conversion; non-element target; many targets; rollback on one invalid target. |
| XPATH-ASSIGN-008 | Location evaluating to empty node set, scalar, boolean/string/number, unsupported object, detached/stale iterator, read-only node, or an expression error. Assert required error rather than silent return and absolutely no mutation. |
| XPATH-ASSIGN-009 | Value expression is evaluated exactly once even with many locations. Use a counting/custom function and a value tied to pre-mutation tree state. Location set is likewise fixed according to XPath semantics before mutation. |
| XPATH-ASSIGN-010 | Multi-target atomicity: inject failure at target 1/middle/last for each action. The standard's no-effect guarantee requires transaction/snapshot rollback; assert no first-target mutation remains after any failure. |
| XPATH-ASSIGN-011 | Deep-copy semantics for node-set values: changing source after assignment does not mutate destination and vice versa; namespace/attributes/children/metadata/order all copy; node identity is distinct; copying a target into itself terminates. |
| XPATH-ASSIGN-012 | Expression `set`/get/name/declare-variable APIs used by generic evaluator and foreach: exact variable target, current scope versus global, unknown variable extension, system variable access, wrong cardinality, and scope restoration. |
| XPATH-ASSIGN-013 | Property/model differential test: apply generated action to a small canonical XML tree in an independent functional mutation model and to Xtate; compare complete canonical trees or expected failure/rollback. Shrink mismatches. |

## 10. XPath `foreach`

| ID | Required scenarios and oracle |
|---|---|
| XPATH-FOREACH-001 | Array expression empty/one/many node set in document order. Item receives a shallow copy of each node; index is 1 through count per the XPath data-model Note. Test element/text/attribute nodes as input. |
| XPATH-FOREACH-002 | Item only, index only if accepted, both, same name, collision with global/system/outer-loop variable, unknown variable, invalid/namespaced variable name, and read/write inside body. Scope shadows then restores exact prior binding. |
| XPATH-FOREACH-003 | Nested loops over same/different arrays with same/different variable names. Assert outer value/index restoration after inner loop, including inner body error/cancel/early machine exit. |
| XPATH-FOREACH-004 | Mutate original collection/tree during loop by delete/insert/replace and mutate item copy. Assert the data-model-defined iteration snapshot/order and shallow-copy behavior; no skipped/duplicated iteration unless specified. |
| XPATH-FOREACH-005 | Array returns non-node-set scalar/unsupported/empty, compile error, evaluation error, custom function side effect, cancellation, and huge node set. Fail at correct boundary, evaluate once, and always pop scope. |
| XPATH-FOREACH-006 | Compare generic data-model foreach (normally zero-based over materialized values) and XPath foreach (one-based node-set semantics). Ensure handler dispatch selects the correct evaluator and no shared default leaks into XPath. |

## 11. XPath inline content, external data, event data, and serialization

| ID | Required scenarios and oracle |
|---|---|
| XPATH-CONTENT-001 | Inline XML with one/many roots, text only, whitespace-only, mixed content, namespaces, comments/PI/CDATA, empty content, malformed XML, inherited base URI, and very large/deep content. Define whether a wrapper is stripped and assert exact value. |
| XPATH-CONTENT-002 | Inline-content parse caching: repeated execution after success and after failure, concurrent first use, different machine contexts/namespaces, and disposal. A previous parse exception must not cause unbounded repeated allocations/logging unless explicitly intended. |
| XPATH-CONTENT-003 | External XPath data from exact and parameterized/case-varied `application/xml`, `text/xml`, `application/*+xml`, text/JSON/binary/no content type. Use the documented media-type matching rules and expose overly strict or permissive behavior. |
| XPATH-CONTENT-004 | External data encoding/BOM/charset, empty stream, malformed/truncated XML, non-seekable/chunked stream, cancellation, loader error, stream error/disposal error, redirects/base URI, and resource reuse. Assert exactly-once ownership cleanup. |
| XPATH-CONTENT-005 | XPath result serialization to SCXML string for string/boolean/number/node-set and all node kinds. Test empty/many nodes, namespace output, escaping, ordering, decimal path, large output, and culture. |

## 12. Mandatory high-risk probes from production review

These are not alternate expected behaviors. They are focused tests designed to prove or disprove implementation risks while retaining the standards-derived oracle.

| ID | Risk that the test must expose if present |
|---|---|
| XPATH-PROBE-001 | Effective-boolean conversion may parse node/string text as XML booleans instead of XPath EBV; nonempty node set/string and NaN are discriminating cases. |
| XPATH-PROBE-002 | Condition validation may reject valid string/number/node-set expressions even though the XPath data model applies effective-boolean conversion. |
| XPATH-PROBE-003 | `In()` over a multi-node node set may require all node string values active instead of applying the required conversion semantics. |
| XPATH-PROBE-004 | A location returning a non-node-set or empty result may silently skip instead of producing the specified error and no effect. |
| XPATH-PROBE-005 | Multi-location assignment may mutate early targets before a later target fails, violating the no-effect requirement. |
| XPATH-PROBE-006 | XML mixed-content reading may return on the first text node and truncate following siblings. Use text-element-text and text-comment-element-text witnesses. |
| XPATH-PROBE-007 | Decimal serialization may differ between span/buffer and writer paths. Force both small and expanded-buffer paths. |
| XPATH-PROBE-008 | Late binding may reinitialize state data on every re-entry rather than only on first entry. Use a counted expression and mutate after first entry. |
| XPATH-PROBE-009 | Generic zero-based foreach indexing may be reused accidentally for XPath, whose index starts at one. |
| XPATH-PROBE-010 | Runtime `AsyncLocal` context may survive callback completion and retain session/data objects. Use weak references and cross-session callbacks. |
| XPATH-PROBE-011 | Duplicate attribute creation, root mutation, namespace traversal, or tree normalization may leave an invalid XML model or stale navigator path. Validate canonical XML after every generated mutation. |
| XPATH-PROBE-012 | Failed inline-content parsing may be retried/logged/allocated on every execution rather than caching a stable failure. Measure loader/parse/log call count and retained exception graph. |

## 13. Property, differential, and mutation requirements for data models

| ID | Campaign |
|---|---|
| DM-PROP-001 | For every acyclic generated data graph within bounds, XML round trip and persistence round trip preserve value kind, value, order, key, metadata, and access. JSON round trip follows its documented loss model. |
| DM-PROP-002 | For every generated immutable XML tree and supported XPath 1.0 expression, Xtate result matches an independent XPath engine after canonical result conversion. Record expression/tree seed and shrink both. |
| DM-PROP-003 | For every generated valid location/action/value/tree tuple, Xtate mutation matches the independent functional mutation oracle; for invalid tuples, both reject and Xtate tree is unchanged. |
| DM-PROP-004 | Repeated serialize/deserialize and XML/value conversion reaches a fixed point after the first documented normalization; no further drift in type/order/metadata/text. |
| DM-PROP-005 | Mutation testing must kill changes to every conversion branch, node movement boundary, assignment action, access check, namespace comparison, result-kind gate, EBV rule, foreach index/order/scope cleanup, and system-variable field. |
| DM-PROP-006 | Grammar fuzz XPath compile/evaluate and XML/value conversion. Outcomes are valid result or documented diagnostic within time/memory limits—never process crash, stack overflow, hang, uncontrolled allocation, or cross-session state. |

## 14. XPath requirement ledger

Create one ledger row for every normative statement in the XPath data-model Note, grouped under:

- data-model document structure and variable binding;
- early/late initialization;
- expression and effective-boolean evaluation;
- `In()`;
- locations and all eight assignment actions;
- deep-copy and no-effect-on-error rules;
- `_name`, `_sessionid`, `_event`, `_ioprocessors`;
- event payload representation;
- string serialization;
- absence of scripting;
- foreach node-set, document order, shallow copy, and 1-based index.

Link each row to a direct component test and at least one complete SCXML witness. Then map XPath 1.0 grammar/operators/axes/node tests/core functions/conversions to the expression differential corpus. No row can be marked covered solely because an imported W3C SCXML fixture happened to execute the code.
