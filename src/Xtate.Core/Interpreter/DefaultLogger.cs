#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Core
{
	internal sealed class DefaultLogger : ILogger
	{
		public static ILogger Instance { get; } = new DefaultLogger();

		private DefaultLogger() { }

		public ValueTask ExecuteLog(ILoggerContext loggerContext, LogLevel logLevel, string? message, DataModelValue arguments, Exception? exception, CancellationToken token)
		{
			switch (logLevel)
			{
				case LogLevel.Info:
					Trace.TraceInformation(Resources.DefaultLogger_LogInfo, loggerContext.StateMachineName, loggerContext.SessionId?.Value, message, loggerContext.ConvertToText(arguments), exception);
					break;

				case LogLevel.Warning:
					Trace.TraceWarning(Resources.DefaultLogger_LogInfo, loggerContext.StateMachineName, loggerContext.SessionId?.Value, message, loggerContext.ConvertToText(arguments), exception);
					break;

				case LogLevel.Error:
					Trace.TraceError(Resources.DefaultLogger_LogInfo, loggerContext.StateMachineName, loggerContext.SessionId?.Value, message, loggerContext.ConvertToText(arguments), exception);
					break;

				default:
					Infrastructure.UnexpectedValue(logLevel);
					break;
			}

			return default;
		}

		public ValueTask LogError(ILoggerContext loggerContext, ErrorType errorType, Exception exception, string? sourceEntityId, CancellationToken token)
		{
			Trace.TraceError(Resources.DefaultLogger_LogError, errorType, loggerContext.StateMachineName, loggerContext.SessionId?.Value, sourceEntityId, exception);

			return default;
		}

#if DEBUG
		public bool IsTracingEnabled => true;
#else
		public bool IsTracingEnabled => false;
#endif

		public ValueTask TraceProcessingEvent(ILoggerContext loggerContext, IEvent evt, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceProcessingEvent, evt.Type, EventName.ToName(evt.NameParts), evt.SendId?.Value, evt.InvokeId?.Value,
								   evt.Data, evt.OriginType, evt.Origin, loggerContext.SessionId?.Value);

			return default;
		}

		public ValueTask TraceEnteringState(ILoggerContext loggerContext, IIdentifier stateId, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceEnteringState, stateId.Value, loggerContext.SessionId?.Value);

			return default;
		}

		public ValueTask TraceEnteredState(ILoggerContext loggerContext, IIdentifier stateId, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceEnteredState, stateId.Value, loggerContext.SessionId?.Value);

			return default;
		}

		public ValueTask TraceExitingState(ILoggerContext loggerContext, IIdentifier stateId, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceExitingState, stateId.Value, loggerContext.SessionId?.Value);

			return default;
		}

		public ValueTask TraceExitedState(ILoggerContext loggerContext, IIdentifier stateId, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceExitedState, stateId.Value, loggerContext.SessionId?.Value);

			return default;
		}

		public ValueTask TracePerformingTransition(ILoggerContext loggerContext, TransitionType type, string? eventDescriptor, string? target, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TracePerformingTransition, type, target, eventDescriptor, loggerContext.SessionId?.Value);

			return default;
		}

		public ValueTask TracePerformedTransition(ILoggerContext loggerContext, TransitionType type, string? eventDescriptor, string? target, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TracePerformedTransition, type, target, eventDescriptor, loggerContext.SessionId?.Value);

			return default;
		}

		public ValueTask TraceInterpreterState(ILoggerContext loggerContext, StateMachineInterpreterState state, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceInterpreterState, state, loggerContext.SessionId?.Value);

			return default;
		}

		public ValueTask TraceSendEvent(ILoggerContext loggerContext, IOutgoingEvent outgoingEvent, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceSendEvent, !outgoingEvent.NameParts.IsDefaultOrEmpty ? EventName.ToName(outgoingEvent.NameParts) : null, outgoingEvent.SendId?.Value);

			return default;
		}

		public ValueTask TraceCancelEvent(ILoggerContext loggerContext, SendId sendId, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceCancelEvent, sendId.Value);

			return default;
		}

		public ValueTask TraceStartInvoke(ILoggerContext loggerContext, InvokeData invokeData, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceStartInvoke, invokeData.InvokeId.Value);

			return default;
		}

		public ValueTask TraceCancelInvoke(ILoggerContext loggerContext, InvokeId invokeId, CancellationToken token)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceCancelInvoke, invokeId.Value);

			return default;
		}
	}
}