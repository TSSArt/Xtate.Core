﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using Xtate.Core;

namespace Xtate.Builder
{
	public class ParallelBuilder : BuilderBase, IParallelBuilder
	{
		private IDataModel?                           _dataModel;
		private ImmutableArray<IHistory>.Builder?     _historyStates;
		private IIdentifier?                          _id;
		private ImmutableArray<IInvoke>.Builder?      _invokeList;
		private ImmutableArray<IOnEntry>.Builder?     _onEntryList;
		private ImmutableArray<IOnExit>.Builder?      _onExitList;
		private ImmutableArray<IStateEntity>.Builder? _states;
		private ImmutableArray<ITransition>.Builder?  _transitions;

		public ParallelBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IParallelBuilder

		public IParallel Build() =>
			new ParallelEntity
			{
				Ancestor = Ancestor, Id = _id, States = _states?.ToImmutable() ?? default, HistoryStates = _historyStates?.ToImmutable() ?? default,
				Transitions = _transitions?.ToImmutable() ?? default, DataModel = _dataModel, OnEntry = _onEntryList?.ToImmutable() ?? default,
				OnExit = _onExitList?.ToImmutable() ?? default, Invoke = _invokeList?.ToImmutable() ?? default
			};

		public void SetId(IIdentifier id) => _id = id ?? throw new ArgumentNullException(nameof(id));

		public void AddState(IState state)
		{
			if (state is null) throw new ArgumentNullException(nameof(state));

			(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(state);
		}

		public void AddParallel(IParallel parallel)
		{
			if (parallel is null) throw new ArgumentNullException(nameof(parallel));

			(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(parallel);
		}

		public void AddHistory(IHistory history)
		{
			if (history is null) throw new ArgumentNullException(nameof(history));

			(_historyStates ??= ImmutableArray.CreateBuilder<IHistory>()).Add(history);
		}

		public void AddTransition(ITransition transition)
		{
			if (transition is null) throw new ArgumentNullException(nameof(transition));

			(_transitions ??= ImmutableArray.CreateBuilder<ITransition>()).Add(transition);
		}

		public void AddOnEntry(IOnEntry onEntry)
		{
			if (onEntry is null) throw new ArgumentNullException(nameof(onEntry));

			(_onEntryList ??= ImmutableArray.CreateBuilder<IOnEntry>()).Add(onEntry);
		}

		public void AddOnExit(IOnExit onExit)
		{
			if (onExit is null) throw new ArgumentNullException(nameof(onExit));

			(_onExitList ??= ImmutableArray.CreateBuilder<IOnExit>()).Add(onExit);
		}

		public void AddInvoke(IInvoke invoke)
		{
			if (invoke is null) throw new ArgumentNullException(nameof(invoke));

			(_invokeList ??= ImmutableArray.CreateBuilder<IInvoke>()).Add(invoke);
		}

		public void SetDataModel(IDataModel dataModel) => _dataModel = dataModel ?? throw new ArgumentNullException(nameof(dataModel));

	#endregion
	}
}