# SCXML Test Coverage

## Generated source groups

- `Basic/InitialMachines.cs`
- `Basic/CompoundMachines.cs`
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

## Deliberately skipped or kept conservative

- `send`, delayed send, and `cancel`
- `invoke` and `done.invoke.*`
- Complex `if` / `elseif` / `else` / `foreach` combinations unless a proven runtime-safe pattern is added later
- External event injection scenarios requiring custom harness support

## Notes

All machines are intentionally small and should terminate by reaching `<final id="done"/>`.
