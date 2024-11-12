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

public class EventQueue : IEventQueueReader, IEventQueueWriter, IEventDispatcher
{
	private readonly Channel<IIncomingEvent> _channel = Channel.CreateUnbounded<IIncomingEvent>();

	public required DisposeToken DisposeToken { private get; [UsedImplicitly] init; }
	
	#region Interface IEventDispatcher

	public ValueTask Dispatch(IIncomingEvent incomingEvent) => WriteAsync(incomingEvent);

#endregion

#region Interface IEventQueueReader

	public bool TryReadEvent([MaybeNullWhen(false)] out IIncomingEvent incomingEvent) => _channel.Reader.TryRead(out incomingEvent);

	public ValueTask<bool> WaitToEvent() => _channel.Reader.WaitToReadAsync(DisposeToken);

	public void Complete() => _channel.Writer.TryComplete();

#endregion

#region Interface IEventQueueWriter

	public ValueTask WriteAsync(IIncomingEvent incomingEvent) => _channel.Writer.WriteAsync(incomingEvent, DisposeToken);

#endregion
}