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

public class TraceLogWriter(string name)
{
	private static readonly string[] Formats = new string[16];

	private readonly TraceSource _traceSource = new(name, SourceLevels.All);

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
			_             => Infra.Unexpected<TraceEventType>(level)
		};

	public async ValueTask Write(Level level,
								 int eventId,
								 string? message,
								 IAsyncEnumerable<LoggingParameter>? parameters)
	{
		var traceEventType = GetTraceEventType(level);

		if (_traceSource.Switch.ShouldTrace(traceEventType))
		{
			var args = new List<LoggingParameter> { new(string.Empty, message ?? string.Empty) };

			if (parameters is not null)
			{
				await parameters.AppendCollectionAsync(args).ConfigureAwait(false);
			}

			_traceSource.TraceEvent(traceEventType, eventId, GetFormat(args.Count), [..args]);
		}
	}
}

public class TraceLogWriter<TSource>() : TraceLogWriter(typeof(TSource).FullName!), ILogWriter<TSource>;