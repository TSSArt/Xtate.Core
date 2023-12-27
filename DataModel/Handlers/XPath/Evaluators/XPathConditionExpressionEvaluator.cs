#region Copyright © 2019-2023 Sergii Artemenko

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

namespace Xtate.DataModel.XPath;

public class XPathConditionExpressionEvaluator(IConditionExpression conditionExpression, XPathCompiledExpression compiledExpression) : IConditionExpression, IBooleanEvaluator, IAncestorProvider
{
	public required Func<ValueTask<XPathEngine>> EngineFactory { private get; [UsedImplicitly] init; }

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => conditionExpression;

#endregion

#region Interface IBooleanEvaluator

	async ValueTask<bool> IBooleanEvaluator.EvaluateBoolean()
	{
		var engine = await EngineFactory().ConfigureAwait(false);

		var obj = await engine.EvalObject(compiledExpression, stripRoots: true).ConfigureAwait(false);

		return obj.AsBoolean();
	}

#endregion

#region Interface IConditionExpression

	public string? Expression => conditionExpression.Expression;

#endregion
}