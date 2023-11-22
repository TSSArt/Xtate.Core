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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate
{
	[PublicAPI]
	public record InvokeData
	{
		public InvokeData(InvokeId invokeId, Uri type)
		{
			InvokeId = invokeId;
			Type = type;
		}

		public InvokeId       InvokeId   { get; }
		public Uri            Type       { get; }
		public Uri?           Source     { get; init; }
		public string?        RawContent { get; init; }
		public DataModelValue Content    { get; init; }
		public DataModelValue Parameters { get; init; }
	}

	public interface IEventController
	{
		ValueTask Send(IOutgoingEvent outgoingEvent);

		ValueTask Cancel(SendId sendId);
	}

	public interface IInvokeController
	{
		ValueTask Start(InvokeData invokeData);

		ValueTask Cancel(InvokeId invokeId);
	}

	//TODO:delete
	[Obsolete]
	public interface IExecutionContext : ILogController, IEventController, IInvokeController, IInStateController, IDataModelController
	{
		IContextItems RuntimeItems { get; }

		ISecurityContext SecurityContext { get; }
	}

	public interface IInStateController
	{
		bool InState(IIdentifier id);
	}

	public interface IDataModelController
	{
		DataModelList DataModel { get; }
	}

	public interface IContextItems
	{
		object? this[object key] { get; set; }
	}
}