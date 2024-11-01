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

using Xtate.DataModel;
using Xtate.IoProcessor;

namespace Xtate.Core;

public class ExternalEventCommunication : IExternalEventCommunication, IDisposable
{
	private readonly DisposingToken _disposingToken = new();

	public required Func<Uri?, IIoProcessor> IoProcessorFactory { private get; [UsedImplicitly] init; }

	public required IEventScheduler EventScheduler { private get; [UsedImplicitly] init; }

	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

	private SessionId SessionId => StateMachineSessionId.SessionId;

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IExternalEventCommunication

	public async ValueTask<SendStatus> TrySend(IOutgoingEvent outgoingEvent)
	{
		if (outgoingEvent is null) throw new ArgumentNullException(nameof(outgoingEvent));

		var ioProcessor = IoProcessorFactory(outgoingEvent.Type);

		if (ioProcessor.IsInternalTarget(outgoingEvent.Target))
		{
			return SendStatus.ToInternalQueue;
		}

		var ioProcessorEvent = await ioProcessor.GetHostEvent(StateMachineSessionId.SessionId, outgoingEvent, _disposingToken.Token).ConfigureAwait(false);

		if (outgoingEvent.DelayMs > 0)
		{
			await EventScheduler.ScheduleEvent(ioProcessorEvent, _disposingToken.Token).ConfigureAwait(false);

			return SendStatus.Scheduled;
		}

		await ioProcessor.Dispatch(ioProcessorEvent, _disposingToken.Token).ConfigureAwait(false);

		return SendStatus.Sent;
	}

	public ValueTask Cancel(SendId sendId) => EventScheduler.CancelEvent(SessionId, sendId, _disposingToken.Token);

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();
		}
	}
}