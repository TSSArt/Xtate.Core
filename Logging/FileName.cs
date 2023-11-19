using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xtate.DataModel;

namespace Xtate.Core
{
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
		public required IEnumerable<IEntityParserProvider> Providers { private get; init; }

		public IEnumerable<LoggingParameter> EnumerateProperties<T>(T entity)
		{
			foreach (var provider in Providers)
			{
				if (provider.TryGetEntityParserHandler(entity) is {} handler)
				{
					return handler.EnumerateProperties(entity);
				}
			}

			throw new InfrastructureException(Res.Format(Resources.Exception_CantFindEntityParser, typeof(T)));
		}
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

	public interface ILogSource<T>
	{
		IEnumerable<(string Name, object Value)> EnumerateProperties();
	}

	public interface ILogProperties
	{
		IEnumerable<(string Name, object Value)> EnumerateProperties();
	}

	public readonly struct RawString
	{
		public readonly string Value;

		public RawString(string value) => Value = value;

		public static implicit operator RawString(string val) => new(val);

		[Obsolete("Method has been added to prevent 'Ambiguous invocation' compiler error", true)]
		public static implicit operator RawString(FormattableString formattable) => Infra.Fail<RawString>();
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
		private readonly StringBuilder?                            _stringBuilder;
		private readonly IFormatProvider?                          _provider;
		private readonly ImmutableArray<LoggingParameter>.Builder? _parametersBuilder;

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
									   [CallerArgumentExpression(nameof(value))] string? expression = default)
		{
			var start = _stringBuilder!.Length;

			AppendFormatted(value, format, expression);

			if (Math.Abs(alignment) - (_stringBuilder.Length - start) is var paddingRequired and > 0)
			{
				if (alignment < 0)
				{
					_stringBuilder.Append(' ', paddingRequired);
				}
				else
				{
					_stringBuilder.Insert(start, @" ", paddingRequired);
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

	public interface ILogger<TSource> : ILogger
	{
		ValueTask Write(Level level, string? message);

		ValueTask Write<TEntity>(Level level, string? message, TEntity entity);

		ValueTask Write(Level level, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage);

		ValueTask Write<TEntity>(Level level, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage, TEntity entity);
	}

	public interface ILogWriter
	{
		bool IsEnabled(Level level);

		ValueTask Write(Level level, string source, string? message, IEnumerable<LoggingParameter>? parameters = default);
	}

	public class TraceLogWriter : ILogWriter
	{

		public virtual bool IsEnabled(Level level) => true;

		public ValueTask Write(Level level,
							   string source,
							   string? message,
							   IEnumerable<LoggingParameter>? parameters)
		{
			Trace.WriteLine(string.IsNullOrWhiteSpace(message) 
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
	}

	public class FileLogWriter : ILogWriter
	{
		private object _lock = new object();
		private readonly string _file;
		public FileLogWriter(string file) {
			_file = file;
		}
		public virtual bool IsEnabled(Level level) => true;

		public ValueTask Write(Level level,
							   string source,
							   string? message,
							   IEnumerable<LoggingParameter>? parameters)
		{
			lock (_lock)
			{


				File.AppendAllText(
					_file, string.IsNullOrWhiteSpace(message)
						? $"[{DateTime.Now:u}] [{source}] {level}\r\n"
						: $"[{DateTime.Now:u}] [{source}] {level}: {message}\r\n");


				if (parameters is not null)
				{
					foreach (var parameter in parameters)
					{
						File.AppendAllText(_file, $"[{DateTime.Now:u}] [{source}] {parameter.Name}: {parameter.Value}\r\n");
					}
				}

				return default;
			}
		}
	}

	public class Logger<TSource> : ILogger<TSource>
	{
		public required IEntityParserHandler EntityParserHandler { private get; init; }

		public required ILogWriter? LogWriter { private get; init; }

		private static string Source => typeof(TSource).Name;

		public virtual IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

		public virtual bool IsEnabled(Level level) => LogWriter?.IsEnabled(level) ?? false;

		public virtual ValueTask Write(Level level, string message)
		{
			if (IsEnabled(level))
			{
				return LogWriter!.Write(level, Source, message);
			}

			return default;
		}

		public virtual ValueTask Write(Level level, LoggingInterpolatedStringHandler formattedMessage)
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

		public virtual ValueTask Write<TEntity>(Level level, LoggingInterpolatedStringHandler formattedMessage, TEntity entity)
		{
			if (IsEnabled(level))
			{
				var message = formattedMessage.ToString(out var parameters);
				var loggingParameters = parameters.Concat(EntityParserHandler.EnumerateProperties(entity));

				return LogWriter!.Write(level, Source, message, loggingParameters);
			}

			return default;
		}
	}



	public interface ITmpEntityLogger<in TEntity>
	{
		bool IsEnabled(Level level);

ValueTask Write(Level level,
						RawString message,
						TEntity entity,
						Exception? exception = default);

		ValueTask Write(Level level,
						FormattableString message,
						TEntity entity,
						Exception? exception = default);
	}

	public interface ITmpEntityLogger<TSource, in TEntity> : ITmpEntityLogger<TEntity> { }

	[Obsolete]
	public class StateMachineInterpreterLogger33 : IStateMachineInterpreterLogger1
	{
		public required ITmpEntityLogger<StateMachineInterpreterLogger33, IDebugEntityId?> Logger { private get; init; }

		public virtual bool IsEnabled => Logger.IsEnabled(Level.Error);

		public virtual ValueTask LogError(ErrorType errorType, Exception exception, IDebugEntityId? entityId)
		{
			Infra.Requires(exception);

			//return Logger.Write(TmpLoggerLevel.Error, exception.Message, entityId, exception);
			return Logger.Write(Level.Error, $@"dd:{exception.Message}", entityId, exception);
		}
	}

	public class StateMachineInterpreterTracerParser : ILogEntityParser<SendId>, ILogEntityParser<InvokeId>, ILogEntityParser<InvokeData>, ILogEntityParser<IOutgoingEvent>, ILogEntityParser<IEvent>,
													   ILogEntityParser<IStateEntity>, ILogEntityParser<StateMachineInterpreterState>
	{
		public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(SendId sendId)
		{
			yield return (@"SendId", sendId);
		}

		public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(InvokeId invokeId)
		{
			yield return (@"InvokeId", invokeId);
		}

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

		public virtual IEnumerable<(string Name, object Value)> EnumerateProperties(IStateEntity stateEntity)
		{
			Infra.Requires(stateEntity);

			if (stateEntity.Id is { } stateId)
			{
				yield return (@"StateId", stateId);
			}
		}

		public IEnumerable<(string Name, object Value)> EnumerateProperties(StateMachineInterpreterState state)
		{
			yield return (@"InterpreterState", state);
		}
	}

	public class StateMachineInterpreterTracerVerboseParser : StateMachineInterpreterTracerParser
	{
		public required IDataModelHandler DataModelHandler { private get; init; }

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

	public class StateMachineInterpreterTracer : IStateMachineInterpreterTracer1
	{
		public required ITmpEntityLogger<StateMachineInterpreterTracer, SendId> LoggerSendId { private get; init; }

		public required ITmpEntityLogger<StateMachineInterpreterTracer, InvokeId> LoggerInvokeId { private get; init; }

		public required ITmpEntityLogger<StateMachineInterpreterTracer, InvokeData> LoggerInvokeData { private get; init; }

		public required ITmpEntityLogger<StateMachineInterpreterTracer, IOutgoingEvent> LoggerOutgoingEvent { private get; init; }

		public required ITmpEntityLogger<StateMachineInterpreterTracer, IEvent> LoggerEvent { private get; init; }

		public required ITmpEntityLogger<StateMachineInterpreterTracer, IStateEntity> LoggerState { private get; init; }

		public required ITmpEntityLogger<StateMachineInterpreterTracer, StateMachineInterpreterState> LoggerInterpreterState { private get; init; }

		public virtual ValueTask TraceCancelEvent(SendId sendId) => LoggerSendId.Write(Level.Trace, $@"Cancel Event '{sendId}'", sendId);

		public virtual ValueTask TraceStartInvoke(InvokeData invokeData)
		{
			Infra.Requires(invokeData);

			return LoggerInvokeData.Write(Level.Trace, $@"Start Invoke '{invokeData.InvokeId}'", invokeData);
		}

		public virtual ValueTask TraceCancelInvoke(InvokeId invokeId) => LoggerInvokeId.Write(Level.Trace, $@"Cancel Invoke '{invokeId}'", invokeId);

		public virtual ValueTask TraceSendEvent(IOutgoingEvent outgoingEvent)
		{
			Infra.Requires(outgoingEvent);

			return LoggerOutgoingEvent.Write(Level.Trace, $@"Send event '{EventName.ToName(outgoingEvent.NameParts)}'", outgoingEvent);
		}

		public virtual ValueTask TraceProcessingEvent(IEvent evt)
		{
			Infra.Requires(evt);

			return LoggerEvent.Write(Level.Trace, $@"Processing {evt.Type} event '{EventName.ToName(evt.NameParts)}'", evt);
		}

		public virtual ValueTask TraceEnteringState(IStateEntity stateEntity)
		{
			Infra.Requires(stateEntity);

			return LoggerState.Write(Level.Trace, $@"Entering state '{stateEntity.Id}'", stateEntity);
		}

		public virtual ValueTask TraceEnteredState(IStateEntity stateEntity)
		{
			Infra.Requires(stateEntity);

			return LoggerState.Write(Level.Trace, $@"Entered state '{stateEntity.Id}'", stateEntity);
		}

		public virtual ValueTask TraceExitingState(IStateEntity stateEntity)
		{
			Infra.Requires(stateEntity);

			return LoggerState.Write(Level.Trace, $@"Exiting state '{stateEntity.Id}'", stateEntity);
		}

		public virtual ValueTask TraceExitedState(IStateEntity stateEntity)
		{
			Infra.Requires(stateEntity);

			return LoggerState.Write(Level.Trace, $@"Exited state '{stateEntity.Id}'", stateEntity);
		}

		public virtual ValueTask TraceInterpreterState(StateMachineInterpreterState state) => LoggerInterpreterState.Write(Level.Trace, $@"Interpreter state has changed to '{state}'", state);

		public virtual ValueTask TracePerformingTransition(ITransition transition)
		{
			

			return default;
		}

		public virtual ValueTask TracePerformedTransition(ITransition transition)
		{
			

			return default;
		}

		private static string? Join<T>(ImmutableArray<T> list, Func<T, string> convert) =>
			!list.IsDefaultOrEmpty ? list.Length == 1 ? convert(list[0]) : string.Join(@" ", list.Select(convert)) : null;
	}}
