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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Xtate.Core;

public class TraceLogWriter(string name)
{
	private static readonly ConcurrentDictionary<int, string> Formats = new();

	private readonly TraceSource _traceSource = new(name, SourceLevels.All);

	public virtual bool IsEnabled(Level level) => _traceSource.Switch.ShouldTrace(GetTraceEventType(level));

	private static string GetFormat(int len) =>
		Formats.GetOrAdd(
			len, static argsCount =>
				 {
					 var sb = new StringBuilder(argsCount * 8 + 8);

					 sb.AppendLine(@"{0}");

					 for (var i = 1; i <= argsCount; i ++)
					 {
						 sb.Append(@"  {").Append(i).Append('}').AppendLine();
					 }

					 return sb.ToString();
				 });

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

	protected async ValueTask Write(Level level,
							  int eventId,
							  string? message,
							  IAsyncEnumerable<LoggingParameter>? parameters)
	{
		var traceEventType = GetTraceEventType(level);

		if (_traceSource.Switch.ShouldTrace(traceEventType))
		{
			var args = new List<LoggingParameter>();

			if (!string.IsNullOrEmpty(message))
			{
				args.Add(new LoggingParameter(string.Empty, message));
			}

			if (parameters is not null)
			{
				await parameters.AppendCollectionAsync(args).ConfigureAwait(false);
			}

			_traceSource.TraceEvent(traceEventType, eventId, GetFormat(args.Count - 1), args);
		}
	}
}

public class TraceLogWriter<TSource>() : TraceLogWriter(typeof(TSource).FullName!), ILogWriter<TSource>
{
#region Interface ILogWriter<TSource>

	ValueTask ILogWriter<TSource>.Write(Level level,
										int eventId,
										string? message,
										IAsyncEnumerable<LoggingParameter>? parameters) =>
		Write(level, eventId, message, parameters);

#endregion
}