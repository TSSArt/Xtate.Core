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

public class TraceLogWriter(string source, SourceLevels sourceLevels) : ILogWriter
{
	private static readonly string[] Formats = new string[20];

	private readonly TraceSource _traceSource = new(source, sourceLevels);

#region Interface ILogWriter

	public virtual bool IsEnabled(Level level) => _traceSource.Switch.ShouldTrace(GetTraceEventType(level));

	public ValueTask Write(Level level, string? message, IEnumerable<LoggingParameter>? parameters)
	{
		var traceEventType = GetTraceEventType(level);

		if (_traceSource.Switch.ShouldTrace(traceEventType))
		{
			object[] args = parameters is not null ? [message, ..parameters] : [message];

			_traceSource.TraceEvent(traceEventType, id: 0, GetFormat(args.Length - 1), args);
		}

		return default;
	}

	private static TraceEventType GetTraceEventType(Level level)
	{
		return level switch
			   {
				   Level.Error   => TraceEventType.Error,
				   Level.Warning => TraceEventType.Warning,
				   Level.Info    => TraceEventType.Information,
				   Level.Debug   => TraceEventType.Verbose,
				   Level.Trace   => TraceEventType.Verbose,
				   Level.Verbose => TraceEventType.Verbose,
				   _             => Infra.Unexpected<TraceEventType>(level)
			   };
	}

#endregion

	private static string GetFormat(int len) => len < Formats.Length ? Formats[len] ??= CreateFormatString(len) : CreateFormatString(len);

	private static string CreateFormatString(int len)
	{
		var sb = new StringBuilder(len * 8 + 8);

		sb.AppendLine(@"{0}");

		for (var i = 1; i <= len; i ++)
		{
			sb.AppendLine(@$"  {{{i}}}");
		}

		return sb.ToString();
	}
}