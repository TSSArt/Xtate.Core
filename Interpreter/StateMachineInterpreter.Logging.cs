using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public sealed partial class StateMachineInterpreter : ILoggerContext
	{
	#region Interface ILoggerContext

		SessionId? ILoggerContext.SessionId => _sessionId;

		string? ILoggerContext.StateMachineName => _model.Root.Name;

	#endregion

		private bool IsPlatformError(Exception exception)
		{
			for (; exception != null; exception = exception.InnerException)
			{
				if (exception is PlatformException ex && ex.SessionId == _sessionId)
				{
					return true;
				}
			}

			return false;
		}

		private async ValueTask LogInformation(string? label, DataModelValue data, CancellationToken token)
		{
			try
			{
				await _logger.ExecuteLog(this, label, data, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new PlatformException(ex, _sessionId);
			}
		}

		private async ValueTask LogError(ErrorType errorType, string? sourceEntityId, Exception exception, CancellationToken token)
		{
			try
			{
				await _logger.LogError(this, errorType, exception, sourceEntityId, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new PlatformException(ex, _sessionId);
			}
		}

		private void LogProcessingEvent(IEvent evt)
		{
			if (_logger.IsTracingEnabled)
			{
				_logger.TraceProcessingEvent(this, evt);
			}
		}

		private void LogEnteringState(StateEntityNode state)
		{
			if (_logger.IsTracingEnabled)
			{
				_logger.TraceEnteringState(this, state.Id);
			}
		}

		private void LogExitingState(StateEntityNode state)
		{
			if (_logger.IsTracingEnabled)
			{
				_logger.TraceExitingState(this, state.Id);
			}
		}

		private void LogPerformingTransition(TransitionNode transition)
		{
			if (_logger.IsTracingEnabled)
			{
				_logger.TracePerformingTransition(this, transition.Type, EventDescriptorToString(transition.EventDescriptors), TargetToString(transition.Target));
			}

			static string? TargetToString(ImmutableArray<IIdentifier> list)
			{
				if (list.IsDefault)
				{
					return null;
				}

				return string.Join(separator: @" ", list.Select(id => id.Value));
			}

			static string? EventDescriptorToString(ImmutableArray<IEventDescriptor> list)
			{
				if (list.IsDefault)
				{
					return null;
				}

				return string.Join(separator: @" ", list.Select(id => id.Value));
			}
		}

		private void LogInterpreterState(StateMachineInterpreterState state)
		{
			if (_logger.IsTracingEnabled)
			{
				_logger.TraceInterpreterState(this, state);
			}
		}
	}
}