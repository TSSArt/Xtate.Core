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

using System.IO;
using Xtate.IoC;

namespace Xtate.Core;

public class ScxmlStreamStateMachine(Stream stream) : StateMachineClass, IScxmlStateMachine, IStateMachineLocation, IStateMachineArguments
{
	public Uri Location { get; init; } = default!;

	public DataModelValue Arguments { get; init; }

	public override void AddServices(IServiceCollection services)
	{
		base.AddServices(services);

		services.AddModule<ScxmlStateMachineModule>();
		services.AddConstant<IScxmlStateMachine>(this);
		services.AddConstant<IStateMachineArguments>(this);

		if (Location is not null)
		{
			services.AddConstant<IStateMachineLocation>(this);
		}
	}

	TextReader IScxmlStateMachine.CreateTextReader() => new StreamReader(stream);
}