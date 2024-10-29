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

using Xtate.Service;

namespace Xtate.Core;

public class StateMachineControllerProxy(StateMachineRuntimeController stateMachineRuntimeController) : IStateMachineController
{
	private readonly IStateMachineController _baseStateMachineController = stateMachineRuntimeController;

#region Interface IEventDispatcher

	public ValueTask Send(IEvent evt, CancellationToken token = default) => _baseStateMachineController.Send(evt, token);

#endregion

#region Interface IExternalService

	public ValueTask Destroy() => _baseStateMachineController.Destroy();

	ValueTask<DataModelValue> IExternalService.GetResult() => _baseStateMachineController.GetResult();

#endregion

	//TODO:
	//public ValueTask DisposeAsync() => _baseStateMachineController.DisposeAsync();

	//public void TriggerDestroySignal() => _baseStateMachineController.TriggerDestroySignal();

	//public ValueTask StartAsync(CancellationToken token) => _baseStateMachineController.StartAsync(token);

	//public SessionId SessionId            => _baseStateMachineController.SessionId;
	//public Uri       StateMachineLocation => _baseStateMachineController.StateMachineLocation;
}