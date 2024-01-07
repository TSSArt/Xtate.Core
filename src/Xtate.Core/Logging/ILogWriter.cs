namespace Xtate.Core;

public interface ILogWriter
{
	bool IsEnabled(Level level);

	ValueTask Write(Level level, string? message, IEnumerable<LoggingParameter>? parameters = default);
}