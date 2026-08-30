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

using Xtate.Logging;
using Xtate.Logging.Internal;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

internal sealed class InterpreterLogTrace<TSource> : ILogger<TSource>
{
	private readonly List<LogEntry> _entries = [];

	public IReadOnlyList<int> EventIds => [.. _entries.Select(static entry => entry.EventId)];

	public IReadOnlyList<LogEntry> Entries => _entries;

#region Interface ILogger

	public bool IsEnabled(Level level) => true;

	public IFormatProvider? FormatProvider => null;

#endregion

#region Interface ILogger<TSource>

	public ValueTask Write(Level level, int eventId, string? message)
	{
		_entries.Add(new LogEntry(eventId, message));

		return ValueTask.CompletedTask;
	}

	public ValueTask Write<TEntity>(Level level,
									int eventId,
									string? message,
									TEntity entity) =>
		Write(level, eventId, message);

	public ValueTask Write(Level level, int eventId, LoggingInterpolatedStringHandler formattedMessage) => Write(level, eventId, formattedMessage.ToString(out _));

	public ValueTask Write<TEntity>(Level level,
									int eventId,
									LoggingInterpolatedStringHandler formattedMessage,
									TEntity entity) =>
		Write(level, eventId, formattedMessage.ToString(out _));

#endregion

	internal sealed record LogEntry(int EventId, string? Message);
}
