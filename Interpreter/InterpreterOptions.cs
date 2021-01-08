#region Copyright © 2019-2020 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Immutable;
using System.Threading;
using Xtate.CustomAction;
using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate
{
	public sealed class InterpreterOptions
	{
		public static InterpreterOptions Default { get; } = new();

		public ImmutableArray<IDataModelHandlerFactory> DataModelHandlerFactories { get; set; }
		public ImmutableArray<ICustomActionFactory>     CustomActionProviders     { get; set; }
		public ImmutableArray<IResourceLoaderFactory>   ResourceLoaderFactories   { get; set; }
		public SecurityContext?                         SecurityContext           { get; set; }
		public DataModelList?                           Host                      { get; set; }
		public DataModelList?                           Configuration             { get; set; }
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
		public Uri?                                     BaseUri                   { get; set; }

		public InterpreterOptions Clone() => (InterpreterOptions) MemberwiseClone();
	}
}