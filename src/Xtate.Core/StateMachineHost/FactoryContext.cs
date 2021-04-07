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
using System.Threading.Tasks;

namespace Xtate.Core
{
	internal class FactoryContext : IFactoryContext
	{
		private readonly ILogger?        _logger;
		private readonly ILoggerContext? _loggerContext;

		public FactoryContext(ImmutableArray<IResourceLoaderFactory> resourceLoaderFactories,
							  ISecurityContext? securityContext,
							  ILogger? logger,
							  ILoggerContext? loggerContext)
		{
			_logger = logger;
			_loggerContext = loggerContext;
			SecurityContext = securityContext ?? Core.SecurityContext.NoAccess;
			ResourceLoaderFactories = resourceLoaderFactories;
		}

	#region Interface IFactoryContext

		public ImmutableArray<IResourceLoaderFactory> ResourceLoaderFactories { get; }

		public ISecurityContext SecurityContext { get; }

	#endregion

	#region Interface ILogEvent

		public ValueTask Log(LogLevel logLevel,
							 string? message = default,
							 DataModelValue arguments = default,
							 Exception? exception = default,
							 CancellationToken token = default) =>
			_logger?.ExecuteLog(_loggerContext, logLevel, message, arguments, exception, token) ?? default;

	#endregion
	}
}