# SCXML Test Coverage

This is the current runtime coverage summary. See [SCXML_TEST_PLAN.md](SCXML_TEST_PLAN.md) for the specification-wide SCXML test catalog.

## Generated source groups

- `Basic/InitialMachines.cs`
- `Basic/CompoundMachines.cs`
- `Basic/StateElementMachines.cs`
- `Basic/OnEntryOnExitMachines.cs`
- `Transitions/EventlessTransitionMachines.cs`
- `Parallel/ParallelMachines.cs`
- `History/ShallowHistoryMachines.cs`
- `History/DeepHistoryMachines.cs`
- `DataModel/NullAndXPathMachines.cs`

## Already existing groups

- `Basic/BasicMachines.cs`
- `DataModel/DataModelMachines.cs`
- `Metadata/MetadataMachines.cs`
- `Transitions/TransitionMachines.cs`
- `XPath/XPathMachines.cs`

## Validation test groups

- `DocumentConfigurationValidationTest.cs`
- `StatesValidationTest.cs`
- `ScxmlPlanValidationTest.cs`

## Covered runtime-stable features

- Root `initial`, `name`, `binding`, and `datamodel`
- Default initial child selection
- Eventless transitions
- Compound state nesting
- `onentry` and `onexit`
- Parallel completion and `done.state.*`
- Shallow and deep history
- Null datamodel transitions
- XPath datamodel assignment and conditions
- Final-state done-data string assertions where stable
- XPath executable content for assignment, conditionals, document-order actions, and parent/child entry order
- Raised internal event matching: exact, token-prefix, non-lexical prefix, multiple descriptors, wildcard, and case sensitivity
- Eventless-before-internal-event ordering and FIFO processing of raised internal events
- Transition selection priority: first matching transition in document order, false-condition fall-through, descendant preemption, and ancestor fallback
- Document/configuration behavior: orthogonal multi-target root initial, ancestor/default descendant entry, default binding, missing datamodel default, namespace/version validation, unsupported datamodel failure, extension-element parser policy, duplicate ID build failure, and anonymous/generated ID parse stability
- State behavior: `<initial>` child transitions, initial-transition executable content order, nested-descendant initial targets, final done-data expressions, illegal final children, invalid history type/placement, and invalid history transition targets
- Plan-wide validation behavior: unknown transition targets, illegal executable-content containers, mutually exclusive data/send/invoke initializers, null-datamodel data manipulation rejection, illegal finalize placement, and malformed XML rejection
- Parser/unit behavior: root content model with top-level datamodel/script/state/parallel/final/comments, data `expr`/`src`/inline/empty forms, log label/expression variants, top-level and executable script elements, done-data content `expr`, inline XML namespace bodies, done-data `param` `expr`/`location`, invalid `param` combinations, XML entity/CDATA/comment/processing-instruction handling, and malformed transition event descriptors

## Known runtime gaps discovered by generated cases

- Parent/child `onexit` order currently observes parent before child. SCXML specifies descendant exit before ancestor exit, so the corresponding planned case is not registered as a passing test yet.
- Targetless transition with executable content currently throws during interpreter entry-set computation, so that planned case is not registered yet.
- External self-transition and `type="internal"` descendant-transition probes currently do not observe the SCXML-specified re-entry/exit behavior, so those planned cases are not registered as passing tests yet.
- Non-orthogonal root multi-target initial configurations currently destroy on idle instead of failing validation/build promptly, so that planned rejection case is not registered as a passing test yet.
- Late-binding lifecycle and post-termination queue/action behavior still need harness-level observability before they can be asserted directly.
- Initial child transitions with illegal `event`/`cond` attributes and states that combine an `initial` attribute with an `<initial>` child are currently accepted by parser/validation, so those rejection probes are documented but not registered as passing tests.
- Shallow-history re-entry into a compound child, deep-history restoration of parallel-region final configurations, and repeated history replacement probes currently livelock or skip history default-transition executable content, so those cases remain documented gaps.
- Duplicate/invalid data IDs and standalone conditional partition elements such as `<elseif>` outside `<if>` are currently accepted by parser/validation, so those rejection probes are documented but not registered as passing tests.
- Non-orthogonal multi-target transitions currently destroy on idle instead of failing validation/build promptly, so that planned rejection case is documented but not registered as a passing test.
- Empty `<raise event="">` is currently accepted by parser/validation, so that invalid raised-event-name rejection probe is documented but not registered as a passing test.

## Deliberately skipped or kept conservative

- `send`, delayed send, and `cancel`
- `invoke` and `done.invoke.*`
- Complex `if` / `elseif` / `else` / `foreach` combinations unless a proven runtime-safe pattern is added later
- External event injection scenarios requiring custom harness support

## Notes

All machines are intentionally small and should terminate by reaching `<final id="done"/>`.
