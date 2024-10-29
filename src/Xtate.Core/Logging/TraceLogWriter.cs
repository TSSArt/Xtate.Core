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

using System.Diagnostics;
using System.Text;

namespace Xtate.Core;

public abstract class TraceLogWriter
{
	private static readonly string[] Formats = new string[16];

	private readonly TraceSource _traceSource;

	protected TraceLogWriter(string name, IEnumerable<TraceListener> traceListeners) 
	{
		_traceSource = new TraceSource(name, SourceLevels.Information);

		foreach (var traceListener in traceListeners)
		{
			_traceSource.Listeners.Add(traceListener);
		}
	}

	public virtual bool IsEnabled(Level level) => _traceSource.Switch.ShouldTrace(GetTraceEventType(level));

	private static string GetFormat(int len) => len < Formats.Length ? Formats[len] ??= FormatFactory(len) : FormatFactory(len);

	private static string FormatFactory(int argsCount)
	{
		var sb = new StringBuilder(argsCount * 10);

		sb.Append(@"{0}");

		for (var i = 1; i < argsCount; i ++)
		{
			sb.AppendLine().Append(@"  {").Append(i).Append('}');
		}

		return sb.ToString();
	}

	private static TraceEventType GetTraceEventType(Level level) =>
		level switch
		{
			Level.Error   => TraceEventType.Error,
			Level.Warning => TraceEventType.Warning,
			Level.Info    => TraceEventType.Information,
			Level.Debug   => TraceEventType.Verbose,
			Level.Trace   => TraceEventType.Verbose,
			Level.Verbose => TraceEventType.Verbose,
			_             => throw Infra.Unmatched(level)
		};

	public ValueTask Write(Level level,
								 int eventId,
								 string? message,
								 IEnumerable<LoggingParameter>? parameters)
	{
		var traceEventType = GetTraceEventType(level);

		if (_traceSource.Switch.ShouldTrace(traceEventType))
		{
			var args = new List<LoggingParameter> { new(string.Empty, message ?? string.Empty) };

			if (parameters is not null)
			{
				args.AddRange(parameters);
			}

			_traceSource.TraceEvent(traceEventType, eventId, GetFormat(args.Count), [..args]);
		}

		return default;
	}
}


public class TraceLogWriter<TSource>(IEnumerable<TraceListener> traceListeners) : TraceLogWriter(typeof(TSource).FullName!, traceListeners), ILogWriter<TSource>;