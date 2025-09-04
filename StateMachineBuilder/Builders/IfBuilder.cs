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

namespace Xtate.Builder;

public class IfBuilder : BuilderBase, IIfBuilder
{
    private ImmutableArray<IExecutableEntity>.Builder? _actions;

    private IConditionExpression? _condition;

#region Interface IIfBuilder

    public IIf Build() => new IfEntity { Ancestor = Ancestor, Condition = _condition, Action = _actions?.ToImmutable() ?? default };

    public void SetCondition(IConditionExpression condition)
    {
        Infra.Requires(condition);

        _condition = condition;
    }

    public void AddAction(IExecutableEntity action)
    {
        Infra.Requires(action);

        (_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);
    }

#endregion
}