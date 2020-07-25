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