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

using Xtate.DataModel;

namespace Xtate.CustomAction;

public class CustomActionContainer : ICustomAction, IAncestorProvider
{
    private readonly IAction _action;

    private readonly ICustomAction _customAction;

    public CustomActionContainer(ICustomAction customAction, Func<ICustomAction, IAction> customActionFactory)
    {
        Infra.Requires(customAction);
        Infra.Requires(customActionFactory);

        Infra.Assert(customAction.Locations.IsDefault);
        Infra.Assert(customAction.Values.IsDefault);

        _customAction = customAction;

        _action = customActionFactory(customAction);

        var valueExpressions = ImmutableArray.CreateBuilder<IValueExpression>();

        foreach (var value in _action.GetValues())
        {
            if (value is IValueExpression { Expression: not null } valueExpression)
            {
                valueExpressions.Add(valueExpression);
            }
        }

        Values = valueExpressions.ToImmutable();

        var locationExpressions = ImmutableArray.CreateBuilder<ILocationExpression>();

        foreach (var location in _action.GetLocations())
        {
            if (location is ILocationExpression { Expression: not null } locationExpression)
            {
                locationExpressions.Add(locationExpression);
            }
        }

        Locations = locationExpressions.ToImmutable();
    }

#region Interface IAncestorProvider

    object IAncestorProvider.Ancestor => _customAction;

#endregion

#region Interface ICustomAction

    public ImmutableArray<IValueExpression> Values { get; }

    public ImmutableArray<ILocationExpression> Locations { get; }

    public string? Xml => _customAction.Xml;

    public string? XmlName => _customAction.XmlName;

    public string? XmlNamespace => _customAction.XmlNamespace;

#endregion

    public void SetEvaluators(ImmutableArray<IValueExpression> values, ImmutableArray<ILocationExpression> locations)
    {
        foreach (var value in values)
        {
            value.As<IActionValue>().SetEvaluator(value.As<IValueEvaluator>());
        }

        foreach (var location in locations)
        {
            location.As<IActionLocation>().SetEvaluator(location.As<ILocationEvaluator>());
        }
    }

    public virtual ValueTask Execute() => _action.Execute();
}