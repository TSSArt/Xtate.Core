#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoProcessor;

namespace Xtate
{
	public enum SendStatus
	{
		Sent,
		ToSchedule,
		ToInternalQueue
	}

	public class InvokeData
	{
		public InvokeData(InvokeId invokeId, Uri type, Uri? source, string? rawContent, DataModelValue content, DataModelValue parameters)
		{
			InvokeId = invokeId;
			Type = type;
			Source = source;
			RawContent = rawContent;
			Content = content;
			Parameters = parameters;
		}

		public InvokeId       InvokeId   { get; }
		public Uri            Type       { get; }
		public Uri?           Source     { get; }
		public string?        RawContent { get; }
		public DataModelValue Content    { get; }
		public DataModelValue Parameters { get; }
	}

	public interface IExternalCommunication
	{
		ImmutableArray<IIoProcessor> GetIoProcessors();

		ValueTask             StartInvoke(InvokeData invokeData, CancellationToken token);
		ValueTask             CancelInvoke(InvokeId invokeId, CancellationToken token);
		bool                  IsInvokeActive(InvokeId invokeId);
		ValueTask<SendStatus> TrySendEvent(IOutgoingEvent evt, CancellationToken token);
		ValueTask             ForwardEvent(IEvent evt, InvokeId invokeId, CancellationToken token);
		ValueTask             CancelEvent(SendId sendId, CancellationToken token);
	}
}