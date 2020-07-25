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