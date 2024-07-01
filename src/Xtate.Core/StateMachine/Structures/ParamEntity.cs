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

namespace Xtate.Core;

public struct ParamEntity : IParam, IVisitorEntity<ParamEntity, IParam>, IAncestorProvider
{
	internal object? Ancestor;

	#region Interface IAncestorProvider

	readonly object? IAncestorProvider.Ancestor => Ancestor;

#endregion

#region Interface IParam

	public IValueExpression?    Expression { get; set; }
	public ILocationExpression? Location   { get; set; }
	public string?              Name       { get; set; }

#endregion

#region Interface IVisitorEntity<ParamEntity,IParam>

	void IVisitorEntity<ParamEntity, IParam>.Init(IParam source)
	{
		Ancestor = source;
		Expression = source.Expression;
		Location = source.Location;
		Name = source.Name;
	}

	readonly bool IVisitorEntity<ParamEntity, IParam>.RefEquals(ref ParamEntity other) =>
		ReferenceEquals(Expression, other.Expression) &&
		ReferenceEquals(Location, other.Location) &&
		ReferenceEquals(Name, other.Name);

#endregion
}