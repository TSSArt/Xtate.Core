#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System;

namespace Xtate
{
	public struct HistoryEntity : IHistory, IVisitorEntity<HistoryEntity, IHistory>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IHistory

		public IIdentifier? Id         { get; set; }
		public HistoryType  Type       { get; set; }
		public ITransition? Transition { get; set; }

	#endregion

	#region Interface IVisitorEntity<HistoryEntity,IHistory>

		void IVisitorEntity<HistoryEntity, IHistory>.Init(IHistory source)
		{
			Ancestor = source;
			Id = source.Id;
			Type = source.Type;
			Transition = source.Transition;
		}

		bool IVisitorEntity<HistoryEntity, IHistory>.RefEquals(ref HistoryEntity other) =>
				Type == other.Type &&
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(Transition, other.Transition);

	#endregion
	}
}