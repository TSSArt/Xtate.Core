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

using Xtate.DataModel.Null.DependencyInjection;
using Xtate.DataModel.Runtime.DependencyInjection;
using Xtate.DataModel.Services;
using Xtate.DataModel.XPath.DependencyInjection;
using Xtate.IoC;
using Xtate.StateMachine.Validator.DependencyInjection;

namespace Xtate.DataModel.DependencyInjection;

public class DataModelHandlersModule : Module<NullDataModelHandlerModule, RuntimeDataModelHandlerModule, XPathDataModelHandlerModule, ValidatorModule>
{
	protected override void AddServices()
	{
		Services.AddType<UnknownDataModelHandler>();
		Services.AddImplementation<DataModelHandlerService>().For<IDataModelHandlerService>();
		Services.AddFactory<DataModelHandlerGetter>().For<IDataModelHandler>(SharedWithin.Scope, Option.DoNotDispose);
	}
}