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

using Xtate.IoC;
using Xtate.StateMachine;
using Xtate.StateMachineHost;

namespace Xtate.Persistence.Services;

public class ResumedInvokedStateMachine(
	SessionId sessionId,
	SessionId parentSessionId,
	InvokeId invokeId,
	FullUri type) : ResumedStateMachine(sessionId), IInvokedStateMachine
{
#region Interface IExternalServiceInvokeId

	public InvokeId InvokeId { get; } = invokeId;

#endregion

#region Interface IExternalServiceType

	public FullUri Type { get; } = type;

#endregion

#region Interface IParentStateMachineSessionId

	public SessionId ParentSessionId { get; } = parentSessionId;

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
