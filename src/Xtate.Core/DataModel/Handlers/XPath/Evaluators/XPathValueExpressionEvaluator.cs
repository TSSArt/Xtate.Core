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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel.XPath
{
	internal class XPathValueExpressionEvaluator : IValueExpression, IObjectEvaluator, IStringEvaluator, IIntegerEvaluator, IArrayEvaluator, IAncestorProvider, IDebugEntityId
	{
		private readonly XPathCompiledExpression _compiledExpression;
		private readonly ValueExpression         _valueExpression;

		public XPathValueExpressionEvaluator(in ValueExpression valueExpression, XPathCompiledExpression compiledExpression)
		{
			_valueExpression = valueExpression;
			_compiledExpression = compiledExpression;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _valueExpression.Ancestor;

	#endregion

	#region Interface IArrayEvaluator

		public ValueTask<IObject[]> EvaluateArray(IExecutionContext executionContext, CancellationToken token)
		{
			var iterator = executionContext.Engine().EvalObject(_compiledExpression).AsIterator();

			var list = new List<IObject>();

			foreach (DataModelXPathNavigator navigator in iterator)
			{
				list.Add(new XPathObject(new XPathSingleElementIterator(navigator)));
			}

			return new ValueTask<IObject[]>(list.ToArray());
		}

	#endregion

	#region Interface IDebugEntityId

		FormattableString? IDebugEntityId.EntityId => null;

	#endregion

	#region Interface IIntegerEvaluator

		ValueTask<int> IIntegerEvaluator.EvaluateInteger(IExecutionContext executionContext, CancellationToken token) =>
				new ValueTask<int>(executionContext.Engine().EvalObject(_compiledExpression).AsInteger());

	#endregion

	#region Interface IObjectEvaluator

		ValueTask<IObject> IObjectEvaluator.EvaluateObject(IExecutionContext executionContext, CancellationToken token) =>
				new ValueTask<IObject>(executionContext.Engine().EvalObject(_compiledExpression));

	#endregion

	#region Interface IStringEvaluator

		ValueTask<string> IStringEvaluator.EvaluateString(IExecutionContext executionContext, CancellationToken token) =>
				new ValueTask<string>(executionContext.Engine().EvalObject(_compiledExpression).AsString());

	#endregion

	#region Interface IValueExpression

		public string? Expression => _valueExpression.Expression;

	#endregion
	}
}