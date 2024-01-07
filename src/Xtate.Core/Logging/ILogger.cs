namespace Xtate.Core;

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