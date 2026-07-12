// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.Interpreter.Model;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Persistence;

[InstantiatedByIoC]
public class PersistedSendNode(DocumentIdNode documentIdNode, ISend send) : SendNode(documentIdNode, send), IStoreSupport
{
#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
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

#endregion
}