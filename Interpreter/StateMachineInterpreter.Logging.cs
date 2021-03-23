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
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Core
{
	public sealed partial class StateMachineInterpreter : ILoggerContext
	{
	#region Interface ILoggerContext

		public string GetDataModelAsText() => _dataModelHandler.ConvertToText(_context.DataModel);

		public string ConvertToText(DataModelValue value) => _dataModelHandler.ConvertToText(value);

		DataModelList ILoggerContext.GetDataModel() => _context.DataModel.AsConstant();

		ImmutableArray<string> ILoggerContext.GetActiveStates()
		{
			var configuration = _context.Configuration;

			var list = ImmutableArray.CreateBuilder<string>(configuration.Count);

			foreach (var node in configuration)
			{
				list.Add(node.Id.Value);
			}

			return list.MoveToImmutable();
		}

		IStateMachine ILoggerContext.StateMachine => _model.Root;

		SessionId ILoggerContext.SessionId => _sessionId;

		string? ILoggerContext.StateMachineName => _model.Root.Name;

	#endregion

		private bool IsPlatformError(Exception exception)
		{
			for (var ex = exception; ex is not null; ex = ex.InnerException)
			{
				if (ex is PlatformException platformException && platformException.SessionId == _sessionId)
				{
					return true;
				}
			}

			return false;
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

		private ValueTask TraceProcessingEvent(IEvent evt) => _logger.IsTracingEnabled ? _logger.TraceProcessingEvent(this, evt, _stopToken) : default;

		private ValueTask TraceEnteringState(StateEntityNode state) => _logger.IsTracingEnabled ? _logger.TraceEnteringState(this, state.Id, _stopToken) : default;

		private ValueTask TraceEnteredState(StateEntityNode state) => _logger.IsTracingEnabled ? _logger.TraceEnteredState(this, state.Id, _stopToken) : default;

		private ValueTask TraceExitingState(StateEntityNode state) => _logger.IsTracingEnabled ? _logger.TraceExitingState(this, state.Id, _stopToken) : default;

		private ValueTask TraceExitedState(StateEntityNode state) => _logger.IsTracingEnabled ? _logger.TraceExitedState(this, state.Id, _stopToken) : default;

		private ValueTask TracePerformingTransition(TransitionNode transition) =>
				_logger.IsTracingEnabled
						? _logger.TracePerformingTransition(this, transition.Type, EventDescriptorToString(transition.EventDescriptors), TargetToString(transition.Target), _stopToken)
						: default;

		private ValueTask TracePerformedTransition(TransitionNode transition) =>
				_logger.IsTracingEnabled
						? _logger.TracePerformedTransition(this, transition.Type, EventDescriptorToString(transition.EventDescriptors), TargetToString(transition.Target), _stopToken)
						: default;

		private static string? TargetToString(ImmutableArray<IIdentifier> list) => !list.IsDefault ? string.Join(separator: @" ", list.Select(id => id.Value)) : null;

		private static string? EventDescriptorToString(ImmutableArray<IEventDescriptor> list) => !list.IsDefault ? string.Join(separator: @" ", list.Select(id => id.Value)) : null;

		private ValueTask TraceInterpreterState(StateMachineInterpreterState state) => _logger.IsTracingEnabled ? _logger.TraceInterpreterState(this, state, _stopToken) : default;
	}
}