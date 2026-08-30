using Xtate.Logging;
using Xtate.Logging.Internal;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

internal sealed class InterpreterLogTrace<TSource> : ILogger<TSource>
{
	private readonly List<LogEntry> _entries = [];

	public IReadOnlyList<int> EventIds => _entries.Select(static entry => entry.EventId).ToArray();

	public IReadOnlyList<LogEntry> Entries => _entries;

	public IFormatProvider? FormatProvider => null;

	public bool IsEnabled(Level level) => true;

	public ValueTask Write(Level level, int eventId, string? message)
	{
		_entries.Add(new LogEntry(eventId, message));
		return ValueTask.CompletedTask;
	}

	public ValueTask Write<TEntity>(Level level, int eventId, string? message, TEntity entity) => Write(level, eventId, message);

	public ValueTask Write(Level level, int eventId, LoggingInterpolatedStringHandler formattedMessage) => Write(level, eventId, formattedMessage.ToString(out _));

	public ValueTask Write<TEntity>(Level level, int eventId, LoggingInterpolatedStringHandler formattedMessage, TEntity entity) => Write(level, eventId, formattedMessage.ToString(out _));

	internal sealed record LogEntry(int EventId, string? Message);
}
