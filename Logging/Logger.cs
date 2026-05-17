// Copyright © 2019-2026 Sergii Artemenko
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

[InstantiatedByIoC]
public class Logger<TSource> : ILogger<TSource>
{
	private ImmutableArray<IEntityParserHandler<TSource>> _entityParserHandlers;

	private ImmutableArray<ILogEnricher<TSource>> _logEnrichers;

	public required IReadOnlyCollection<ILogWriter> NonGenericLogWriters { private get; [SetByIoC] init; }

	public required IReadOnlyCollection<ILogWriter<TSource>> LogWriters { private get; [SetByIoC] init; }

	public required IAsyncEnumerable<ILogEnricher<TSource>> LogEnrichers { private get; [SetByIoC] init; }

	public required IAsyncEnumerable<IEntityParserHandler<TSource>> EntityParserHandlers { private get; [SetByIoC] init; }

#region Interface ILogger

	public virtual bool IsEnabled(Level level)
	{
		foreach (var logWriter in LogWriters)
		{
			if (logWriter.IsEnabled(level))
			{
				return true;
			}
		}

		foreach (var logWriter in NonGenericLogWriters)
		{
			if (logWriter.IsEnabled(typeof(TSource), level))
			{
				return true;
			}
		}

		return false;
	}

	public virtual IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

#endregion

#region Interface ILogger<TSource>

	public virtual ValueTask Write(Level level, int eventId, string? message) => Write(level, eventId, message, formattedMessage: default, default(ValueTuple));

	public virtual ValueTask Write(Level level, int eventId, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage) =>
		Write(level, eventId, message: null, formattedMessage, default(ValueTuple));

	public virtual ValueTask Write<TEntity>(Level level,
											int eventId,
											string? message,
											TEntity entity) =>
		Write(level, eventId, message, formattedMessage: default, entity);

	public virtual ValueTask Write<TEntity>(Level level,
											int eventId,
											[InterpolatedStringHandlerArgument("", "level")]
											LoggingInterpolatedStringHandler formattedMessage,
											TEntity entity) =>
		Write(level, eventId, message: null, formattedMessage, entity);

#endregion

	private ValueTask Write<TEntity>(Level level,
									 int eventId,
									 string? message,
									 LoggingInterpolatedStringHandler formattedMessage,
									 TEntity entity)
	{
		ImmutableArray<LoggingParameter> messageParameters = default;

		if (message is null)
		{
			if (!IsEnabled(level))
			{
				return default;
			}

			message = formattedMessage.ToString(out messageParameters);
		}

		return Write(level, eventId, message, messageParameters, entity);
	}

	private async ValueTask Write<TEntity>(Level level,
										   int eventId,
										   string? message,
										   ImmutableArray<LoggingParameter> messageParameters,
										   TEntity entity)
	{
		if (_logEnrichers.IsDefault)
		{
			_logEnrichers = await LogEnrichers.ToImmutableArrayAsync().ConfigureAwait(false);
		}

		if (_entityParserHandlers.IsDefault)
		{
			_entityParserHandlers = await EntityParserHandlers.ToImmutableArrayAsync().ConfigureAwait(false);
		}

		foreach (var logWriter in LogWriters)
		{
			if (logWriter.IsEnabled(level))
			{
				var properties = entity is not ValueTuple ? EnumerateProperties(logWriter, entity) : null;
				var parameters = EnumerateParameters(logWriter, messageParameters, properties);

				await logWriter.Write(level, eventId, message, parameters).ConfigureAwait(false);
			}
		}

		foreach (var logWriter in NonGenericLogWriters)
		{
			if (logWriter.IsEnabled(typeof(TSource), level))
			{
				var properties = entity is not ValueTuple ? EnumerateProperties(logWriter, typeof(TSource), entity) : null;
				var parameters = EnumerateParameters(logWriter, typeof(TSource), messageParameters, properties);

				await logWriter.Write(typeof(TSource), level, eventId, message, parameters).ConfigureAwait(false);
			}
		}
	}

	private IEnumerable<LoggingParameter> EnumerateProperties<TEntity>(ILogWriter<TSource> logWriter, TEntity entity)
	{
		foreach (var entityParserHandler in _entityParserHandlers)
		{
			if (logWriter.IsEnabled(entityParserHandler.Level) && entityParserHandler.EnumerateProperties(entity) is { } enumerable)
			{
				foreach (var parameter in enumerable)
				{
					yield return parameter;
				}
			}
		}
	}

	private IEnumerable<LoggingParameter> EnumerateProperties<TEntity>(ILogWriter logWriter, Type source, TEntity entity)
	{
		foreach (var entityParserHandler in _entityParserHandlers)
		{
			if (logWriter.IsEnabled(source, entityParserHandler.Level) && entityParserHandler.EnumerateProperties(entity) is { } enumerable)
			{
				foreach (var parameter in enumerable)
				{
					yield return parameter;
				}
			}
		}
	}

	private IEnumerable<LoggingParameter> EnumerateParameters(ILogWriter<TSource> logWriter,
															  ImmutableArray<LoggingParameter> parameters = default,
															  IEnumerable<LoggingParameter>? entityProperties = null)
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
				yield return parameter with { Namespace = @"prop" };
			}
		}

		foreach (var enricher in _logEnrichers)
		{
			if (logWriter.IsEnabled(enricher.Level))
			{
				string? ns = null;

				if (enricher.EnumerateProperties() is { } properties)
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

	private IEnumerable<LoggingParameter> EnumerateParameters(ILogWriter logWriter,
															  Type source,
															  ImmutableArray<LoggingParameter> parameters = default,
															  IEnumerable<LoggingParameter>? entityProperties = null)
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
				yield return parameter with { Namespace = @"prop" };
			}
		}

		foreach (var enricher in _logEnrichers)
		{
			if (logWriter.IsEnabled(source, enricher.Level))
			{
				string? ns = null;

				if (enricher.EnumerateProperties() is { } properties)
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