#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel.None
{
	internal sealed class NoneConditionExpressionEvaluator : IConditionExpression, IBooleanEvaluator, IAncestorProvider, IDebugEntityId
	{
		private readonly ConditionExpression _conditionExpression;
		private readonly IIdentifier         _inState;

		public NoneConditionExpressionEvaluator(in ConditionExpression conditionExpression, IIdentifier inState)
		{
			_conditionExpression = conditionExpression;
			_inState = inState;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _conditionExpression.Ancestor;

	#endregion

	#region Interface IBooleanEvaluator

		ValueTask<bool> IBooleanEvaluator.EvaluateBoolean(IExecutionContext executionContext, CancellationToken token) => new ValueTask<bool>(executionContext.InState(_inState));

	#endregion

	#region Interface IConditionExpression

		public string? Expression => _conditionExpression.Expression;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{_inState}";

	#endregion
	}
}