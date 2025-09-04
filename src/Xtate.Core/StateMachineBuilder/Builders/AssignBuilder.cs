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

public class AssignBuilder : BuilderBase, IAssignBuilder
{
    private string? _attribute;

    private IValueExpression? _expression;

    private IInlineContent? _inlineContent;

    private ILocationExpression? _location;

    private string? _type;

#region Interface IAssignBuilder

    public IAssign Build() =>
        new AssignEntity
        {
            Ancestor = Ancestor, Location = _location, Expression = _expression,
            InlineContent = _inlineContent, Type = _type, Attribute = _attribute
        };

    public void SetLocation(ILocationExpression location)
    {
        Infra.Requires(location);

        _location = location;
    }

    public void SetExpression(IValueExpression expression)
    {
        Infra.Requires(expression);

        _expression = expression;
    }

    public void SetInlineContent(IInlineContent inlineContent)
    {
        Infra.Requires(inlineContent);

        _inlineContent = inlineContent;
    }

    public void SetType(string type)
    {
        Infra.Requires(type);

        _type = type;
    }

    public void SetAttribute(string attribute)
    {
        Infra.Requires(attribute);

        _attribute = attribute;
    }

#endregion
}