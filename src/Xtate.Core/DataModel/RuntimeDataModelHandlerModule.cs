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

using Xtate.DataModel.Runtime;
using Xtate.IoC;

namespace Xtate.DataModel;

public class RuntimeDataModelHandlerModule : Module<DataModelHandlerBaseModule, ErrorProcessorModule>
{
	protected override void AddServices()
	{
		Services.AddTypeSync<RuntimeActionExecutor, RuntimeAction>();
		Services.AddTypeSync<RuntimeValueEvaluator, RuntimeValue>();
		Services.AddTypeSync<RuntimePredicateEvaluator, RuntimePredicate>();
		Services.AddSharedType<RuntimeExecutionContext>(SharedWithin.Scope);
		Services.AddImplementation<RuntimeDataModelHandlerProvider>().For<IDataModelHandlerProvider>();

		var implementation = Services.AddImplementation<RuntimeDataModelHandler>().For<RuntimeDataModelHandler>();

		if (!Services.IsRegistered<IDataModelHandler>())
		{
			implementation.For<IDataModelHandler>();
		}
	}
}