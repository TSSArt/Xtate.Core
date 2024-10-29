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

using Xtate.IoC;

namespace Xtate.Core;

public abstract class StateMachineClass : IStateMachineSessionId, IStateMachineArguments, IStateMachineLocation
{
	private readonly DataModelValue _arguments;

	private readonly Uri? _location;

	private SessionId? _sessionId;

	public Uri Location { init => _location = value; }

	public DataModelValue Arguments { init => _arguments = value; }

#region Interface IStateMachineArguments

	DataModelValue IStateMachineArguments.Arguments => _arguments;

#endregion

#region Interface IStateMachineLocation

	Uri? IStateMachineLocation.Location => _location;

#endregion

#region Interface IStateMachineSessionId

	public SessionId SessionId
	{
		get => _sessionId ??= SessionId.New();
		init => _sessionId = value;
	}

#endregion

	public virtual void AddServices(IServiceCollection services)
	{
		services.AddConstant<IStateMachineSessionId>(this);
		services.AddConstant<IStateMachineArguments>(this);
		services.AddConstant<IStateMachineLocation>(this);
	}
}