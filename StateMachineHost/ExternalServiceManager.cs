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

namespace Xtate.Core;

public class ExternalServiceManager : IExternalServiceManager
{
	public required ExternalServiceEventRouter ExternalServiceEventRouter { private get; [UsedImplicitly] init; }

	public required IExternalServiceScopeManager ExternalServiceScopeManager { private get; [UsedImplicitly] init; }

	public required DisposeToken DisposeToken { private get; [UsedImplicitly] init; }

#region Interface IExternalServiceManager

	public ValueTask Forward(InvokeId invokeId, IIncomingEvent incomingEvent) => ExternalServiceEventRouter.Dispatch(invokeId, incomingEvent).WaitAsync(DisposeToken);

	public ValueTask Start(InvokeData invokeData) => ExternalServiceScopeManager.Start(invokeData).WaitAsync(DisposeToken);

	public ValueTask Cancel(InvokeId invokeId) => ExternalServiceScopeManager.Cancel(invokeId).WaitAsync(DisposeToken);

#endregion
}