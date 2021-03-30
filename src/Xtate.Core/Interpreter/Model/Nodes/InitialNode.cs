#region Copyright © 2019-2021 Sergii Artemenko

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

namespace Xtate.Core
{
	internal sealed class InitialNode : StateEntityNode, IInitial, IAncestorProvider, IDebugEntityId
	{
		private readonly IInitial? _initial;

		public InitialNode(DocumentIdNode documentIdNode, IInitial initial) : base(documentIdNode, children: null)
		{
			Infra.NotNull(initial.Transition);

			_initial = initial;
			Transition = initial.Transition.As<TransitionNode>();

			Transition.SetSource(this);
		}

		public InitialNode(DocumentIdNode documentIdNode, TransitionNode transition) : base(documentIdNode, children: null)
		{
			Transition = transition ?? throw new ArgumentNullException(nameof(transition));

			Transition.SetSource(this);
		}

		public TransitionNode Transition { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _initial;

	#endregion

	#region Interface IDebugEntityId

		public FormattableString EntityId => @$"(#{DocumentId})";

	#endregion

	#region Interface IInitial

		ITransition IInitial.Transition => _initial?.Transition ?? Infra.Fail<ITransition>();

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.InitialNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Transition, Transition);
		}
	}
}