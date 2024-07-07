// Copyright © 2019-2024 Sergii Artemenko
// 
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

using System.Threading.Channels;

namespace Xtate.Core;

public class StateMachineRuntimeController : StateMachineControllerBase
{
	private static readonly UnboundedChannelOptions UnboundedSynchronousChannelOptions  = new() { SingleReader = true, AllowSynchronousContinuations = true };
	private static readonly UnboundedChannelOptions UnboundedAsynchronousChannelOptions = new() { SingleReader = true, AllowSynchronousContinuations = false };

	private readonly TimeSpan? _idlePeriod;

	private CancellationTokenSource? _suspendOnIdleTokenSource;
	private CancellationTokenSource? _suspendTokenSource;

	//TODO:delete
	[Obsolete]
	public StateMachineRuntimeController(SessionId sessionId,
										 IStateMachineOptions? options,
										 IStateMachine? stateMachine,
										 Uri? stateMachineLocation,
										 IStateMachineHost stateMachineHost,
										 TimeSpan? idlePeriod,
										 InterpreterOptions defaultOptions)
		: base(sessionId, options, stateMachine, stateMachineLocation, stateMachineHost, defaultOptions)
	{
		_idlePeriod = idlePeriod;

		EventChannel = CreateChannel(options);
	}

	public StateMachineRuntimeController(IStateMachineStartOptions stateMachineStartOptions,
										 IStateMachineOptions? options,
										 IStateMachine? stateMachine,
										 IStateMachineLocation? stateMachineLocation,
										 IStateMachineHost stateMachineHost,
										 IStateMachineIdlePeriod? idlePeriod)
		: base(stateMachineStartOptions.SessionId, options, stateMachine, stateMachineLocation?.Location, stateMachineHost, /* defaultOptions.options*/new InterpreterOptions())
	{
		_idlePeriod = idlePeriod?.IdlePeriod;

		EventChannel = CreateChannel(options);
	}

	protected override Channel<IEvent> EventChannel { get; }

	private static Channel<IEvent> CreateChannel(IStateMachineOptions? options)
	{
		if (options is null)
		{
			return Channel.CreateUnbounded<IEvent>(UnboundedAsynchronousChannelOptions);
		}

		var sync = options.SynchronousEventProcessing ?? false;
		var queueSize = options.ExternalQueueSize ?? 0;

		if (options.IsStateMachinePersistable() || queueSize <= 0)
		{
			return Channel.CreateUnbounded<IEvent>(sync ? UnboundedSynchronousChannelOptions : UnboundedAsynchronousChannelOptions);
		}

		var channelOptions = new BoundedChannelOptions(queueSize) { AllowSynchronousContinuations = sync, SingleReader = true };

		return Channel.CreateBounded<IEvent>(channelOptions);
	}

	protected override void StateChanged(StateMachineInterpreterState state)
	{
		base.StateChanged(state);

		if (state == StateMachineInterpreterState.Waiting && _suspendOnIdleTokenSource is { } src && _idlePeriod is { } delay)
		{
			src.CancelAfter(delay);
		}
	}

	protected override ValueTask DisposeAsyncCore()
	{
		_suspendTokenSource?.Dispose();
		_suspendOnIdleTokenSource?.Dispose();

		return base.DisposeAsyncCore();
	}

	protected override CancellationToken GetSuspendToken()
	{
		var defaultSuspendToken = base.GetSuspendToken();

		if (_idlePeriod is not { Ticks: >= 0 } idlePeriod)
		{
			return defaultSuspendToken;
		}

		_suspendTokenSource?.Dispose();
		_suspendOnIdleTokenSource?.Dispose();

		_suspendOnIdleTokenSource = new CancellationTokenSource(idlePeriod);

		_suspendTokenSource = defaultSuspendToken.CanBeCanceled
			? CancellationTokenSource.CreateLinkedTokenSource(defaultSuspendToken, _suspendOnIdleTokenSource.Token)
			: _suspendOnIdleTokenSource;

		return _suspendTokenSource.Token;
	}
}

public interface IStateMachineIdlePeriod
{
	TimeSpan? IdlePeriod { get; }
}

public interface IStateMachineLocation
{
	Uri Location { get; }
}

public interface IStateMachineSessionId
{
	SessionId SessionId { get; }
}