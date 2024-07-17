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

public interface ILogEnricher<[UsedImplicitly]TSource>
{
	string? Namespace { get; }

	IEnumerable<LoggingParameter> EnumerateProperties(Level level, int eventId);
}

public class Logger<TSource> : ILogger<TSource>, IAsyncInitialization
{
	public required ILogWriter<TSource>?                    LogWriter   { private get; [UsedImplicitly]init; }

	public required IAsyncEnumerable<ILogEnricher<TSource>> LogEnrichers { private get; [UsedImplicitly]init; }

	private readonly AsyncInit<ImmutableArray<ILogEnricher<TSource>>>  _logEnrichers;

	public Logger() => _logEnrichers = AsyncInit.Run(this, logger => logger.LogEnrichers.ToImmutableArrayAsync());

	public required IEntityParserHandler EntityParserHandler { private get; [UsedImplicitly] init; }

#region Interface IAsyncInitialization

	public Task Initialization => _logEnrichers.Task;

#endregion

#region Interface ILogger

	public virtual bool IsEnabled(Level level) => LogWriter?.IsEnabled(level) ?? false;

	public virtual IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

#endregion

#region Interface ILogger<TSource>

	public virtual ValueTask Write(Level level, int eventId, string? message)
	{
		if (!IsEnabled(level))
		{
			return default;
		}

		return LogWriter!.Write(level, eventId, message, EnumerateParameters(level, eventId));
	}

	public virtual ValueTask Write(Level level, int eventId, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage)
	{
		if (!IsEnabled(level))
		{
			return default;
		}

		var message = formattedMessage.ToString(out var parameters);

		return LogWriter!.Write(level, eventId, message, EnumerateParameters(level, eventId, parameters));
	}

	public virtual ValueTask Write<TEntity>(Level level,
											int eventId,
											string? message,
											TEntity entity)
	{
		if (!IsEnabled(level))
		{
			return default;
		}

		return LogWriter!.Write(level, eventId, message, EnumerateParameters(level, eventId, default, EntityParserHandler.EnumerateProperties(entity)));
	}

	public virtual ValueTask Write<TEntity>(Level level,
											int eventId,
											[InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage,
											TEntity entity)
	{
		if (!IsEnabled(level))
		{
			return default;
		}
		
		var message = formattedMessage.ToString(out var parameters);

		return LogWriter!.Write(level, eventId, message, EnumerateParameters(level, eventId, parameters, EntityParserHandler.EnumerateProperties(entity)));
	}
#endregion
	
	private async IAsyncEnumerable<LoggingParameter> EnumerateParameters(Level level, int eventId, ImmutableArray<LoggingParameter> parameters = default, IEnumerable<LoggingParameter>? entityProperties = default)
	{
		if (!parameters.IsDefaultOrEmpty)
		{
			foreach (var parameter in parameters)
			{
				yield return parameter;
			}
		}

		if (entityProperties is not null)
		{
			foreach (var parameter in entityProperties)
			{
				yield return parameter;
			}
		}

		if (!_logEnrichers.Value.IsDefaultOrEmpty)
		{
			foreach (var enricher in _logEnrichers.Value)
			{
				string? ns = default;

				if (enricher.EnumerateProperties(level, eventId) is { } properties)
				{
					ns ??= enricher.Namespace ?? enricher.GetType().Name;

					foreach (var parameter in properties)
					{
						yield return parameter with { Namespace = ns };
					}
				}
			}
		}
	}
}