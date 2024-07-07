// Copyright © 2019-2024 Sergii Artemenko
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

using System.Globalization;
using Xtate.IoC;

namespace Xtate.Core;

public class Logger<TSource> : ILogger<TSource>, IAsyncInitialization
{
	private readonly AsyncInit<ILogWriter?> _logWriterAsyncInit;

	public Logger() => _logWriterAsyncInit = AsyncInit.Run(this, logger => logger.LogWriterFactory(typeof(TSource)));

	public required Func<Type, ValueTask<ILogWriter?>> LogWriterFactory { private get; [UsedImplicitly] init; }

	public required IEntityParserHandler EntityParserHandler { private get; [UsedImplicitly] init; }

#region Interface IAsyncInitialization

	public Task Initialization => _logWriterAsyncInit.Task;

#endregion

#region Interface ILogger

	public virtual bool IsEnabled(Level level) => _logWriterAsyncInit.Value?.IsEnabled(level) ?? false;

	public virtual IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

#endregion

#region Interface ILogger<TSource>

	public virtual ValueTask Write(Level level, int eventId, string? message)
	{
		if (IsEnabled(level))
		{
			return _logWriterAsyncInit.Value!.Write(level, eventId, message);
		}

		return default;
	}

	public virtual ValueTask Write(Level level, int eventId, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage)
	{
		if (IsEnabled(level))
		{
			var message = formattedMessage.ToString(out var parameters);

			return _logWriterAsyncInit.Value!.Write(level, eventId, message, parameters);
		}

		return default;
	}

	public virtual ValueTask Write<TEntity>(Level level, int eventId, string? message, TEntity entity)
	{
		if (IsEnabled(level))
		{
			return _logWriterAsyncInit.Value!.Write(level, eventId, message, EntityParserHandler.EnumerateProperties(entity));
		}

		return default;
	}

	public virtual ValueTask Write<TEntity>(Level level, int eventId, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage, TEntity entity)
	{
		if (IsEnabled(level))
		{
			var message = formattedMessage.ToString(out var parameters);
			var loggingParameters = parameters.Concat(EntityParserHandler.EnumerateProperties(entity));

			return _logWriterAsyncInit.Value!.Write(level, eventId, message, loggingParameters);
		}

		return default;
	}

#endregion
}