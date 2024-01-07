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

namespace Xtate.DataModel.XPath;

public class XPathLocationExpressionEvaluator : ILocationEvaluator, ILocationExpression, IAncestorProvider
{
	private readonly XPathAssignType         _assignType;
	private readonly string?                 _attribute;
	private readonly XPathCompiledExpression _compiledExpression;
	private readonly ILocationExpression     _locationExpression;

	public XPathLocationExpressionEvaluator(ILocationExpression locationExpression, XPathCompiledExpression compiledExpression)
	{
		_locationExpression = locationExpression;
		_compiledExpression = compiledExpression;

		if (_locationExpression.Is<XPathLocationExpression>(out var xPathLocationExpression))
		{
			_assignType = xPathLocationExpression.AssignType;
			_attribute = xPathLocationExpression.Attribute;
		}
		else
		{
			_assignType = XPathAssignType.ReplaceChildren;
		}
	}

	public required Func<ValueTask<XPathEngine>> EngineFactory { private get; [UsedImplicitly] init; }

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _locationExpression;

#endregion

#region Interface ILocationEvaluator

	public async ValueTask SetValue(IObject value)
	{
		var engine = await EngineFactory().ConfigureAwait(false);

		await engine.Assign(_compiledExpression, _assignType, _attribute, value).ConfigureAwait(false);
	}

	public async ValueTask<IObject> GetValue()
	{
		var engine = await EngineFactory().ConfigureAwait(false);

		return await engine.EvalObject(_compiledExpression, stripRoots: true).ConfigureAwait(false);
	}

	public async ValueTask<string> GetName()
	{
		var engine = await EngineFactory().ConfigureAwait(false);

		return engine.GetName(_compiledExpression);
	}

#endregion

#region Interface ILocationExpression

	public string? Expression => _locationExpression.Expression;

#endregion

	public async ValueTask DeclareLocalVariable()
	{
		var engine = await EngineFactory().ConfigureAwait(false);

		engine.DeclareVariable(_compiledExpression);
	}
}