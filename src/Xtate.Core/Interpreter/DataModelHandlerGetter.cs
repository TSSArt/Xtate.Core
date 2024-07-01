<<<<<<< Updated upstream
﻿using System.Threading.Tasks;
using Xtate.DataModel;
using Xtate.IoC;
=======
﻿#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using Xtate.DataModel;
>>>>>>> Stashed changes

namespace Xtate.Core;

public class DataModelHandlerGetter
{
<<<<<<< Updated upstream
	public required IDataModelHandlerService DataModelHandlerService { private get; init; }
	public required IStateMachine?           StateMachine            { private get; init; }

	public virtual async ValueTask<IDataModelHandler?> GetDataModelHandler() =>
		StateMachine is not null ? await DataModelHandlerService.GetDataModelHandler(StateMachine.DataModelType).ConfigureAwait(false) : default;
=======
	public required IDataModelHandlerService DataModelHandlerService { private get; [UsedImplicitly] init; }
	public required IStateMachine?           StateMachine            { private get; [UsedImplicitly] init; }

<<<<<<<< Updated upstream:src/Xtate.Core/src/Xtate.Core/Interpreter/Interfaces/ICustomActionContext.cs
		string XmlName { get; }

		string Xml { get; }
		
		void AddValidationError<T>(string message, Exception? exception = default) where T : ICustomActionExecutor;

		ILocationAssigner RegisterLocationExpression(string expression);

		IExpressionEvaluator RegisterValueExpression(string expression, ExpectedValueType expectedValueType);
	}
========
	[UsedImplicitly]
	public virtual ValueTask<IDataModelHandler?> GetDataModelHandler() => StateMachine is not null ? DataModelHandlerService.GetDataModelHandler(StateMachine.DataModelType) : default;
>>>>>>>> Stashed changes:src/Xtate.Core/src/Xtate.Core/Interpreter/DataModelHandlerGetter.cs
>>>>>>> Stashed changes
}