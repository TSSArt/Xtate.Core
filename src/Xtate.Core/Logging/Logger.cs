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

namespace Xtate.Core;

public interface ILogEnricher<[UsedImplicitly] TSource>
{
	string? Namespace { get; }

	IAsyncEnumerable<LoggingParameter> EnumerateProperties(Level level, int eventId);
}

public class Logger<TSource> : ILogger<TSource>
{
	public required ILogWriter? NonGenericLogWriter { private get; [UsedImplicitly] init; }

	public required ILogWriter<TSource>? LogWriter { private get; [UsedImplicitly] init; }

	public required IAsyncEnumerable<ILogEnricher<TSource>> LogEnrichers { private get; [UsedImplicitly] init; }

	public required IEntityParserHandler<TSource> EntityParserHandler { private get; [UsedImplicitly] init; }

#region Interface ILogger

	public virtual bool IsEnabled(Level level) => LogWriter?.IsEnabled(level) ?? NonGenericLogWriter?.IsEnabled(typeof(TSource), level) ?? false;

	public virtual IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

#endregion

#region Interface ILogger<TSource>

	public virtual ValueTask Write(Level level, int eventId, string? message)
	{
		if (LogWriter?.IsEnabled(level) == true)
		{
			return LogWriter.Write(level, eventId, message, EnumerateParameters(level, eventId));
		}

		if (NonGenericLogWriter?.IsEnabled(typeof(TSource), level) == true)
		{
			return NonGenericLogWriter.Write(typeof(TSource), level, eventId, message, EnumerateParameters(level, eventId));
		}

		return default;
	}

	public virtual ValueTask Write(Level level, int eventId, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage)
	{
		if (LogWriter?.IsEnabled(level) == true)
		{
			var message = formattedMessage.ToString(out var parameters);

			return LogWriter.Write(level, eventId, message, EnumerateParameters(level, eventId, parameters));
		}

		if (NonGenericLogWriter?.IsEnabled(typeof(TSource), level) == true)
		{
			var message = formattedMessage.ToString(out var parameters);

			return NonGenericLogWriter.Write(typeof(TSource), level, eventId, message, EnumerateParameters(level, eventId, parameters));
		}

		return default;
	}

	public virtual ValueTask Write<TEntity>(Level level,
											int eventId,
											string? message,
											TEntity entity)
	{
		if (LogWriter?.IsEnabled(level) == true)
		{
			return LogWriter!.Write(level, eventId, message, EnumerateParameters(level, eventId, parameters: default, EntityParserHandler.EnumerateProperties(entity)));
		}

		if (NonGenericLogWriter?.IsEnabled(typeof(TSource), level) == true)
		{
			return NonGenericLogWriter.Write(typeof(TSource), level, eventId, message, EnumerateParameters(level, eventId, parameters: default, EntityParserHandler.EnumerateProperties(entity)));
		}

		return default;
	}

	public virtual ValueTask Write<TEntity>(Level level,
											int eventId,
											[InterpolatedStringHandlerArgument("", "level")]
											LoggingInterpolatedStringHandler formattedMessage,
											TEntity entity)
	{
		if (LogWriter?.IsEnabled(level) == true)
		{
			var message = formattedMessage.ToString(out var parameters);

			return LogWriter.Write(level, eventId, message, EnumerateParameters(level, eventId, parameters, EntityParserHandler.EnumerateProperties(entity)));
		}

		if (NonGenericLogWriter?.IsEnabled(typeof(TSource), level) == true)
		{
			var message = formattedMessage.ToString(out var parameters);

			return NonGenericLogWriter.Write(typeof(TSource), level, eventId, message, EnumerateParameters(level, eventId, parameters, EntityParserHandler.EnumerateProperties(entity)));
		}

		return default;
	}

#endregion

	private async IAsyncEnumerable<LoggingParameter> EnumerateParameters(Level level,
																		 int eventId,
																		 ImmutableArray<LoggingParameter> parameters = default,
																		 IAsyncEnumerable<LoggingParameter>? entityProperties = default)
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
			await foreach (var parameter in entityProperties.ConfigureAwait(false))
			{
				yield return parameter with { Namespace = @"prop" };
			}
		}

		await foreach (var enricher in LogEnrichers.ConfigureAwait(false))
		{
			string? ns = default;

			if (enricher.EnumerateProperties(level, eventId) is { } properties)
			{
				ns ??= enricher.Namespace ?? enricher.GetType().Name;

				await foreach (var parameter in properties.ConfigureAwait(false))
				{
					yield return parameter with { Namespace = ns };
				}
			}
		}
	}
}