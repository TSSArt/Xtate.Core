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
public class PersistedTransitionNode(DocumentIdNode documentIdNode, ITransition transition) : TransitionNode(documentIdNode, transition), IStoreSupport
{
#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.TransitionNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.AddEntityList(Key.Event, EventDescriptors.Array);
		bucket.AddEntity(Key.Condition, Condition);
		bucket.AddEntityList(Key.Target, Target.Array);
		bucket.Add(Key.TransitionType, Type);
		bucket.AddEntityList(Key.Action, Action);
	}

#endregion
}