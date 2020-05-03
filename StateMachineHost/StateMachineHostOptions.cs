﻿using System;
using System.Collections.Immutable;
using TSSArt.StateMachine.Annotations;

namespace TSSArt.StateMachine
{
	[PublicAPI]
	public class StateMachineHostOptions
	{
		public ImmutableArray<IIoProcessorFactory>      IoProcessorFactories      { get; set; }
		public ImmutableArray<IServiceFactory>          ServiceFactories          { get; set; }
		public ImmutableArray<IDataModelHandlerFactory> DataModelHandlerFactories { get; set; }
		public ImmutableArray<ICustomActionFactory>     CustomActionFactories     { get; set; }
		public ImmutableArray<IResourceLoader>          ResourceLoaders           { get; set; }
		public ImmutableDictionary<string, string>?     Configuration             { get; set; }
		public Uri?                                     BaseUri                   { get; set; }
		public ILogger?                                 Logger                    { get; set; }
		public PersistenceLevel                         PersistenceLevel          { get; set; }
		public IStorageProvider?                        StorageProvider           { get; set; }
		public TimeSpan                                 SuspendIdlePeriod         { get; set; }
		public bool                                     VerboseValidation         { get; set; }
	}
}