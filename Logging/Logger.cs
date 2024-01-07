using System.Globalization;
using Xtate.IoC;

namespace Xtate.Core;

public class Logger<TSource>(Func<string, ValueTask<ILogWriter?>> logWriterFactory) : ILogger<TSource>, IAsyncInitialization
{
	public Task Initialization => _logWriterAsyncInit.Task;

	public required IEntityParserHandler EntityParserHandler { private get; [UsedImplicitly] init; }

	private readonly AsyncInit<ILogWriter?> _logWriterAsyncInit = AsyncInit.RunNow(logWriterFactory, f => f(typeof(TSource).Name));

	#region Interface ILogger

	public virtual bool IsEnabled(Level level) => _logWriterAsyncInit.Value?.IsEnabled(level) ?? false;

	public virtual IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

#endregion

#region Interface ILogger<TSource>

	public virtual ValueTask Write(Level level, string? message)
	{
		if (IsEnabled(level))
		{
			return _logWriterAsyncInit.Value!.Write(level, message);
		}

		return default;
	}

	public virtual ValueTask Write(Level level, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage)
	{
		if (IsEnabled(level))
		{
			var message = formattedMessage.ToString(out var parameters);

			return _logWriterAsyncInit.Value!.Write(level, message, parameters);
		}

		return default;
	}

	public virtual ValueTask Write<TEntity>(Level level, string? message, TEntity entity)
	{
		if (IsEnabled(level))
		{
			return _logWriterAsyncInit.Value!.Write(level, message, EntityParserHandler.EnumerateProperties(entity));
		}

		return default;
	}

	public virtual ValueTask Write<TEntity>(Level level, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage, TEntity entity)
	{
		if (IsEnabled(level))
		{
			var message = formattedMessage.ToString(out var parameters);
			var loggingParameters = parameters.Concat(EntityParserHandler.EnumerateProperties(entity));

			return _logWriterAsyncInit.Value!.Write(level, message, loggingParameters);
		}

		return default;
	}

#endregion
}