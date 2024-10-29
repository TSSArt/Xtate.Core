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

namespace Xtate.Builder;

public class ParamBuilder : BuilderBase, IParamBuilder
{
	private IValueExpression? _expression;

	private ILocationExpression? _location;

	private string? _name;

#region Interface IParamBuilder

	public IParam Build() => new ParamEntity { Ancestor = Ancestor, Name = _name, Expression = _expression, Location = _location };

	public void SetName(string name)
	{
		Infra.RequiresNonEmptyString(name);

		_name = name;
	}

	public void SetExpression(IValueExpression expression)
	{
		Infra.Requires(expression);

		_expression = expression;
	}

	public void SetLocation(ILocationExpression location)
	{
		Infra.Requires(location);

		_location = location;
	}

#endregion
}