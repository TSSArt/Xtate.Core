#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public interface ILogger
	{
		bool      IsTracingEnabled { get; }
		ValueTask ExecuteLog(ILoggerContext loggerContext, string? label, DataModelValue data, CancellationToken token);
		ValueTask LogError(ILoggerContext loggerContext, ErrorType errorType, Exception exception, string? sourceEntityId, CancellationToken token);
		void      TraceProcessingEvent(ILoggerContext loggerContext, IEvent evt);
		void      TraceEnteringState(ILoggerContext loggerContext, IIdentifier stateId);
		void      TraceExitingState(ILoggerContext loggerContext, IIdentifier stateId);
		void      TracePerformingTransition(ILoggerContext loggerContext, TransitionType type, string? eventDescriptor, string? target);
		void      TraceInterpreterState(ILoggerContext loggerContext, StateMachineInterpreterState state);
	}
}