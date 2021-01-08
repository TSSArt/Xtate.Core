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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.IoProcessor
{
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

		Uri IIoProcessor.GetTarget(SessionId sessionId) => GetTarget(sessionId);

		ValueTask IIoProcessor.Dispatch(SessionId sessionId, IOutgoingEvent evt, CancellationToken token) => OutgoingEvent(sessionId, evt, token);

		bool IIoProcessor.CanHandle(Uri? type, Uri? target) => FullUriComparer.Instance.Equals(type, IoProcessorId) || FullUriComparer.Instance.Equals(type, _ioProcessorAliasId);

		Uri IIoProcessor.Id => IoProcessorId;

	#endregion

		protected abstract Uri GetTarget(SessionId sessionId);

		protected abstract ValueTask OutgoingEvent(SessionId sessionId, IOutgoingEvent evt, CancellationToken token);

		protected bool TryGetEventDispatcher(SessionId sessionId, [NotNullWhen(true)] out IEventDispatcher? eventDispatcher) => _eventConsumer.TryGetEventDispatcher(sessionId, out eventDispatcher);
	}
}