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
using System.Collections.Immutable;

namespace Xtate.Core
{
	public struct ParallelEntity : IParallel, IVisitorEntity<ParallelEntity, IParallel>, IAncestorProvider, IDebugEntityId
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Id}";

	#endregion

	#region Interface IParallel

		public IIdentifier?                 Id            { get; set; }
		public ImmutableArray<IStateEntity> States        { get; set; }
		public ImmutableArray<IHistory>     HistoryStates { get; set; }
		public ImmutableArray<ITransition>  Transitions   { get; set; }
		public IDataModel?                  DataModel     { get; set; }
		public ImmutableArray<IOnEntry>     OnEntry       { get; set; }
		public ImmutableArray<IOnExit>      OnExit        { get; set; }
		public ImmutableArray<IInvoke>      Invoke        { get; set; }

	#endregion

	#region Interface IVisitorEntity<ParallelEntity,IParallel>

		void IVisitorEntity<ParallelEntity, IParallel>.Init(IParallel source)
		{
			Ancestor = source;
			Id = source.Id;
			Invoke = source.Invoke;
			States = source.States;
			HistoryStates = source.HistoryStates;
			DataModel = source.DataModel;
			OnExit = source.OnExit;
			OnEntry = source.OnEntry;
			Transitions = source.Transitions;
		}

		bool IVisitorEntity<ParallelEntity, IParallel>.RefEquals(ref ParallelEntity other) =>
				ReferenceEquals(Id, other.Id) &&
				ReferenceEquals(DataModel, other.DataModel) &&
				Invoke == other.Invoke &&
				States == other.States &&
				HistoryStates == other.HistoryStates &&
				OnExit == other.OnExit &&
				OnEntry == other.OnEntry &&
				Transitions == other.Transitions;

	#endregion
	}
}