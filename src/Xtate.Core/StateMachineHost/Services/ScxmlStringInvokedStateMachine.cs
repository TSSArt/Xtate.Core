// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.Class;
using Xtate.IoC;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost.Services;

public class ScxmlStringInvokedStateMachine(string scxml) : ScxmlStringStateMachine(scxml), IInvokedStateMachine
{
#region Interface IExternalServiceInvokeId

	public required InvokeId InvokeId { get; init; }

#endregion

#region Interface IExternalServiceType

	public required FullUri Type { get; init; }

#endregion

#region Interface IParentStateMachineSessionId

	public required SessionId ParentSessionId { get; init; }

#endregion

	public override void AddServices(IServiceCollection services)
	{
		base.AddServices(services);

		services.AddForwarding<IInvokedStateMachine>(_ => this);
		services.AddForwarding<IParentStateMachineSessionId>(_ => this);
		services.AddForwarding<IExternalServiceInvokeId>(_ => this);
		services.AddForwarding<IExternalServiceType>(_ => this);
	}
}
