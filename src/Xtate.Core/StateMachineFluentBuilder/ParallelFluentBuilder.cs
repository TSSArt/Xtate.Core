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
using System.Diagnostics.CodeAnalysis;
using Xtate.Core;

namespace Xtate.Builder
{
	[PublicAPI]
	public class ParallelFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
	{
		private readonly IParallelBuilder  _builder;
		private readonly Action<IParallel> _builtAction;
		private readonly IBuilderFactory   _factory;
		private readonly TOuterBuilder     _outerBuilder;

		public ParallelFluentBuilder(IBuilderFactory factory, TOuterBuilder outerBuilder, Action<IParallel> builtAction)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_builder = factory.CreateParallelBuilder(null);
			_outerBuilder = outerBuilder;
			_builtAction = builtAction;
		}

		[return: NotNull]
		public TOuterBuilder EndParallel()
		{
			_builtAction(_builder.Build());

			return _outerBuilder;
		}

		public ParallelFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier) id);

		public ParallelFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
		{
			if (id is null) throw new ArgumentNullException(nameof(id));

			_builder.SetId(id);

			return this;
		}

		private ParallelFluentBuilder<TOuterBuilder> AddOnEntry(RuntimeAction action)
		{
			_builder.AddOnEntry(new OnEntryEntity { Action = ImmutableArray.Create<IExecutableEntity>(action) });

			return this;
		}

		public ParallelFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableAction action) => AddOnEntry(new RuntimeAction(action));

		public ParallelFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableTask task) => AddOnEntry(new RuntimeAction(task));

		public ParallelFluentBuilder<TOuterBuilder> AddOnEntry(ExecutableCancellableTask task) => AddOnEntry(new RuntimeAction(task));

		private ParallelFluentBuilder<TOuterBuilder> AddOnExit(RuntimeAction action)
		{
			_builder.AddOnExit(new OnExitEntity { Action = ImmutableArray.Create<IExecutableEntity>(action) });

			return this;
		}

		public ParallelFluentBuilder<TOuterBuilder> AddOnExit(ExecutableAction action) => AddOnExit(new RuntimeAction(action));

		public ParallelFluentBuilder<TOuterBuilder> AddOnExit(ExecutableTask task) => AddOnExit(new RuntimeAction(task));

		public ParallelFluentBuilder<TOuterBuilder> AddOnExit(ExecutableCancellableTask task) => AddOnExit(new RuntimeAction(task));

		public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState() => new(_factory, this, _builder.AddState);

		public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel() => new(_factory, this, _builder.AddParallel);

		public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory() => new(_factory, this, _builder.AddHistory);

		public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState(string id) => BeginState((Identifier) id);

		public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState(IIdentifier id) =>
			new StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddState).SetId(id);

		public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel(string id) => BeginParallel((Identifier) id);

		public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel(IIdentifier id) =>
			new ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddParallel).SetId(id);

		public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory(string id) => BeginHistory((Identifier) id);

		public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory(IIdentifier id) =>
			new HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>(_factory, this, _builder.AddHistory).SetId(id);

		public TransitionFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginTransition() => new(_factory, this, _builder.AddTransition);

		public ParallelFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, string target) => AddTransition(eventDescriptor, (Identifier) target);

		public ParallelFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, IIdentifier target) => BeginTransition().SetEvent(eventDescriptor).SetTarget(target).EndTransition();

		public ParallelFluentBuilder<TOuterBuilder> AddTransition(Predicate condition, string target) => AddTransition(condition, (Identifier) target);

		public ParallelFluentBuilder<TOuterBuilder> AddTransition(Predicate condition, IIdentifier target) => BeginTransition().SetCondition(condition).SetTarget(target).EndTransition();
	}
}