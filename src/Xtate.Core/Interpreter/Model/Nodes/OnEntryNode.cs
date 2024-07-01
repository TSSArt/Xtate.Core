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

using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public sealed class OnEntryNode : IOnEntry, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
{
<<<<<<< Updated upstream
	public sealed class OnEntryNode : IOnEntry, IStoreSupport, IAncestorProvider, IDocumentId, IDebugEntityId
=======
	private readonly IOnEntry       _onEntry;
	private          DocumentIdSlot _documentIdSlot;

	public OnEntryNode(DocumentIdNode documentIdNode, IOnEntry onEntry)
>>>>>>> Stashed changes
	{
		_onEntry = onEntry;
		documentIdNode.SaveToSlot(out _documentIdSlot);
		ActionEvaluators = onEntry.Action.AsArrayOf<IExecutableEntity, IExecEvaluator>();
	}

	public ImmutableArray<IExecEvaluator> ActionEvaluators { get; }

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _onEntry;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"(#{DocumentId})";

#endregion

#region Interface IDocumentId

	public int DocumentId => _documentIdSlot.CreateValue();

#endregion

#region Interface IOnEntry

	public ImmutableArray<IExecutableEntity> Action => _onEntry.Action;

#endregion

#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.OnEntryNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.AddEntityList(Key.Action, Action);
	}

#endregion
}