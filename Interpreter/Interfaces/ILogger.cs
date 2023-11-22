#region Copyright © 2019-2023 Sergii Artemenko

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
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate;

//TODO:move to file
[Obsolete]
public interface IStateMachineInterpreterLogger1
{
	bool IsEnabled { get; }

	ValueTask LogError(ErrorType errorType, Exception exception, IDebugEntityId? entityId);
}

//TODO:move to file
[Obsolete]
public interface IStateMachineInterpreterTracer1
{
	ValueTask TracePerformingTransition(ITransition transition);
	ValueTask TracePerformedTransition(ITransition transition);
	ValueTask TraceProcessingEvent(IEvent evt);
	ValueTask TraceEnteringState(IStateEntity stateEntity);
	ValueTask TraceEnteredState(IStateEntity stateEntity);
	ValueTask TraceExitingState(IStateEntity stateEntity);
	ValueTask TraceExitedState(IStateEntity stateEntity);
	ValueTask TraceInterpreterState(StateMachineInterpreterState state);
	ValueTask TraceSendEvent(IOutgoingEvent outgoingEvent);
	ValueTask TraceCancelEvent(SendId sendId);
	ValueTask TraceStartInvoke(InvokeData invokeData);
	ValueTask TraceCancelInvoke(InvokeId invokeId);
}

public interface ILoggerOld
{
	[Obsolete]
	bool IsTracingEnabled { get; }

	//TODO:delete
	[Obsolete]
	ValueTask ExecuteLogOld(LogLevel logLevel,
							string? message,
							DataModelValue arguments,
							Exception? exception);

	//TODO:delete
	[Obsolete]
	ValueTask LogErrorOld(ErrorType errorType,
						  Exception exception,
						  string? sourceEntityId);

	ValueTask TracePerformingTransition(TransitionType type,
										string? eventDescriptor,
										string? target);

	ValueTask TracePerformedTransition(TransitionType type,
									   string? eventDescriptor,
									   string? target);

	ValueTask TraceProcessingEvent(IEvent evt);
	ValueTask TraceEnteringState(IIdentifier stateId);
	ValueTask TraceEnteredState(IIdentifier stateId);
	ValueTask TraceExitingState(IIdentifier stateId);
	ValueTask TraceExitedState(IIdentifier stateId);
	ValueTask TraceInterpreterState(StateMachineInterpreterState state);
	ValueTask TraceSendEvent(IOutgoingEvent outgoingEvent);
	ValueTask TraceCancelEvent(SendId sendId);
	ValueTask TraceStartInvoke(InvokeData invokeData);
	ValueTask TraceCancelInvoke(InvokeId invokeId);
}