#region Copyright © 2019-2023 Sergii Artemenko

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

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Xtate.DataModel;

namespace Xtate.Core;

public enum Level
{
	Error,
	Warning,
	Info,
	Debug,
	Trace,
	Verbose
}

public class LogEntityParserService : IEntityParserHandler
{
	public required IEnumerable<IEntityParserProvider> Providers { private get; [UsedImplicitly] init; }

#region Interface IEntityParserHandler

	public IEnumerable<LoggingParameter> EnumerateProperties<T>(T entity)
	{
		foreach (var provider in Providers)
		{
			if (provider.TryGetEntityParserHandler(entity) is { } handler)
			{
				return handler.EnumerateProperties(entity);
			}
		}

		throw new InfrastructureException(Res.Format(Resources.Exception_CantFindEntityParser, typeof(T)));
	}

#endregion
}

public interface IEntityParserProvider
{
	IEntityParserHandler? TryGetEntityParserHandler<T>(T entity);
}

public interface IEntityParserHandler
{
	IEnumerable<LoggingParameter> EnumerateProperties<T>(T entity);
}

public interface ILogEntityParser<in TEntity>
{
	IEnumerable<(string Name, object Value)> EnumerateProperties(TEntity entity);
}

public readonly struct LoggingParameter
{
	public LoggingParameter(string name, object? value)
	{
		Name = name;
		Value = value;
	}

	public LoggingParameter(string name, object? value, string? format)
	{
		Name = name;
		Format = format;
		Value = value;
	}

	public string  Name   { get; }
	public object? Value  { get; }
	public string? Format { get; }
}

[InterpolatedStringHandler]
public readonly struct LoggingInterpolatedStringHandler
{
	private readonly ImmutableArray<LoggingParameter>.Builder? _parametersBuilder;
	private readonly IFormatProvider?                          _provider;
	private readonly StringBuilder?                            _stringBuilder;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public LoggingInterpolatedStringHandler(int literalLength,
											int formattedCount,
											ILogger logger,
											Level level,
											out bool shouldFormat)
	{
		if (logger.IsEnabled(level))
		{
			if (formattedCount > 0)
			{
				_provider = logger.FormatProvider;
				_parametersBuilder = ImmutableArray.CreateBuilder<LoggingParameter>(formattedCount);
			}

			_stringBuilder = new StringBuilder(literalLength + formattedCount * 16);
			shouldFormat = true;
		}
		else
		{
			shouldFormat = false;
		}
	}

	public string ToString(out ImmutableArray<LoggingParameter> parameters)
	{
		parameters = _parametersBuilder?.MoveToImmutable() ?? default;
		var result = _stringBuilder!.ToString();

		return result;
	}

	public void AppendLiteral(string value) => _stringBuilder!.Append(value);

	[SuppressMessage("Style", "IDE0038:Use pattern matching", Justification = "Avoid boxing if T is struct")]
	[SuppressMessage("ReSharper", "MergeCastWithTypeCheck", Justification = "Avoid boxing if T is struct")]
	private string? ToStringFormatted<T>(T value, string? format)
	{
		if (_provider is not null && _provider.GetType() != typeof(CultureInfo) && _provider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter customFormatter)
		{
			customFormatter.Format(format, value, _provider);
		}

		if (value is IFormattable)
		{
			return ((IFormattable) value).ToString(format, _provider);
		}

		return value is not null ? value.ToString() : default;
	}

	public void AppendFormatted<T>(T value, string? format = default, [CallerArgumentExpression(nameof(value))] string? expression = default)
	{
		if (ToStringFormatted(value, format) is { } str)
		{
			_stringBuilder!.Append(str);
		}

		_parametersBuilder!.Add(new LoggingParameter(expression!, value, format));
	}

	public void AppendFormatted<T>(T value,
								   int alignment,
								   string? format = default,
								   [CallerArgumentExpression(nameof(value))]
								   string? expression = default)
	{
		var start = _stringBuilder!.Length;

		AppendFormatted(value, format, expression);

		if (Math.Abs(alignment) - (_stringBuilder.Length - start) is var paddingRequired and > 0)
		{
			if (alignment < 0)
			{
				_stringBuilder.Append(value: ' ', paddingRequired);
			}
			else
			{
				_stringBuilder.Insert(start, value: @" ", paddingRequired);
			}
		}
	}

	public void AppendFormatted(object? value,
								int alignment = 0,
								string? format = null,
								[CallerArgumentExpression(nameof(value))]
								string? expression = default) =>
		AppendFormatted<object?>(value, alignment, format, expression);
}

public interface ILogger
{
	IFormatProvider? FormatProvider { get; }

	bool IsEnabled(Level level);
}

public interface ILogger<[UsedImplicitly] TSource> : ILogger
{
	ValueTask Write(Level level, string? message);

	ValueTask Write<TEntity>(Level level, string? message, TEntity entity);

	ValueTask Write(Level level, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage);

	ValueTask Write<TEntity>(Level level, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage, TEntity entity);
}

public interface ILogWriter
{
	bool IsEnabled(Level level);

	ValueTask Write(Level level,
					string source,
					string? message,
					IEnumerable<LoggingParameter>? parameters = default);
}

public class TraceLogWriter : ILogWriter
{
#region Interface ILogWriter

	public virtual bool IsEnabled(Level level) => true;

	public ValueTask Write(Level level,
						   string source,
						   string? message,
						   IEnumerable<LoggingParameter>? parameters)
	{
		Trace.WriteLine(
			string.IsNullOrWhiteSpace(message)
				? $@"[{DateTime.Now:u}] [{source}] {level}"
				: $@"[{DateTime.Now:u}] [{source}] {level}: {message}");

		if (parameters is not null)
		{
			foreach (var parameter in parameters)
			{
				Trace.WriteLine($@"[{DateTime.Now:u}] [{source}] {parameter.Name}: {parameter.Value}");
			}
		}

		return default;
	}

#endregion
}

public class FileLogWriter(string file) : ILogWriter
{
	private readonly object _lock = new ();

	#region Interface ILogWriter

	public virtual bool IsEnabled(Level level) => true;

	public ValueTask Write(Level level,
						   string source,
						   string? message,
						   IEnumerable<LoggingParameter>? parameters)
	{
		lock (_lock)
		{
			File.AppendAllText(
				file, string.IsNullOrWhiteSpace(message)
					? @$"[{DateTime.Now:u}] [{source}] {level}\r\n"
					: @$"[{DateTime.Now:u}] [{source}] {level}: {message}\r\n");

			if (parameters is not null)
			{
				foreach (var parameter in parameters)
				{
					File.AppendAllText(file, @$"[{DateTime.Now:u}] [{source}] {parameter.Name}: {parameter.Value}\r\n");
				}
			}

			return default;
		}
	}

#endregion
}

public class Logger<TSource> : ILogger<TSource>
{
	public required IEntityParserHandler EntityParserHandler { private get; [UsedImplicitly] init; }

	public required ILogWriter? LogWriter { private get; [UsedImplicitly] init; }

	private static string Source => typeof(TSource).Name;

#region Interface ILogger

	public virtual bool IsEnabled(Level level) => LogWriter?.IsEnabled(level) ?? false;

	public virtual IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

#endregion

#region Interface ILogger<TSource>

	public virtual ValueTask Write(Level level, string? message)
	{
		if (IsEnabled(level))
		{
			return LogWriter!.Write(level, Source, message);
		}

		return default;
	}

	public virtual ValueTask Write(Level level, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage)
	{
		if (IsEnabled(level))
		{
			var message = formattedMessage.ToString(out var parameters);

			return LogWriter!.Write(level, Source, message, parameters);
		}

		return default;
	}

	public virtual ValueTask Write<TEntity>(Level level, string? message, TEntity entity)
	{
		if (IsEnabled(level))
		{
			return LogWriter!.Write(level, Source, message, EntityParserHandler.EnumerateProperties(entity));
		}

		return default;
	}

	public virtual ValueTask Write<TEntity>(Level level, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage, TEntity entity)
	{
		if (IsEnabled(level))
		{
			var message = formattedMessage.ToString(out var parameters);
			var loggingParameters = parameters.Concat(EntityParserHandler.EnumerateProperties(entity));

			return LogWriter!.Write(level, Source, message, loggingParameters);
		}

		return default;
	}

#endregion
}

public class StateMachineInterpreterTracerParser : ILogEntityParser<SendId>, ILogEntityParser<InvokeId>, ILogEntityParser<InvokeData>, ILogEntityParser<IOutgoingEvent>, ILogEntityParser<IEvent>,
												   ILogEntityParser<IStateEntity>, ILogEntityParser<StateMachineInterpreterState>
{
#region Interface ILogEntityParser<IEvent>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(IEvent evt)
	{
		Infra.Requires(evt);

		if (!evt.NameParts.IsDefaultOrEmpty)
		{
			yield return (@"EventName", EventName.ToName(evt.NameParts));
		}

		yield return (@"EventType", evt.Type);

		if (evt.Origin is { } origin)
		{
			yield return (@"Origin", origin);
		}

		if (evt.OriginType is { } originType)
		{
			yield return (@"OriginType", originType);
		}

		if (evt.SendId is { } sendId)
		{
			yield return (@"SendId", sendId);
		}

		if (evt.InvokeId is { } invokeId)
		{
			yield return (@"InvokeId", invokeId);
		}
	}

#endregion

#region Interface ILogEntityParser<InvokeData>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(InvokeData invokeData)
	{
		Infra.Requires(invokeData);

		if (invokeData.InvokeId is { } invokeId)
		{
			yield return (@"InvokeId", invokeId);
		}

		yield return (@"InvokeType", invokeData.Type);

		if (invokeData.Source is { } source)
		{
			yield return (@"InvokeSource", source);
		}
	}

#endregion

#region Interface ILogEntityParser<InvokeId>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(InvokeId invokeId)
	{
		yield return (@"InvokeId", invokeId);
	}

#endregion

#region Interface ILogEntityParser<IOutgoingEvent>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(IOutgoingEvent outgoingEvent)
	{
		Infra.Requires(outgoingEvent);

		if (!outgoingEvent.NameParts.IsDefaultOrEmpty)
		{
			yield return (@"EventName", EventName.ToName(outgoingEvent.NameParts));
		}

		if (outgoingEvent.Type is { } type)
		{
			yield return (@"EventType", type);
		}

		if (outgoingEvent.Target is { } target)
		{
			yield return (@"EventTarget", target);
		}

		if (outgoingEvent.SendId is { } sendId)
		{
			yield return (@"SendId", sendId);
		}

		if (outgoingEvent.DelayMs > 0)
		{
			yield return (@"Delay", outgoingEvent.DelayMs);
		}
	}

#endregion

#region Interface ILogEntityParser<IStateEntity>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(IStateEntity stateEntity)
	{
		Infra.Requires(stateEntity);

		if (stateEntity.Id is { } stateId)
		{
			yield return (@"StateId", stateId);
		}
	}

#endregion

#region Interface ILogEntityParser<SendId>

	public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(SendId sendId)
	{
		yield return (@"SendId", sendId);
	}

#endregion

#region Interface ILogEntityParser<StateMachineInterpreterState>

	public IEnumerable<(string Name, object Value)> EnumerateProperties(StateMachineInterpreterState state)
	{
		yield return (@"InterpreterState", state);
	}

#endregion
}

public class StateMachineInterpreterTracerVerboseParser : StateMachineInterpreterTracerParser
{
	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	public override IEnumerable<(string Name, object Value)> EnumerateProperties(InvokeData invokeData)
	{
		Infra.Requires(invokeData);

		foreach (var property in base.EnumerateProperties(invokeData))
		{
			yield return property;
		}

		if (invokeData.RawContent is { } rawContent)
		{
			yield return (@"RawContent", rawContent);
		}

		if (!invokeData.Content.IsUndefined())
		{
			yield return (@"Content", invokeData.Content.ToObject()!);
			yield return (@"ContentText", DataModelHandler.ConvertToText(invokeData.Content));
		}

		if (!invokeData.Parameters.IsUndefined())
		{
			yield return (@"Parameters", invokeData.Parameters.ToObject()!);
			yield return (@"ParametersText", DataModelHandler.ConvertToText(invokeData.Parameters));
		}
	}

	public override IEnumerable<(string Name, object Value)> EnumerateProperties(IOutgoingEvent outgoingEvent)
	{
		Infra.Requires(outgoingEvent);

		foreach (var property in base.EnumerateProperties(outgoingEvent))
		{
			yield return property;
		}

		if (!outgoingEvent.Data.IsUndefined())
		{
			yield return (@"Data", outgoingEvent.Data.ToObject()!);
			yield return (@"DataText", DataModelHandler.ConvertToText(outgoingEvent.Data));
		}
	}

	public override IEnumerable<(string Name, object Value)> EnumerateProperties(IEvent evt)
	{
		Infra.Requires(evt);

		foreach (var property in base.EnumerateProperties(evt))
		{
			yield return property;
		}

		if (!evt.Data.IsUndefined())
		{
			yield return (@"Data", evt.Data.ToObject()!);
			yield return (@"DataText", DataModelHandler.ConvertToText(evt.Data));
		}
	}
}
