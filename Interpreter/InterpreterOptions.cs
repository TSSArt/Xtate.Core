using System.Collections.Immutable;
using System.Threading;
using Xtate.CustomAction;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	public struct InterpreterOptions
	{
		public ImmutableArray<IDataModelHandlerFactory> DataModelHandlerFactories { get; set; }
		public ImmutableArray<ICustomActionFactory>     CustomActionProviders     { get; set; }
		public ImmutableArray<IResourceLoader>          ResourceLoaders           { get; set; }
		public DataModelObject?                         Host                      { get; set; }
		public DataModelObject?                         Configuration             { get; set; }
		public ImmutableDictionary<object, object>?     ContextRuntimeItems       { get; set; }
		public DataModelValue                           Arguments                 { get; set; }
		public IExternalCommunication?                  ExternalCommunication     { get; set; }
		public INotifyStateChanged?                     NotifyStateChanged        { get; set; }
		public CancellationToken                        SuspendToken              { get; set; }
		public CancellationToken                        StopToken                 { get; set; }
		public CancellationToken                        DestroyToken              { get; set; }
		public PersistenceLevel                         PersistenceLevel          { get; set; }
		public IStorageProvider?                        StorageProvider           { get; set; }
		public ILogger?                                 Logger                    { get; set; }
		public IErrorProcessor?                         ErrorProcessor            { get; set; }
		public UnhandledErrorBehaviour                  UnhandledErrorBehaviour   { get; set; }
	}
}