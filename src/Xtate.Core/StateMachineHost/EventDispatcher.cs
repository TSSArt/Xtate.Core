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

using Xtate.ExternalService;
using Xtate.IoC;

namespace Xtate.Core;

public class EventDispatcher : IEventDispatcher, IAsyncInitialization
{
	private readonly AsyncInit<IEventDispatcher?> _eventDispatcherAsyncInit;

	public EventDispatcher() => _eventDispatcherAsyncInit = AsyncInit.Run(this, ed => ed.GetEventDispatcher());

	public required IExternalServiceInvokeId? ExternalServiceInvokeId { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<IEventQueueWriter>> EventQueueWriterFactory { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<IExternalService>> ExternalServiceFactory { private get; [UsedImplicitly] init; }

#region Interface IAsyncInitialization

	public Task Initialization => _eventDispatcherAsyncInit.Task;

#endregion

#region Interface IEventDispatcher

	public ValueTask Dispatch(IIncomingEvent incomingEvent) => _eventDispatcherAsyncInit.Value?.Dispatch(incomingEvent) ?? default;

#endregion

	private async ValueTask<IEventDispatcher> GetEventDispatcher() =>
		ExternalServiceInvokeId is null
			? await EventQueueWriterFactory().ConfigureAwait(false) as IEventDispatcher
			: await ExternalServiceFactory().ConfigureAwait(false) as IEventDispatcher;
}