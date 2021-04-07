#region Copyright © 2019-2021 Sergii Artemenko

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

namespace Xtate.Core
{
	public record InterpreterOptions
	{
		private readonly DataModelValue _arguments;
		private readonly DataModelValue _configuration;

		private readonly IErrorProcessor?   _errorProcessor;
		private readonly DataModelValue     _host;
		public static    InterpreterOptions Default { get; } = new();

		public ISecurityContext?                        SecurityContext           { get; init; }
		public ImmutableArray<IDataModelHandlerFactory> DataModelHandlerFactories { get; init; }
		public ImmutableArray<ICustomActionFactory>     CustomActionProviders     { get; init; }
		public ImmutableArray<IResourceLoaderFactory>   ResourceLoaderFactories   { get; init; }
		public ImmutableDictionary<object, object>?     ContextRuntimeItems       { get; init; }
		public IExternalCommunication?                  ExternalCommunication     { get; init; }
		public INotifyStateChanged?                     NotifyStateChanged        { get; init; }
		public CancellationToken                        SuspendToken              { get; init; }
		public CancellationToken                        StopToken                 { get; init; }
		public CancellationToken                        DestroyToken              { get; init; }
		public PersistenceLevel                         PersistenceLevel          { get; init; }
		public IStorageProvider?                        StorageProvider           { get; init; }
		public ILogger?                                 Logger                    { get; init; }
		public UnhandledErrorBehaviour                  UnhandledErrorBehaviour   { get; init; }
		public Uri?                                     BaseUri                   { get; init; }

		public DataModelValue Arguments
		{
			get => _arguments;
			init => _arguments = value.AsConstant();
		}

		public DataModelValue Host
		{
			get => _host;
			init => _host = value.AsConstant();
		}

		public DataModelValue Configuration
		{
			get => _configuration;
			init => _configuration = value.AsConstant();
		}

		public IErrorProcessor? ErrorProcessor
		{
			get => _errorProcessor;
			init => _errorProcessor = value is null or DefaultErrorProcessor or DetailedErrorProcessor or WrapperErrorProcessor ? value : new WrapperErrorProcessor(value);
		}
	}
}