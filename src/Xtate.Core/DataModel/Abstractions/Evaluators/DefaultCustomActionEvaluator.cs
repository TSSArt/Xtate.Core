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

using Xtate.CustomAction;

namespace Xtate.DataModel;

public abstract class CustomActionEvaluator(ICustomAction customAction) : ICustomAction, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => customAction;

#endregion

#region Interface ICustomAction

	public virtual string? XmlNamespace => customAction.XmlNamespace;

	public virtual string? XmlName => customAction.XmlName;

	public virtual string? Xml => customAction.Xml;

	public virtual ImmutableArray<ILocationExpression> Locations => customAction.Locations;

	public virtual ImmutableArray<IValueExpression> Values => customAction.Values;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion
}

public class DefaultCustomActionEvaluator : CustomActionEvaluator
{
	private readonly CustomActionContainer? _customActionContainer;

	public DefaultCustomActionEvaluator(ICustomAction customAction) : base(customAction)
	{
		if (customAction.Is(out _customActionContainer))
		{
			_customActionContainer.SetEvaluators(base.Values, base.Locations);
		}
	}

	public override ValueTask Execute() => _customActionContainer?.Execute() ?? default;
}