﻿// Copyright © 2019-2024 Sergii Artemenko
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
using Xtate.Scxml;

namespace Xtate.Core;

public class LocationStateMachine : StateMachineClass
{
	public LocationStateMachine(Uri location) => Location = location;

	public override void AddServices(IServiceCollection services)
	{
		base.AddServices(services);

		services.AddConstant<IStateMachineLocation>(this);

		services.AddModule<ScxmlModule>();
		services.AddSharedFactory<ScxmlLocationStateMachineGetter>(SharedWithin.Scope).For<IStateMachine>();
	}
}