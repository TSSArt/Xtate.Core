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
	[PublicAPI]
	public sealed class TraceLogger : ILoggerOld
	{
		private readonly bool _isTracingEnabled;

		public required ILoggerContext LoggerContext { private get; init; }

		public TraceLogger() : this(true) { }

		[Obsolete]//move to config interface
		public TraceLogger(bool stateMachineTracingEnabled) => _isTracingEnabled = stateMachineTracingEnabled;

	#region Interface ILogger

		ValueTask ILoggerOld.ExecuteLogOld(LogLevel logLevel,
									 string? message,
									 DataModelValue arguments,
									 Exception? exception)
		{
			switch (LoggerContext)
			{
				case IInterpreterLoggerContext interpreterLoggerContext:
					TraceInternal(logLevel,
								  Resources.DefaultLogger_InterpreterLogEntry,
								  interpreterLoggerContext.StateMachine.Name,
								  interpreterLoggerContext.SessionId.Value,
								  message,
								  interpreterLoggerContext.ConvertToText(arguments),
								  exception);
					break;

				default:
					TraceInternal(logLevel,
								  Resources.DefaultLogger_LogEntry,
								  message,
								  arguments,
								  exception,
								  LoggerContext?.LoggerContextType,
								  LoggerContext?.GetProperties());
					break;
			}

			return default;
		}

		ValueTask ILoggerOld.LogErrorOld(ErrorType errorType,
								   Exception exception,
								   string? sourceEntityId)
		{
			Trace.TraceError(Resources.DefaultLogger_LogError, errorType, GetStateMachineName(LoggerContext), GetSessionId(LoggerContext), sourceEntityId, exception);

			return default;
		}

		ValueTask ILoggerOld.TraceProcessingEvent(IEvent evt)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceProcessingEvent, evt.Type, EventName.ToName(evt.NameParts), evt.SendId?.Value, evt.InvokeId?.Value,
								   evt.Data, evt.OriginType, evt.Origin, GetSessionId(LoggerContext));

			return default;
		}

		ValueTask ILoggerOld.TraceEnteringState(IIdentifier stateId)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceEnteringState, stateId.Value, GetSessionId(LoggerContext));

			return default;
		}

		ValueTask ILoggerOld.TraceEnteredState(IIdentifier stateId)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceEnteredState, stateId.Value, GetSessionId(LoggerContext));

			return default;
		}

		ValueTask ILoggerOld.TraceExitingState(IIdentifier stateId)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceExitingState, stateId.Value, GetSessionId(LoggerContext));

			return default;
		}

		ValueTask ILoggerOld.TraceExitedState(IIdentifier stateId)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceExitedState, stateId.Value, GetSessionId(LoggerContext));

			return default;
		}

		ValueTask ILoggerOld.TracePerformingTransition(TransitionType type,
													string? eventDescriptor,
													string? target)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TracePerformingTransition, type, target, eventDescriptor, GetSessionId(LoggerContext));

			return default;
		}

		ValueTask ILoggerOld.TracePerformedTransition(TransitionType type,
												   string? eventDescriptor,
												   string? target)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TracePerformedTransition, type, target, eventDescriptor, GetSessionId(LoggerContext));

			return default;
		}

		ValueTask ILoggerOld.TraceInterpreterState(StateMachineInterpreterState state)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceInterpreterState, state, GetSessionId(LoggerContext));

			return default;
		}

		ValueTask ILoggerOld.TraceSendEvent(IOutgoingEvent outgoingEvent)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceSendEvent, !outgoingEvent.NameParts.IsDefaultOrEmpty ? EventName.ToName(outgoingEvent.NameParts) : null, outgoingEvent.SendId?.Value);

			return default;
		}

		ValueTask ILoggerOld.TraceCancelEvent(SendId sendId)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceCancelEvent, sendId.Value);

			return default;
		}

		ValueTask ILoggerOld.TraceStartInvoke(InvokeData invokeData)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceStartInvoke, invokeData.InvokeId.Value);

			return default;
		}

		ValueTask ILoggerOld.TraceCancelInvoke(InvokeId invokeId)
		{
			Trace.TraceInformation(Resources.DefaultLogger_TraceCancelInvoke, invokeId.Value);

			return default;
		}

		bool ILoggerOld.IsTracingEnabled => _isTracingEnabled;

	#endregion

		private static SessionId? GetSessionId(ILoggerContext? loggerContext)
		{
			if (loggerContext is IInterpreterLoggerContext interpreterLoggerContext)
			{
				return interpreterLoggerContext.SessionId;
			}

			if (loggerContext?.GetProperties()[@"SessionId"].AsStringOrDefault() is { } sessionIdString)
			{
				return SessionId.FromString(sessionIdString);
			}

			return default;
		}

		private static string? GetStateMachineName(ILoggerContext? loggerContext)
		{
			if (loggerContext is IInterpreterLoggerContext interpreterLoggerContext)
			{
				return interpreterLoggerContext.StateMachine.Name;
			}

			if (loggerContext?.GetProperties()[@"StateMachineName"].AsStringOrDefault() is { } stateMachineName)
			{
				return stateMachineName;
			}

			return default;
		}

		private static void TraceInternal(LogLevel logLevel, string format, params object?[]? args)
		{
			switch (logLevel)
			{
				case LogLevel.Info:
					Trace.TraceInformation(format, args);
					break;

				case LogLevel.Warning:
					Trace.TraceWarning(format, args);
					break;

				case LogLevel.Error:
					Trace.TraceError(format, args);
					break;

				default:
					Infra.Unexpected(logLevel);
					break;
			}
		}
	}
}