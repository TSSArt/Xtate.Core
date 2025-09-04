// Copyright © 2019-2025 Sergii Artemenko
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

public class XPathValueExpressionEvaluator(IValueExpression valueExpression, XPathCompiledExpression compiledExpression)
    : IValueExpression, IObjectEvaluator, IStringEvaluator, IIntegerEvaluator, IArrayEvaluator, IAncestorProvider
{
    public required Func<ValueTask<XPathEngine>> EngineFactory { private get; [UsedImplicitly] init; }

#region Interface IAncestorProvider

    object IAncestorProvider.Ancestor => valueExpression;

#endregion

#region Interface IArrayEvaluator

    public async ValueTask<IObject[]> EvaluateArray()
    {
        var engine = await EngineFactory().ConfigureAwait(false);

        var obj = await engine.EvalObject(compiledExpression, stripRoots: true).ConfigureAwait(false);

        var iterator = obj.AsIterator();

        var list = new List<IObject>();

        foreach (DataModelXPathNavigator navigator in iterator)
        {
            list.Add(new XPathObject(new XPathSingleElementIterator(navigator)));
        }

        return [.. list];
    }

#endregion

#region Interface IIntegerEvaluator

    async ValueTask<int> IIntegerEvaluator.EvaluateInteger()
    {
        var engine = await EngineFactory().ConfigureAwait(false);

        var obj = await engine.EvalObject(compiledExpression, stripRoots: true).ConfigureAwait(false);

        return obj.AsInteger();
    }

#endregion

#region Interface IObjectEvaluator

    async ValueTask<IObject> IObjectEvaluator.EvaluateObject()
    {
        var engine = await EngineFactory().ConfigureAwait(false);

        return await engine.EvalObject(compiledExpression, stripRoots: true).ConfigureAwait(false);
    }

#endregion

#region Interface IStringEvaluator

    async ValueTask<string> IStringEvaluator.EvaluateString()
    {
        var engine = await EngineFactory().ConfigureAwait(false);

        var obj = await engine.EvalObject(compiledExpression, stripRoots: true).ConfigureAwait(false);

        return obj.AsString();
    }

#endregion

#region Interface IValueExpression

    public string? Expression => valueExpression.Expression;

#endregion
}