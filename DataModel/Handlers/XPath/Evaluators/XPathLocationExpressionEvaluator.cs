#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel.XPath
{
	internal class XPathLocationExpressionEvaluator : ILocationEvaluator, ILocationExpression, IAncestorProvider
	{
		private readonly XPathCompiledExpression _compiledExpression;
		private readonly LocationExpression      _locationExpression;

		public XPathLocationExpressionEvaluator(in LocationExpression locationExpression, XPathCompiledExpression compiledExpression)
		{
			_locationExpression = locationExpression;
			_compiledExpression = compiledExpression;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _locationExpression.Ancestor;

	#endregion

	#region Interface ILocationEvaluator

		public void DeclareLocalVariable(IExecutionContext executionContext) => executionContext.Engine().DeclareVariable(_compiledExpression);

		public ValueTask SetValue(IObject value, IExecutionContext executionContext, CancellationToken token)
		{
			executionContext.Engine().Assign(_compiledExpression, value);

			return default;
		}

		public ValueTask<IObject> GetValue(IExecutionContext executionContext, CancellationToken token) => new ValueTask<IObject>(executionContext.Engine().EvalObject(_compiledExpression));

		public string GetName(IExecutionContext executionContext) => executionContext.Engine().GetName(_compiledExpression);

	#endregion

	#region Interface ILocationExpression

		public string? Expression => _locationExpression.Expression;

	#endregion
	}
}