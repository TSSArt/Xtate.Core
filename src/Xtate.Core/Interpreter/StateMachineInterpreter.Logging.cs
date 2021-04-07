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
	public sealed partial class StateMachineInterpreter : IInterpreterLoggerContext
	{
	#region Interface IInterpreterLoggerContext

		public string ConvertToText(DataModelValue value)
		{
			Infra.NotNull(_dataModelHandler);

			return _dataModelHandler.ConvertToText(value);
		}

		public DataModelValue GetDataModel()
		{
			if (_context is null!)
			{
				return default;
			}

			return new LazyValue<IStateMachineContext>(stateMachineContext => stateMachineContext.DataModel.AsConstant(), _context);
		}

		ImmutableArray<string> IInterpreterLoggerContext.GetActiveStates()
		{
			if (_context is null!)
			{
				return ImmutableArray<string>.Empty;
			}

			var configuration = _context.Configuration;

			var list = ImmutableArray.CreateBuilder<string>(configuration.Count);

			foreach (var node in configuration)
			{
				list.Add(node.Id.Value);
			}

			return list.MoveToImmutable();
		}

		IStateMachine IInterpreterLoggerContext.StateMachine => GetStateMachine();

		SessionId IInterpreterLoggerContext.SessionId => _sessionId;

	#endregion

	#region Interface ILoggerContext

		DataModelList ILoggerContext.GetProperties()
		{
			var properties = new DataModelList { { @"SessionId", _sessionId } };

			if (GetStateMachine().Name is { } stateMachineName)
			{
				properties.Add(key: @"StateMachineName", stateMachineName);
			}

			if (_context.Configuration.Count > 0)
			{
				var activeStates = new DataModelList();
				foreach (var node in _context.Configuration)
				{
					activeStates.Add(node.Id.Value);
				}

				activeStates.MakeDeepConstant();

				properties.Add(key: @"ActiveStates", activeStates);
			}

			properties.Add(key: @"DataModel", GetDataModel());

			properties.MakeDeepConstant();

			return properties;
		}

		string ILoggerContext.LoggerContextType => nameof(IInterpreterLoggerContext);

	#endregion

		private IStateMachine GetStateMachine()
		{
			if (_model is not null)
			{
				return _model.Root;
			}

			Infra.NotNull(_stateMachine);

			return _stateMachine;
		}

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

		private async ValueTask LogError(ErrorType errorType,
										 string? sourceEntityId,
										 Exception exception,
										 CancellationToken token)
		{
			if (_options.Logger is { } logger)
			{
				try
				{
					await logger.LogError(this, errorType, exception, sourceEntityId, token).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new PlatformException(ex, _sessionId);
				}
			}
		}

		private ValueTask TraceProcessingEvent(IEvent evt) => _options.Logger is { IsTracingEnabled: true } logger ? logger.TraceProcessingEvent(this, evt, _options.StopToken) : default;

		private ValueTask TraceEnteringState(StateEntityNode state) => _options.Logger is { IsTracingEnabled: true } logger ? logger.TraceEnteringState(this, state.Id, _options.StopToken) : default;

		private ValueTask TraceEnteredState(StateEntityNode state) => _options.Logger is { IsTracingEnabled: true } logger ? logger.TraceEnteredState(this, state.Id, _options.StopToken) : default;

		private ValueTask TraceExitingState(StateEntityNode state) => _options.Logger is { IsTracingEnabled: true } logger ? logger.TraceExitingState(this, state.Id, _options.StopToken) : default;

		private ValueTask TraceExitedState(StateEntityNode state) => _options.Logger is { IsTracingEnabled: true } logger ? logger.TraceExitedState(this, state.Id, _options.StopToken) : default;

		private ValueTask TracePerformingTransition(TransitionNode transition) =>
			_options.Logger is { IsTracingEnabled: true } logger
				? logger.TracePerformingTransition(this, transition.Type, EventDescriptorToString(transition.EventDescriptors), TargetToString(transition.Target), _options.StopToken)
				: default;

		private ValueTask TracePerformedTransition(TransitionNode transition) =>
			_options.Logger is { IsTracingEnabled: true } logger
				? logger.TracePerformedTransition(this, transition.Type, EventDescriptorToString(transition.EventDescriptors), TargetToString(transition.Target), _options.StopToken)
				: default;

		private static string? TargetToString(ImmutableArray<IIdentifier> list) => !list.IsDefault ? string.Join(separator: @" ", list.Select(id => id.Value)) : null;

		private static string? EventDescriptorToString(ImmutableArray<IEventDescriptor> list) => !list.IsDefault ? string.Join(separator: @" ", list.Select(id => id.Value)) : null;

		private ValueTask TraceInterpreterState(StateMachineInterpreterState state) =>
			_options.Logger is { IsTracingEnabled: true } logger ? logger.TraceInterpreterState(this, state, _options.StopToken) : default;
	}
}