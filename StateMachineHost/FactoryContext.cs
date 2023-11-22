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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoC;

namespace Xtate.Core
{
	//TODO:delete class
	internal class FactoryContext : IFactoryContext, IAsyncInitialization
	{
		private readonly IAsyncEnumerable<IResourceLoaderFactory> _resourceLoaderFactories;
		private readonly ILoggerOld?                                 _logger;
		private readonly ILoggerContext?                          _loggerContext;

		public FactoryContext(ServiceLocator serviceLocator,
							  ImmutableArray<IResourceLoaderFactory> resourceLoaderFactories,
							  ISecurityContext? securityContext,
							  ILoggerOld? logger,
							  ILoggerContext? loggerContext)
		{
			_resourceLoaderFactories = serviceLocator.GetServices<IResourceLoaderFactory>();
			ResourceLoaderFactories = resourceLoaderFactories;
			_logger = logger;
			_loggerContext = loggerContext;
			SecurityContext = securityContext ?? Core.SecurityContext.NoAccess;

			Initialization = InitializeAsync();
		}

	#region Interface IFactoryContext

		public ImmutableArray<IResourceLoaderFactory> ResourceLoaderFactories { get; private set; }

		public ISecurityContext SecurityContext { get; }

	#endregion

	#region Interface ILogEvent

		public bool IsEnabled => throw new NotImplementedException();

		public async ValueTask Log(string? message = default, DataModelValue arguments = default) => throw new NotImplementedException();

		public ValueTask LogOld(LogLevel logLevel,
								string? message = default,
								DataModelValue arguments = default,
								Exception? exception = default) =>
			_logger?.ExecuteLogOld(logLevel, message, arguments, exception) ?? default;

	#endregion

		public Task Initialization { get; }

		private async Task InitializeAsync()
		{
			var builder = ImmutableArray.CreateBuilder<IResourceLoaderFactory>();

			await foreach (var resourceLoaderFactory in _resourceLoaderFactories.ConfigureAwait(false))
			{
				builder.Add(resourceLoaderFactory);
			}

			ResourceLoaderFactories = builder.ToImmutable();
		}
	}
}