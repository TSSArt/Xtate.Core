namespace Xtate.Core;

public interface ILogger
{
	IFormatProvider? FormatProvider { get; }

	bool IsEnabled(Level level);
}

public interface ILogger<[UsedImplicitly] TSource> : ILogger
{
	ValueTask Write(Level level, int eventId, string? message);

	ValueTask Write<TEntity>(Level level, int eventId, string? message, TEntity entity);

	ValueTask Write(Level level, int eventId, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage);

	ValueTask Write<TEntity>(Level level, int eventId, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage, TEntity entity);
}