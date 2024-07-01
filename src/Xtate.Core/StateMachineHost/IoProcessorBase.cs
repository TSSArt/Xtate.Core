#region Copyright © 2019-2023 Sergii Artemenko

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

namespace Xtate.IoProcessor;


public abstract class IoProcessorBase : IIoProcessor
{
	private readonly IEventConsumer _eventConsumer;
	private readonly Uri?           _ioProcessorAliasId;

	protected IoProcessorBase(IEventConsumer eventConsumer, string ioProcessorId, string? ioProcessorAlias = default)
	{
		if (string.IsNullOrEmpty(ioProcessorId)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(ioProcessorId));
		if (ioProcessorAlias is { Length: 0 }) throw new ArgumentException(Resources.Exception_ValueCantBeEmpty, nameof(ioProcessorAlias));

		_eventConsumer = eventConsumer ?? throw new ArgumentNullException(nameof(eventConsumer));

		IoProcessorId = new Uri(ioProcessorId, UriKind.RelativeOrAbsolute);
		_ioProcessorAliasId = ioProcessorAlias is not null ? new Uri(ioProcessorAlias, UriKind.RelativeOrAbsolute) : null;
	}

	protected Uri IoProcessorId { get; }

#region Interface IIoProcessor

	Uri? IIoProcessor.GetTarget(ServiceId serviceId) => GetTarget(serviceId);

	ValueTask<IHostEvent> IIoProcessor.GetHostEvent(ServiceId senderServiceId, IOutgoingEvent outgoingEvent, CancellationToken token) => CreateHostEventAsync(senderServiceId, outgoingEvent, token);

	ValueTask IIoProcessor.Dispatch(IHostEvent hostEvent, CancellationToken token) => OutgoingEvent(hostEvent, token);

	bool IIoProcessor.CanHandle(Uri? type) => FullUriComparer.Instance.Equals(type, IoProcessorId) || FullUriComparer.Instance.Equals(type, _ioProcessorAliasId);

	Uri IIoProcessor.Id => IoProcessorId;

#endregion

	protected abstract Uri? GetTarget(ServiceId serviceId);

	protected virtual ValueTask<IHostEvent> CreateHostEventAsync(ServiceId senderServiceId, IOutgoingEvent outgoingEvent, CancellationToken token) =>
		new(CreateHostEvent(senderServiceId, outgoingEvent));

	protected virtual IHostEvent CreateHostEvent(ServiceId senderServiceId, IOutgoingEvent outgoingEvent)
	{
		if (outgoingEvent is null) throw new ArgumentNullException(nameof(outgoingEvent));

		return new HostEvent(this, senderServiceId, outgoingEvent.Target is { } target ? UriId.FromUri(target) : null, outgoingEvent);
	}

	protected abstract ValueTask OutgoingEvent(IHostEvent hostEvent, CancellationToken token);

	protected ValueTask<IEventDispatcher?> TryGetEventDispatcher(SessionId sessionId, CancellationToken token) => _eventConsumer.TryGetEventDispatcher(sessionId, token);
}