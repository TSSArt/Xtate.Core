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

using Xtate.IoC.AncestorTracker.DependencyInjection;
using Xtate.IoC.Options.DependencyInjection;
using Xtate.IoC.ServiceArray.DependencyInjection;
using Xtate.IoC.Tools.DependencyInjection;
using Xtate.IoC.TransformArgs.DependencyInjection;

namespace Xtate.IoC.DependencyInjection;

[InstantiatedByIoC]
public class IoCModule : Module
{
	protected override void AddServices()
	{
		Services.AddModule<AncestorTrackerModule>();
		Services.AddModule<OptionsModule>();
		Services.AddModule<ServiceArrayModule>();
		Services.AddModule<ToolsModule>();
		Services.AddModule<TransformArgsModule>();
	}
}