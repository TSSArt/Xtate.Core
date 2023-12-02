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

using Xtate.Persistence;

namespace Xtate.Core;

public sealed class AssignNode : ExecutableEntityNode, IAssign, IAncestorProvider, IDebugEntityId
{
	private readonly IAssign _assign;

	public AssignNode(DocumentIdNode documentIdNode, IAssign assign) : base(documentIdNode, assign) => _assign = assign;

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _assign;

#endregion

#region Interface IAssign

	public ILocationExpression? Location => _assign.Location;

	public IValueExpression? Expression => _assign.Expression;

	public IInlineContent? InlineContent => _assign.InlineContent;

	public string? Type => _assign.Type;

	public string? Attribute => _assign.Attribute;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

#endregion

	protected override void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.AssignNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.AddEntity(Key.Location, Location);
		bucket.AddEntity(Key.Expression, Expression);
		bucket.Add(Key.InlineContent, InlineContent?.Value);
		bucket.Add(Key.Type, Type);
		bucket.Add(Key.Attribute, Attribute);
	}
}