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

public sealed class SendNode(DocumentIdNode documentIdNode, ISend send) : ExecutableEntityNode(documentIdNode, send), ISend, IAncestorProvider, IDebugEntityId
{

	#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => send;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

#endregion

#region Interface ISend

	public string?                             EventName        => send.EventName;
	public IValueExpression?                   EventExpression  => send.EventExpression;
	public Uri?                                Target           => send.Target;
	public IValueExpression?                   TargetExpression => send.TargetExpression;
	public Uri?                                Type             => send.Type;
	public IValueExpression?                   TypeExpression   => send.TypeExpression;
	public string?                             Id               => send.Id;
	public ILocationExpression?                IdLocation       => send.IdLocation;
	public int?                                DelayMs          => send.DelayMs;
	public IValueExpression?                   DelayExpression  => send.DelayExpression;
	public ImmutableArray<ILocationExpression> NameList         => send.NameList;
	public ImmutableArray<IParam>              Parameters       => send.Parameters;
	public IContent?                           Content          => send.Content;

#endregion

	protected override void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.SendNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.Add(Key.Id, Id);
		bucket.Add(Key.Type, Type);
		bucket.Add(Key.Event, EventName);
		bucket.Add(Key.Target, Target);
		bucket.Add(Key.DelayMs, DelayMs ?? 0);
		bucket.AddEntity(Key.TypeExpression, TypeExpression);
		bucket.AddEntity(Key.EventExpression, EventExpression);
		bucket.AddEntity(Key.TargetExpression, TargetExpression);
		bucket.AddEntity(Key.DelayExpression, DelayExpression);
		bucket.AddEntity(Key.IdLocation, IdLocation);
		bucket.AddEntityList(Key.NameList, NameList);
		bucket.AddEntityList(Key.Parameters, Parameters);
		bucket.AddEntity(Key.Content, Content);
	}
}