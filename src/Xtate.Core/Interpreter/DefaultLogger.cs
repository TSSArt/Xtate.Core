#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	internal sealed class DefaultLogger : ILogger
	{
		public static readonly ILogger Instance = new DefaultLogger();

		private DefaultLogger() { }

		public ValueTask ExecuteLog(ILoggerContext loggerContext, string? label, DataModelValue data, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_LogInfo, loggerContext?.StateMachineName, loggerContext?.SessionId?.Value, label, data);

			return default;
		}

		public ValueTask LogError(ILoggerContext loggerContext, ErrorType errorType, Exception exception, string? sourceEntityId, CancellationToken token)
		{
			Trace.TraceError(Resources.DefaultLogger_LogError, errorType, loggerContext?.StateMachineName, loggerContext?.SessionId?.Value, sourceEntityId, exception);

			return default;
		}

#if DEBUG
		public bool IsTracingEnabled => true;
#else
		public bool IsTracingEnabled => false;
#endif

		public void TraceProcessingEvent(ILoggerContext loggerContext, IEvent evt)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceProcessingEvent, evt.Type, EventName.ToName(evt.NameParts), evt.SendId?.Value, evt.InvokeId?.Value,
								   evt.Data, evt.OriginType, evt.Origin, loggerContext?.SessionId?.Value);
		}

		public void TraceEnteringState(ILoggerContext loggerContext, IIdentifier stateId)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceEnteringState, stateId.Value, loggerContext?.SessionId?.Value);
		}

		public void TraceExitingState(ILoggerContext loggerContext, IIdentifier stateId)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceExitingState, stateId.Value, loggerContext?.SessionId?.Value);
		}

		public void TracePerformingTransition(ILoggerContext loggerContext, TransitionType type, string? eventDescriptor, string? target)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TracePerformingTransition, type, target, eventDescriptor, loggerContext?.SessionId?.Value);
		}

		public void TraceInterpreterState(ILoggerContext loggerContext, StateMachineInterpreterState state)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceInterpreterState, state, loggerContext?.SessionId?.Value);
		}
	}
}