#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Xtate.Annotations;

namespace Xtate.Builder
{
	[PublicAPI]
	public class StateFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
	{
		private readonly IStateBuilder   _builder;
		private readonly Action<IState>  _builtAction;
		private readonly IBuilderFactory _factory;
		private readonly TOuterBuilder   _outerBuilder;

		public StateFluentBuilder(IBuilderFactory factory, TOuterBuilder outerBuilder, Action<IState> builtAction)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_builder = factory.CreateStateBuilder(null);
			_outerBuilder = outerBuilder;
			_builtAction = builtAction;
		}

		[return: NotNull]
		public TOuterBuilder EndState()
		{
			_builtAction(_builder.Build());

			return _outerBuilder;
		}

		public StateFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier) id);

		public StateFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));

			_builder.SetId(id);

			return this;
		}

		public StateFluentBuilder<TOuterBuilder> SetInitial(params string[] initial)
		{
			if (initial == null) throw new ArgumentNullException(nameof(initial));
			if (initial.Length == 0) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyCollection, nameof(initial));

			var builder = ImmutableArray.CreateBuilder<IIdentifier>(initial.Length);

			foreach (var s in initial)
			{
				builder.Add((Identifier) s);
			}

			_builder.SetInitial(builder.MoveToImmutable());

			return this;
		}

		public StateFluentBuilder<TOuterBuilder> SetInitial(params IIdentifier[] initial)
		{
			if (initial == null) throw new ArgumentNullException(nameof(initial));
			if (initial.Length == 0) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyCollection, nameof(initial));

			_builder.SetInitial(initial.ToImmutableArray());

			return this;
		}

		public StateFluentBuilder<TOuterBuilder> SetInitial(ImmutableArray<string> initial)
		{
			if (initial.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyList, nameof(initial));

			_builder.SetInitial(ImmutableArray.CreateRange<string, IIdentifier>(initial, id => (Identifier) id));

			return this;
		}

		public StateFluentBuilder<TOuterBuilder> SetInitial(ImmutableArray<IIdentifier> initial)
		{
			if (initial.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyList, nameof(initial));

			_builder.SetInitial(initial);

			return this;
		}

		private StateFluentBuilder<TOuterBuilder> AddOnEntry(RuntimeAction action)
		{
			_builder.AddOnEntry(new OnEntryEntity { Action = ImmutableArray.Create<IExecutableEntity>(action) });

			return this;
		}

		public StateFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableAction action) => AddOnEntry(new RuntimeAction(action));

		public StateFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableTask task) => AddOnEntry(new RuntimeAction(task));

		public StateFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableCancellableTask task) => AddOnEntry(new RuntimeAction(task));

		private StateFluentBuilder<TOuterBuilder> AddOnExit(RuntimeAction action)
		{
			_builder.AddOnExit(new OnExitEntity { Action = ImmutableArray.Create<IExecutableEntity>(action) });

			return this;
		}

		public StateFluentBuilder<TOuterBuilder> AddOnExit(ExecutableAction action) => AddOnExit(new RuntimeAction(action));

		public StateFluentBuilder<TOuterBuilder> AddOnExit(ExecutableTask task) => AddOnExit(new RuntimeAction(task));

		public StateFluentBuilder<TOuterBuilder> AddOnExit(ExecutableCancellableTask task) => AddOnExit(new RuntimeAction(task));

		public InitialFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginInitial() => new InitialFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.SetInitial);

		public StateFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginState() => new StateFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddState);

		public ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginParallel() => new ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddParallel);

		public FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginFinal() => new FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddFinal);

		public HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginHistory() => new HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddHistory);

		public StateFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginState(string id) => BeginState((Identifier) id);

		public StateFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginState(IIdentifier id) =>
				new StateFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddState).SetId(id);

		public ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginParallel(string id) => BeginParallel((Identifier) id);

		public ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginParallel(IIdentifier id) =>
				new ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddParallel).SetId(id);

		public FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginFinal(string id) => BeginFinal((Identifier) id);

		public FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginFinal(IIdentifier id) =>
				new FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddFinal).SetId(id);

		public HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginHistory(string id) => BeginHistory((Identifier) id);

		public HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginHistory(IIdentifier id) =>
				new HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddHistory).SetId(id);

		public TransitionFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginTransition() => new TransitionFluentBuilder<StateFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddTransition);

		public StateFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, string target) => AddTransition(eventDescriptor, (Identifier) target);

		public StateFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, IIdentifier target) => BeginTransition().SetEvent(eventDescriptor).SetTarget(target).EndTransition();

		public StateFluentBuilder<TOuterBuilder> AddTransition(Predicate condition, string target) => AddTransition(condition, (Identifier) target);

		public StateFluentBuilder<TOuterBuilder> AddTransition(Predicate condition, IIdentifier target) => BeginTransition().SetCondition(condition).SetTarget(target).EndTransition();
	}
}