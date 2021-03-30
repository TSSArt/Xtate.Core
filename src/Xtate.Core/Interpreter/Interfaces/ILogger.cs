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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate
{
	[PublicAPI]
	public interface ILogger
	{
		bool IsTracingEnabled { get; }

		ValueTask ExecuteLog(ILoggerContext loggerContext,
							 LogLevel logLevel,
							 string? message,
							 DataModelValue arguments,
							 Exception? exception,
							 CancellationToken token);

		ValueTask LogError(ILoggerContext loggerContext,
						   ErrorType errorType,
						   Exception exception,
						   string? sourceEntityId,
						   CancellationToken token);

		ValueTask TraceProcessingEvent(ILoggerContext loggerContext, IEvent evt, CancellationToken token);
		ValueTask TraceEnteringState(ILoggerContext loggerContext, IIdentifier stateId, CancellationToken token);
		ValueTask TraceEnteredState(ILoggerContext loggerContext, IIdentifier stateId, CancellationToken token);
		ValueTask TraceExitingState(ILoggerContext loggerContext, IIdentifier stateId, CancellationToken token);
		ValueTask TraceExitedState(ILoggerContext loggerContext, IIdentifier stateId, CancellationToken token);

		ValueTask TracePerformingTransition(ILoggerContext loggerContext,
											TransitionType type,
											string? eventDescriptor,
											string? target,
											CancellationToken token);

		ValueTask TracePerformedTransition(ILoggerContext loggerContext,
										   TransitionType type,
										   string? eventDescriptor,
										   string? target,
										   CancellationToken token);

		ValueTask TraceInterpreterState(ILoggerContext loggerContext, StateMachineInterpreterState state, CancellationToken token);
		ValueTask TraceSendEvent(ILoggerContext loggerContext, IOutgoingEvent outgoingEvent, CancellationToken token);
		ValueTask TraceCancelEvent(ILoggerContext loggerContext, SendId sendId, CancellationToken token);
		ValueTask TraceStartInvoke(ILoggerContext loggerContext, InvokeData invokeData, CancellationToken token);
		ValueTask TraceCancelInvoke(ILoggerContext loggerContext, InvokeId invokeId, CancellationToken token);
	}
}