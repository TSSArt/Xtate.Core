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
using System.Collections.Immutable;

namespace Xtate.Core
{
	public struct FinalEntity : IFinal, IVisitorEntity<FinalEntity, IFinal>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IFinal

		public IIdentifier?             Id       { get; set; }
		public ImmutableArray<IOnEntry> OnEntry  { get; set; }
		public ImmutableArray<IOnExit>  OnExit   { get; set; }
		public IDoneData?               DoneData { get; set; }

	#endregion

	#region Interface IVisitorEntity<FinalEntity,IFinal>

		void IVisitorEntity<FinalEntity, IFinal>.Init(IFinal source)
		{
			Ancestor = source;
			Id = source.Id;
			OnEntry = source.OnEntry;
			OnExit = source.OnExit;
			DoneData = source.DoneData;
		}

		bool IVisitorEntity<FinalEntity, IFinal>.RefEquals(ref FinalEntity other) =>
				OnExit == other.OnExit &&
				OnEntry == other.OnEntry &&
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(DoneData, other.DoneData);

	#endregion
	}
}