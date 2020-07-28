#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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

using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public interface IExecutionContext
	{
		IContextItems RuntimeItems { get; }

		DataModelObject DataModel { get; }

		bool InState(IIdentifier id);

		ValueTask Cancel(SendId sendId, CancellationToken token = default);

		ValueTask Log(string? label, DataModelValue arguments = default, CancellationToken token = default);

		ValueTask Send(IOutgoingEvent evt, CancellationToken token = default);

		ValueTask StartInvoke(InvokeData invokeData, CancellationToken token = default);

		ValueTask CancelInvoke(InvokeId invokeId, CancellationToken token = default);
	}

	public interface IContextItems
	{
		object? this[object key] { get; set; }
	}
}