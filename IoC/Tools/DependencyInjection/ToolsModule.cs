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

using Xtate.IoC.Tools.Services;

namespace Xtate.IoC.Tools.DependencyInjection;

[InstantiatedByIoC]
public class ToolsModule : Module
{
	protected override void AddServices()
	{
		Services.AddForwarding(sp => new DisposeToken(sp.DisposeToken));

		Services.AddFactory<SafeFactory<Any>>().For<Safe<Any>>();

		Services.AddFactorySync<DeferredFactory<Any>>().For<Deferred<Any>>();

		Services.AddSharedTypeSync<ScopeObject>(SharedWithin.Scope);
	}
}