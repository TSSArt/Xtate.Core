// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.DataModel.Runtime;

namespace Xtate.Builder;

public class ParallelFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
{
	public required IParallelBuilder  Builder      { private get; [UsedImplicitly] init; }
	public required Action<IParallel> BuiltAction  { private get; [UsedImplicitly] init; }
	public required TOuterBuilder     OuterBuilder { private get; [UsedImplicitly] init; }

	public required Func<ParallelFluentBuilder<TOuterBuilder>, Action<IState>, StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>> StateFluentBuilderFactory
	{
		private get;
		[UsedImplicitly] init;
	}

	public required Func<ParallelFluentBuilder<TOuterBuilder>, Action<IParallel>, ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>> ParallelFluentBuilderFactory
	{
		private get;
		[UsedImplicitly] init;
	}

	public required Func<ParallelFluentBuilder<TOuterBuilder>, Action<IHistory>, HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>> HistoryFluentBuilderFactory
	{
		private get;
		[UsedImplicitly] init;
	}

	public required Func<ParallelFluentBuilder<TOuterBuilder>, Action<ITransition>, TransitionFluentBuilder<ParallelFluentBuilder<TOuterBuilder>>> TransitionFluentBuilderFactory
	{
		private get;
		init;
	}

	public TOuterBuilder EndParallel()
	{
		BuiltAction(Builder.Build());

		return OuterBuilder;
	}

	public ParallelFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier) id);

	public ParallelFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
	{
		Infra.Requires(id);

		Builder.SetId(id);

		return this;
	}

	private ParallelFluentBuilder<TOuterBuilder> AddOnEntry(IExecutableEntity action)
	{
		Builder.AddOnEntry(new OnEntryEntity { Action = [action] });

		return this;
	}

	public ParallelFluentBuilder<TOuterBuilder> AddOnEntry(Action action) => AddOnEntry(RuntimeAction.GetAction(action));

	public ParallelFluentBuilder<TOuterBuilder> AddOnEntry(Func<ValueTask> action) => AddOnEntry(RuntimeAction.GetAction(action));

	private ParallelFluentBuilder<TOuterBuilder> AddOnExit(IExecutableEntity action)
	{
		Builder.AddOnExit(new OnExitEntity { Action = [action] });

		return this;
	}

	public ParallelFluentBuilder<TOuterBuilder> AddOnExit(Action action) => AddOnExit(RuntimeAction.GetAction(action));

	public ParallelFluentBuilder<TOuterBuilder> AddOnExit(Func<ValueTask> action) => AddOnExit(RuntimeAction.GetAction(action));

	public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState() => StateFluentBuilderFactory(this, Builder.AddState);

	public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel() => ParallelFluentBuilderFactory(this, Builder.AddParallel);

	public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory() => HistoryFluentBuilderFactory(this, Builder.AddHistory);

	public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState(string id) => BeginState((Identifier) id);

	public StateFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginState(IIdentifier id) => StateFluentBuilderFactory(this, Builder.AddState).SetId(id);

	public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel(string id) => BeginParallel((Identifier) id);

	public ParallelFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginParallel(IIdentifier id) => ParallelFluentBuilderFactory(this, Builder.AddParallel).SetId(id);

	public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory(string id) => BeginHistory((Identifier) id);

	public HistoryFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginHistory(IIdentifier id) => HistoryFluentBuilderFactory(this, Builder.AddHistory).SetId(id);

	public TransitionFluentBuilder<ParallelFluentBuilder<TOuterBuilder>> BeginTransition() => TransitionFluentBuilderFactory(this, Builder.AddTransition);

	public ParallelFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, string target) => AddTransition(eventDescriptor, (Identifier) target);

	public ParallelFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, IIdentifier target) => BeginTransition().SetEvent(eventDescriptor).SetTarget(target).EndTransition();

	public ParallelFluentBuilder<TOuterBuilder> AddTransition(Func<bool> condition, string target) => AddTransition(condition, (Identifier) target);

	public ParallelFluentBuilder<TOuterBuilder> AddTransition(Func<bool> condition, IIdentifier target) => BeginTransition().SetCondition(condition).SetTarget(target).EndTransition();
}