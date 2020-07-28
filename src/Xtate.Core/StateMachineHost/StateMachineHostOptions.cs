#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 
#endregion

using System;
using System.Collections.Immutable;
using Xtate.Annotations;
using Xtate.CustomAction;
using Xtate.DataModel;
using Xtate.IoProcessor;
using Xtate.Persistence;
using Xtate.Service;

namespace Xtate
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
		public UnhandledErrorBehaviour                  UnhandledErrorBehaviour   { get; set; }
	}
}