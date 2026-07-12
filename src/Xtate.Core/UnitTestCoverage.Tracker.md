# Xtate.Core Unit-Test Coverage Tracker

Source baseline: `src/Xtate.Core/UnitTestCoverage.RequiredClasses.md`
Coverage input: `src/Xtate.Core/UnitTestCoverage.xml`

Use this file to track unit-test coverage progress. Mark a class as covered only after a refreshed coverage report shows the class is no longer listed in `UnitTestCoverage.RequiredClasses.md`, or shows 100% aggregate line and block coverage.

## Status summary

- Baseline classes requiring coverage: 461
- Covered: 0
- In progress / tests added, coverage not yet verified: 204
- Not started: 257
- Remaining not fully verified: 461

## Legend

- `[ ]` Not fully covered / needs tests
- `[~]` Tests in progress, coverage not yet verified
- `[x]` Fully covered and verified by refreshed coverage report

## Tracker

| Status | Class | Line % | Block % | Uncovered lines | Partial lines | Uncovered blocks | Functions | Notes |
|---|---|---:|---:|---:|---:|---:|---:|---|
| [~] | `Xtate.Scxml.Services.ScxmlSerializerWriter` | 3.81 | 4.03 | 391 | 1 | 500 | 38 | `ScxmlSerializerWriterTest` added; rerun coverage to verify. |
| [ ] | `Xtate.ExternalServices.HttpClient.Services.HttpClientService` | 0 | 0 | 209 | 0 | 411 | 15 |  |
| [ ] | `Xtate.IoProcessors.Http.Services.HttpController` | 0 | 0 | 196 | 0 | 352 | 13 |  |
| [~] | `Xtate.DataTypes.DataModelValue` | 59.27 | 51.29 | 182 | 36 | 395 | 98 | `DataModelDynamicCoverageTest` added for dynamic scalar/list conversion paths; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList` | 75.48 | 70.54 | 182 | 21 | 251 | 110 | `DataModelListCoverageTest` added; rerun coverage to verify. Includes ignored clone/case-insensitive preservation probe for current product gap. |
| [ ] | `Xtate.DataModel.XPath.Internal.DataModelXPathNavigator` | 49.41 | 54.6 | 167 | 8 | 217 | 49 |  |
| [ ] | `Xtate.IoProcessors.NamedPipe.Services.NamedPipeController` | 0 | 0 | 161 | 0 | 339 | 16 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.XmlConverter` | 42.93 | 31.73 | 158 | 31 | 327 | 29 |  |
| [ ] | `Xtate.Interpreter.Services.StateMachineInterpreter` | 80.59 | 81.66 | 152 | 24 | 302 | 73 |  |
| [ ] | `Xtate.StateMachine.Validator.Services.StateMachineValidator` | 53.21 | 65.33 | 130 | 2 | 173 | 25 |  |
| [ ] | `Xtate.Scxml.Services.XIncludeReader` | 50.41 | 50.12 | 116 | 8 | 216 | 22 |  |
| [ ] | `Xtate.Persistence.Services.StateMachinePersistingInterpreter` | 71.88 | 75.83 | 101 | 5 | 117 | 47 |  |
| [~] | `Xtate.DataTypes.DataModelList.Dynamic` | 0 | 0 | 100 | 0 | 138 | 10 | `DataModelDynamicCoverageTest` added; rerun coverage to verify. Includes ignored probes for case-insensitive dynamic member lookup and .NET Core indexed get behavior. |
| [~] | `Xtate.DataTypes.DataModelNumber` | 58.8 | 50.59 | 96 | 28 | 251 | 70 | `DataModelNumberCoverageTest` added; rerun coverage to verify. Includes ignored compact negative integer serialization probe. |
| [~] | `Xtate.ExtDictionary<TKey, TValue>` | 37.41 | 31.96 | 88 | 3 | 132 | 24 | `ExtCollectionCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.ResourceLoaders.Internal.DelegatedStream` | 12.5 | 14.97 | 84 | 0 | 142 | 44 | `DelegatedStreamCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.ExtCollection<TValue1, TValue2>` | 9.34 | 11.54 | 82 | 1 | 92 | 9 | `ExtCollectionCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.TaskMonitor.Services.TaskMonitor` | 33.33 | 35.71 | 72 | 8 | 117 | 14 |  |
| [ ] | `Xtate.StateMachineHost.Services.InProcEventScheduler` | 18.24 | 16.78 | 69 | 1 | 119 | 13 |  |
| [~] | `Xtate.IoProcessors.Http.Internal.QueryStringHelper` | 0 | 0 | 67 | 0 | 72 | 3 | `QueryStringHelperTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineHost.Services.ExternalServiceScopeManager` | 19.14 | 16.15 | 65 | 1 | 109 | 11 |  |
| [ ] | `Xtate.ExternalServices.HttpClient.Internal.HttpClientMimeTypeHandler` | 0 | 0 | 65 | 0 | 86 | 6 |  |
| [~] | `Xtate.BitConverterPolyfills` | 0 | 0 | 60 | 0 | 63 | 10 | `PolyfillsCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.SharedMemoryStreams.ReadWriteMemoryStream<TKey>` | 34.78 | 43.88 | 58 | 4 | 55 | 24 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathDataModelHandler` | 53.63 | 57.47 | 57 | 1 | 74 | 15 |  |
| [ ] | `Xtate.DataModel.Services.DataModelConverter` | 26.92 | 30.94 | 56 | 2 | 96 | 22 |  |
| [~] | `Xtate.DataTypes.DataModelValue.Dynamic` | 0 | 0 | 56 | 0 | 67 | 8 | `DataModelDynamicCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Http.Services.HttpClientFactory` | 0 | 0 | 56 | 0 | 62 | 6 |  |
| [ ] | `Xtate.StateMachineHost.Services.StateMachineExternalService` | 0 | 0 | 53 | 0 | 73 | 6 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.XPathObject` | 46.19 | 25.16 | 52 | 9 | 116 | 10 |  |
| [~] | `Xtate.IoProcessors.Http.Internal.CounterStream` | 0 | 0 | 52 | 0 | 90 | 17 | `HttpCounterStreamCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineFluentBuilder.TransitionFluentBuilder<TOuterBuilder>` | 21.54 | 23.08 | 51 | 0 | 70 | 15 |  |
| [ ] | `Xtate.Persistence.Extensions.BucketExtensions` | 63.06 | 60.7 | 47 | 5 | 90 | 24 |  |
| [~] | `Xtate.Scxml.Internal.TextContentReader` | 0 | 0 | 47 | 0 | 64 | 38 | `CommunicationAndUtilityCoverageTest` added for single-text-node reader state, XML surface, and unsupported members; rerun coverage to verify. |
| [ ] | `Xtate.ExternalServices.HttpClient.Internal.HttpClientFormUrlEncodedHandler` | 0 | 0 | 46 | 0 | 88 | 8 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathEngine` | 51.61 | 59.52 | 44 | 2 | 51 | 11 |  |
| [~] | `Xtate.Actions.ActionBase` | 16.67 | 13.04 | 43 | 4 | 100 | 6 | `ActionValueCoverageTest` added for array/string/integer/boolean/object helper paths and defaults; rerun coverage to verify. |
| [ ] | `Xtate.Logging.Services.Logger` | 61.4 | 62.93 | 43 | 2 | 76 | 5 |  |
| [ ] | `Xtate.StateMachineFluentBuilder.StateFluentBuilder<TOuterBuilder>` | 35.38 | 31.82 | 42 | 0 | 105 | 31 |  |
| [ ] | `Xtate.Persistence.Services.InvokeIdSetPersistingController` | 28.45 | 28.57 | 41 | 1 | 40 | 3 |  |
| [~] | `Xtate.LazyTask<T>` | 0 | 0 | 39 | 0 | 37 | 5 | `LazyTaskCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineHost.Services.StateMachineScopeManager` | 69.29 | 65.4 | 38 | 2 | 73 | 17 |  |
| [ ] | `Xtate.StateMachineHost.Services.ScxmlIoProcessor` | 5 | 7.79 | 38 | 0 | 71 | 9 |  |
| [~] | `Xtate.ResourceLoaders.Resource` | 52.44 | 50.91 | 38 | 2 | 54 | 9 | `ResourceCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Scxml.Services.ScxmlDirector` | 93.31 | 95.9 | 36 | 7 | 73 | 231 |  |
| [~] | `Xtate.IoC.ServiceArray.Internal.ReadOnlyList<T>` | 8.75 | 9.21 | 36 | 1 | 69 | 35 | `CommunicationAndUtilityCoverageTest` added via `ServiceSyncList<T>` for read-only list/collection/span/mutation paths; rerun coverage to verify. |
| [~] | `Xtate.ResourceLoaders.Extensions.StreamExtensions` | 2.7 | 2.99 | 36 | 0 | 65 | 3 | `ResourceCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.SharedMemoryStreams<TKey>` | 20 | 23.61 | 36 | 0 | 55 | 7 |  |
| [ ] | `Xtate.Actions.System.StartAction` | 50.62 | 60.45 | 36 | 8 | 53 | 6 |  |
| [~] | `Xtate.IoProcessors.NamedPipe.NamedPipeEventMessage` | 0 | 0 | 36 | 0 | 52 | 3 | Round-trip, event-field, invalid metadata, and ignored InvokeId regression coverage added in `PersistedEventCoverageTest`. |
| [~] | `Xtate.DataTypes.DataModelList.KeyValueByKeyEnumerator` | 0 | 0 | 36 | 0 | 34 | 5 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.KeyListPersistingController<T>` | 31 | 21.79 | 34 | 1 | 61 | 3 |  |
| [~] | `Xtate.Persistence.Internal.Encode` | 50.74 | 31.62 | 33 | 1 | 80 | 5 | `EncodeCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Persistence.PersistedRouterEvent` | 0 | 0 | 33 | 0 | 66 | 2 | Round-trip, optional target, invalid metadata, and ignored target-key regression coverage added in `PersistedEventCoverageTest`. |
| [ ] | `Xtate.Actions.System.DestroyAction` | 0 | 0 | 32 | 0 | 55 | 4 |  |
| [ ] | `Xtate.StateMachineHost.Services.ExternalServiceCollection` | 3.03 | 6.25 | 32 | 0 | 45 | 5 |  |
| [ ] | `Xtate.IoProcessors.Http.Services.HttpIoProcessorHost` | 0 | 0 | 32 | 0 | 44 | 6 |  |
| [ ] | `Xtate.StateMachineHost.Services.ExternalServiceClass` | 0 | 0 | 32 | 0 | 43 | 9 |  |
| [~] | `Xtate.Scxml.XIncludeException` | 0 | 0 | 31 | 0 | 62 | 6 | `XIncludeExceptionCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.ExternalServices.SmtpClient.Services.SmtpClientService` | 0 | 0 | 30 | 0 | 82 | 1 |  |
| [~] | `Xtate.ResourceLoaders.Internal.InjectedCancellationStream` | 18.42 | 16.67 | 30 | 2 | 80 | 14 | `ResourceCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineHost.Services.ExternalServiceRunner` | 0 | 0 | 30 | 0 | 68 | 7 |  |
| [ ] | `Xtate.Persistence.Services.DataModelListPersistingController` | 87.7 | 86.38 | 30 | 2 | 41 | 11 |  |
| [~] | `Xtate.DataTypes.DataModelList.ValueByKeyEnumerator` | 0 | 0 | 30 | 0 | 36 | 6 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.IoProcessors.IoProcessorHostBase` | 0 | 0 | 29 | 0 | 50 | 7 |  |
| [~] | `Xtate.IoProcessors.NamedPipe.NamedPipeResponseMessage` | 0 | 0 | 29 | 0 | 44 | 4 | Success/exception round-trips, optional fields, and invalid metadata covered in `PersistedEventCoverageTest`. |
| [ ] | `Xtate.Persistence.Services.StreamStorage` | 85.45 | 87.97 | 29 | 6 | 35 | 17 |  |
| [~] | `Xtate.ExternalServices.ExternalServiceBase` | 0 | 0 | 27 | 0 | 42 | 8 | `CommunicationAndUtilityCoverageTest` added for IoC-set source/parameters/token/task monitor, lazy result, and dispatch paths; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.StateMachineReader` | 62.19 | 81.78 | 26 | 249 | 133 | 43 |  |
| [~] | `Xtate.StateMachineHost.Services.ExternalCommunication` | 0 | 0 | 26 | 0 | 55 | 4 | `CommunicationAndUtilityCoverageTest` added for internal routing, immediate dispatch, delayed scheduling, cancel, and invalid type paths; rerun coverage to verify. |
| [ ] | `Xtate.IoProcessors.ResilientIoProcessorHostBase` | 0 | 0 | 26 | 0 | 39 | 1 |  |
| [ ] | `Xtate.StateMachineFluentBuilder.ParallelFluentBuilder<TOuterBuilder>` | 30.56 | 20.59 | 25 | 0 | 81 | 23 |  |
| [~] | `Xtate.IoProcessors.NamedPipe.NamedPipeIoProcessorOptions` | 0 | 0 | 25 | 0 | 35 | 5 | `IoProcessorOptionsCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.LazyTask` | 0 | 0 | 25 | 0 | 24 | 2 |  |
| [ ] | `System.HashCode` | 21.21 | 21.33 | 24 | 4 | 59 | 7 |  |
| [ ] | `Xtate.StateMachineHost.Services.ExternalServiceEventRouter` | 0 | 0 | 24 | 0 | 51 | 5 |  |
| [ ] | `Xtate.ExternalServices.HttpClient.Internal.HttpClientXmlHandler` | 0 | 0 | 24 | 0 | 50 | 6 |  |
| [~] | `Xtate.IoProcessors.NamedPipe.Internal.DelayedTries` | 0 | 0 | 24 | 0 | 26 | 4 | `DelayedTriesCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineHost.Services.ExternalServiceGlobalCollection` | 4 | 10.71 | 24 | 0 | 25 | 5 |  |
| [ ] | `Xtate.ResourceLoaders.Web.Services.WebResourceLoader` | 0 | 0 | 23 | 0 | 58 | 2 |  |
| [ ] | `Xtate.StateMachineHost.Services.ExternalServiceFactory` | 0 | 0 | 23 | 0 | 44 | 2 |  |
| [~] | `Xtate.Persistence.PersistedIncomingEvent` | 0 | 0 | 23 | 0 | 41 | 2 | Event-field round-trip, invalid metadata, and ignored InvokeId regression coverage added in `PersistedEventCoverageTest`. |
| [~] | `Xtate.StateMachineHost.RouterEvent` | 0 | 0 | 23 | 0 | 27 | 3 | `CommunicationAndUtilityCoverageTest` added for outgoing-event copy and routing fields; rerun coverage to verify. |
| [ ] | `Xtate.StateMachine.Services.StateMachineVisitor` | 96.62 | 92.49 | 22 | 20 | 72 | 148 |  |
| [ ] | `Xtate.StateMachine.Services.StateMachineVisitor.TrackList<T>` | 52.13 | 46.74 | 22 | 1 | 49 | 11 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathForEachEvaluator` | 0 | 0 | 22 | 0 | 38 | 2 |  |
| [~] | `Xtate.StateMachine.Internal.SegmentedName` | 60.32 | 72.65 | 22 | 6 | 32 | 5 | `SegmentedNameCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Services.OutgoingEventEntityParser` | 0 | 0 | 22 | 0 | 28 | 1 | Populated/default property enumeration, handler level, and incompatible entity behavior covered in `InterpreterServiceCoverageTest`. |
| [ ] | `Xtate.Scxml.Services.XmlDirector.PolicyBuilder<TDirector, TEntity>` | 75.28 | 69.66 | 22 | 0 | 27 | 19 |  |
| [ ] | `Xtate.IoBoundTask.Services.IoBoundTaskScheduler` | 81.25 | 80.31 | 22 | 1 | 25 | 13 |  |
| [ ] | `Xtate.Scxml.Services.DelegatedXmlReader` | 54.35 | 50.35 | 21 | 0 | 70 | 45 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathValueExpressionEvaluator` | 25 | 23.81 | 21 | 0 | 48 | 7 |  |
| [ ] | `Xtate.Persistence.Services.EntityQueuePersistingController<T>` | 33.82 | 33.33 | 21 | 3 | 26 | 3 |  |
| [~] | `Xtate.IoProcessors.Http.HttpIoProcessorOptions` | 0 | 0 | 21 | 0 | 18 | 5 | `IoProcessorOptionsCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Scxml.Services.XmlBaseReader` | 64.52 | 63.92 | 20 | 4 | 35 | 8 |  |
| [ ] | `Xtate.ExtCollection` | 0 | 0 | 20 | 0 | 34 | 3 |  |
| [~] | `Xtate.StateMachine.EventName` | 81.58 | 83.78 | 19 | 4 | 30 | 28 | `EventNameCoverageTest` added; rerun coverage to verify. Includes ignored default descriptor matching probe for current default ImmutableArray behavior. |
| [ ] | `Xtate.Scxml.Services.XmlDirector` | 75 | 83.33 | 19 | 0 | 17 | 5 |  |
| [~] | `Xtate.DataModel.Services.DefaultForEachEvaluator` | 41.18 | 39.71 | 18 | 4 | 41 | 5 | `DataConverterAndForEachCoverageTest` added for array iteration, item/index location assignment, action execution, and no-index path; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Services.InvokeDataVerboseEntityParser` | 5.26 | 5 | 18 | 0 | 38 | 3 |  |
| [ ] | `Xtate.StateMachineFluentBuilder.HistoryFluentBuilder<TOuterBuilder>` | 0 | 0 | 18 | 0 | 32 | 7 |  |
| [~] | `System.Collections.Concurrent.ConcurrentDictionaryPolyfills` | 0 | 0 | 18 | 0 | 23 | 3 | `PolyfillsCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Services.EventController` | 51.35 | 41.98 | 17 | 2 | 47 | 4 |  |
| [ ] | `Xtate.Interpreter.Services.InvokeController` | 43.33 | 48.05 | 17 | 0 | 40 | 3 |  |
| [ ] | `Xtate.Persistence.Services.SharedMemoryStreams.ReadOnlyMemoryStream<TKey>` | 0 | 0 | 17 | 0 | 27 | 2 |  |
| [ ] | `Xtate.IoProcessors.NamedPipe.Services.NamedPipeIoProcessor` | 0 | 0 | 16 | 0 | 34 | 4 |  |
| [ ] | `Xtate.StateMachineHost.Services.StateMachineHostNew` | 0 | 0 | 16 | 0 | 32 | 5 |  |
| [~] | `System.IO.StreamReaderPolyfills` | 0 | 0 | 16 | 0 | 26 | 1 | `PolyfillsCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.IoProcessors.Http.Internal.ReadLimitStream` | 0 | 0 | 16 | 0 | 19 | 3 | `HttpCounterStreamCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.AdapterBase` | 66 | 66.67 | 16 | 2 | 10 | 5 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.XPath.Services.XPathLocationExpression` | 38.46 | 50 | 16 | 0 | 8 | 4 |  |
| [~] | `Xtate.FullUri` | 16.67 | 13.33 | 15 | 0 | 39 | 11 | `FullUriCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineFluentBuilder.StateMachineFluentBuilder` | 61.54 | 61.45 | 15 | 0 | 32 | 15 |  |
| [~] | `Xtate.DataModel.Services.DataConverter` | 78.71 | 84.54 | 15 | 13 | 30 | 11 | `DataConverterAndForEachCoverageTest` added for content, params, resource, event, and exception conversion paths; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.InvokeEntity` | 44.44 | 59.38 | 15 | 0 | 26 | 4 | All source properties, ancestor/debug identity, and matching/differing reference comparisons covered in `StateMachineEntityStructuresCoverageTest`. |
| [ ] | `Xtate.DataModel.XPath.Internal.NodeAdapter` | 42.59 | 38.46 | 15 | 1 | 16 | 15 |  |
| [~] | `Xtate.DataTypes.DataModelList.EntryByKeyEnumerator` | 56.94 | 57.69 | 15 | 1 | 11 | 6 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineFluentBuilder.FinalFluentBuilder<TOuterBuilder>` | 57.58 | 50 | 14 | 0 | 30 | 13 |  |
| [ ] | `Xtate.DataTypes.Internal.MetaObject` | 0 | 0 | 14 | 0 | 27 | 13 |  |
| [ ] | `Xtate.ExternalServices.HttpClient.Internal.HttpClientJsonHandler` | 0 | 0 | 14 | 0 | 24 | 5 |  |
| [ ] | `Xtate.Logging.Internal.LoggingInterpolatedStringHandler` | 70 | 73.58 | 14 | 2 | 14 | 5 |  |
| [ ] | `Xtate.DataTypes.DataModelList.MetaValueAdapter` | 39.13 | 45.45 | 14 | 0 | 12 | 9 |  |
| [ ] | `Xtate.StateMachineHost.Services.IoProcessorBase` | 25 | 34.62 | 13 | 1 | 34 | 14 |  |
| [~] | `Xtate.DataModel.Services.DefaultExternalDataExpressionEvaluator` | 0 | 0 | 13 | 0 | 31 | 3 | `DataModelServiceCoverageTest` added for resource-loader and data-converter evaluation path; rerun coverage to verify. |
| [ ] | `Xtate.IoProcessors.Http.Services.HttpIoProcessor` | 0 | 0 | 13 | 0 | 27 | 4 |  |
| [~] | `Xtate.StateMachine.StateEntity` | 43.48 | 62.07 | 13 | 0 | 22 | 4 | All source properties, ancestor/debug identity, and matching/differing reference comparisons covered in `StateMachineEntityStructuresCoverageTest`. |
| [~] | `Xtate.StateMachine.ParallelEntity` | 38.1 | 60.38 | 13 | 0 | 21 | 4 | All source properties, ancestor/debug identity, and matching/differing reference comparisons covered in `StateMachineEntityStructuresCoverageTest`. |
| [ ] | `Xtate.StateMachineHost.Services.StateMachineCollection` | 58.06 | 54.76 | 13 | 0 | 19 | 6 |  |
| [~] | `Xtate.StateMachine.Validator.StateMachineValidationException` | 40.91 | 39.29 | 13 | 0 | 17 | 2 | `StateMachineValidationExceptionCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineHost.Services.StateMachineDestroyOnIdle.StateTracker` | 58.06 | 73.81 | 13 | 0 | 11 | 6 |  |
| [ ] | `Xtate.Http.Services.HttpClientFactory.HandlerEntry` | 0 | 0 | 12 | 0 | 33 | 4 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathLocationExpressionEvaluator` | 61.29 | 45.83 | 12 | 0 | 26 | 7 |  |
| [ ] | `Xtate.StateMachineHost.Services.ExternalEventDispatcher` | 0 | 0 | 12 | 0 | 26 | 1 |  |
| [ ] | `Xtate.Scxml.Services.XmlDirector<TDirector>` | 73.88 | 73.63 | 12 | 11 | 24 | 14 |  |
| [ ] | `Xtate.IoC.TransformArgs.Services.ServiceFactoryNewArgsSync<T, TArg, TNewArg>` | 0 | 0 | 12 | 0 | 21 | 1 |  |
| [~] | `Xtate.Logging.Provider.LoggingParameter` | 78.07 | 79.78 | 12 | 1 | 18 | 6 | Namespace/name/value formatting, span formatting, fallback values, and insufficient-buffer branches covered in `LoggingParameterCoverageTest`. |
| [~] | `Xtate.Http.JsonHttpContent` | 0 | 0 | 12 | 0 | 16 | 5 | `HttpContentCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Http.XmlHttpContent` | 0 | 0 | 12 | 0 | 16 | 5 | `HttpContentCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineHost.Services.StateMachineControllerBase` | 67.57 | 73.77 | 12 | 0 | 16 | 8 |  |
| [~] | `Xtate.Interpreter.Services.EventEntityParser` | 47.83 | 61.29 | 12 | 0 | 12 | 1 | All populated event properties, optional omissions, level, and incompatible entity behavior covered in `EntityParserCoverageTest`. |
| [~] | `Xtate.DataModel.Null.Services.NullDataModelHandler` | 45.45 | 38.78 | 11 | 2 | 30 | 7 | `NullDataModelHandlerCoverageTest` added for valid In-state expression handling and unsupported value/location/foreach/script/data-model/donedata errors; rerun coverage to verify. |
| [ ] | `Xtate.Scxml.Services.XmlDirector.Policy.ValidationContext<TDirector, TEntity>` | 80.41 | 83.54 | 11 | 7 | 27 | 8 |  |
| [~] | `Xtate.StateMachine.LazyId` | 80.33 | 76.92 | 11 | 2 | 21 | 15 | `LazyIdCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.StateMachineEntity` | 42.11 | 54.35 | 11 | 0 | 21 | 4 | Source copy, null guard, ancestor/debug identity, and matching/differing reference comparisons covered in `StateMachineEntityStructuresCoverageTest`. |
| [~] | `Xtate.DataModel.Runtime.Services.RuntimeDataModelHandler` | 62.07 | 42.86 | 11 | 0 | 20 | 5 | `RuntimeAndActionProviderCoverageTest` added for runtime predicate/value/action wrapping and unsupported script/data-model/entity errors; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.Validator.Services.DetailedErrorProcessor` | 0 | 0 | 11 | 0 | 13 | 3 | `DetailedErrorProcessorCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.XPath.Services.XPathExternalDataExpressionEvaluator` | 0 | 0 | 10 | 0 | 29 | 2 |  |
| [ ] | `Xtate.Interpreter.Model.StateNode` | 69.7 | 57.81 | 10 | 0 | 27 | 13 |  |
| [ ] | `Xtate.Interpreter.Model.StateEntityNode` | 80.19 | 67.11 | 10 | 1 | 25 | 17 |  |
| [ ] | `Xtate.ResourceLoaders.File.Services.FileResourceLoader` | 0 | 0 | 10 | 0 | 24 | 3 |  |
| [ ] | `Xtate.IoProcessors.NamedPipe.Services.NamedPipeIoProcessorHost` | 0 | 0 | 10 | 0 | 21 | 2 |  |
| [ ] | `Xtate.Persistence.Services.Bucket.DateTimeOffsetValueConverter<TValue>` | 0 | 0 | 10 | 0 | 17 | 3 |  |
| [~] | `Xtate.Interpreter.IncomingEvent` | 44.44 | 42.86 | 10 | 0 | 16 | 4 | Incoming- and outgoing-event copy constructors, copied/default properties, data, and ancestor behavior covered in `EntityParserCoverageTest`. |
| [ ] | `Xtate.Persistence.PersistedCustomActionNode` | 0 | 0 | 10 | 0 | 16 | 2 |  |
| [~] | `Xtate.DataTypes.DataModelList.KeyValue` | 30.77 | 3.7 | 9 | 0 | 26 | 7 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync<T, TArg>` | 18.18 | 16.13 | 9 | 0 | 26 | 11 |  |
| [ ] | `Xtate.Interpreter.Model.ParallelNode` | 68.97 | 55.36 | 9 | 0 | 25 | 12 |  |
| [ ] | `Xtate.IoC.TransformArgs.DependencyInjection.IServiceCollectionExtensions` | 10 | 10 | 9 | 0 | 18 | 10 |  |
| [ ] | `Xtate.Persistence.Services.StateMachinePersistedContext` | 75 | 85.05 | 9 | 7 | 16 | 13 |  |
| [~] | `Xtate.StateMachine.FinalEntity` | 30.77 | 55.17 | 9 | 0 | 13 | 4 | `StateMachineEntityStructuresCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.InMemoryStorageProvider` | 30.77 | 42.86 | 9 | 0 | 12 | 4 |  |
| [~] | `Xtate.StateMachine.TransitionEntity` | 35.71 | 66.67 | 9 | 0 | 12 | 3 | `StateMachineEntityStructuresCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.StateMachineHost.ScheduledEvent` | 0 | 0 | 9 | 0 | 12 | 5 | `StateMachineHostModelCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Actions.AsyncAction.Location` | 0 | 0 | 8 | 0 | 20 | 5 | `ActionValueCoverageTest` added for evaluator binding, get, set, and expression paths; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Services.OutgoingEventVerboseEntityParser` | 11.11 | 9.09 | 8 | 0 | 20 | 3 |  |
| [ ] | `Xtate.IoC.TransformArgs.Internal.ServiceSelectorAsync` | 11.11 | 5.88 | 8 | 0 | 16 | 9 |  |
| [ ] | `Xtate.StateMachineHost.Services.LocationChildStateMachine` | 0 | 0 | 8 | 0 | 15 | 5 |  |
| [~] | `Xtate.DataModel.Null.Services.NullConditionExpressionEvaluator` | 11.11 | 14.29 | 8 | 0 | 12 | 4 | `NullConditionExpressionEvaluatorCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.DataEntity` | 38.46 | 55.56 | 8 | 0 | 12 | 4 | `StateMachineEntityStructuresCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.HistoryEntity` | 27.27 | 50 | 8 | 0 | 11 | 4 | `StateMachineEntityStructuresCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Services.InvokeDataEntityParser` | 0 | 0 | 8 | 0 | 10 | 1 | Invoke type and present/absent source branches covered in `EntityParserCoverageTest`. |
| [ ] | `Xtate.IoC.TransformArgs.Internal.TransformArgs<T, TArg, TNewArg>` | 69.23 | 62.5 | 8 | 0 | 9 | 2 |  |
| [~] | `Xtate.DataTypes.DataModelList.KeyValueByKeyEnumerable` | 0 | 0 | 8 | 0 | 7 | 4 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.ValueByKeyEnumerable` | 0 | 0 | 8 | 0 | 7 | 4 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachine.Builder.Services.AssignBuilder` | 68 | 76.47 | 8 | 0 | 4 | 6 |  |
| [~] | `Xtate.IoC.Tools.DisposeToken` | 22.22 | 12 | 7 | 0 | 22 | 9 | `TokenAndLazyValueCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.StateMachineFluentBuilder.InitialFluentBuilder<TOuterBuilder>` | 0 | 0 | 7 | 0 | 19 | 4 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathExpressionContext` | 87.3 | 83.49 | 7 | 2 | 18 | 9 |  |
| [~] | `Xtate.StateMachine.EventDescriptors` | 53.12 | 50 | 7 | 1 | 18 | 16 | `CollectionTypesCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Internal.InvokeIdSet` | 63.64 | 57.89 | 7 | 2 | 16 | 7 |  |
| [~] | `Xtate.DataTypes.DataModelList.KeyEnumerator` | 0 | 0 | 7 | 0 | 15 | 7 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.IoProcessors.Http.Internal.WriteLimitStream` | 0 | 0 | 7 | 0 | 14 | 3 | `HttpCounterStreamCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.XPath.Internal.ItemNodeAdapter` | 82.79 | 83.33 | 7 | 7 | 13 | 10 |  |
| [~] | `Xtate.ExternalServices.HttpClient.Internal.HttpClientServiceOptions` | 0 | 0 | 7 | 0 | 13 | 2 | `HttpClientServiceOptionsCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.Services.CustomActionContainer` | 78.75 | 84 | 7 | 3 | 12 | 7 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.ListItemNodeAdapter` | 50 | 50 | 7 | 0 | 12 | 2 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.ListNodeAdapter` | 50 | 50 | 7 | 0 | 12 | 2 |  |
| [ ] | `Xtate.StateMachineHost.Services.ScxmlStringChildStateMachine` | 0 | 0 | 7 | 0 | 12 | 4 |  |
| [~] | `Xtate.DataTypes.LazyValue` | 65 | 61.9 | 7 | 0 | 8 | 4 | `TokenAndLazyValueCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataTypes.DataModelDateTime` | 90.29 | 82.8 | 6 | 21 | 43 | 61 |  |
| [~] | `Xtate.DataTypes.DataModelList.Entry` | 82.86 | 11.9 | 6 | 0 | 37 | 11 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Runtime.Runtime` | 62.5 | 41.67 | 6 | 0 | 28 | 11 | `RuntimeAndActionProviderCoverageTest` added for current-context data, arguments, in-state, log, event, and invoke facade calls; rerun coverage to verify. |
| [ ] | `System.SpanFormattablePolyfills` | 0 | 0 | 6 | 0 | 23 | 6 |  |
| [ ] | `Xtate.Interpreter.Model.InvokeNode` | 80.49 | 79.28 | 6 | 4 | 23 | 16 |  |
| [ ] | `Xtate.IoC.TransformArgs.Internal.ServiceSelectorSync<T, TArg>` | 0 | 0 | 6 | 0 | 17 | 6 |  |
| [ ] | `Xtate.Persistence.Services.InMemoryStorage` | 96.65 | 95.45 | 6 | 4 | 17 | 22 |  |
| [~] | `Xtate.StateMachine.Target` | 59.38 | 55.56 | 6 | 1 | 16 | 16 | `CollectionTypesCoverageTest` added; rerun coverage to verify. Includes ignored boxed equality probe for current `Target.Equals(object?)` type-check behavior. |
| [~] | `Xtate.DataModel.Services.DefaultContentBodyEvaluator` | 63.89 | 55.17 | 6 | 1 | 13 | 3 | `XPathEvaluatorCoverageTest` added for successful XML content-body evaluation/cache path; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.DefaultInlineContentEvaluator` | 63.89 | 55.17 | 6 | 1 | 13 | 3 | `XPathEvaluatorCoverageTest` added for successful XML inline-content evaluation/cache path; rerun coverage to verify. |
| [ ] | `Xtate.Scxml.Services.RedirectXmlResolver` | 63.89 | 80.3 | 6 | 1 | 13 | 5 |  |
| [ ] | `Xtate.IoC.Options.Services.OptionsAsyncImpl` | 41.67 | 58.62 | 6 | 2 | 12 | 1 |  |
| [ ] | `Xtate.Persistence.Services.Bucket.KeyHelper<T>` | 64.71 | 61.29 | 6 | 0 | 12 | 2 |  |
| [ ] | `Xtate.StateMachineHost.Services.SecurityContext` | 77.94 | 72.09 | 6 | 3 | 12 | 8 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathXmlParserContextFactory` | 45.83 | 50 | 6 | 1 | 11 | 1 |  |
| [ ] | `Xtate.Interpreter.Model.CustomActionNode` | 45.45 | 26.67 | 6 | 0 | 11 | 7 |  |
| [ ] | `Xtate.IAsyncEnumerableExtensions` | 57.14 | 56.52 | 6 | 0 | 10 | 2 |  |
| [ ] | `Xtate.StateMachineHost.Services.InternalEventDispatcher<TSource>` | 0 | 0 | 6 | 0 | 10 | 1 |  |
| [ ] | `Xtate.DataModel.DataModelHandlerBase` | 91.67 | 91.49 | 6 | 0 | 8 | 35 |  |
| [~] | `Xtate.StateMachine.ParamEntity` | 40 | 63.16 | 6 | 0 | 7 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.KeyValuePairEnumerator` | 72.73 | 76 | 6 | 0 | 6 | 5 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Services.DataModelValueEntityParser` | 0 | 0 | 6 | 0 | 6 | 1 | Defined, undefined, and incompatible value behavior covered in `EntityParserCoverageTest`. |
| [~] | `Xtate.StateMachine.DoneDataEntity` | 25 | 60 | 6 | 0 | 6 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Services.InvokeIdEntityParser` | 0 | 0 | 6 | 0 | 5 | 1 | Present and null identifier behavior covered in `EntityParserCoverageTest`. |
| [~] | `Xtate.Interpreter.Services.SendIdEntityParser` | 0 | 0 | 6 | 0 | 5 | 1 | Present and null identifier behavior covered in `EntityParserCoverageTest`. |
| [ ] | `JetBrains.Annotations.ContractAnnotationAttribute` | 0 | 0 | 6 | 0 | 4 | 2 |  |
| [~] | `Xtate.StateMachine.Identifier` | 79.69 | 93.88 | 6 | 1 | 3 | 10 | `TokenAndLazyValueCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DisposingToken` | 82.35 | 94.29 | 6 | 0 | 2 | 4 | `TokenAndLazyValueCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataTypes.DataModelValue.NumberValue` | 88.18 | 67.12 | 5 | 3 | 24 | 12 |  |
| [ ] | `Xtate.Interpreter.Services.EventVerboseEntityParser` | 44.44 | 27.27 | 5 | 0 | 16 | 3 |  |
| [ ] | `Xtate.Interpreter.Internal.StateMachineRuntimeError` | 80.36 | 71.15 | 5 | 1 | 15 | 12 |  |
| [ ] | `Xtate.Interpreter.Services.NoExternalConnections` | 0 | 0 | 5 | 0 | 15 | 5 |  |
| [ ] | `System.Threading.Tasks.ValueTaskPolyfills` | 16.67 | 6.67 | 5 | 0 | 14 | 6 |  |
| [ ] | `Xtate.Interpreter.Model.FinalNode` | 64.29 | 59.38 | 5 | 0 | 13 | 11 |  |
| [ ] | `Xtate.Persistence.Services.Bucket.UnsupportedConverter<T>` | 0 | 0 | 5 | 0 | 13 | 5 |  |
| [ ] | `Xtate.Interpreter.Model.StateMachineNode` | 76.19 | 73.33 | 5 | 0 | 12 | 10 |  |
| [ ] | `Xtate.StateMachine.Validator.Services.ErrorProcessorService<TSource>` | 50 | 63.64 | 5 | 2 | 12 | 1 |  |
| [ ] | `Xtate.IoProcessors.NamedPipe.DependencyInjection.NamedPipeIoProcessorModule` | 0 | 0 | 5 | 0 | 10 | 1 |  |
| [ ] | `Xtate.IoC.TransformArgs.Internal.ServiceSelectorSync` | 0 | 0 | 5 | 0 | 9 | 5 |  |
| [ ] | `Xtate.DataModel.Services.CustomActionFactory` | 78.26 | 75 | 5 | 0 | 8 | 1 |  |
| [ ] | `Xtate.IoC.TransformArgs.Internal.ArgsTransformerAsync<T, TArg, TNewArg>` | 0 | 0 | 5 | 0 | 8 | 5 |  |
| [ ] | `Xtate.IoC.TransformArgs.Services.ServiceFactoryNewArgsAsync<T, TArg, TNewArg>` | 54.17 | 61.9 | 5 | 1 | 8 | 1 |  |
| [ ] | `Xtate.DataModel.XPath.Functions.InFunction` | 70.83 | 84.78 | 5 | 4 | 7 | 4 |  |
| [ ] | `Xtate.IoC.Options.Internal.ConfigureSync<T>` | 0 | 0 | 5 | 0 | 6 | 2 |  |
| [~] | `Xtate.CancellationTokenRegistrationExtensions` | 0 | 0 | 5 | 0 | 5 | 2 | `TokenAndLazyValueCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.KeyValueEnumerator` | 81.48 | 78.26 | 5 | 0 | 5 | 5 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.ContentEntity` | 37.5 | 64.29 | 5 | 0 | 5 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.XPath.Internal.XPathStripRootsIterator` | 76.09 | 84.62 | 5 | 1 | 4 | 5 |  |
| [~] | `Xtate.StateMachine.DataModelEntity` | 16.67 | 50 | 5 | 0 | 4 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.ElseIfEntity` | 16.67 | 42.86 | 5 | 0 | 4 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.InitialEntity` | 16.67 | 42.86 | 5 | 0 | 4 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.OnEntryEntity` | 16.67 | 50 | 5 | 0 | 4 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.OnExitEntity` | 16.67 | 50 | 5 | 0 | 4 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.EntryEnumerator` | 81.48 | 81.25 | 5 | 0 | 3 | 6 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Scxml.Services.ScxmlDirector.XmlLineInfo` | 0 | 0 | 5 | 0 | 3 | 2 |  |
| [ ] | `Xtate.Scxml.Services.XIncludeReader.Strings` | 50 | 50 | 4 | 0 | 16 | 8 |  |
| [~] | `Xtate.StateMachine.SendId` | 43.75 | 36 | 4 | 1 | 16 | 8 | `TokenAndLazyValueCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.Services.DataModelConverter.JsonValueConverter` | 89.29 | 82.28 | 4 | 1 | 14 | 3 |  |
| [ ] | `Xtate.Ancestor.Ancestor<TEntity>` | 77.78 | 54.17 | 4 | 0 | 11 | 3 |  |
| [ ] | `Xtate.Interpreter.Model.EventDescriptorNode` | 43.75 | 42.11 | 4 | 1 | 11 | 8 |  |
| [~] | `Xtate.Actions.SyncAction.Location` | 43.75 | 50 | 4 | 1 | 10 | 5 | `ActionValueCoverageTest` added for evaluator binding, set, and expression paths; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Internal.KeyList<T>` | 50 | 41.18 | 4 | 1 | 10 | 6 |  |
| [ ] | `Xtate.IoC.TransformArgs.Services.ServiceFactoryNewArgsAsync` | 0 | 0 | 4 | 0 | 10 | 1 |  |
| [ ] | `Xtate.StateMachineHost.Services.SecurityContext.NoAccessTaskScheduler` | 20 | 18.18 | 4 | 0 | 9 | 5 |  |
| [~] | `Xtate.Interpreter.CommunicationException` | 0 | 0 | 4 | 0 | 8 | 4 | `SpecificExceptionCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.PlatformException` | 0 | 0 | 4 | 0 | 8 | 4 | `SpecificExceptionCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Services.InterpreterModelBuilder` | 98.56 | 98.29 | 4 | 3 | 8 | 54 |  |
| [ ] | `Xtate.StateMachineOptions.Services.StateMachineOptionsProvider` | 42.86 | 38.46 | 4 | 0 | 8 | 6 |  |
| [~] | `Xtate.DataModel.Services.DefaultLogEvaluator` | 71.43 | 76.67 | 4 | 0 | 7 | 2 | `EvaluatorBaseCoverageTest` added for enabled and disabled log-controller paths plus expression evaluation; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.KeyEnumerable` | 0 | 0 | 4 | 0 | 7 | 4 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.ExternalServices.HttpClient.DependencyInjection.HttpClientModule` | 0 | 0 | 4 | 0 | 6 | 1 |  |
| [ ] | `Xtate.ExternalServices.SmtpClient.DependencyInjection.SmtpClientModule` | 0 | 0 | 4 | 0 | 6 | 1 |  |
| [ ] | `Xtate.Persistence.Services.PersistedInterpreterModelGetter` | 94.03 | 95.08 | 4 | 0 | 6 | 4 |  |
| [ ] | `System.Xml.XmlWriterPolyfills` | 0 | 0 | 4 | 0 | 5 | 2 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.ElementNodeAdapter` | 88.46 | 89.58 | 4 | 1 | 5 | 7 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.TypeAttributeNodeAdapter` | 0 | 0 | 4 | 0 | 5 | 4 |  |
| [ ] | `Xtate.Interpreter.Services.StateMachineContext` | 89.29 | 94.51 | 4 | 1 | 5 | 7 |  |
| [ ] | `Xtate.Persistence.Services.SuspendEventDispatcher` | 50 | 58.33 | 4 | 1 | 5 | 3 |  |
| [~] | `Xtate.DataModel.Runtime.RuntimeAction` | 50 | 50 | 4 | 0 | 4 | 2 | `RuntimeAndActionProviderCoverageTest` added for sync/async delegate wrappers and null delegate guards; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Runtime.RuntimePredicate` | 55.56 | 55.56 | 4 | 0 | 4 | 3 | `RuntimeAndActionProviderCoverageTest` added for sync/async predicate wrappers and null delegate guards; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Runtime.RuntimeValue` | 60 | 63.64 | 4 | 0 | 4 | 4 | `RuntimeAndActionProviderCoverageTest` added for constant/sync/async value wrappers and null delegate guards; rerun coverage to verify. |
| [ ] | `Xtate.Logging.Services.ConsoleLogProvider.ConsoleTraceListener<TSource>` | 33.33 | 66.67 | 4 | 0 | 3 | 3 |  |
| [~] | `Xtate.StateMachine.FinalizeEntity` | 33.33 | 62.5 | 4 | 0 | 3 | 3 | Action copy, ancestor, and matching/differing reference comparisons covered in `StateMachineEntityStructuresCoverageTest`. |
| [ ] | `Xtate.Persistence.Services.Bucket` | 94.12 | 90.37 | 3 | 8 | 18 | 15 |  |
| [ ] | `Xtate.StateMachineHost.Services.ExternalServiceManager` | 0 | 0 | 3 | 0 | 15 | 3 |  |
| [ ] | `Xtate.ValueTaskExtensions` | 62.5 | 56.25 | 3 | 0 | 14 | 8 |  |
| [~] | `Xtate.DataModel.Services.UnknownDataModelHandler` | 0 | 0 | 3 | 0 | 12 | 3 | `DataModelServiceCoverageTest` added for unknown executable processing error path; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.DataModelReferenceTracker` | 93.48 | 89.74 | 3 | 6 | 12 | 10 |  |
| [ ] | `Xtate.Interpreter.Model.InitialNode` | 78.57 | 60 | 3 | 0 | 10 | 6 |  |
| [ ] | `Xtate.Class.LocationStateMachine` | 76.67 | 65.38 | 3 | 1 | 9 | 5 |  |
| [ ] | `Xtate.Interpreter.Services.InterpreterDebugLogEnricher` | 75 | 80 | 3 | 2 | 8 | 1 |  |
| [ ] | `Xtate.IoC.TransformArgs.Internal.TransformArgs` | 50 | 50 | 3 | 0 | 8 | 2 |  |
| [~] | `Xtate.DataTypes.DataModelList.ValueEnumerator` | 83.33 | 70.83 | 3 | 1 | 7 | 6 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Model.HistoryNode` | 75 | 65 | 3 | 0 | 7 | 6 |  |
| [ ] | `Xtate.Persistence.Services.Bucket.DateTimeValueConverter<TValue>` | 0 | 0 | 3 | 0 | 7 | 3 |  |
| [ ] | `Xtate.Persistence.Services.Bucket.DoubleValueConverter<TValue>` | 0 | 0 | 3 | 0 | 7 | 3 |  |
| [ ] | `System.Threading.Tasks.TaskCompletionSource` | 57.14 | 57.14 | 3 | 0 | 6 | 7 |  |
| [~] | `Xtate.DataModel.XPath.XPathDataModelException` | 0 | 0 | 3 | 0 | 6 | 3 | `SpecificExceptionCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Persistence.PersistenceException` | 0 | 0 | 3 | 0 | 6 | 3 | `SpecificExceptionCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.StateMachineHost.Exceptions.StateMachineSecurityException` | 0 | 0 | 3 | 0 | 6 | 3 | `SpecificExceptionCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.ExternalDataExpressionEvaluator` | 0 | 0 | 3 | 0 | 5 | 3 | `DataModelServiceCoverageTest` added for ancestor, URI, and object evaluation path; rerun coverage to verify. |
| [ ] | `Xtate.StackSpan<T>` | 83.33 | 79.17 | 3 | 1 | 5 | 9 |  |
| [ ] | `Xtate.Disposer` | 81.58 | 86.67 | 3 | 1 | 4 | 3 |  |
| [~] | `Xtate.Interpreter.StateMachineInterpreterState` | 78.57 | 76.47 | 3 | 0 | 4 | 7 | Display name, identity equality, operators, unrelated objects, and identity hash code covered in `EntityParserCoverageTest`. |
| [ ] | `Xtate.IoC.Tools.Services.SafeFactory` | 57.14 | 60 | 3 | 0 | 4 | 1 |  |
| [ ] | `Xtate.Logging.Services.Logger<TSource>` | 90.32 | 90.91 | 3 | 0 | 4 | 7 |  |
| [ ] | `Xtate.Ancestor.AncestorContainer` | 0 | 0 | 3 | 0 | 3 | 2 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.XmlnsXmlNodeAdapter` | 0 | 0 | 3 | 0 | 3 | 3 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathVarDescriptor` | 66.67 | 82.35 | 3 | 0 | 3 | 6 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathVarDescriptor.EmptyIterator` | 25 | 25 | 3 | 0 | 3 | 4 |  |
| [~] | `Xtate.Interpreter.Services.InterpreterStateParser` | 0 | 0 | 3 | 0 | 3 | 1 | State property, info level, and incompatible entity behavior covered in `EntityParserCoverageTest`. |
| [ ] | `Xtate.IoProcessors.Http.DependencyInjection.HttpIoProcessorModule` | 0 | 0 | 3 | 0 | 3 | 1 |  |
| [ ] | `Xtate.Interpreter.Model.ExternalScriptExpressionNode` | 78.57 | 85.71 | 3 | 0 | 2 | 4 |  |
| [~] | `Xtate.StateMachineHost.StateMachineStatus` | 85 | 90.48 | 3 | 0 | 2 | 6 | Initial/state-change/accepted completion plus forced completion, failure, and cancellation covered in `StateMachineHostModelCoverageTest`. |
| [ ] | `Xtate.DataTypes.Extensions.IConvertibleExtensions` | 46.88 | 46.88 | 2 | 13 | 17 | 16 |  |
| [ ] | `Xtate.ResourceLoaders.Web.Services.WebResourceLoader.Provider` | 0 | 0 | 2 | 0 | 17 | 2 |  |
| [~] | `Xtate.ExternalServices.ExternalServiceProviderBase<TService>` | 33.33 | 12.5 | 2 | 0 | 14 | 3 | `ExternalServiceProviderBaseCoverageTest` exercises generic provider matching and service creation; rerun coverage to verify. |
| [ ] | `Xtate.Scxml.Services.XmlDirector.Policy.ValidationContext` | 57.14 | 57.69 | 2 | 2 | 11 | 7 |  |
| [ ] | `Xtate.DataTypes.DataModelValue.Marker` | 60 | 30.77 | 2 | 0 | 9 | 4 |  |
| [~] | `Xtate.StateMachine.SessionId` | 73.08 | 76.92 | 2 | 3 | 9 | 13 | `TokenAndLazyValueCoverageTest` added for explicit/generated/null/equality paths; rerun coverage to verify. |
| [~] | `Xtate.DataModel.XPath.Internal.XPathSingleElementIterator` | 72.22 | 46.67 | 2 | 1 | 8 | 5 | `InterpreterCollectionCoverageTest` added for current/current-position/move-next/clone paths; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.XPath.Internal.DataModelXPathNavigator.Node` | 90.48 | 79.41 | 2 | 0 | 7 | 16 |  |
| [~] | `Xtate.DataTypes.DataModelList.ValueAdapter` | 91.3 | 77.42 | 2 | 0 | 7 | 9 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.Services.DataModelConverter.JsonListConverter` | 97.13 | 93.81 | 2 | 1 | 6 | 7 |  |
| [ ] | `Xtate.IoC.Options.DependencyInjection.IServiceCollectionExtensions` | 0 | 0 | 2 | 0 | 6 | 2 |  |
| [~] | `Xtate.Actions.AsyncAction.ArrayValue` | 0 | 0 | 2 | 0 | 5 | 2 | `ActionValueCoverageTest` added for array evaluator and object fallback paths; rerun coverage to verify. |
| [~] | `Xtate.Actions.AsyncAction.BooleanValue` | 0 | 0 | 2 | 0 | 5 | 2 | `ActionValueCoverageTest` added for boolean evaluator, object fallback, and default paths; rerun coverage to verify. |
| [~] | `Xtate.Actions.AsyncAction.IntegerValue` | 0 | 0 | 2 | 0 | 5 | 2 | `ActionValueCoverageTest` added for integer evaluator, object fallback, and default paths; rerun coverage to verify. |
| [~] | `Xtate.Actions.AsyncAction.ObjectValue` | 0 | 0 | 2 | 0 | 5 | 2 | `ActionValueCoverageTest` added for object evaluator and default paths; rerun coverage to verify. |
| [~] | `Xtate.Actions.SyncAction.ArrayValue` | 0 | 0 | 2 | 0 | 5 | 2 | `ActionValueCoverageTest` added for sync evaluation path; rerun coverage to verify. |
| [~] | `Xtate.Actions.SyncAction.BooleanValue` | 0 | 0 | 2 | 0 | 5 | 2 | `ActionValueCoverageTest` added for sync evaluation path; rerun coverage to verify. |
| [~] | `Xtate.Actions.SyncAction.IntegerValue` | 0 | 0 | 2 | 0 | 5 | 2 | `ActionValueCoverageTest` added for sync evaluation path; rerun coverage to verify. |
| [~] | `Xtate.Actions.SyncAction.StringValue` | 0 | 0 | 2 | 0 | 5 | 2 | `ActionValueCoverageTest` added for sync evaluation path; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.KeyValueAdapter` | 89.13 | 80.77 | 2 | 1 | 5 | 9 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.DataNode` | 86.67 | 84.85 | 2 | 0 | 5 | 8 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, evaluator capture, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.ParamNode` | 83.33 | 72.22 | 2 | 0 | 5 | 7 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.SendNode` | 87.5 | 84.85 | 2 | 0 | 5 | 16 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, delegated execution, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Actions.AsyncAction` | 60 | 60 | 2 | 0 | 4 | 5 | `ActionValueCoverageTest` added for IAction facade over values, locations, and execute; rerun coverage to verify. |
| [~] | `Xtate.Actions.SyncAction` | 90.91 | 90.91 | 2 | 0 | 4 | 8 | `ActionValueCoverageTest` added for IAction facade, value evaluation, location output, and reset flow; rerun coverage to verify. |
| [ ] | `Xtate.Class.ScxmlStreamStateMachine` | 0 | 0 | 2 | 0 | 4 | 2 |  |
| [~] | `Xtate.DataModel.Runtime.RuntimeAction.ActionAsync` | 0 | 0 | 2 | 0 | 4 | 2 | `RuntimeAndActionProviderCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Runtime.RuntimePredicate.EvaluatorAsync` | 0 | 0 | 2 | 0 | 4 | 2 | `RuntimeAndActionProviderCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Runtime.RuntimeValue.EvaluatorAsync` | 0 | 0 | 2 | 0 | 4 | 2 | `RuntimeAndActionProviderCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataModel.XPath.XPathFunctionBase` | 71.43 | 60 | 2 | 0 | 4 | 5 | `XPathEvaluatorCoverageTest` added for metadata, initialization, and invoke adapter paths; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.EntryByKeyEnumerable` | 75 | 42.86 | 2 | 0 | 4 | 4 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.EntryEnumerable` | 50 | 42.86 | 2 | 0 | 4 | 4 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.KeyValueEnumerable` | 50 | 42.86 | 2 | 0 | 4 | 4 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.KeyValuePairEnumerable` | 50 | 42.86 | 2 | 0 | 4 | 4 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.DataModelList.ValueEnumerable` | 50 | 42.86 | 2 | 0 | 4 | 4 | `DataModelListCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataTypes.LazyValue.NoArg` | 0 | 0 | 2 | 0 | 4 | 2 | `TokenAndLazyValueCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.AssignNode` | 75 | 75 | 2 | 0 | 4 | 8 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, delegated execution, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.CancelNode` | 60 | 60 | 2 | 0 | 4 | 5 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, delegated execution, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.DoneDataNode` | 88.24 | 90.24 | 2 | 0 | 4 | 7 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, data-converter evaluation, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.ElseIfNode` | 80 | 69.23 | 2 | 0 | 4 | 5 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.ElseNode` | 75 | 55.56 | 2 | 0 | 4 | 4 | `InterpreterNodeCoverageTest` added for ancestor forwarding, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.ForEachNode` | 71.43 | 71.43 | 2 | 0 | 4 | 7 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, delegated execution, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.IfNode` | 60 | 60 | 2 | 0 | 4 | 5 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, delegated execution, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.LogNode` | 60 | 60 | 2 | 0 | 4 | 5 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, delegated execution, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.OnEntryNode` | 80 | 71.43 | 2 | 0 | 4 | 5 | `InterpreterNodeCoverageTest` added for ancestor/action forwarding, action evaluator capture, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.OnExitNode` | 80 | 71.43 | 2 | 0 | 4 | 5 | `InterpreterNodeCoverageTest` added for ancestor/action forwarding, action evaluator capture, document id, and debug id; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.RaiseNode` | 50 | 50 | 2 | 0 | 4 | 4 | `InterpreterNodeCoverageTest` added for ancestor/property forwarding, delegated execution, document id, and debug id; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Model.TransitionNode` | 94.12 | 92.98 | 2 | 0 | 4 | 13 |  |
| [~] | `Xtate.Interpreter.OwnedXtateException` | 66.67 | 60 | 2 | 0 | 4 | 6 | `SpecificExceptionCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.StateMachineUnhandledErrorException` | 33.33 | 33.33 | 2 | 0 | 4 | 3 | `SpecificExceptionCoverageTest` extended; rerun coverage to verify. |
| [ ] | `Xtate.IoC.Options.Internal.ConfigureAsync<T>` | 0 | 0 | 2 | 0 | 4 | 2 |  |
| [ ] | `Xtate.IoC.TransformArgs.Internal.ArgsTransformerSync<T, TArg, TNewArg>` | 60 | 55.56 | 2 | 0 | 4 | 5 |  |
| [ ] | `Xtate.Persistence.Services.Bucket.EnumIndexKeyConverter<TKey>` | 96.88 | 91.11 | 2 | 0 | 4 | 4 |  |
| [~] | `Xtate.Persistence.StateMachineSuspendedException` | 33.33 | 33.33 | 2 | 0 | 4 | 3 | `SpecificExceptionCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.Internal.IdGenerator` | 66.67 | 69.23 | 2 | 0 | 4 | 6 | `SmallHelperCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.DefaultForEachEvaluator.IndexObject` | 0 | 0 | 2 | 0 | 3 | 2 | `DataConverterAndForEachCoverageTest` added for cached and uncached foreach index objects; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Model.DataModelNode` | 77.78 | 72.73 | 2 | 0 | 3 | 4 | `InterpreterNodeCoverageTest` added for ancestor/data forwarding, compiled data node list, and document id; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Services.InStateController` | 72.22 | 80 | 2 | 1 | 3 | 1 |  |
| [ ] | `Xtate.StateMachine.Services.StateMachineVisitor.VisitListData<T>` | 80.77 | 81.25 | 2 | 1 | 3 | 2 |  |
| [ ] | `Xtate.ConcurrentDictionaryExtensions` | 87.5 | 87.5 | 2 | 0 | 2 | 1 |  |
| [ ] | `Xtate.IoC.AncestorTracker.Services.AncestorFactory<T>` | 77.78 | 80 | 2 | 0 | 2 | 4 |  |
| [~] | `Xtate.StateMachine.ElseEntity` | 33.33 | 33.33 | 2 | 0 | 2 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [ ] | `Xtate.ExternalServices.DependencyInjection.ExternalServicesModule` | 0 | 0 | 2 | 0 | 1 | 1 |  |
| [~] | `Xtate.Interpreter.Internal.DocumentIdSlot` | 85.71 | 87.5 | 2 | 0 | 1 | 2 | `SmallHelperCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Services.StateMachineInterpreter.LiveLockDetector` | 92.86 | 94.44 | 2 | 0 | 1 | 3 |  |
| [~] | `Xtate.SpanFormattableExtensions` | 93.1 | 96.15 | 2 | 0 | 1 | 3 | `SmallHelperCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.XPath.Internal.AdapterFactory` | 81.4 | 61.22 | 1 | 14 | 19 | 5 |  |
| [ ] | `Xtate.StateMachineHost.Services.DeadLetterQueue<TSource>` | 0 | 0 | 1 | 0 | 12 | 1 |  |
| [ ] | `Xtate.DataTypes.DataModelValue.DateTimeValue` | 94.59 | 80 | 1 | 2 | 10 | 9 |  |
| [ ] | `Xtate.Scxml.Services.XmlDirector.QualifiedName<TDirector>` | 70 | 50 | 1 | 1 | 6 | 4 |  |
| [~] | `Xtate.Actions.AsyncAction.Value` | 75 | 37.5 | 1 | 0 | 5 | 3 | `ActionValueCoverageTest` added for evaluator binding and expression paths; rerun coverage to verify. |
| [~] | `Xtate.Actions.SyncAction.Value` | 75 | 37.5 | 1 | 0 | 5 | 3 | `ActionValueCoverageTest` added for evaluator binding and expression paths; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.DataModelHandlerService` | 90 | 79.17 | 1 | 0 | 5 | 1 | `DataModelServiceCoverageTest` added for first matching provider and unknown-handler fallback paths; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.InlineContentEvaluator` | 75 | 50 | 1 | 0 | 5 | 4 | `XPathEvaluatorCoverageTest` added for ancestor, string, value, and object evaluation paths; rerun coverage to verify. |
| [~] | `Xtate.ExternalServices.ExternalServiceProviderBase` | 0 | 0 | 1 | 0 | 5 | 1 | `ExternalServiceProviderBaseCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.Forward` | 0 | 0 | 1 | 0 | 4 | 1 | `SmallHelperCoverageTest` added for missing-service forwarding path; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Model.CompoundNode` | 85.71 | 63.64 | 1 | 0 | 4 | 4 |  |
| [ ] | `Xtate.IoC.ServiceArray.Internal.ServiceSyncListBuilder` | 0 | 0 | 1 | 0 | 4 | 1 |  |
| [ ] | `Xtate.ResourceLoaders.Resx.Services.ResxResourceLoader` | 90.91 | 83.33 | 1 | 0 | 4 | 4 |  |
| [ ] | `Xtate.ResourceLoaders.Services.ResourceLoaderService` | 92.31 | 86.21 | 1 | 0 | 4 | 1 |  |
| [ ] | `System.Net.Http.HttpContentPolyfills` | 0 | 0 | 1 | 0 | 3 | 1 |  |
| [ ] | `System.Xml.XmlWriterPolyfills.ConfiguredAwaitable` | 0 | 0 | 1 | 0 | 3 | 1 |  |
| [~] | `Xtate.CancellationTokenRegistrationExtensions.ConfiguredAwaitable` | 0 | 0 | 1 | 0 | 3 | 1 | `TokenAndLazyValueCoverageTest` added; rerun coverage to verify. |
| [ ] | `Xtate.DataTypes.DataModelList.KeyMetaValueAdapter` | 93.48 | 84.21 | 1 | 1 | 3 | 9 |  |
| [ ] | `Xtate.Interpreter.Model.IdentifierNode` | 78.57 | 80 | 1 | 1 | 3 | 7 |  |
| [ ] | `Xtate.IoC.AncestorTracker.Services.AncestorTracker` | 95.45 | 95 | 1 | 2 | 3 | 7 |  |
| [ ] | `Xtate.Persistence.Services.SharedMemoryStreams` | 50 | 50 | 1 | 0 | 3 | 2 |  |
| [~] | `Xtate.StateMachine.InvokeId` | 90 | 92.5 | 1 | 2 | 3 | 12 | `TokenAndLazyValueCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.SendEntity` | 96.77 | 96.15 | 1 | 0 | 3 | 4 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.XPath.Internal.AttributeNodeAdapter` | 70 | 80 | 1 | 1 | 2 | 5 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.NamespaceNodeAdapter` | 50 | 66.67 | 1 | 1 | 2 | 3 |  |
| [ ] | `Xtate.DataModel.XPath.Internal.XPathMetadata` | 83.33 | 81.82 | 1 | 1 | 2 | 1 |  |
| [ ] | `Xtate.DataModel.XPath.Services.XPathCompiledExpression` | 92.31 | 85.71 | 1 | 0 | 2 | 4 |  |
| [~] | `Xtate.ExternalServices.HttpClient.Services.HttpClientService.Provider` | 0 | 0 | 1 | 0 | 2 | 1 | `ExternalServiceProviderBaseCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.ExternalServices.SmtpClient.Services.SmtpClientService.Provider` | 0 | 0 | 1 | 0 | 2 | 1 | `ExternalServiceProviderBaseCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.StateMachineDestroyedException` | 66.67 | 66.67 | 1 | 0 | 2 | 3 | `SpecificExceptionCoverageTest` extended; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.Bucket.KeyConverterBase<TKey, TInternal>` | 66.67 | 75 | 1 | 0 | 2 | 3 |  |
| [ ] | `Xtate.Persistence.Services.OrderedSetPersistingController<T>` | 98.51 | 97.3 | 1 | 0 | 2 | 3 |  |
| [ ] | `Xtate.StateMachineFluentBuilder.StateFluentBuilder` | 0 | 0 | 1 | 0 | 2 | 1 |  |
| [ ] | `Xtate.StateMachineFluentBuilder.TransitionFluentBuilder` | 0 | 0 | 1 | 0 | 2 | 1 |  |
| [~] | `Xtate.DataModel.Services.AssignEvaluator` | 85.71 | 92.31 | 1 | 0 | 1 | 7 | `EvaluatorBaseCoverageTest` added for ancestor/property forwarding and execute override path; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.CancelEvaluator` | 75 | 85.71 | 1 | 0 | 1 | 4 | `EvaluatorBaseCoverageTest` added for ancestor/property forwarding and execute override path; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.CustomActionEvaluator` | 85.71 | 92.31 | 1 | 0 | 1 | 7 | `EvaluatorBaseCoverageTest` added for ancestor/property forwarding and execute override path; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.ForEachEvaluator` | 83.33 | 90.91 | 1 | 0 | 1 | 6 | `DataConverterAndForEachCoverageTest` added for base ancestor and property forwarding through `DefaultForEachEvaluator`; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.IfEvaluator` | 75 | 85.71 | 1 | 0 | 1 | 4 | `EvaluatorBaseCoverageTest` added for ancestor/property forwarding and execute override path; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.LogEvaluator` | 75 | 85.71 | 1 | 0 | 1 | 4 | `EvaluatorBaseCoverageTest` added for ancestor/property forwarding and execute override path; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.RaiseEvaluator` | 66.67 | 80 | 1 | 0 | 1 | 3 | `EvaluatorBaseCoverageTest` added for ancestor/property forwarding and execute override path; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.ScriptEvaluator` | 75 | 85.71 | 1 | 0 | 1 | 4 | `CommunicationAndUtilityCoverageTest` added via `DefaultScriptEvaluator` for ancestor/content/source exposure; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.SendEvaluator` | 93.33 | 96.55 | 1 | 0 | 1 | 15 | `EvaluatorBaseCoverageTest` added for ancestor/property forwarding and execute override path; rerun coverage to verify. |
| [ ] | `Xtate.DataTypes.DataModelValue.ObjectContainer` | 66.67 | 91.67 | 1 | 0 | 1 | 3 |  |
| [~] | `Xtate.DateTimeExtensions` | 90 | 91.67 | 1 | 0 | 1 | 1 | `DateTimeExtensionsTest` extended for stored-value-ahead branch; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Model.ScriptNode` | 75 | 85.71 | 1 | 0 | 1 | 4 |  |
| [~] | `Xtate.Interpreter.Services.NoStateMachineArguments` | 0 | 0 | 1 | 0 | 1 | 1 | `InterpreterServiceCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.ExternalDataExpression` | 83.33 | 85.71 | 1 | 0 | 1 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.ExternalScriptExpression` | 83.33 | 85.71 | 1 | 0 | 1 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [~] | `Xtate.StateMachine.ScriptExpression` | 83.33 | 85.71 | 1 | 0 | 1 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.Bucket.ValueHelper<T>` | 80.43 | 66.04 | 0 | 9 | 18 | 2 |  |
| [ ] | `Xtate.Logging.Services.TraceLogProvider` | 95.35 | 90.77 | 0 | 4 | 6 | 7 |  |
| [ ] | `System.Threading.Tasks.TaskPolyfills` | 58.33 | 88.57 | 0 | 10 | 4 | 4 |  |
| [~] | `Xtate.Interpreter.Internal.EntityQueue<T>` | 87.5 | 69.23 | 0 | 2 | 4 | 2 | `InterpreterCollectionCoverageTest` added for enqueue/dequeue events; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Services.AssemblyTypeInfo` | 62.5 | 76.47 | 0 | 3 | 4 | 1 | `SmallHelperCoverageTest` extended; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Extensions.BucketExtensions.EnumGetter<TKey>` | 50 | 42.86 | 0 | 1 | 4 | 1 |  |
| [ ] | `Xtate.Persistence.Services.Bucket.ValueConverterBase<TValue, TInternal>` | 88.89 | 75 | 0 | 2 | 4 | 3 |  |
| [~] | `Xtate.Actions.SyncAction.TypedValue<T>` | 83.33 | 70 | 0 | 1 | 3 | 3 | `ActionValueCoverageTest` added for evaluate, value access, and reset exception paths; rerun coverage to verify. |
| [ ] | `Xtate.DataModel.Services.DefaultIfEvaluator` | 96.25 | 95.65 | 0 | 3 | 3 | 2 |  |
| [ ] | `Xtate.DataModel.Services.DefaultSendEvaluator` | 97.14 | 97.27 | 0 | 2 | 3 | 2 |  |
| [ ] | `Xtate.Interpreter.Model.StateEntityNode.DocumentOrderComparer` | 81.25 | 83.33 | 0 | 3 | 3 | 3 |  |
| [ ] | `System.Threading.CancellationTokenSourcePolyfills` | 50 | 71.43 | 0 | 1 | 2 | 1 |  |
| [ ] | `Xtate.DataModel.Services.DefaultAssignEvaluator` | 92.31 | 93.1 | 0 | 2 | 2 | 2 |  |
| [ ] | `Xtate.DataModel.Services.DefaultCustomActionEvaluator` | 93.75 | 85.71 | 0 | 1 | 2 | 2 |  |
| [~] | `Xtate.DataModel.XPath.Services.XPathContentBodyEvaluator` | 75 | 81.82 | 0 | 1 | 2 | 2 | `XPathEvaluatorCoverageTest` added for XPath XML parsing path; rerun coverage to verify. |
| [~] | `Xtate.DataModel.XPath.Services.XPathInlineContentEvaluator` | 75 | 81.82 | 0 | 1 | 2 | 2 | `XPathEvaluatorCoverageTest` added for XPath XML parsing path; rerun coverage to verify. |
| [~] | `Xtate.Interpreter.Internal.OrderedSet<T>` | 98.84 | 96.3 | 0 | 1 | 2 | 9 | `InterpreterCollectionCoverageTest` added for add/add-if-not-exists/delete/clear/filter/sort paths; rerun coverage to verify. |
| [ ] | `Xtate.Persistence.Services.Bucket.StringKeyConverter<TString>` | 96.15 | 90.91 | 0 | 1 | 2 | 2 |  |
| [ ] | `Xtate.ResourceLoaders.Resx.Services.ResxResourceLoader.Provider` | 75 | 88.24 | 0 | 1 | 2 | 2 |  |
| [ ] | `Xtate.Scxml.Services.ScxmlLocationStateMachineGetter` | 97.37 | 94.74 | 0 | 1 | 2 | 5 |  |
| [ ] | `Xtate.Scxml.Services.ScxmlReaderStateMachineGetter` | 97.37 | 95 | 0 | 1 | 2 | 4 |  |
| [ ] | `Xtate.StateMachine.Builder.Services.ParallelBuilder` | 98.81 | 97.3 | 0 | 1 | 2 | 10 |  |
| [ ] | `Xtate.StateMachine.Services.StateMachineVisitor.VisitData<TEntity, TIEntity>` | 97.22 | 87.5 | 0 | 1 | 2 | 2 |  |
| [~] | `Xtate.Ancestor.AncestorArray` | 50 | 80 | 0 | 1 | 1 | 1 | `InterpreterCollectionCoverageTest` added for default, empty-default, null item, and ancestor mapping paths; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.ActionProvider<TCustomAction>` | 97.22 | 97.14 | 0 | 1 | 1 | 4 | `RuntimeAndActionProviderCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.CaseSensitivity` | 75 | 80 | 0 | 1 | 1 | 1 | `RuntimeAndActionProviderCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.ContentBodyEvaluator` | 87.5 | 90 | 0 | 1 | 1 | 4 | `RuntimeAndActionProviderCoverageTest` added; rerun coverage to verify. |
| [~] | `Xtate.DataModel.Services.DefaultScriptEvaluator` | 92.86 | 93.75 | 0 | 1 | 1 | 2 | `CommunicationAndUtilityCoverageTest` added for content-preferred and source-only evaluator execution; rerun coverage to verify. |
| [~] | `Xtate.DataModel.XPath.XPathFunctionProviderBase<TXPathFunction>` | 75 | 90 | 0 | 1 | 1 | 2 | `XPathEvaluatorCoverageTest` added for matching and non-matching function lookup; rerun coverage to verify. |
| [ ] | `Xtate.Interpreter.Services.InterpreterInfoLogEnricher` | 92.86 | 90.91 | 0 | 1 | 1 | 1 |  |
| [ ] | `Xtate.Interpreter.Services.InterpreterModelBuilder.EntityMap` | 90 | 85.71 | 0 | 1 | 1 | 2 |  |
| [ ] | `Xtate.IoC.Options.Services.OptionsAsyncImpl<T>` | 50 | 88.89 | 0 | 1 | 1 | 1 |  |
| [ ] | `Xtate.IoC.Tools.Services.DeferredFactory<T>` | 75 | 91.67 | 0 | 1 | 1 | 2 |  |
| [ ] | `Xtate.Persistence.Services.InMemoryStorage.Entry` | 96.67 | 94.12 | 0 | 1 | 1 | 3 |  |
| [ ] | `Xtate.ResourceLoaders.File.Services.FileResourceLoader.Provider` | 75 | 92.31 | 0 | 1 | 1 | 2 |  |
| [ ] | `Xtate.StateMachine.Builder.Services.FinalizeBuilder` | 90 | 91.67 | 0 | 1 | 1 | 2 |  |
| [ ] | `Xtate.StateMachine.Builder.Services.ForEachBuilder` | 97.06 | 95.24 | 0 | 1 | 1 | 5 |  |
| [ ] | `Xtate.StateMachine.Builder.Services.IfBuilder` | 94.44 | 93.33 | 0 | 1 | 1 | 3 |  |
| [~] | `Xtate.StateMachine.CancelEntity` | 87.5 | 92.86 | 0 | 2 | 1 | 3 | `StateMachineEntityStructuresCoverageTest` extended; rerun coverage to verify. |
| [ ] | `Xtate.StateMachine.EventDescriptor` | 92.86 | 96.15 | 0 | 1 | 1 | 7 |  |
| [ ] | `Xtate.StateMachineHost.Services.SecurityContextFactory` | 87.5 | 90.91 | 0 | 1 | 1 | 4 |  |
| [ ] | `Xtate.StateMachineHost.Services.StateMachineDestroyOnIdle` | 90 | 94.44 | 0 | 1 | 1 | 1 |  |
