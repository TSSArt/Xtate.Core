# Unit Test Coverage Worklist

## Agent objective

Add or improve unit tests for the production methods listed below.

Rules:

- Work against `Xtate.Core.dll`; do not add tests merely to cover test-project code.
- Do not spend time on methods already reported as 100% line coverage and 100% block coverage; they are intentionally omitted.
- Prefer behavior-focused tests over tests written only to execute lines.
- Cover normal results, boundary values, invalid input, exception paths, cancellation, disposal, concurrency, and state transitions where applicable.
- Preserve existing public behavior. Do not change production code solely to make a test pass unless a real defect is demonstrated.
- For compiler-generated async/iterator entries, test the named source method and its relevant completion, failure, cancellation, and enumeration paths. Do not target `MoveNext()` directly.
- After each batch, rerun the same coverage configuration and remove an item only when both line and block coverage reach 100%, or document why a branch is unreachable.

## Report summary

- Assembly line coverage: **92.61%**
- Assembly block coverage: **93.64%**
- Types requiring additional coverage: **152**
- Methods/function bodies requiring additional coverage: **480**
- P0 (0% in at least one metric): **67**
- P1 (<50%): **30**
- P2 (<80%): **172**
- P3 (80–99.99%): **211**

Refreshed from `Xtate.Core.dll` using `TestResults/0a4c77db-2fae-4e76-9f09-f251efae2623/coverage.xml`, generated on 2026-07-15 at 20:28 after 3,412 passing net8.0 tests. The current assembly no longer contains the legacy `DataModelList.Dynamic1` type. The named method worklist below is still being reconciled; verified 100% classes are synchronized in `UnitTestCoverage.Tracker.md` pending the next full method-list regeneration. `StateMachineReader` present/absent Send, Invoke, and DoneData shapes are behaviorally covered; Visual Studio coverage continues to report their expression-bodied initializer sequence points as 0% line with 85.71-95.56% block coverage. Recursive persisted-interpreter transition lookup, persisted-context event restoration, and named-pipe invoke-target construction are now 100% line and block covered.

Coverage format below: `line% / block%`, followed by uncovered line/block counts.

## Worklist

### `JetBrains.Annotations.ContractAnnotationAttribute`

- [ ] **P0** `ContractAnnotationAttribute(string)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `ContractAnnotationAttribute(string, bool)` — 0.00% / 0.00% (5 uncovered lines, 2 uncovered blocks)

### `Xtate.Actions.ActionBase`

- [ ] **P2** `GetArray() [compiler-generated async/iterator body]` — 76.47% / 79.41% (0 uncovered lines, 4 partial lines, 7 uncovered blocks)
- [ ] **P3** `GetString() [compiler-generated async/iterator body]` — 80.00% / 88.00% (0 uncovered lines, 2 partial lines, 3 uncovered blocks)
- [ ] **P3** `GetBoolean() [compiler-generated async/iterator body]` — 90.00% / 95.45% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `GetInteger() [compiler-generated async/iterator body]` — 90.00% / 95.45% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Actions.AsyncAction`

- [ ] **P0** `GetValues()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Actions.SyncAction`

- [ ] **P0** `GetLocations()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `GetValues()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Actions.SyncAction.Location`

- [ ] **P0** `GetValue() [compiler-generated async/iterator body]` — 0.00% / 0.00% (4 uncovered lines, 8 uncovered blocks)
- [ ] **P0** `SetValue(Xtate.DataTypes.DataModelValue)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Ancestor.Ancestor<TEntity>`

- [ ] **P0** `Is<T>()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P1** `As<T>()` — 85.71% / 38.46% (1 uncovered lines, 8 uncovered blocks)

### `Xtate.ConcurrentDictionaryExtensions`

- [ ] **P3** `TryTake<TKey, TValue>(System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, out TKey, out TValue)` — 87.50% / 87.50% (2 uncovered lines, 2 uncovered blocks)

### `Xtate.DataModel.DataModelHandlerBase`

- [ ] **P0** `GetEvaluator(Xtate.StateMachine.IExternalDataExpression)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)

### `Xtate.DataModel.Runtime.Runtime`

- [ ] **P0** `get_Arguments()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.DataModel.Services.ActionProvider<TCustomAction>`

- [ ] **P3** `Activate(string)` — 88.89% / 94.74% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.Services.ContentBodyEvaluator`

- [ ] **P0** `EvaluateString()` — 0.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.Services.CustomActionContainer`

- [ ] **P3** `CustomActionContainer(Xtate.StateMachine.ICustomAction, System.Func<Xtate.StateMachine.ICustomAction, Xtate.DataModel.IAction>)` — 92.00% / 95.35% (0 uncovered lines, 2 partial lines, 2 uncovered blocks)

### `Xtate.DataModel.Services.CustomActionFactory`

- [ ] **P2** `GetAction(Xtate.StateMachine.ICustomAction)` — 86.96% / 78.13% (3 uncovered lines, 7 uncovered blocks)

### `Xtate.DataModel.Services.DataConverter`

- [ ] **P2** `FromEvent() [compiler-generated local/lambda body]` — 85.71% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `FromContent() [compiler-generated async/iterator body]` — 85.71% / 80.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `GetParameters() [compiler-generated async/iterator body]` — 96.88% / 98.39% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.Services.DataModelConverter`

- [ ] **P3** `IsArray(Xtate.DataTypes.DataModelList)` — 88.89% / 94.74% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `IsObject(Xtate.DataTypes.DataModelList)` — 88.89% / 94.74% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.DataModel.Services.DataModelConverter.JsonListConverter`

- [ ] **P2** `Read(ref System.Text.Json.Utf8JsonReader, System.Type, System.Text.Json.JsonSerializerOptions)` — 83.33% / 66.67% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `WriteObject(System.Text.Json.Utf8JsonWriter, Xtate.DataTypes.DataModelList, System.Text.Json.JsonSerializerOptions)` — 91.30% / 90.00% (2 uncovered lines, 3 uncovered blocks)

### `Xtate.DataModel.Services.DataModelConverter.JsonValueConverter`

- [ ] **P2** `Write(System.Text.Json.Utf8JsonWriter, Xtate.DataTypes.DataModelValue, System.Text.Json.JsonSerializerOptions)` — 85.71% / 76.74% (4 uncovered lines, 10 uncovered blocks)
- [ ] **P3** `Read(ref System.Text.Json.Utf8JsonReader, System.Type, System.Text.Json.JsonSerializerOptions)` — 92.31% / 88.24% (0 uncovered lines, 1 partial lines, 4 uncovered blocks)

### `Xtate.DataModel.Services.DefaultAssignEvaluator`

- [ ] **P2** `DefaultAssignEvaluator(Xtate.StateMachine.IAssign)` — 77.78% / 90.00% (0 uncovered lines, 2 partial lines, 2 uncovered blocks)

### `Xtate.DataModel.Services.DefaultContentBodyEvaluator`

- [ ] **P2** `EvaluateObject() [compiler-generated async/iterator body]` — 62.50% / 50.00% (6 uncovered lines, 12 uncovered blocks)

### `Xtate.DataModel.Services.DefaultForEachEvaluator`

- [ ] **P3** `DefaultForEachEvaluator(Xtate.StateMachine.IForEach)` — 81.82% / 90.91% (0 uncovered lines, 2 partial lines, 2 uncovered blocks)

### `Xtate.DataModel.Services.DefaultIfEvaluator`

- [ ] **P3** `Execute() [compiler-generated async/iterator body]` — 91.67% / 95.83% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `DefaultIfEvaluator(Xtate.StateMachine.IIf)` — 92.86% / 95.56% (0 uncovered lines, 2 partial lines, 2 uncovered blocks)

### `Xtate.DataModel.Services.DefaultInlineContentEvaluator`

- [ ] **P2** `EvaluateObject() [compiler-generated async/iterator body]` — 62.50% / 50.00% (6 uncovered lines, 12 uncovered blocks)

### `Xtate.DataModel.Services.DefaultScriptEvaluator`

- [ ] **P3** `DefaultScriptEvaluator(Xtate.StateMachine.IScript)` — 83.33% / 92.86% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.Services.DefaultSendEvaluator`

- [ ] **P3** `DefaultSendEvaluator(Xtate.StateMachine.ISend)` — 80.00% / 93.75% (0 uncovered lines, 2 partial lines, 3 uncovered blocks)

### `Xtate.DataModel.Services.InlineContentEvaluator`

- [ ] **P0** `EvaluateString()` — 0.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.Services.UnknownDataModelHandler`

- [ ] **P0** `Visit(ref Xtate.StateMachine.IDataModel)` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)
- [ ] **P0** `Visit(ref Xtate.StateMachine.IScript)` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.AdapterFactory`

- [ ] **P0** `GetNotSupportedException()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `GetSimpleTypeAdapter(ref Xtate.DataTypes.DataModelValue)` — 83.33% / 75.00% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P3** `GetDefaultAdapter(ref Xtate.DataTypes.DataModelValue)` — 90.91% / 81.82% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `GetItemAdapter(ref Xtate.DataTypes.DataModelList.Entry)` — 90.91% / 83.33% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.AttributeNodeAdapter`

- [ ] **P0** `GetLocalName(ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node modreq(System.Runtime.InteropServices.InAttribute))` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `GetNodeType()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.DataModelXPathNavigator`

- [ ] **P0** `get_BaseURI()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `MoveToId(string)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `MoveToNext()` — 0.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `MoveToNextAttribute()` — 0.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `MoveToPrevious()` — 0.00% / 0.00% (1 uncovered lines, 7 uncovered blocks)
- [ ] **P0** `SetValue(string)` — 0.00% / 0.00% (6 uncovered lines, 9 uncovered blocks)
- [ ] **P1** `PushNode(ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node)` — 29.41% / 33.33% (12 uncovered lines, 6 uncovered blocks)
- [ ] **P2** `AddAttribute(Xtate.DataTypes.DataModelList, string, string, string, string)` — 50.00% / 80.00% (0 uncovered lines, 3 partial lines, 3 uncovered blocks)
- [ ] **P2** `AddChildren(Xtate.DataTypes.IObject, bool, bool)` — 54.76% / 65.15% (16 uncovered lines, 3 partial lines, 23 uncovered blocks)
- [ ] **P2** `MoveToParentFirstNamespace(System.Xml.XPath.XPathNamespaceScope)` — 61.54% / 56.25% (4 uncovered lines, 1 partial lines, 7 uncovered blocks)
- [ ] **P2** `DeleteSelf()` — 57.14% / 73.33% (6 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `SetNewValue(ref Xtate.DataTypes.DataModelValue)` — 60.00% / 78.95% (6 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `AddRange(Xtate.DataTypes.DataModelList, Xtate.DataTypes.IObject, int)` — 68.18% / 61.70% (6 uncovered lines, 1 partial lines, 18 uncovered blocks)
- [ ] **P2** `AddSiblings(Xtate.DataTypes.IObject, int, bool)` — 68.42% / 83.33% (6 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `MoveTo(System.Xml.XPath.XPathNavigator)` — 71.43% / 80.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `CreateAttribute(string, string, string, string)` — 72.22% / 84.62% (5 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `IsSamePosition(System.Xml.XPath.XPathNavigator)` — 73.33% / 85.71% (4 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `ShouldNormalize(System.Collections.Generic.IEnumerable<Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Element>, out Xtate.DataTypes.DataModelValue)` — 78.57% / 88.89% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `Entries() [compiler-generated async/iterator body]` — 81.48% / 91.49% (4 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P3** `ClonePosition(Xtate.DataModel.XPath.Internal.DataModelXPathNavigator, Xtate.DataModel.XPath.Internal.DataModelXPathNavigator)` — 85.71% / 92.31% (3 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `PathItem(int)` — 90.00% / 88.89% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.ElementNodeAdapter`

- [ ] **P2** `GetLocalName(ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node modreq(System.Runtime.InteropServices.InAttribute))` — 50.00% / 50.00% (4 uncovered lines, 3 uncovered blocks)
- [ ] **P3** `GetValue(ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node modreq(System.Runtime.InteropServices.InAttribute))` — 83.33% / 87.50% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.ItemNodeAdapter`

- [ ] **P2** `UseTypeAttribute(ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node)` — 90.91% / 75.00% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.ListItemNodeAdapter`

- [ ] **P0** `GetPreviousChild(ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node modreq(System.Runtime.InteropServices.InAttribute), ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node)` — 0.00% / 0.00% (7 uncovered lines, 12 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.ListNodeAdapter`

- [ ] **P3** `GetPreviousChild(ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node modreq(System.Runtime.InteropServices.InAttribute), ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node)` — 85.71% / 91.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.NamespaceNodeAdapter`

- [ ] **P0** `GetLocalName(ref Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node modreq(System.Runtime.InteropServices.InAttribute))` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.XmlConverter`

- [ ] **P2** `ReadEndElementAsync() [compiler-generated async/iterator body]` — 57.14% / 50.00% (3 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `ReadStartElementAsync() [compiler-generated async/iterator body]` — 57.14% / 50.00% (3 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `NumberToXmlString(ref Xtate.DataTypes.DataModelNumber)` — 75.00% / 64.71% (0 uncovered lines, 2 partial lines, 6 uncovered blocks)
- [ ] **P2** `NsNameToKey(string, string)` — 87.50% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P2** `DateTimeToXmlString(ref Xtate.DataTypes.DataModelDateTime)` — 83.33% / 72.73% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `KeyToPrefixOrDefault(string)` — 83.33% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P2** `GetBufferSizeForValue(ref Xtate.DataTypes.DataModelValue)` — 90.00% / 76.92% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `WriteValueToSpan() [compiler-generated local/lambda body]` — 83.33% / 76.92% (0 uncovered lines, 2 partial lines, 6 uncovered blocks)
- [ ] **P3** `ToString(ref Xtate.DataTypes.DataModelValue)` — 80.00% / 80.00% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P3** `WriteValueToSpan(ref Xtate.DataTypes.DataModelValue, ref System.Span<char>)` — 83.33% / 82.61% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P3** `LoadValue(System.Xml.XmlReader)` — 93.55% / 88.10% (2 uncovered lines, 5 uncovered blocks)
- [ ] **P3** `ToType(ref Xtate.DataTypes.DataModelValue, string)` — 91.67% / 90.48% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `LoadValueAsync() [compiler-generated async/iterator body]` — 93.55% / 91.53% (2 uncovered lines, 5 uncovered blocks)
- [ ] **P3** `WriteNode(System.Xml.XmlWriter, System.Xml.XPath.XPathNavigator)` — 93.75% / 95.65% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.XPathMetadata`

- [ ] **P3** `GetValue(Xtate.DataTypes.DataModelList, int, int)` — 88.89% / 90.91% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.XPathObject`

- [ ] **P1** `XmlString(Xtate.DataTypes.DataModelValue)` — 40.00% / 26.19% (11 uncovered lines, 4 partial lines, 31 uncovered blocks)
- [ ] **P2** `get_Type()` — 87.50% / 50.00% (0 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P2** `AsString()` — 75.00% / 53.85% (0 uncovered lines, 2 partial lines, 6 uncovered blocks)
- [ ] **P2** `AsBoolean()` — 87.50% / 61.54% (0 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P2** `AsInteger()` — 87.50% / 68.75% (0 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P2** `ToDataModelObject(System.Xml.XPath.XPathNodeIterator)` — 78.57% / 76.67% (3 uncovered lines, 7 uncovered blocks)
- [ ] **P3** `XPathObject(object)` — 88.89% / 91.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `ToObject(System.Xml.XPath.XPathNodeIterator)` — 96.43% / 92.86% (1 uncovered lines, 3 uncovered blocks)

### `Xtate.DataModel.XPath.Internal.XPathStripRootsIterator`

- [ ] **P3** `MoveNext()` — 94.44% / 94.74% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Services.XPathContentBodyEvaluator`

- [ ] **P0** `ParseToDataModel()` — 0.00% / 77.78% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.DataModel.XPath.Services.XPathDataModelHandler`

- [ ] **P0** `AddErrorMessage(object, string, System.Exception)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `ConvertToText(Xtate.DataTypes.DataModelValue)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `GetEvaluator(Xtate.StateMachine.IExternalDataExpression)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `GetEvaluator(Xtate.StateMachine.IForEach)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `Visit(ref Xtate.StateMachine.IScript)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.IAssign)` — 36.36% / 52.63% (6 uncovered lines, 1 partial lines, 9 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.IConditionExpression)` — 45.00% / 47.06% (11 uncovered lines, 9 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.ILocationExpression)` — 45.00% / 47.06% (11 uncovered lines, 9 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.IValueExpression)` — 45.00% / 47.06% (11 uncovered lines, 9 uncovered blocks)
- [ ] **P2** `CompileConditionExpression(ref Xtate.StateMachine.IConditionExpression)` — 61.54% / 64.00% (5 uncovered lines, 9 uncovered blocks)
- [ ] **P2** `CompileLocationExpression(ref Xtate.StateMachine.ILocationExpression)` — 61.54% / 64.00% (5 uncovered lines, 9 uncovered blocks)
- [ ] **P2** `CompileValueExpression(ref Xtate.StateMachine.IValueExpression)` — 72.73% / 72.73% (3 uncovered lines, 6 uncovered blocks)

### `Xtate.DataModel.XPath.Services.XPathEngine`

- [ ] **P2** `Assign() [compiler-generated async/iterator body]` — 62.86% / 59.46% (13 uncovered lines, 15 uncovered blocks)
- [ ] **P3** `GetVariable(string)` — 92.86% / 95.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `Assign(Xtate.DataModel.XPath.Internal.DataModelXPathNavigator, Xtate.DataModel.XPath.Internal.XPathAssignType, string, Xtate.DataTypes.IObject)` — 95.45% / 96.15% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Services.XPathExternalDataExpressionEvaluator`

- [ ] **P3** `ParseToDataModel() [compiler-generated async/iterator body]` — 88.89% / 96.30% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Services.XPathForEachEvaluator`

- [ ] **P3** `XPathForEachEvaluator(Xtate.StateMachine.IForEach)` — 85.71% / 92.31% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataModel.XPath.Services.XPathInlineContentEvaluator`

- [ ] **P0** `ParseToDataModel()` — 0.00% / 77.78% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelDateTime`

- [ ] **P0** `Equals(object)` — 0.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToBoolean(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToByte(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToChar(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToDecimal(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToDouble(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt16(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt32(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt64(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToSByte(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToSingle(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToType(System.Type, System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt16(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt32(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt64(System.IFormatProvider)` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)` — 0.00% / 0.00% (6 uncovered lines, 11 uncovered blocks)
- [ ] **P1** `CompareTo(object)` — 66.67% / 42.86% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P2** `System.IConvertible.GetTypeCode()` — 83.33% / 57.14% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `ToObject()` — 83.33% / 66.67% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `ToString(string, System.IFormatProvider)` — 83.33% / 72.73% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `ProcessParseData(ref Xtate.DataTypes.DataModelDateTime.ParseData, out Xtate.DataTypes.DataModelDateTime)` — 91.67% / 91.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList`

- [ ] **P0** `CanAdd()` — 0.00% / 0.00% (4 uncovered lines, 4 uncovered blocks)
- [ ] **P0** `CanClear()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `CanInsert(int)` — 0.00% / 0.00% (6 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `CanSetLength(int)` — 0.00% / 0.00% (4 uncovered lines, 4 uncovered blocks)
- [ ] **P0** `CanSetMetadata()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `ClearInternal(bool)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `CloneAsWritable()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `Contains(Xtate.DataTypes.DataModelValue)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `CopyTo(Xtate.DataTypes.DataModelValue[], int)` — 0.00% / 0.00% (9 uncovered lines, 23 uncovered blocks)
- [ ] **P0** `GetIndex(ref Xtate.DataTypes.DataModelValue)` — 0.00% / 0.00% (9 uncovered lines, 15 uncovered blocks)
- [ ] **P0** `GetMetadata(int)` — 0.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `IndexOf(Xtate.DataTypes.DataModelValue)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `Remove(Xtate.DataTypes.DataModelValue)` — 0.00% / 0.00% (4 uncovered lines, 7 uncovered blocks)
- [ ] **P0** `RemoveFirst(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `Slice(int, int)` — 0.00% / 0.00% (21 uncovered lines, 29 uncovered blocks)
- [ ] **P0** `System.Collections.Generic.ICollection<Xtate.DataTypes.DataModelValue>.get_IsReadOnly()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `ToString(string, System.IFormatProvider)` — 0.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToStringAsArray(System.IFormatProvider)` — 0.00% / 0.00% (21 uncovered lines, 22 uncovered blocks)
- [ ] **P2** `SetLength(int)` — 75.00% / 50.00% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `Add(string, Xtate.DataTypes.DataModelValue)` — 75.00% / 60.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P2** `CreateArgs(out Xtate.DataTypes.DataModelList.Args)` — 94.44% / 61.54% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `get_HasKeys()` — 64.71% / 77.78% (4 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P2** `RemoveFirst(string, bool)` — 69.23% / 66.67% (4 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `SetMetadata(int, Xtate.DataTypes.DataModelList)` — 66.67% / 66.67% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `TryFormatAsArray(System.Span<char>, out int, System.IFormatProvider)` — 70.00% / 73.08% (3 uncovered lines, 3 partial lines, 7 uncovered blocks)
- [ ] **P2** `FindNextKey(ref Xtate.DataTypes.DataModelList.Args, bool)` — 71.43% / 80.00% (3 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `PreviousEntry(ref int, out Xtate.DataTypes.DataModelList.Entry)` — 71.43% / 72.73% (3 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `InsertInternal(int, string, ref Xtate.DataTypes.DataModelValue, Xtate.DataTypes.DataModelAccess, Xtate.DataTypes.DataModelList, bool)` — 75.00% / 77.78% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `TryGet(int, out Xtate.DataTypes.DataModelList.Entry)` — 93.33% / 76.92% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `RemoveNext(ref Xtate.DataTypes.DataModelList.EntryByKeyEnumerator)` — 80.00% / 80.00% (3 uncovered lines, 3 uncovered blocks)
- [ ] **P3** `SetMetadata(Xtate.DataTypes.DataModelList, Xtate.DataTypes.DataModelAccess, bool)` — 80.00% / 87.50% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `AddItem(ref Xtate.DataTypes.DataModelList.Args, Xtate.DataTypes.DataModelAccess, bool)` — 81.82% / 87.50% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `ClearItems(Xtate.DataTypes.DataModelAccess, bool)` — 81.82% / 87.50% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `CheckAccess(ref Xtate.DataTypes.DataModelList.Args, int, int, Xtate.DataTypes.DataModelAccess, bool, out bool)` — 85.71% / 92.31% (3 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `SetLengthItems(int, Xtate.DataTypes.DataModelAccess, bool)` — 85.71% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `DeepCloneWithMap(Xtate.DataTypes.DataModelAccess, ref System.Collections.Generic.Dictionary<object, Xtate.DataTypes.DataModelList>)` — 88.57% / 94.29% (4 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `EnsureTypeAndCapacity(ref Xtate.DataTypes.DataModelList.Args, int)` — 88.89% / 90.91% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `InsertItem(ref Xtate.DataTypes.DataModelList.Args, Xtate.DataTypes.DataModelAccess, bool)` — 89.47% / 92.31% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `set_Access(Xtate.DataTypes.DataModelAccess)` — 92.31% / 90.24% (1 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P3** `NextEntry(ref int, out Xtate.DataTypes.DataModelList.Entry)` — 92.86% / 90.91% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `TryFormatAsObject(System.Span<char>, out int, System.IFormatProvider)` — 90.91% / 94.12% (0 uncovered lines, 2 partial lines, 2 uncovered blocks)
- [ ] **P3** `FindNextKey(ref Xtate.DataTypes.DataModelList.Args, bool, int)` — 94.12% / 94.44% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `OnChange(Xtate.DataTypes.DataModelList.ChangeAction, ref Xtate.DataTypes.DataModelList.Args)` — 94.74% / 94.74% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataTypes.DataModelList.AdapterBase`

- [ ] **P2** `ValueToKeyMetaValue(ref Xtate.DataTypes.DataModelList.Args, int)` — 60.00% / 66.67% (3 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.Dynamic1`

- [ ] **P0** `CreateMetaObject(System.Linq.Expressions.Expression)` — 0.00% / 0.00% (4 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `Dynamic1()` — 0.00% / 0.00% (2 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `Dynamic1(Xtate.DataTypes.DataModelList)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `TryConvert(System.Dynamic.ConvertBinder, out object)` — 0.00% / 0.00% (12 uncovered lines, 13 uncovered blocks)
- [ ] **P0** `TryGetIndex(System.Dynamic.GetIndexBinder, object[], out object)` — 0.00% / 0.00% (12 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `TryGetMember(System.Dynamic.GetMemberBinder, out object)` — 0.00% / 0.00% (4 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `TryInvokeMember() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 7 uncovered blocks)
- [ ] **P0** `TryInvokeMember(System.Dynamic.InvokeMemberBinder, object[], out object)` — 0.00% / 0.00% (50 uncovered lines, 65 uncovered blocks)
- [ ] **P0** `TrySetIndex(System.Dynamic.SetIndexBinder, object[], object)` — 0.00% / 0.00% (10 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `TrySetMember(System.Dynamic.SetMemberBinder, object)` — 0.00% / 0.00% (4 uncovered lines, 6 uncovered blocks)

### `Xtate.DataTypes.DataModelList.Entry`

- [ ] **P0** `Equals(object)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `Equals(Xtate.DataTypes.DataModelList.Entry)` — 0.00% / 0.00% (1 uncovered lines, 20 uncovered blocks)
- [ ] **P0** `GetHashCode()` — 0.00% / 0.00% (1 uncovered lines, 7 uncovered blocks)
- [ ] **P0** `op_Equality(Xtate.DataTypes.DataModelList.Entry, Xtate.DataTypes.DataModelList.Entry)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Inequality(Xtate.DataTypes.DataModelList.Entry, Xtate.DataTypes.DataModelList.Entry)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.EntryByKeyEnumerable`

- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.EntryByKeyEnumerator`

- [ ] **P0** `Reset()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerator.get_Current()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P1** `MoveNext()` — 43.48% / 50.00% (13 uncovered lines, 8 uncovered blocks)

### `Xtate.DataTypes.DataModelList.EntryEnumerable`

- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.EntryEnumerator`

- [ ] **P0** `Reset()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerator.get_Current()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyEnumerable`

- [ ] **P0** `GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyEnumerator`

- [ ] **P0** `Reset()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerator.get_Current()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyMetaValueAdapter`

- [ ] **P0** `CreateArray(ref Xtate.DataTypes.DataModelList.Args, int)` — 0.00% / 60.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `GetValueByIndex(ref Xtate.DataTypes.DataModelList.Args)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyValue`

- [ ] **P0** `Equals(object)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `Equals(Xtate.DataTypes.DataModelList.KeyValue)` — 0.00% / 0.00% (1 uncovered lines, 10 uncovered blocks)
- [ ] **P0** `GetHashCode()` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)
- [ ] **P0** `op_Equality(Xtate.DataTypes.DataModelList.KeyValue, Xtate.DataTypes.DataModelList.KeyValue)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Inequality(Xtate.DataTypes.DataModelList.KeyValue, Xtate.DataTypes.DataModelList.KeyValue)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyValueAdapter`

- [ ] **P0** `CreateArray(ref Xtate.DataTypes.DataModelList.Args, int)` — 0.00% / 60.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `GetAccessByIndex(ref Xtate.DataTypes.DataModelList.Args)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyValueByKeyEnumerable`

- [ ] **P0** `GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyValueByKeyEnumerator`

- [ ] **P0** `Reset()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerator.get_Current()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P1** `MoveNext()` — 41.67% / 39.13% (14 uncovered lines, 14 uncovered blocks)
- [ ] **P3** `KeyValueByKeyEnumerator(Xtate.DataTypes.DataModelList, string, bool)` — 88.89% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyValueEnumerable`

- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyValueEnumerator`

- [ ] **P0** `Reset()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerator.get_Current()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyValuePairEnumerable`

- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.KeyValuePairEnumerator`

- [ ] **P0** `Reset()` — 0.00% / 0.00% (4 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerator.get_Current()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.MetaValueAdapter`

- [ ] **P0** `CreateArray(ref Xtate.DataTypes.DataModelList.Args, int)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `GetValueByIndex(ref Xtate.DataTypes.DataModelList.Args)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `ReadToArgsByIndex(ref Xtate.DataTypes.DataModelList.Args)` — 0.00% / 0.00% (4 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `EnsureArray(ref Xtate.DataTypes.DataModelList.Args, int)` — 58.33% / 75.00% (5 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.ValueAdapter`

- [ ] **P0** `CreateArray(ref Xtate.DataTypes.DataModelList.Args, int)` — 0.00% / 60.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `GetAccessByIndex(ref Xtate.DataTypes.DataModelList.Args)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.ValueByKeyEnumerable`

- [ ] **P0** `GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.ValueByKeyEnumerator`

- [ ] **P0** `Reset()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerator.get_Current()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P1** `MoveNext()` — 58.82% / 45.45% (7 uncovered lines, 12 uncovered blocks)

### `Xtate.DataTypes.DataModelList.ValueEnumerable`

- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelList.ValueEnumerator`

- [ ] **P0** `Reset()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.DataTypes.DataModelNumber`

- [ ] **P0** `Equals(object)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `op_Equality(Xtate.DataTypes.DataModelNumber, Xtate.DataTypes.DataModelNumber)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Explicit(Xtate.DataTypes.DataModelNumber)` — 0.00% / 0.00% (3 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `op_GreaterThanOrEqual(Xtate.DataTypes.DataModelNumber, Xtate.DataTypes.DataModelNumber)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Inequality(Xtate.DataTypes.DataModelNumber, Xtate.DataTypes.DataModelNumber)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_LessThanOrEqual(Xtate.DataTypes.DataModelNumber, Xtate.DataTypes.DataModelNumber)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToByte(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToChar(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToDateTime(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt16(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt32(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt64(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToSByte(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToSingle(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt16(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt32(System.IFormatProvider)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt64(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `ToString(string)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P1** `ToInt64()` — 57.14% / 25.00% (0 uncovered lines, 3 partial lines, 9 uncovered blocks)
- [ ] **P1** `ToInt32()` — 57.14% / 30.77% (0 uncovered lines, 3 partial lines, 9 uncovered blocks)
- [ ] **P1** `System.IConvertible.ToBoolean(System.IFormatProvider)` — 57.14% / 35.71% (0 uncovered lines, 3 partial lines, 9 uncovered blocks)
- [ ] **P1** `System.IConvertible.ToType(System.Type, System.IFormatProvider)` — 57.14% / 35.71% (0 uncovered lines, 3 partial lines, 9 uncovered blocks)
- [ ] **P1** `TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)` — 62.50% / 35.71% (0 uncovered lines, 3 partial lines, 9 uncovered blocks)
- [ ] **P2** `ToDecimal()` — 71.43% / 50.00% (0 uncovered lines, 2 partial lines, 6 uncovered blocks)
- [ ] **P2** `ToDouble()` — 71.43% / 50.00% (0 uncovered lines, 2 partial lines, 6 uncovered blocks)
- [ ] **P2** `System.IConvertible.GetTypeCode()` — 75.00% / 55.56% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P2** `NumberCompare() [compiler-generated local/lambda body]` — 66.67% / 62.50% (4 uncovered lines, 2 partial lines, 9 uncovered blocks)
- [ ] **P2** `WriteToSize()` — 85.71% / 66.67% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `CompareTo(object)` — 83.33% / 71.43% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P2** `ToObject()` — 87.50% / 76.92% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `ToString(string, System.IFormatProvider)` — 87.50% / 78.57% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `WriteTo(System.Span<byte>)` — 92.31% / 82.35% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P3** `GetHashCode()` — 90.00% / 83.33% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `ReadFrom(System.ReadOnlySpan<byte>)` — 91.67% / 90.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `NumberCompare(ref Xtate.DataTypes.DataModelNumber, ref Xtate.DataTypes.DataModelNumber)` — 91.67% / 95.89% (1 uncovered lines, 1 partial lines, 3 uncovered blocks)

### `Xtate.DataTypes.DataModelValue`

- [ ] **P0** `AsBoolean()` — 0.00% / 30.00% (0 uncovered lines, 5 partial lines, 7 uncovered blocks)
- [ ] **P0** `AsBooleanOrDefault()` — 0.00% / 75.00% (0 uncovered lines, 5 partial lines, 2 uncovered blocks)
- [ ] **P0** `AsNullableBoolean()` — 0.00% / 0.00% (7 uncovered lines, 13 uncovered blocks)
- [ ] **P0** `AsNullableDateTime()` — 0.00% / 0.00% (7 uncovered lines, 12 uncovered blocks)
- [ ] **P0** `AsNullableNumber()` — 0.00% / 0.00% (7 uncovered lines, 12 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<bool>)` — 0.00% / 0.00% (4 uncovered lines, 11 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<double>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<int>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<long>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<System.DateTime>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<System.DateTimeOffset>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<System.Decimal>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<Xtate.DataTypes.DataModelDateTime>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `DataModelValue(System.Nullable<Xtate.DataTypes.DataModelNumber>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `Equals(object)` — 0.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `FromBoolean(System.Nullable<bool>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDataModelDateTime(System.Nullable<Xtate.DataTypes.DataModelDateTime>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDataModelDateTime(Xtate.DataTypes.DataModelDateTime)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDataModelNumber(System.Nullable<Xtate.DataTypes.DataModelNumber>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDataModelNumber(Xtate.DataTypes.DataModelNumber)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDateTime(System.DateTime)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDateTime(System.Nullable<System.DateTime>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDateTimeOffset(System.Nullable<System.DateTimeOffset>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDecimal(decimal)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDecimal(System.Nullable<System.Decimal>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromDouble(System.Nullable<double>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromInt32(int)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromInt32(System.Nullable<int>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromInt64(long)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `FromInt64(System.Nullable<long>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Explicit(Xtate.DataTypes.DataModelValue)` — 0.00% / 0.00% (13 uncovered lines, 59 uncovered blocks)
- [ ] **P0** `op_Implicit(decimal)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<bool>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<double>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<int>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<long>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<System.DateTime>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<System.DateTimeOffset>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<System.Decimal>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<Xtate.DataTypes.DataModelDateTime>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `op_Implicit(System.Nullable<Xtate.DataTypes.DataModelNumber>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `System.IConvertible.GetTypeCode()` — 0.00% / 0.00% (11 uncovered lines, 16 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToBoolean(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 13 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToByte(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToChar(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToDateTime(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToDecimal(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToDouble(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt16(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt32(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToInt64(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToSByte(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToSingle(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToType(System.Type, System.IFormatProvider)` — 0.00% / 0.00% (15 uncovered lines, 32 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt16(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt32(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.IConvertible.ToUInt64(System.IFormatProvider)` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)` — 0.00% / 0.00% (6 uncovered lines, 4 uncovered blocks)
- [ ] **P1** `AsDateTime()` — 66.67% / 33.33% (0 uncovered lines, 2 partial lines, 6 uncovered blocks)
- [ ] **P1** `FromObjectWithMap(object, ref System.Collections.Generic.Dictionary<object, Xtate.DataTypes.DataModelList>)` — 52.00% / 36.59% (0 uncovered lines, 12 partial lines, 26 uncovered blocks)
- [ ] **P1** `AsDateTimeOrDefault()` — 57.14% / 44.44% (0 uncovered lines, 3 partial lines, 5 uncovered blocks)
- [ ] **P2** `AsIObject()` — 80.00% / 50.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P2** `CreateDataModelObject(System.Collections.Generic.IDictionary<string, string>, ref System.Collections.Generic.Dictionary<object, Xtate.DataTypes.DataModelList>)` — 53.85% / 68.18% (5 uncovered lines, 1 partial lines, 7 uncovered blocks)
- [ ] **P2** `TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)` — 72.73% / 62.50% (0 uncovered lines, 3 partial lines, 9 uncovered blocks)
- [ ] **P2** `TryFromAnonymousType(object, ref System.Collections.Generic.Dictionary<object, Xtate.DataTypes.DataModelList>, out Xtate.DataTypes.DataModelValue)` — 63.64% / 87.50% (6 uncovered lines, 2 partial lines, 5 uncovered blocks)
- [ ] **P2** `FromUnknownObjectWithMap(object, ref System.Collections.Generic.Dictionary<object, Xtate.DataTypes.DataModelList>)` — 76.92% / 69.57% (0 uncovered lines, 3 partial lines, 7 uncovered blocks)
- [ ] **P2** `ToObject()` — 91.67% / 77.27% (0 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P2** `ToString(string, System.IFormatProvider)` — 91.67% / 79.17% (0 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P3** `get_Type()` — 91.67% / 81.25% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `CreateDataModelList(System.Collections.IEnumerable, ref System.Collections.Generic.Dictionary<object, Xtate.DataTypes.DataModelList>)` — 84.62% / 90.48% (2 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.DataModelValue.DateTimeValue`

- [ ] **P0** `Equals(object)` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P1** `GetDateTimeValue(Xtate.DataTypes.DataModelDateTime, out long)` — 66.67% / 45.45% (0 uncovered lines, 2 partial lines, 6 uncovered blocks)

### `Xtate.DataTypes.DataModelValue.Marker`

- [ ] **P0** `Equals(object)` — 0.00% / 0.00% (1 uncovered lines, 8 uncovered blocks)
- [ ] **P0** `Equals(Xtate.DataTypes.DataModelValue.Marker)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.DataTypes.DataModelValue.NumberValue`

- [ ] **P0** `Equals(long, object, long)` — 0.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `Equals(object)` — 0.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `GetHashCode()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `GetNumberValue(Xtate.DataTypes.DataModelNumber, out long)` — 75.00% / 64.71% (0 uncovered lines, 2 partial lines, 6 uncovered blocks)
- [ ] **P3** `GetDataModelNumber(long)` — 87.50% / 85.71% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.DataTypes.Extensions.IConvertibleExtensions`

- [ ] **P0** `GetTypeCode<T>(T)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `ToByte<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToChar<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToDecimal<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToDouble<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToInt16<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToInt32<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToInt64<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToSByte<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToSingle<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToUInt16<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToUInt32<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ToUInt64<T>(T, System.IFormatProvider)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.DataTypes.Internal.MetaObjectBase`

- [ ] **P0** `BindInvokeMember() [compiler-generated local/lambda body]` — 0.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `BindGetIndex(System.Dynamic.GetIndexBinder, System.Dynamic.DynamicMetaObject[])` — 86.96% / 93.33% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `BindSetIndex(System.Dynamic.SetIndexBinder, System.Dynamic.DynamicMetaObject[], System.Dynamic.DynamicMetaObject)` — 87.50% / 94.12% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `BindInvokeMember(System.Dynamic.InvokeMemberBinder, System.Dynamic.DynamicMetaObject[])` — 94.23% / 97.73% (3 uncovered lines, 2 uncovered blocks)

### `Xtate.DataTypes.LazyValue`

- [ ] **P2** `Xtate.DataTypes.ILazyValue.get_Value()` — 64.71% / 58.33% (6 uncovered lines, 5 uncovered blocks)

### `Xtate.DateTimeExtensions`

- [ ] **P3** `get_UniqueUtcNow()` — 90.00% / 91.67% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.Disposer`

- [ ] **P2** `Dispose<T>(T)` — 63.64% / 73.33% (3 uncovered lines, 1 partial lines, 4 uncovered blocks)

### `Xtate.DisposingToken`

- [ ] **P2** `DisposeAsyncCore() [compiler-generated async/iterator body]` — 76.92% / 92.31% (3 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `Dispose(bool)` — 80.00% / 90.91% (3 uncovered lines, 1 uncovered blocks)

### `Xtate.ExtCollection<TValue1, TValue2>`

- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `Remove(TValue1, TValue2)` — 78.57% / 82.76% (6 uncovered lines, 5 uncovered blocks)
- [ ] **P3** `TryTake(out TValue1, out TValue2)` — 81.25% / 85.71% (2 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `GetEnumerator() [compiler-generated async/iterator body]` — 88.89% / 96.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `TryTake() [compiler-generated local/lambda body]` — 88.89% / 92.86% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.ExtDictionary<TKey, TValue>`

- [ ] **P0** `get_Count()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `get_Item(TKey)` — 0.00% / 40.00% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P0** `get_Keys()` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `get_Values()` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `GetEnumerator()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `GetKeyNotFoundException(TKey)` — 0.00% / 0.00% (1 uncovered lines, 8 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `UpdateOrRemove<TArg>(TKey, System.Func<TKey, TValue, TArg, bool>, System.Func<TKey, TValue, TArg, TValue>, TArg)` — 77.78% / 76.47% (4 uncovered lines, 4 uncovered blocks)
- [ ] **P3** `TryGetValueAsync(TKey)` — 80.00% / 82.14% (4 uncovered lines, 5 uncovered blocks)
- [ ] **P3** `TryRemovePair(TKey, TValue)` — 87.50% / 88.89% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `TryUpdate(TKey, TValue, TValue)` — 87.50% / 87.50% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `GetOrAdd<TArg>(TKey, System.Func<TKey, TArg, TValue>, TArg)` — 91.67% / 90.91% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `TryTake(out TKey, out TValue)` — 93.75% / 93.75% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.ExternalServices.ExternalServiceBase`

- [ ] **P0** `Dispatch(Xtate.DataModel.IIncomingEvent, System.Threading.CancellationToken)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P1** `set_TaskMonitorBase(Xtate.TaskMonitor.ITaskMonitor)` — 42.86% / 50.00% (3 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P3** `set_DisposeTokenBase(Xtate.IoC.Tools.DisposeToken)` — 85.71% / 90.91% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.ExternalServices.ExternalServiceProviderBase<TService>`

- [ ] **P0** `ExternalServiceProviderBase(string, string)` — 0.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.ExternalServices.HttpClient.Internal.HttpClientFormUrlEncodedHandler`

- [ ] **P3** `TryParseResponseAsync() [compiler-generated async/iterator body]` — 92.59% / 97.30% (2 uncovered lines, 1 uncovered blocks)

### `Xtate.ExternalServices.HttpClient.Internal.HttpClientMimeTypeHandler`

- [ ] **P2** `AppendAcceptHeader(System.Net.WebRequest, string)` — 88.89% / 72.73% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `AppendAcceptHeader(ref string, string)` — 86.49% / 89.36% (3 uncovered lines, 2 partial lines, 5 uncovered blocks)

### `Xtate.ExternalServices.HttpClient.Internal.HttpClientXmlHandler`

- [ ] **P3** `TryParseResponseAsync() [compiler-generated async/iterator body]` — 90.91% / 87.50% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.ExternalServices.HttpClient.Services.HttpClientService`

- [ ] **P1** `CreateCookie(ref Xtate.DataTypes.DataModelValue)` — 21.43% / 94.59% (0 uncovered lines, 11 partial lines, 2 uncovered blocks)
- [ ] **P1** `GetResponseCookieList(Xtate.ExternalServices.HttpClient.Services.HttpClientService.Response)` — 43.48% / 95.92% (0 uncovered lines, 13 partial lines, 2 uncovered blocks)
- [ ] **P2** `GetResponseHeaderList(Xtate.ExternalServices.HttpClient.Services.HttpClientService.Response)` — 70.00% / 88.24% (4 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P3** `GetResponse() [compiler-generated async/iterator body]` — 84.62% / 82.35% (2 uncovered lines, 3 uncovered blocks)

### `Xtate.ExternalServices.SmtpClient.Services.SmtpClientService`

- [ ] **P3** `Execute() [compiler-generated async/iterator body]` — 93.33% / 97.56% (0 uncovered lines, 2 partial lines, 2 uncovered blocks)

### `Xtate.Http.JsonHttpContent`

- [ ] **P0** `SerializeToStream(System.IO.Stream, System.Net.TransportContext, System.Threading.CancellationToken)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `SerializeToStreamAsync(System.IO.Stream, System.Net.TransportContext)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Http.Services.HttpClientFactory`

- [ ] **P2** `TryDisposeNextEntry(Xtate.Http.Services.HttpClientFactory.HandlerEntry)` — 64.71% / 58.82% (5 uncovered lines, 1 partial lines, 7 uncovered blocks)
- [ ] **P3** `Dispose(bool)` — 90.91% / 90.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `GetHandler()` — 93.75% / 95.24% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.Http.Services.HttpClientFactory.HandlerEntry`

- [ ] **P0** `get_CanDispose()` — 0.00% / 71.43% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `get_IsExpired()` — 0.00% / 71.43% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Http.XmlHttpContent`

- [ ] **P0** `SerializeToStream(System.IO.Stream, System.Net.TransportContext, System.Threading.CancellationToken)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `SerializeToStreamAsync(System.IO.Stream, System.Net.TransportContext)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Interpreter.Internal.InvokeIdSet`

- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Interpreter.Model.EventDescriptorNode`

- [ ] **P0** `ToString()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Interpreter.Model.IdentifierNode`

- [ ] **P0** `ToString()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Interpreter.Model.InitialNode`

- [ ] **P0** `Xtate.StateMachine.IInitial.get_Transition()` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Interpreter.Model.InvokeNode`

- [ ] **P0** `Xtate.StateMachine.IInvoke.get_Finalize()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Interpreter.Model.StateEntityNode`

- [ ] **P0** `get_DataModel()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_HistoryStates()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_Id()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_Invoke()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_IsAtomicState()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_OnEntry()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_OnExit()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_States()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_Transitions()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `GetNotSupportedException()` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P3** `Register(System.Collections.Immutable.ImmutableArray<Xtate.Interpreter.Model.HistoryNode>)` — 88.89% / 92.86% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `Register(System.Collections.Immutable.ImmutableArray<Xtate.Interpreter.Model.StateEntityNode>)` — 88.89% / 92.86% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `Register(System.Collections.Immutable.ImmutableArray<Xtate.Interpreter.Model.TransitionNode>)` — 88.89% / 92.86% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Interpreter.Model.StateEntityNode.DocumentOrderComparer`

- [ ] **P2** `InternalCompare(Xtate.Interpreter.Model.StateEntityNode, Xtate.Interpreter.Model.StateEntityNode)` — 66.67% / 81.82% (0 uncovered lines, 2 partial lines, 2 uncovered blocks)

### `Xtate.Interpreter.Model.StateMachineNode`

- [ ] **P0** `Xtate.StateMachine.IStateMachine.get_DataModel()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Interpreter.Services.AssemblyTypeInfo`

- [ ] **P1** `AssemblyTypeInfo(System.Type)` — 25.00% / 76.47% (0 uncovered lines, 3 partial lines, 4 uncovered blocks)

### `Xtate.Interpreter.Services.EventController`

- [ ] **P3** `Send() [compiler-generated async/iterator body]` — 91.67% / 80.65% (0 uncovered lines, 1 partial lines, 6 uncovered blocks)
- [ ] **P3** `Cancel() [compiler-generated async/iterator body]` — 88.89% / 83.33% (0 uncovered lines, 1 partial lines, 4 uncovered blocks)

### `Xtate.Interpreter.Services.ExceptionEntityParser`

- [ ] **P0** `EnumerateProperties() [compiler-generated async/iterator body]` — 0.00% / 0.00% (3 uncovered lines, 3 uncovered blocks)

### `Xtate.Interpreter.Services.InterpreterModelBuilder`

- [ ] **P3** `CounterAfter(System.ValueTuple<int, bool>)` — 80.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `Visit(ref Xtate.StateMachine.IExternalScriptExpression)` — 82.35% / 86.36% (3 uncovered lines, 3 uncovered blocks)
- [ ] **P3** `GetEntityMap(System.Collections.Generic.List<Xtate.StateMachine.IEntity>, int)` — 90.91% / 89.47% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `Build(ref Xtate.StateMachine.StateMachineEntity)` — 93.75% / 96.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `BuildModel() [compiler-generated async/iterator body]` — 96.77% / 97.78% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.Interpreter.Services.InvokeController`

- [ ] **P2** `Forward() [compiler-generated async/iterator body]` — 90.91% / 78.57% (0 uncovered lines, 1 partial lines, 6 uncovered blocks)

### `Xtate.Interpreter.Services.StateMachineContext`

- [ ] **P3** `GetIoProcessors()` — 92.86% / 96.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Interpreter.Services.StateMachineInterpreter`

- [ ] **P0** `ApplyFinalize(Xtate.Interpreter.Model.InvokeNode)` — 0.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ForwardEvent() [compiler-generated async/iterator body]` — 0.00% / 0.00% (11 uncovered lines, 17 uncovered blocks)
- [ ] **P0** `HandleMainLoopException(System.Exception)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `IsError(System.Exception)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P0** `LogError() [compiler-generated async/iterator body]` — 0.00% / 0.00% (16 uncovered lines, 29 uncovered blocks)
- [ ] **P0** `TriggerDestroySignal()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P1** `Error() [compiler-generated async/iterator body]` — 40.48% / 51.02% (17 uncovered lines, 8 partial lines, 24 uncovered blocks)
- [ ] **P1** `EvaluateDoneData() [compiler-generated async/iterator body]` — 44.44% / 42.86% (5 uncovered lines, 8 uncovered blocks)
- [ ] **P1** `GetEffectiveTargetStates(Xtate.Interpreter.Model.TransitionNode)` — 52.38% / 43.33% (10 uncovered lines, 17 uncovered blocks)
- [ ] **P1** `AddDescendantStatesToEnter(Xtate.Interpreter.Model.StateEntityNode, System.Collections.Generic.List<Xtate.Interpreter.Model.StateEntityNode>, System.Collections.Generic.List<Xtate.Interpreter.Model.CompoundNode>, System.Collections.Generic.Dictionary<Xtate.StateMachine.IIdentifier, System.Collections.Immutable.ImmutableArray<Xtate.DataModel.IExecEvaluator>>)` — 53.70% / 45.16% (24 uncovered lines, 1 partial lines, 51 uncovered blocks)
- [ ] **P2** `GetTransitionDomain(Xtate.Interpreter.Model.TransitionNode)` — 50.00% / 50.00% (4 uncovered lines, 1 partial lines, 11 uncovered blocks)
- [ ] **P2** `IsInFinalState(Xtate.Interpreter.Model.StateEntityNode)` — 55.56% / 62.50% (4 uncovered lines, 6 uncovered blocks)
- [ ] **P2** `ExitInterpreter() [compiler-generated async/iterator body]` — 57.89% / 72.34% (6 uncovered lines, 2 partial lines, 13 uncovered blocks)
- [ ] **P2** `InitializeData() [compiler-generated async/iterator body]` — 68.75% / 60.00% (4 uncovered lines, 1 partial lines, 14 uncovered blocks)
- [ ] **P2** `ThrowIfDestroying()` — 60.00% / 66.67% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `ExecuteGlobalScript() [compiler-generated async/iterator body]` — 66.67% / 63.16% (4 uncovered lines, 7 uncovered blocks)
- [ ] **P2** `ExecuteTransitionContent() [compiler-generated async/iterator body]` — 77.78% / 66.67% (6 uncovered lines, 28 uncovered blocks)
- [ ] **P2** `FindLcca(Xtate.Interpreter.Model.StateEntityNode, System.Collections.Generic.List<Xtate.Interpreter.Model.StateEntityNode>)` — 69.23% / 83.33% (3 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `StartInvoke() [compiler-generated async/iterator body]` — 69.23% / 72.00% (4 uncovered lines, 7 uncovered blocks)
- [ ] **P2** `RemoveConflictingTransitions(System.Collections.Generic.List<Xtate.Interpreter.Model.TransitionNode>)` — 71.05% / 73.02% (10 uncovered lines, 1 partial lines, 17 uncovered blocks)
- [ ] **P2** `ProcessUnhandledError(Xtate.DataModel.IIncomingEvent)` — 80.00% / 73.33% (2 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `MainEventLoopIteration() [compiler-generated async/iterator body]` — 77.78% / 93.33% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `Interpret() [compiler-generated async/iterator body]` — 78.95% / 90.70% (2 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P3** `AddAncestorStatesToEnter(Xtate.Interpreter.Model.StateEntityNode, Xtate.Interpreter.Model.StateEntityNode, System.Collections.Generic.List<Xtate.Interpreter.Model.StateEntityNode>, System.Collections.Generic.List<Xtate.Interpreter.Model.CompoundNode>, System.Collections.Generic.Dictionary<Xtate.StateMachine.IIdentifier, System.Collections.Immutable.ImmutableArray<Xtate.DataModel.IExecEvaluator>>)` — 80.00% / 85.71% (3 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P3** `SelectInternalEventTransitions() [compiler-generated async/iterator body]` — 92.86% / 85.00% (0 uncovered lines, 1 partial lines, 6 uncovered blocks)
- [ ] **P3** `ExternalEventTransitions() [compiler-generated async/iterator body]` — 86.96% / 92.06% (3 uncovered lines, 5 uncovered blocks)
- [ ] **P3** `EnterStates() [compiler-generated async/iterator body]` — 87.04% / 90.55% (6 uncovered lines, 1 partial lines, 12 uncovered blocks)
- [ ] **P3** `GetProperAncestors(Xtate.Interpreter.Model.StateEntityNode, Xtate.Interpreter.Model.StateEntityNode)` — 90.91% / 93.75% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `ReadExternalEventFiltered() [compiler-generated async/iterator body]` — 91.67% / 93.33% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `InitializeDataModels() [compiler-generated async/iterator body]` — 92.31% / 96.88% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `WaitForExternalEvent() [compiler-generated async/iterator body]` — 92.31% / 96.97% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.Interpreter.Services.StateMachineInterpreter.LiveLockDetector`

- [ ] **P3** `IsLiveLockDetected(int)` — 90.00% / 91.67% (2 uncovered lines, 1 uncovered blocks)

### `Xtate.Interpreter.StateMachineDestroyedException`

- [ ] **P0** `StateMachineDestroyedException()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoBoundTask.Services.IoBoundTaskScheduler`

- [ ] **P0** `GetScheduledTasks()` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `PublishSchedulerException(System.Exception)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P1** `WorkerRunQueuedTasks()` — 35.00% / 47.06% (13 uncovered lines, 9 uncovered blocks)
- [ ] **P2** `RegisterStartNewWorker()` — 57.14% / 71.43% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `set_KeepAliveThreadTimeout(System.TimeSpan)` — 66.67% / 90.91% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `WorkerThread()` — 73.33% / 77.42% (8 uncovered lines, 7 uncovered blocks)
- [ ] **P3** `QueueTask(System.Threading.Tasks.Task)` — 81.82% / 90.91% (4 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.AncestorTracker.Services.AncestorTracker`

- [ ] **P3** `CaptureAncestor<T>(Xtate.IoC.AncestorTracker.Internal.IAncestorConsumer<T>)` — 93.33% / 94.44% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.IoC.ServiceArray.Internal.ReadOnlyList<T>`

- [ ] **P0** `GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `System.Collections.Generic.IList<T>.get_Item(int)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `ReadOnlyList(System.Collections.Immutable.ImmutableArray<T>)` — 50.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.IoC.Tools.DisposeToken`

- [ ] **P0** `Equals(object)` — 0.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ThrowIfCancellationRequested()` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.IoC.Tools.Services.SafeFactory<T>`

- [ ] **P2** `Constructor() [compiler-generated async/iterator body]` — 57.14% / 60.00% (3 uncovered lines, 4 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync.<>c__DisplayClass10_0<T, TArg, TNewArg1, TNewArg2, TNewArg3>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync.<>c__DisplayClass11_0<T, TArg, TNewArg1, TNewArg2, TNewArg3, TNewArg4>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync.<>c__DisplayClass12_0<T, TArg, TNewArg1, TNewArg2, TNewArg3, TNewArg4>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync.<>c__DisplayClass3_0<T, TArg>`

- [ ] **P0** `UseArgFactory() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync.<>c__DisplayClass4_0<T, TArg>`

- [ ] **P0** `UseArgFactory() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync.<>c__DisplayClass7_0<T, TArg, TNewArg1, TNewArg2>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync.<>c__DisplayClass8_0<T, TArg, TNewArg1, TNewArg2>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync.<>c__DisplayClass9_0<T, TArg, TNewArg1, TNewArg2, TNewArg3>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorSync.<>c__DisplayClass2_0<T, TArg>`

- [ ] **P0** `UseArgValue() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorSync.<>c__DisplayClass3_0<T, TArg>`

- [ ] **P0** `UseArgFactory() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorSync.<>c__DisplayClass5_0<T, TArg, TNewArg1, TNewArg2>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorSync.<>c__DisplayClass6_0<T, TArg, TNewArg1, TNewArg2, TNewArg3>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.ServiceSelectorSync.<>c__DisplayClass7_0<T, TArg, TNewArg1, TNewArg2, TNewArg3, TNewArg4>`

- [ ] **P0** `TransformArgs() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.IoC.TransformArgs.Internal.TransformArgs.<>c__DisplayClass3_0<T, TArg, TNewArg, TAncestor>`

- [ ] **P0** `IfAncestor() [compiler-generated local/lambda body]` — 0.00% / 0.00% (3 uncovered lines, 8 uncovered blocks)

### `Xtate.IoProcessors.Http.Internal.CounterStream`

- [ ] **P0** `PostRead(int)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `PostWrite(int)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `PreRead(ref int)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `PreWrite(int)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `Read(System.Span<byte>)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `ReadAsync() [compiler-generated async/iterator body]` — 0.00% / 0.00% (1 uncovered lines, 7 uncovered blocks)
- [ ] **P0** `Write(System.ReadOnlySpan<byte>)` — 0.00% / 0.00% (5 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `WriteAsync() [compiler-generated async/iterator body]` — 0.00% / 0.00% (5 uncovered lines, 10 uncovered blocks)

### `Xtate.IoProcessors.Http.Internal.QueryStringHelper`

- [ ] **P3** `ParseQuery(string)` — 93.88% / 97.83% (3 uncovered lines, 1 uncovered blocks)

### `Xtate.IoProcessors.Http.Services.HttpController`

- [ ] **P0** `GetEventNameAndData() [compiler-generated async/iterator body]` — 0.00% / 0.00% (47 uncovered lines, 100 uncovered blocks)
- [ ] **P0** `GetEventNameAndData() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `HttpController()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `ReceiveAndProcessEvent() [compiler-generated async/iterator body]` — 0.00% / 0.00% (36 uncovered lines, 59 uncovered blocks)
- [ ] **P2** `GetSenderTarget(Xtate.StateMachine.ServiceId)` — 83.33% / 71.43% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `GetContent(Xtate.StateMachineHost.IRouterEvent, out bool)` — 93.75% / 86.96% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P3** `IsStringDictionary(Xtate.DataTypes.DataModelList)` — 88.89% / 90.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `GetParameters() [compiler-generated async/iterator body]` — 92.31% / 95.65% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `SendEvent() [compiler-generated async/iterator body]` — 96.77% / 98.31% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.IoProcessors.Http.Services.HttpIoProcessorHost`

- [ ] **P0** `StartListener()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `StopListener()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P2** `ProtectedBackgroundProcess() [compiler-generated async/iterator body]` — 54.55% / 52.63% (5 uncovered lines, 9 uncovered blocks)

### `Xtate.IoProcessors.IoProcessorHostBase`

- [ ] **P2** `Start() [compiler-generated async/iterator body]` — 75.00% / 76.19% (3 uncovered lines, 5 uncovered blocks)

### `Xtate.IoProcessors.NamedPipe.NamedPipeResponseMessage`

- [ ] **P3** `NamedPipeResponseMessage(ref Xtate.Persistence.Services.Bucket)` — 88.89% / 96.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.IoProcessors.NamedPipe.Services.NamedPipeController`

- [ ] **P0** `HostEquals(Xtate.FullUri, string)` — 0.00% / 91.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P1** `ReadAtLeast() [compiler-generated async/iterator body]` — 44.83% / 47.83% (16 uncovered lines, 12 uncovered blocks)
- [ ] **P3** `SendEvent() [compiler-generated async/iterator body]` — 80.00% / 83.05% (3 uncovered lines, 1 partial lines, 10 uncovered blocks)
- [ ] **P3** `ReceiveAndProcessEvent() [compiler-generated async/iterator body]` — 83.33% / 93.33% (1 uncovered lines, 2 partial lines, 3 uncovered blocks)
- [ ] **P3** `TryParseTarget(Xtate.FullUri, out string, out string, out Xtate.StateMachine.ServiceId)` — 94.74% / 95.83% (0 uncovered lines, 2 partial lines, 3 uncovered blocks)

### `Xtate.IoProcessors.NamedPipe.Services.NamedPipeController<T>`

- [ ] **P2** `ReceiveMessage() [compiler-generated async/iterator body]` — 65.38% / 73.08% (6 uncovered lines, 3 partial lines, 14 uncovered blocks)
- [ ] **P3** `SendMessage() [compiler-generated async/iterator body]` — 84.21% / 83.87% (2 uncovered lines, 1 partial lines, 5 uncovered blocks)

### `Xtate.IoProcessors.NamedPipe.Services.NamedPipeIoProcessor`

- [ ] **P0** `CreateExternalServiceTarget(Xtate.StateMachine.InvokeId)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P3** `Dispatch(Xtate.StateMachineHost.IRouterEvent, System.Threading.CancellationToken)` — 84.62% / 83.33% (1 uncovered lines, 1 partial lines, 4 uncovered blocks)

### `Xtate.IoProcessors.NamedPipe.Services.NamedPipeIoProcessorHost`

- [ ] **P0** `ProcessEvent(Xtate.IoProcessors.NamedPipe.NamedPipeEventMessage)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `ProtectedBackgroundProcess() [compiler-generated async/iterator body]` — 66.67% / 50.00% (3 uncovered lines, 8 uncovered blocks)

### `Xtate.IoProcessors.ResilientIoProcessorHostBase`

- [ ] **P3** `BackgroundProcess() [compiler-generated async/iterator body]` — 88.46% / 89.74% (3 uncovered lines, 4 uncovered blocks)

### `Xtate.LazyTask<T>`

- [ ] **P2** `Execute() [compiler-generated async/iterator body]` — 79.17% / 86.36% (5 uncovered lines, 3 uncovered blocks)
- [ ] **P3** `get_Task()` — 89.47% / 90.48% (2 uncovered lines, 2 uncovered blocks)

### `Xtate.Logging.Services.Logger<TSource>`

- [ ] **P3** `EnumerateParameters() [compiler-generated async/iterator body]` — 80.65% / 84.00% (6 uncovered lines, 8 uncovered blocks)

### `Xtate.Logging.Services.TraceLogProvider`

- [ ] **P2** `GetTraceEventType(Xtate.Logging.Level)` — 90.00% / 77.78% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Persistence.Extensions.BucketExtensions`

- [ ] **P3** `SetDataModelValue(ref Xtate.Persistence.Services.Bucket, Xtate.Persistence.Services.DataModelReferenceTracker, ref Xtate.DataTypes.DataModelValue)` — 95.00% / 91.67% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `GetDataModelValue(ref Xtate.Persistence.Services.Bucket, Xtate.Persistence.Services.DataModelReferenceTracker, ref Xtate.DataTypes.DataModelValue)` — 92.31% / 93.75% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Persistence.PersistedRouterEvent`

- [ ] **P3** `PersistedRouterEvent(ref Xtate.Persistence.Services.Bucket)` — 94.44% / 94.87% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.Bucket`

- [ ] **P3** `Remove<TKey>(TKey)` — 80.00% / 84.62% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `RemoveSubtree<TKey>(TKey)` — 80.00% / 84.62% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `Add<TKey, TValue>(TKey, TValue)` — 83.33% / 85.71% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P3** `TryGet<TKey>(TKey, out System.ReadOnlyMemory<byte>)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `Nested<TKey>(TKey)` — 85.71% / 85.71% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `Add<TKey>(TKey, System.ReadOnlySpan<byte>)` — 88.89% / 88.24% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `TryGet<TKey, TValue>(TKey, out TValue)` — 90.91% / 88.89% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.Bucket.EnumIndexKeyConverter<TKey>`

- [ ] **P3** `GetEncodedLength(ulong)` — 90.00% / 88.24% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `GetEncodedValue(ulong)` — 97.78% / 88.24% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.Bucket.KeyConverterBase<TKey, TInternal>`

- [ ] **P0** `Read(System.ReadOnlySpan<byte>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.Bucket.KeyHelper<T>`

- [ ] **P2** `GetKeyConverter()` — 68.75% / 65.52% (5 uncovered lines, 10 uncovered blocks)

### `Xtate.Persistence.Services.Bucket.UnsupportedConverter<T>`

- [ ] **P0** `Read(System.ReadOnlySpan<byte>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `Write(T, System.Span<byte>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.Bucket.ValueConverterBase<TValue, TInternal>`

- [ ] **P2** `Write(TValue, System.Span<byte>)` — 75.00% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P2** `GetLength(TValue)` — 75.00% / 71.43% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.Bucket.ValueHelper<T>`

- [ ] **P3** `GetValueConverter()` — 90.91% / 92.16% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)

### `Xtate.Persistence.Services.DataModelListPersistingController`

- [ ] **P3** `OnRestoreChange(Xtate.DataTypes.DataModelList.ChangeAction, ref Xtate.DataTypes.DataModelList.Entry)` — 95.83% / 93.75% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `Restore()` — 98.39% / 97.10% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `OnChange(Xtate.DataTypes.DataModelList.ChangeAction, ref Xtate.DataTypes.DataModelList.Entry)` — 98.48% / 97.18% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.DataModelReferenceTracker`

- [ ] **P3** `Dispose()` — 80.00% / 92.31% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `GetRefId(Xtate.DataTypes.DataModelList, System.Func<Xtate.Persistence.Services.Bucket, Xtate.DataTypes.DataModelList, Xtate.Persistence.Services.DataModelPersistingController>, bool)` — 94.44% / 95.45% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `RemoveReference(ref Xtate.DataTypes.DataModelValue)` — 94.74% / 95.65% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Persistence.Services.EntityQueuePersistingController<T>`

- [ ] **P2** `EntityQueuePersistingController(ref Xtate.Persistence.Services.Bucket, Xtate.Interpreter.Internal.EntityQueue<T>, System.Func<Xtate.Persistence.Services.Bucket, T>)` — 84.62% / 77.78% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P3** `OnChanged(Xtate.Interpreter.Internal.EntityQueue.ChangedAction<T>, T)` — 94.44% / 88.89% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.InMemoryStorage`

- [ ] **P3** `Dispose(bool)` — 87.50% / 90.91% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Persistence.Services.InMemoryStorage.Entry`

- [ ] **P3** `CompareTo(Xtate.Persistence.Services.InMemoryStorage.Entry)` — 88.89% / 93.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Persistence.Services.InvokeIdSetPersistingController`

- [ ] **P3** `OnChanged(Xtate.Interpreter.Internal.InvokeIdSet.ChangedAction, Xtate.StateMachine.InvokeId)` — 95.00% / 88.24% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `InvokeIdSetPersistingController(ref Xtate.Persistence.Services.Bucket, Xtate.Interpreter.Internal.InvokeIdSet)` — 91.43% / 91.67% (2 uncovered lines, 1 partial lines, 3 uncovered blocks)

### `Xtate.Persistence.Services.KeyListPersistingController<T>`

- [ ] **P1** `KeyListPersistingController(Xtate.Persistence.Services.Bucket, Xtate.Interpreter.Internal.KeyList<T>, Xtate.Interpreter.IEntityMap)` — 42.86% / 38.89% (15 uncovered lines, 1 partial lines, 22 uncovered blocks)
- [ ] **P3** `OnChanged(Xtate.Interpreter.Internal.KeyList.ChangedAction<T>, Xtate.StateMachine.IEntity, System.Collections.Generic.List<T>)` — 84.21% / 89.19% (2 uncovered lines, 1 partial lines, 4 uncovered blocks)

### `Xtate.Persistence.Services.OrderedSetPersistingController<T>`

- [ ] **P3** `OnChanged(Xtate.Interpreter.Internal.OrderedSet.ChangedAction<T>, T)` — 95.65% / 92.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `OrderedSetPersistingController(ref Xtate.Persistence.Services.Bucket, Xtate.Interpreter.Internal.OrderedSet<T>, Xtate.Interpreter.IEntityMap)` — 97.56% / 95.74% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Persistence.Services.SharedMemoryStreams.<>c<TKey>`

- [ ] **P0** `OpenRead() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)

### `Xtate.Persistence.Services.SharedMemoryStreams.ReadOnlyMemoryStream<TKey>`

- [ ] **P3** `Dispose(bool)` — 80.00% / 86.96% (1 uncovered lines, 2 partial lines, 3 uncovered blocks)

### `Xtate.Persistence.Services.SharedMemoryStreams.ReadWriteMemoryStream<TKey>`

- [ ] **P3** `Dispose(bool)` — 88.89% / 91.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Persistence.Services.SharedMemoryStreams<TKey>`

- [ ] **P3** `Delete(TKey)` — 92.31% / 93.75% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.Persistence.Services.StateMachinePersistedContext`

- [ ] **P0** `EventCreator(Xtate.Persistence.Services.Bucket)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P1** `DisposeControllers()` — 22.22% / 75.86% (0 uncovered lines, 7 partial lines, 7 uncovered blocks)

### `Xtate.Persistence.Services.StateMachinePersistingInterpreter`

- [ ] **P0** `FindTransitionNode(Xtate.Interpreter.Model.StateEntityNode, int)` — 0.00% / 0.00% (21 uncovered lines, 40 uncovered blocks)
- [ ] **P0** `HandleMainLoopException(System.Exception)` — 0.00% / 80.00% (0 uncovered lines, 3 partial lines, 2 uncovered blocks)
- [ ] **P0** `SelectInternalEventTransitions() [compiler-generated async/iterator body]` — 0.00% / 0.00% (8 uncovered lines, 10 uncovered blocks)
- [ ] **P0** `StartInvoke() [compiler-generated async/iterator body]` — 0.00% / 0.00% (7 uncovered lines, 10 uncovered blocks)
- [ ] **P1** `Enter(Xtate.Persistence.Services.StateMachinePersistingInterpreter.StateBagKey, out System.Collections.Generic.List<Xtate.Interpreter.Model.TransitionNode>)` — 30.00% / 16.00% (14 uncovered lines, 21 uncovered blocks)
- [ ] **P2** `ExternalQueueCompleted() [compiler-generated async/iterator body]` — 66.67% / 50.00% (2 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `Run()` — 57.14% / 66.67% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `set_PersistenceOptions(Xtate.Persistence.IPersistenceOptions)` — 71.43% / 66.67% (2 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `set_SuspendEventDispatcher(Xtate.Persistence.ISuspendEventDispatcher)` — 66.67% / 85.71% (3 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `CancelInvoke() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `EnterStates() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `ExecuteTransitionContent() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `ExitInterpreter() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `ExitStates() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `ExitSteps() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `InitializeData() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `InitializeDataModel() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `Interpret() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `MainEventLoop() [compiler-generated async/iterator body]` — 71.43% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `Exit(Xtate.Persistence.Services.StateMachinePersistingInterpreter.StateBagKey, out Xtate.Persistence.Services.Bucket, bool)` — 75.00% / 83.33% (4 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P2** `InternalQueueProcess() [compiler-generated async/iterator body]` — 75.00% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `IsInternalQueueEmpty() [compiler-generated async/iterator body]` — 75.00% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `RunExecutableEntity() [compiler-generated async/iterator body]` — 75.00% / 92.86% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `SelectTransitions() [compiler-generated async/iterator body]` — 75.00% / 90.00% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `ExternalQueueProcess() [compiler-generated async/iterator body]` — 77.78% / 92.86% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P2** `Microstep() [compiler-generated async/iterator body]` — 77.78% / 92.86% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `Enter(Xtate.Persistence.Services.StateMachinePersistingInterpreter.StateBagKey, out Xtate.Persistence.Services.Bucket, bool)` — 80.00% / 81.82% (5 uncovered lines, 4 uncovered blocks)

### `Xtate.Persistence.Services.StateMachineReader`

- [ ] **P0** `RestoreAssign(Xtate.Persistence.Services.Bucket)` — 0.00% / 95.45% (0 uncovered lines, 11 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreCancel(Xtate.Persistence.Services.Bucket)` — 0.00% / 90.91% (0 uncovered lines, 8 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreCompound(Xtate.Persistence.Services.Bucket)` — 0.00% / 96.97% (0 uncovered lines, 15 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreConditionExpression(Xtate.Persistence.Services.Bucket)` — 0.00% / 87.50% (0 uncovered lines, 7 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreCustomAction(Xtate.Persistence.Services.Bucket)` — 0.00% / 0.00% (11 uncovered lines, 22 uncovered blocks)
- [ ] **P0** `RestoreData(Xtate.Persistence.Services.Bucket)` — 0.00% / 95.00% (0 uncovered lines, 10 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreElse(Xtate.Persistence.Services.Bucket)` — 0.00% / 83.33% (0 uncovered lines, 6 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreElseIf(Xtate.Persistence.Services.Bucket)` — 0.00% / 88.89% (0 uncovered lines, 7 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreFinal(Xtate.Persistence.Services.Bucket)` — 0.00% / 94.44% (0 uncovered lines, 10 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreForEach(Xtate.Persistence.Services.Bucket)` — 0.00% / 94.44% (0 uncovered lines, 10 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreHistory(Xtate.Persistence.Services.Bucket)` — 0.00% / 93.33% (0 uncovered lines, 9 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreIf(Xtate.Persistence.Services.Bucket)` — 0.00% / 91.67% (0 uncovered lines, 8 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreInvoke(Xtate.Persistence.Services.Bucket)` — 0.00% / 87.18% (0 uncovered lines, 17 partial lines, 5 uncovered blocks)
- [ ] **P0** `RestoreLog(Xtate.Persistence.Services.Bucket)` — 0.00% / 90.91% (0 uncovered lines, 8 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreOnEntry(Xtate.Persistence.Services.Bucket)` — 0.00% / 88.89% (0 uncovered lines, 7 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreOnExit(Xtate.Persistence.Services.Bucket)` — 0.00% / 88.89% (0 uncovered lines, 7 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreParallel(Xtate.Persistence.Services.Bucket)` — 0.00% / 96.67% (0 uncovered lines, 14 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreParam(Xtate.Persistence.Services.Bucket)` — 0.00% / 92.86% (0 uncovered lines, 9 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreRaise(Xtate.Persistence.Services.Bucket)` — 0.00% / 88.89% (0 uncovered lines, 7 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreSend(Xtate.Persistence.Services.Bucket)` — 0.00% / 93.33% (0 uncovered lines, 19 partial lines, 3 uncovered blocks)
- [ ] **P0** `RestoreState(Xtate.Persistence.Services.Bucket)` — 0.00% / 96.97% (0 uncovered lines, 15 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreStateMachine(Xtate.Persistence.Services.Bucket)` — 0.00% / 96.00% (0 uncovered lines, 13 partial lines, 1 uncovered blocks)
- [ ] **P0** `RestoreTransition(Xtate.Persistence.Services.Bucket)` — 0.00% / 96.30% (0 uncovered lines, 11 partial lines, 1 uncovered blocks)
- [ ] **P1** `RestoreExecutableEntity(Xtate.Persistence.Services.Bucket)` — 36.84% / 48.00% (0 uncovered lines, 12 partial lines, 39 uncovered blocks)
- [ ] **P1** `ForwardExecEntity(Xtate.Persistence.Services.Bucket)` — 53.85% / 43.75% (6 uncovered lines, 9 uncovered blocks)
- [ ] **P1** `RestoreCondition(Xtate.Persistence.Services.Bucket)` — 62.50% / 46.67% (2 uncovered lines, 1 partial lines, 8 uncovered blocks)
- [ ] **P2** `RestoreStateEntity(Xtate.Persistence.Services.Bucket)` — 54.55% / 51.61% (0 uncovered lines, 5 partial lines, 15 uncovered blocks)
- [ ] **P2** `Build(Xtate.Persistence.Services.Bucket, Xtate.Interpreter.IEntityMap)` — 75.00% / 57.14% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `RestoreExternalScriptExpression(Xtate.Persistence.Services.Bucket)` — 61.54% / 71.43% (5 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Exist(ref Xtate.Persistence.Services.Bucket, Xtate.Persistence.Internal.TypeInfo)` — 77.78% / 66.67% (2 uncovered lines, 3 uncovered blocks)
- [ ] **P2** `RestoreEventDescriptor(Xtate.Persistence.Services.Bucket)` — 75.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P2** `RestoreIdentifier(Xtate.Persistence.Services.Bucket)` — 75.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Persistence.Services.StreamStorage`

- [ ] **P0** `GetMarkSizeLength(int, System.Nullable<int>)` — 0.00% / 87.50% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `WriteMarkSize(System.Span<byte>, int, System.Nullable<int>)` — 85.71% / 91.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `ReadStream() [compiler-generated async/iterator body]` — 93.89% / 96.47% (6 uncovered lines, 2 partial lines, 6 uncovered blocks)

### `Xtate.ResourceLoaders.Extensions.StreamExtensions`

- [ ] **P3** `ReadToEnd(System.IO.Stream, System.Threading.CancellationToken)` — 83.33% / 83.87% (0 uncovered lines, 3 partial lines, 5 uncovered blocks)
- [ ] **P3** `ReadToEndAsync() [compiler-generated async/iterator body]` — 83.33% / 88.24% (0 uncovered lines, 3 partial lines, 4 uncovered blocks)

### `Xtate.ResourceLoaders.Internal.DelegatedStream`

- [ ] **P0** `Dispose(bool)` — 0.00% / 0.00% (4 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_ReadTimeout()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `get_WriteTimeout()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.ResourceLoaders.Internal.InjectedCancellationStream`

- [ ] **P0** `CopyToAsync(System.IO.Stream, int, System.Threading.CancellationToken)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `CopyToAsyncInternal() [compiler-generated async/iterator body]` — 0.00% / 0.00% (4 uncovered lines, 8 uncovered blocks)
- [ ] **P0** `FlushAsync(System.Threading.CancellationToken)` — 0.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `FlushAsyncInternal() [compiler-generated async/iterator body]` — 0.00% / 0.00% (4 uncovered lines, 8 uncovered blocks)
- [ ] **P0** `ReadAsync(byte[], int, int, System.Threading.CancellationToken)` — 0.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ReadAsync(System.Memory<byte>, System.Threading.CancellationToken)` — 0.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ReadAsyncInternal() [compiler-generated async/iterator body]` — 0.00% / 0.00% (8 uncovered lines, 16 uncovered blocks)
- [ ] **P0** `WriteAsync(byte[], int, int, System.Threading.CancellationToken)` — 0.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `WriteAsync(System.ReadOnlyMemory<byte>, System.Threading.CancellationToken)` — 0.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `WriteAsyncInternal() [compiler-generated async/iterator body]` — 0.00% / 0.00% (8 uncovered lines, 16 uncovered blocks)
- [ ] **P3** `IsCombinedTokenRequired(ref System.Threading.CancellationToken)` — 85.71% / 80.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.ResourceLoaders.Resource`

- [ ] **P1** `GetStream() [compiler-generated async/iterator body]` — 64.71% / 36.36% (6 uncovered lines, 14 uncovered blocks)
- [ ] **P3** `Dispose(bool)` — 84.62% / 87.50% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `GetBytes() [compiler-generated async/iterator body]` — 84.62% / 93.33% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `GetContent() [compiler-generated async/iterator body]` — 86.67% / 95.83% (2 uncovered lines, 1 uncovered blocks)

### `Xtate.ResourceLoaders.Resx.Services.ResxResourceLoader`

- [ ] **P2** `GetResourceStream(System.Uri)` — 87.50% / 66.67% (1 uncovered lines, 4 uncovered blocks)

### `Xtate.ResourceLoaders.Services.ResourceLoaderService`

- [ ] **P3** `Request() [compiler-generated async/iterator body]` — 92.31% / 86.21% (1 uncovered lines, 4 uncovered blocks)

### `Xtate.ResourceLoaders.Web.Services.WebResourceLoader`

- [ ] **P3** `CreateRequestMessage(System.Uri, System.Collections.Specialized.NameValueCollection)` — 92.86% / 95.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Scxml.Internal.TextContentReader`

- [ ] **P0** `get_Depth()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `get_IsDefault()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_IsEmptyElement()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_Item(int)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_Item(string)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_Item(string, string)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_LocalName()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_Name()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_NamespaceURI()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_Prefix()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_QuoteChar()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_XmlLang()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `get_XmlSpace()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `MoveToAttribute(int)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `ReadInnerXml()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ReadOuterXml()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ReadString()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `ResolveEntity()` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `Read()` — 87.50% / 80.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.Scxml.Services.DelegatedXmlReader`

- [ ] **P0** `get_AttributeCount()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_EOF()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_HasAttributes()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_HasValue()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_Item(int)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_Item(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_Item(string, string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_LineNumber()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `get_LinePosition()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `get_Name()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_ValueType()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_XmlLang()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `get_XmlSpace()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `GetAttribute(int)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `GetAttribute(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `GetAttribute(string, string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `HasLineInfo()` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `MoveToAttribute(int)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `MoveToAttribute(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `MoveToAttribute(string, string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `ResolveEntity()` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)

### `Xtate.Scxml.Services.RedirectXmlResolver`

- [ ] **P0** `GetEntity(System.Uri, string, System.Type)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `GetEntityAsync() [compiler-generated async/iterator body]` — 92.86% / 98.04% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlDirector`

- [ ] **P0** `CreateXmlLineInfo(object)` — 0.00% / 46.15% (0 uncovered lines, 3 partial lines, 7 uncovered blocks)
- [ ] **P0** `OnError(string, System.Exception)` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P1** `NamespaceAttribute(string)` — 40.00% / 73.33% (5 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P2** `CompareArrays(System.Collections.Immutable.ImmutableArray<System.ValueTuple<string, string>>, System.ValueTuple<string, string>[], int)` — 58.33% / 81.25% (4 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `AsLocationExpressionList(string)` — 66.67% / 61.54% (6 uncovered lines, 10 uncovered blocks)
- [ ] **P2** `AsConditionalExpression(string)` — 66.67% / 66.67% (2 uncovered lines, 3 uncovered blocks)
- [ ] **P2** `AsLocationExpression(string)` — 66.67% / 66.67% (2 uncovered lines, 3 uncovered blocks)
- [ ] **P2** `AsMilliseconds(string)` — 76.92% / 83.33% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P3** `CreateXmlNamespacesInfo(object)` — 86.36% / 93.55% (2 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `AsEventDescriptorList(string)` — 88.89% / 87.50% (2 uncovered lines, 3 uncovered blocks)
- [ ] **P3** `ResolveThroughCache(System.ValueTuple<string, string>[], int)` — 91.67% / 94.12% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlDirector.<>c`

- [ ] **P0** `AssignBuildPolicy() [compiler-generated local/lambda body]` — 0.00% / 0.00% (2 uncovered lines, 6 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlDirector.<>c.<<FinalizeBuildPolicy>b__203_6>d`

- [ ] **P0** `MoveNext()` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlDirector.<>c.<<ForEachBuildPolicy>b__213_11>d`

- [ ] **P0** `MoveNext()` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlDirector.<>c.<<IfBuildPolicy>b__215_11>d`

- [ ] **P0** `MoveNext()` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlDirector.<>c.<<OnExitBuildPolicy>b__199_8>d`

- [ ] **P0** `MoveNext()` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlDirector.<>c.<<TransitionBuildPolicy>b__187_12>d`

- [ ] **P0** `MoveNext()` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlLocationStateMachineGetter`

- [ ] **P0** `get_Location()` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlSerializerWriter`

- [ ] **P0** `Visit(ref Xtate.StateMachine.IElse)` — 0.00% / 0.00% (4 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `Visit(ref Xtate.StateMachine.IElseIf)` — 0.00% / 0.00% (9 uncovered lines, 11 uncovered blocks)
- [ ] **P0** `Visit(ref Xtate.StateMachine.IFinalize)` — 0.00% / 0.00% (5 uncovered lines, 4 uncovered blocks)
- [ ] **P0** `Visit(ref Xtate.StateMachine.IForEach)` — 0.00% / 0.00% (18 uncovered lines, 26 uncovered blocks)
- [ ] **P0** `Visit(ref Xtate.StateMachine.IIf)` — 0.00% / 0.00% (10 uncovered lines, 12 uncovered blocks)
- [ ] **P0** `Visit(ref Xtate.StateMachine.IInvoke)` — 0.00% / 0.00% (40 uncovered lines, 55 uncovered blocks)
- [ ] **P0** `Visit(ref Xtate.StateMachine.IOnExit)` — 0.00% / 0.00% (5 uncovered lines, 4 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.ISend)` — 49.06% / 69.05% (20 uncovered lines, 7 partial lines, 26 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IScript)` — 61.54% / 73.68% (3 uncovered lines, 2 partial lines, 5 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.ICancel)` — 69.23% / 80.00% (3 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IParam)` — 70.59% / 81.82% (3 uncovered lines, 2 partial lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IData)` — 77.78% / 83.33% (3 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P3** `Visit(ref Xtate.StateMachine.IAssign)` — 85.71% / 89.47% (0 uncovered lines, 2 partial lines, 2 uncovered blocks)
- [ ] **P3** `Visit(ref Xtate.StateMachine.IRaise)` — 91.67% / 92.86% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `Visit(ref Xtate.StateMachine.IStateMachine)` — 94.44% / 96.55% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Scxml.Services.ScxmlSerializerWriter.<>c`

- [ ] **P0** `Visit() [compiler-generated local/lambda body]` — 0.00% / 0.00% (2 uncovered lines, 8 uncovered blocks)

### `Xtate.Scxml.Services.XIncludeReader`

- [ ] **P0** `Close()` — 0.00% / 0.00% (11 uncovered lines, 12 uncovered blocks)
- [ ] **P0** `GetEncoding(Xtate.ResourceLoaders.Resource)` — 0.00% / 0.00% (9 uncovered lines, 21 uncovered blocks)
- [ ] **P0** `IsXml(Xtate.ResourceLoaders.Resource)` — 0.00% / 0.00% (9 uncovered lines, 21 uncovered blocks)
- [ ] **P0** `ProcessInterDocTextInclusion() [compiler-generated async/iterator body]` — 0.00% / 0.00% (8 uncovered lines, 17 uncovered blocks)
- [ ] **P0** `ReadStreamAsText() [compiler-generated async/iterator body]` — 0.00% / 0.00% (7 uncovered lines, 14 uncovered blocks)
- [ ] **P0** `ReadStreamAsXml() [compiler-generated async/iterator body]` — 0.00% / 0.00% (15 uncovered lines, 22 uncovered blocks)
- [ ] **P1** `LoadAcquiredData() [compiler-generated async/iterator body]` — 37.50% / 36.21% (24 uncovered lines, 1 partial lines, 37 uncovered blocks)
- [ ] **P1** `ResolveHref(string)` — 36.36% / 55.00% (6 uncovered lines, 1 partial lines, 9 uncovered blocks)
- [ ] **P1** `ProcessIncludeElement() [compiler-generated async/iterator body]` — 46.15% / 38.71% (6 uncovered lines, 1 partial lines, 19 uncovered blocks)
- [ ] **P1** `ExtractIncludeElementAttributes()` — 45.45% / 40.82% (18 uncovered lines, 29 uncovered blocks)
- [ ] **P2** `PushInnerReader(System.Xml.XmlReader)` — 55.56% / 65.00% (2 uncovered lines, 2 partial lines, 7 uncovered blocks)
- [ ] **P3** `GetXmlReaderSettings(bool)` — 87.50% / 85.71% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P3** `Read() [compiler-generated async/iterator body]` — 90.91% / 85.71% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `ReadNext() [compiler-generated async/iterator body]` — 88.24% / 89.74% (0 uncovered lines, 2 partial lines, 4 uncovered blocks)

### `Xtate.Scxml.Services.XmlBaseReader`

- [ ] **P0** `get_BaseURI()` — 0.00% / 60.00% (0 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P1** `PostProcessNode()` — 27.27% / 30.43% (8 uncovered lines, 16 uncovered blocks)
- [ ] **P1** `PreProcessNode()` — 28.57% / 68.75% (4 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P2** `TryPeek(out int, out System.Uri)` — 55.56% / 55.56% (3 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P2** `GetXmlBaseValue()` — 63.64% / 70.59% (3 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P2** `Read()` — 75.00% / 85.71% (2 uncovered lines, 1 uncovered blocks)

### `Xtate.Scxml.Services.XmlDirector.Policy.ValidationContext.<>c<TDirector, TEntity>`

- [ ] **P0** `CreateMessage() [compiler-generated local/lambda body]` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `ProcessElementsCompleted() [compiler-generated local/lambda body]` — 0.00% / 0.00% (2 uncovered lines, 1 partial lines, 9 uncovered blocks)

### `Xtate.Scxml.Services.XmlDirector.Policy.ValidationContext<TDirector, TEntity>`

- [ ] **P0** `CreateMessage(string, string, string)` — 0.00% / 87.50% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `OnError(string)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P1** `ProcessElementsCompleted()` — 28.57% / 38.10% (4 uncovered lines, 1 partial lines, 13 uncovered blocks)
- [ ] **P2** `ValidateAttribute(string, string)` — 50.00% / 68.42% (3 uncovered lines, 2 partial lines, 6 uncovered blocks)
- [ ] **P2** `ValidateElement(string, string)` — 78.57% / 87.50% (2 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P3** `ProcessAttributesCompleted()` — 85.71% / 95.24% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `ValidationContext(Xtate.Scxml.Services.XmlDirector.Policy<TDirector, TEntity>, Xtate.Scxml.Services.XmlDirector<TDirector>)` — 93.33% / 96.67% (1 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.Scxml.Services.XmlDirector.PolicyBuilder<TDirector, TEntity>`

- [ ] **P0** `DenyUnknownElements()` — 0.00% / 0.00% (4 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `IgnoreUnknownElements()` — 0.00% / 0.00% (4 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `IgnoreUnknownElements(bool)` — 0.00% / 0.00% (4 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `MultipleElements(string, System.Func<TDirector, TEntity, System.Threading.Tasks.ValueTask>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `OptionalElement(string, System.Func<TDirector, TEntity, System.Threading.Tasks.ValueTask>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `SingleElement(string, System.Func<TDirector, TEntity, System.Threading.Tasks.ValueTask>)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `ValidateElementName(string)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `UseRawContent(bool)` — 62.50% / 50.00% (6 uncovered lines, 10 uncovered blocks)

### `Xtate.Scxml.Services.XmlDirector.QualifiedName<TDirector>`

- [ ] **P0** `Equals(object)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `Equals(Xtate.Scxml.Services.XmlDirector.QualifiedName<TDirector>)` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.Scxml.Services.XmlDirector<TDirector, TEntity>`

- [ ] **P2** `Populate() [compiler-generated async/iterator body]` — 76.92% / 87.88% (6 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `PopulateElements() [compiler-generated async/iterator body]` — 77.42% / 78.05% (7 uncovered lines, 9 uncovered blocks)

### `Xtate.Scxml.Services.XmlDirector<TDirector>`

- [ ] **P0** `get_RawContent()` — 0.00% / 66.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `get_UseAsync()` — 0.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `MoveToContent()` — 0.00% / 62.50% (0 uncovered lines, 3 partial lines, 3 uncovered blocks)
- [ ] **P0** `NamespaceAttribute(string)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `ReadInnerXml()` — 0.00% / 62.50% (0 uncovered lines, 3 partial lines, 3 uncovered blocks)
- [ ] **P0** `ReadOuterXml()` — 0.00% / 62.50% (0 uncovered lines, 3 partial lines, 3 uncovered blocks)
- [ ] **P0** `Skip()` — 0.00% / 0.00% (7 uncovered lines, 8 uncovered blocks)
- [ ] **P2** `ReadEndElement() [compiler-generated async/iterator body]` — 66.67% / 83.33% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P2** `ReadStartElement() [compiler-generated async/iterator body]` — 66.67% / 83.33% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `PopulateAttributes<TEntity>(TEntity, Xtate.Scxml.Services.XmlDirector.Policy<TDirector, TEntity>, Xtate.Scxml.Services.XmlDirector.Policy.ValidationContext<TDirector, TEntity>)` — 87.10% / 87.10% (4 uncovered lines, 4 uncovered blocks)

### `Xtate.Scxml.XIncludeException`

- [ ] **P3** `AddLocationInfo(string, System.Xml.XmlReader)` — 81.25% / 90.91% (1 uncovered lines, 2 partial lines, 3 uncovered blocks)

### `Xtate.StackSpan<T>`

- [ ] **P0** `op_False(Xtate.StackSpan<T>)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `op_Implicit(Xtate.StackSpan<T>)` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `UsagePatternException()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.StateMachine.Builder.Services.ParallelBuilder`

- [ ] **P2** `AddParallel(Xtate.StateMachine.IParallel)` — 75.00% / 66.67% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.StateMachine.EventDescriptors`

- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `ToString(string, System.IFormatProvider)` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachine.EventName`

- [ ] **P0** `Equals(object)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `IsError()` — 0.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `GetEventName(Xtate.StateMachine.IIdentifier, Xtate.StateMachine.IIdentifier, string)` — 81.82% / 88.89% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `SetParts(System.Span<Xtate.StateMachine.IIdentifier>, string)` — 84.62% / 92.86% (2 uncovered lines, 1 uncovered blocks)

### `Xtate.StateMachine.Identifier`

- [ ] **P0** `Equals(object)` — 0.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachine.Internal.SegmentedName`

- [ ] **P2** `ToString<T>(System.Collections.Immutable.ImmutableArray<T>, string)` — 66.67% / 82.50% (0 uncovered lines, 3 partial lines, 7 uncovered blocks)
- [ ] **P3** `TryFormat<T>(System.Collections.Immutable.ImmutableArray<T>, string, System.Span<char>, out int)` — 85.19% / 93.10% (4 uncovered lines, 2 uncovered blocks)

### `Xtate.StateMachine.InvokeId`

- [ ] **P0** `FromString(string, string)` — 0.00% / 83.33% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachine.LazyId`

- [ ] **P3** `get_Value()` — 88.89% / 92.86% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachine.Services.StateMachineVisitor`

- [ ] **P0** `Enter<T>(System.Collections.Immutable.ImmutableArray<T>)` — 0.00% / 37.50% (0 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P0** `Enter<T>(T)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 3 uncovered blocks)
- [ ] **P0** `EntityName(System.ValueTuple<object, System.Collections.Immutable.ImmutableArray<object>>)` — 0.00% / 0.00% (6 uncovered lines, 8 uncovered blocks)
- [ ] **P0** `Exit()` — 0.00% / 60.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `get_CurrentPath()` — 0.00% / 0.00% (1 uncovered lines, 9 uncovered blocks)
- [ ] **P0** `SetRootPath(object)` — 0.00% / 0.00% (7 uncovered lines, 17 uncovered blocks)
- [ ] **P0** `VisitUnknown(ref Xtate.StateMachine.IStateEntity)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P1** `Build(ref Xtate.StateMachine.Services.StateMachineVisitor.TrackList<Xtate.StateMachine.IValueExpression>)` — 25.00% / 50.00% (5 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P2** `StateMachineVisitor(bool)` — 50.00% / 80.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IEventDescriptor)` — 66.67% / 50.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IIdentifier)` — 66.67% / 50.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IOutgoingEvent)` — 66.67% / 50.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IStateEntity)` — 80.00% / 69.23% (2 uncovered lines, 1 partial lines, 4 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IData)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IEventDescriptor)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IHistory)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IInvoke)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IOnEntry)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IOnExit)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IOutgoingEvent)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IParam)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.IStateEntity)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `VisitWrapper(ref Xtate.StateMachine.ITransition)` — 83.33% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `Visit(ref Xtate.StateMachine.IExecutableEntity)` — 97.44% / 93.10% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.StateMachine.Services.StateMachineVisitor.TrackList<T>`

- [ ] **P0** `Add(T)` — 0.00% / 0.00% (1 uncovered lines, 7 uncovered blocks)
- [ ] **P0** `Clear()` — 0.00% / 0.00% (13 uncovered lines, 12 uncovered blocks)
- [ ] **P0** `Contains(T)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 7 uncovered blocks)
- [ ] **P0** `Insert(int, T)` — 0.00% / 0.00% (1 uncovered lines, 7 uncovered blocks)
- [ ] **P0** `RemoveAt(int)` — 0.00% / 0.00% (1 uncovered lines, 7 uncovered blocks)
- [ ] **P2** `get_IsModified()` — 66.67% / 84.21% (4 uncovered lines, 1 partial lines, 3 uncovered blocks)

### `Xtate.StateMachine.Services.StateMachineVisitor.VisitData<TEntity, TIEntity>`

- [ ] **P3** `VisitData(TIEntity)` — 91.67% / 83.33% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.StateMachine.Services.StateMachineVisitor.VisitListData<T>`

- [ ] **P2** `VisitListData(System.Collections.Immutable.ImmutableArray<T>)` — 71.43% / 66.67% (2 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `Update(ref System.Collections.Immutable.ImmutableArray<T>)` — 83.33% / 90.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachine.SessionId`

- [ ] **P0** `Equals(object)` — 0.00% / 87.50% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `op_Implicit(Xtate.StateMachine.SessionId)` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachine.Target`

- [ ] **P0** `GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `System.Collections.IEnumerable.GetEnumerator()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `ToString(string, System.IFormatProvider)` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachine.Validator.Services.StateMachineValidator`

- [ ] **P1** `Visit(ref Xtate.StateMachine.IHistory)` — 36.36% / 52.63% (6 uncovered lines, 1 partial lines, 9 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.IAssign)` — 40.00% / 57.14% (9 uncovered lines, 12 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.ICancel)` — 40.00% / 62.50% (9 uncovered lines, 12 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.ISend)` — 40.54% / 70.10% (22 uncovered lines, 29 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.IForEach)` — 45.45% / 42.86% (6 uncovered lines, 8 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.IInvoke)` — 43.48% / 68.52% (13 uncovered lines, 17 uncovered blocks)
- [ ] **P1** `Visit(ref Xtate.StateMachine.IContent)` — 45.45% / 63.64% (6 uncovered lines, 8 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IConditionExpression)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IContentBody)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.ICustomAction)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IElseIf)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IInitial)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IInlineContent)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.ILocationExpression)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IRaise)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IScriptExpression)` — 57.14% / 50.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IStateMachine)` — 54.55% / 75.00% (4 uncovered lines, 1 partial lines, 6 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IFinalize)` — 57.14% / 57.89% (6 uncovered lines, 8 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IIf)` — 60.87% / 57.14% (9 uncovered lines, 12 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.ITransition)` — 57.14% / 75.00% (3 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IParam)` — 63.64% / 72.22% (4 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `Visit(ref Xtate.StateMachine.IData)` — 69.23% / 82.76% (4 uncovered lines, 5 uncovered blocks)
- [ ] **P3** `Visit(ref Xtate.StateMachine.IScript)` — 85.71% / 91.67% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `Visit(ref Xtate.StateMachine.IState)` — 85.71% / 92.31% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.StateMachineFluentBuilder.ParallelFluentBuilder<TOuterBuilder>`

- [ ] **P0** `AddOnEntry(System.Action)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddOnEntry(System.Func<System.Threading.Tasks.ValueTask>)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddOnEntry(Xtate.StateMachine.IExecutableEntity)` — 0.00% / 0.00% (4 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `AddOnExit(System.Action)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddOnExit(System.Func<System.Threading.Tasks.ValueTask>)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddOnExit(Xtate.StateMachine.IExecutableEntity)` — 0.00% / 0.00% (4 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `AddTransition(System.Func<bool>, string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddTransition(System.Func<bool>, Xtate.StateMachine.IIdentifier)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `AddTransition(Xtate.StateMachine.EventDescriptor, string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddTransition(Xtate.StateMachine.EventDescriptor, Xtate.StateMachine.IIdentifier)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginHistory()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginHistory(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `BeginHistory(Xtate.StateMachine.IIdentifier)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `BeginParallel()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginParallel(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `BeginParallel(Xtate.StateMachine.IIdentifier)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `BeginState()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginTransition()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `SetId(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)

### `Xtate.StateMachineFluentBuilder.StateFluentBuilder.<>c<TOuterBuilder>`

- [ ] **P0** `SetInitial() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.StateMachineFluentBuilder.StateFluentBuilder<TOuterBuilder>`

- [ ] **P0** `AddOnEntry(System.Func<System.Threading.Tasks.ValueTask>)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddOnExit(System.Func<System.Threading.Tasks.ValueTask>)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddTransition(System.Func<bool>, string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddTransition(Xtate.StateMachine.EventDescriptor, string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddTransition(Xtate.StateMachine.EventDescriptor, Xtate.StateMachine.IIdentifier)` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginFinal()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginFinal(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `BeginFinal(Xtate.StateMachine.IIdentifier)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `BeginHistory()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginHistory(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `BeginHistory(Xtate.StateMachine.IIdentifier)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `BeginInitial()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginParallel()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginState()` — 0.00% / 0.00% (1 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `BeginState(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `BeginState(Xtate.StateMachine.IIdentifier)` — 0.00% / 0.00% (1 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `SetId(string)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `SetInitial(string[])` — 0.00% / 0.00% (10 uncovered lines, 12 uncovered blocks)
- [ ] **P0** `SetInitial(System.Collections.Immutable.ImmutableArray<string>)` — 0.00% / 0.00% (5 uncovered lines, 9 uncovered blocks)
- [ ] **P0** `SetInitial(System.Collections.Immutable.ImmutableArray<Xtate.StateMachine.IIdentifier>)` — 0.00% / 0.00% (5 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `SetInitial(Xtate.StateMachine.IIdentifier[])` — 0.00% / 0.00% (5 uncovered lines, 7 uncovered blocks)

### `Xtate.StateMachineFluentBuilder.TransitionFluentBuilder.<>c<TOuterBuilder>`

- [ ] **P0** `SetTarget() [compiler-generated local/lambda body]` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.StateMachineFluentBuilder.TransitionFluentBuilder<TOuterBuilder>`

- [ ] **P0** `AddOnTransition(System.Action)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddOnTransition(System.Func<System.Threading.Tasks.ValueTask>)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `AddOnTransition(Xtate.StateMachine.IExecutableEntity)` — 0.00% / 0.00% (4 uncovered lines, 4 uncovered blocks)
- [ ] **P0** `SetCondition(System.Func<System.Threading.Tasks.ValueTask<bool>>)` — 0.00% / 0.00% (1 uncovered lines, 3 uncovered blocks)
- [ ] **P0** `SetEvent(string[])` — 0.00% / 0.00% (10 uncovered lines, 13 uncovered blocks)
- [ ] **P0** `SetEvent(System.Collections.Immutable.ImmutableArray<Xtate.StateMachine.IEventDescriptor>)` — 0.00% / 0.00% (5 uncovered lines, 6 uncovered blocks)
- [ ] **P0** `SetEvent(Xtate.StateMachine.IEventDescriptor[])` — 0.00% / 0.00% (5 uncovered lines, 8 uncovered blocks)
- [ ] **P0** `SetTarget(string[])` — 0.00% / 0.00% (10 uncovered lines, 12 uncovered blocks)
- [ ] **P0** `SetTarget(System.Collections.Immutable.ImmutableArray<string>)` — 0.00% / 0.00% (5 uncovered lines, 9 uncovered blocks)
- [ ] **P0** `SetTarget(System.Collections.Immutable.ImmutableArray<Xtate.StateMachine.IIdentifier>)` — 0.00% / 0.00% (5 uncovered lines, 5 uncovered blocks)
- [ ] **P0** `SetType(Xtate.StateMachine.TransitionType)` — 0.00% / 0.00% (4 uncovered lines, 4 uncovered blocks)

### `Xtate.StateMachineHost.Services.DeadLetterQueue<TSource>`

- [ ] **P0** `Enqueue(Xtate.StateMachine.ServiceId, Xtate.DataModel.IIncomingEvent)` — 0.00% / 41.67% (0 uncovered lines, 1 partial lines, 7 uncovered blocks)

### `Xtate.StateMachineHost.Services.ExternalServiceFactory`

- [ ] **P3** `GetServiceActivator() [compiler-generated async/iterator body]` — 89.47% / 93.94% (1 uncovered lines, 1 partial lines, 2 uncovered blocks)

### `Xtate.StateMachineHost.Services.ExternalServiceScopeManager`

- [ ] **P0** `Cancel(Xtate.StateMachine.InvokeId, System.Threading.CancellationToken)` — 0.00% / 85.71% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P2** `DisposeAsyncCore() [compiler-generated async/iterator body]` — 70.00% / 54.55% (3 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `CreateServiceScope(Xtate.StateMachine.InvokeId, Xtate.StateMachineHost.Services.ExternalServiceClass)` — 80.00% / 66.67% (2 uncovered lines, 4 uncovered blocks)
- [ ] **P2** `Dispose(bool)` — 70.00% / 81.82% (3 uncovered lines, 2 uncovered blocks)

### `Xtate.StateMachineHost.Services.IoProcessorBase`

- [ ] **P0** `CanHandle(Xtate.FullUri)` — 0.00% / 88.89% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `CreateExternalServiceTarget(Xtate.StateMachine.InvokeId)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P0** `CreateStateMachineTarget(Xtate.StateMachine.SessionId)` — 0.00% / 0.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.StateMachineHost.Services.SecurityContext`

- [ ] **P0** `Create(Xtate.StateMachineHost.SecurityContextType, Xtate.StateMachineHost.SecurityContextPermissions)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `CreateNested(Xtate.StateMachineHost.SecurityContextType)` — 92.31% / 86.67% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.StateMachineHost.Services.SecurityContext.NoAccessTaskScheduler`

- [ ] **P0** `GetScheduledTasks()` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)
- [ ] **P0** `QueueTask(System.Threading.Tasks.Task)` — 0.00% / 50.00% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P0** `TryExecuteTaskInline(System.Threading.Tasks.Task, bool)` — 0.00% / 0.00% (1 uncovered lines, 2 uncovered blocks)

### `Xtate.StateMachineHost.Services.StateMachineControllerBase`

- [ ] **P3** `Destroy() [compiler-generated async/iterator body]` — 85.71% / 90.00% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.StateMachineHost.Services.StateMachineDestroyOnIdle`

- [ ] **P3** `Factory()` — 80.00% / 94.44% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachineHost.Services.StateMachineDestroyOnIdle.StateTracker`

- [ ] **P0** `DestroyOnIdle(object)` — 0.00% / 0.00% (12 uncovered lines, 9 uncovered blocks)

### `Xtate.StateMachineHost.Services.StateMachineExternalService`

- [ ] **P3** `Execute()` — 95.65% / 96.88% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.StateMachineHost.Services.StateMachineScopeManager`

- [ ] **P0** `Destroy(Xtate.StateMachine.SessionId)` — 0.00% / 75.00% (0 uncovered lines, 1 partial lines, 2 uncovered blocks)
- [ ] **P0** `Terminate(Xtate.StateMachine.SessionId)` — 0.00% / 87.50% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P2** `DisposeAsyncCore() [compiler-generated async/iterator body]` — 70.00% / 54.55% (3 uncovered lines, 5 uncovered blocks)
- [ ] **P2** `Dispose(bool)` — 70.00% / 81.82% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `Start() [compiler-generated async/iterator body]` — 83.33% / 80.65% (3 uncovered lines, 6 uncovered blocks)
- [ ] **P3** `DestroyTasks() [compiler-generated async/iterator body]` — 83.33% / 94.12% (2 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `Cleanup() [compiler-generated async/iterator body]` — 85.71% / 91.67% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)
- [ ] **P3** `CreateServiceScope(Xtate.Class.StateMachineClass)` — 90.00% / 92.31% (0 uncovered lines, 1 partial lines, 1 uncovered blocks)

### `Xtate.TaskMonitor.Services.TaskMonitor`

- [ ] **P0** `TaskCancelled(System.OperationCanceledException)` — 0.00% / 0.00% (1 uncovered lines, 4 uncovered blocks)
- [ ] **P0** `WaitAsync(System.Threading.Tasks.Task, System.Threading.CancellationToken)` — 0.00% / 33.33% (0 uncovered lines, 5 partial lines, 8 uncovered blocks)
- [ ] **P2** `Forget(System.Threading.Tasks.Task)` — 70.00% / 66.67% (2 uncovered lines, 1 partial lines, 5 uncovered blocks)
- [ ] **P2** `MonitorTaskCompletion() [compiler-generated async/iterator body]` — 69.23% / 66.67% (5 uncovered lines, 7 uncovered blocks)
- [ ] **P2** `WaitAndMonitor() [compiler-generated async/iterator body]` — 72.73% / 83.33% (3 uncovered lines, 2 uncovered blocks)
- [ ] **P3** `Forget(System.Threading.Tasks.ValueTask)` — 90.00% / 93.33% (1 uncovered lines, 1 uncovered blocks)
- [ ] **P3** `Forget<TResult>(System.Threading.Tasks.ValueTask<TResult>)` — 90.00% / 93.33% (1 uncovered lines, 1 uncovered blocks)

### `Xtate.TaskMonitor.Services.TaskMonitor<TResult>`

- [ ] **P3** `MonitorTaskCompletion() [compiler-generated async/iterator body]` — 92.31% / 94.44% (1 uncovered lines, 1 uncovered blocks)

## Completion notes

For each completed type, record:

- Test class/file added or changed.
- Scenarios and branches covered.
- Remaining uncovered branches and the reason, if any.
- Updated line and block coverage.
