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

	Level Level { get; }

	IEnumerable<LoggingParameter> EnumerateProperties();
}

[SuppressMessage(category: "ReSharper", checkId: "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
[SuppressMessage(category: "ReSharper", checkId: "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
public class Logger<TSource> : ILogger<TSource>
{
	public required ServiceList<ILogWriter> NonGenericLogWriters { private get; [UsedImplicitly] init; }

	public required ServiceList<ILogWriter<TSource>> LogWriters { private get; [UsedImplicitly] init; }

	public required ServiceList<ILogEnricher<TSource>> LogEnrichers { private get; [UsedImplicitly] init; }

	public required ServiceList<IEntityParserHandler<TSource>> EntityParserHandlers { private get; [UsedImplicitly] init; }

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
		Write(level, eventId, message: default, formattedMessage, default(ValueTuple));

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
		Write(level, eventId, message: default, formattedMessage, entity);

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
		foreach (var logWriter in LogWriters)
		{
			if (logWriter.IsEnabled(level))
			{
				var properties = entity is not ValueTuple ? EnumerateProperties(logWriter, entity) : default;
				var parameters = EnumerateParameters(logWriter, messageParameters, properties);

				await logWriter.Write(level, eventId, message, parameters).ConfigureAwait(false);
			}
		}

		foreach (var logWriter in NonGenericLogWriters)
		{
			if (logWriter.IsEnabled(typeof(TSource), level))
			{
				var properties = entity is not ValueTuple ? EnumerateProperties(logWriter, typeof(TSource), entity) : default;
				var parameters = EnumerateParameters(logWriter, typeof(TSource), messageParameters, properties);

				await logWriter.Write(typeof(TSource), level, eventId, message, parameters).ConfigureAwait(false);
			}
		}
	}

	private IEnumerable<LoggingParameter> EnumerateProperties<TEntity>(ILogWriter<TSource> logWriter, TEntity entity)
	{
		foreach (var entityParserHandler in EntityParserHandlers)
		{
			if (logWriter.IsEnabled(entityParserHandler.Level))
			{
				foreach (var parameter in entityParserHandler.EnumerateProperties(entity))
				{
					yield return parameter;
				}
			}
		}
	}

	private IEnumerable<LoggingParameter> EnumerateProperties<TEntity>(ILogWriter logWriter, Type source, TEntity entity)
	{
		foreach (var entityParserHandler in EntityParserHandlers)
		{
			if (logWriter.IsEnabled(source, entityParserHandler.Level))
			{
				foreach (var parameter in entityParserHandler.EnumerateProperties(entity))
				{
					yield return parameter;
				}
			}
		}
	}

	private IEnumerable<LoggingParameter> EnumerateParameters(ILogWriter<TSource> logWriter,
															  ImmutableArray<LoggingParameter> parameters = default,
															  IEnumerable<LoggingParameter>? entityProperties = default)
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

		foreach (var enricher in LogEnrichers)
		{
			if (logWriter.IsEnabled(enricher.Level))
			{
				string? ns = default;

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
															  IEnumerable<LoggingParameter>? entityProperties = default)
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

		foreach (var enricher in LogEnrichers)
		{
			if (logWriter.IsEnabled(source, enricher.Level))
			{
				string? ns = default;

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