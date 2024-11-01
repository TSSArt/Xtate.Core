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

namespace Xtate.DataModel.Runtime;

public class RuntimeValueEvaluator : IValueExpression, IObjectEvaluator
{
	public required RuntimeValue Value { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<RuntimeExecutionContext>> RuntimeExecutionContextFactory { private get; [UsedImplicitly] init; }

#region Interface IObjectEvaluator

	public async ValueTask<IObject> EvaluateObject()
	{
		var executionContext = await RuntimeExecutionContextFactory().ConfigureAwait(false);

		Xtate.Runtime.SetCurrentExecutionContext(executionContext);

		return await Value.Evaluate().ConfigureAwait(false);
	}

#endregion

#region Interface IValueExpression

	public string? Expression => Value.Expression;

#endregion
}