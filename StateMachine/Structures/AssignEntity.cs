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

namespace Xtate.Core;

public struct AssignEntity : IAssign, IVisitorEntity<AssignEntity, IAssign>, IAncestorProvider
{
	internal object? Ancestor;

#region Interface IAncestorProvider

	readonly object? IAncestorProvider.Ancestor => Ancestor;

#endregion

#region Interface IAssign

	public ILocationExpression? Location      { get; set; }
	public IValueExpression?    Expression    { get; set; }
	public IInlineContent?      InlineContent { get; set; }
	public string?              Type          { get; set; }
	public string?              Attribute     { get; set; }

#endregion

#region Interface IVisitorEntity<AssignEntity,IAssign>

	void IVisitorEntity<AssignEntity, IAssign>.Init(IAssign source)
	{
		Ancestor = source;
		Location = source.Location;
		InlineContent = source.InlineContent;
		Expression = source.Expression;
		Type = source.Type;
		Attribute = source.Attribute;
	}

	readonly bool IVisitorEntity<AssignEntity, IAssign>.RefEquals(ref AssignEntity other) =>
		ReferenceEquals(Location, other.Location) &&
		ReferenceEquals(Expression, other.Expression) &&
		ReferenceEquals(InlineContent, other.InlineContent) &&
		ReferenceEquals(Type, other.Type) &&
		ReferenceEquals(Attribute, other.Attribute);

#endregion
}