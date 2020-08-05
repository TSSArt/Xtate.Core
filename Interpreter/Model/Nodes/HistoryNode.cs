#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class HistoryNode : StateEntityNode, IHistory, IAncestorProvider, IDebugEntityId
	{
		private readonly HistoryEntity _history;

		public HistoryNode(in DocumentIdRecord documentIdNode, in HistoryEntity history) : base(documentIdNode, children: null)
		{
			Infrastructure.Assert(history.Transition != null);

			_history = history;

			Id = history.Id ?? new IdentifierNode(Identifier.New());
			Transition = history.Transition.As<TransitionNode>();
			Transition.SetSource(this);
		}

		public TransitionNode Transition { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _history.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}(#{DocumentId})";

	#endregion

	#region Interface IHistory

		public override IIdentifier Id   { get; }
		public          HistoryType Type => _history.Type;

		ITransition IHistory.Transition => _history.Transition!;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.HistoryNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.Add(Key.HistoryType, Type);
			bucket.AddEntity(Key.Id, Id);
			bucket.AddEntity(Key.Transition, Transition);
		}
	}
}