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

using Xtate.DataModel.Runtime;

namespace Xtate.Builder;

public class StateFluentBuilder<TOuterBuilder> where TOuterBuilder : notnull
{
	public required IStateBuilder  Builder      { private get; [UsedImplicitly] init; }
	public required Action<IState> BuiltAction  { private get; [UsedImplicitly] init; }
	public required TOuterBuilder  OuterBuilder { private get; [UsedImplicitly] init; }

	public required Func<StateFluentBuilder<TOuterBuilder>, Action<IInitial>, InitialFluentBuilder<StateFluentBuilder<TOuterBuilder>>>       InitialFluentBuilderFactory    { private get; [UsedImplicitly] init; }
	public required Func<StateFluentBuilder<TOuterBuilder>, Action<IState>, StateFluentBuilder<StateFluentBuilder<TOuterBuilder>>>           StateFluentBuilderFactory      { private get; [UsedImplicitly] init; }
	public required Func<StateFluentBuilder<TOuterBuilder>, Action<IParallel>, ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>>>     ParallelFluentBuilderFactory   { private get; [UsedImplicitly] init; }
	public required Func<StateFluentBuilder<TOuterBuilder>, Action<IFinal>, FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>>>           FinalFluentBuilderFactory      { private get; [UsedImplicitly] init; }
	public required Func<StateFluentBuilder<TOuterBuilder>, Action<IHistory>, HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>>>       HistoryFluentBuilderFactory    { private get; [UsedImplicitly] init; }
	public required Func<StateFluentBuilder<TOuterBuilder>, Action<ITransition>, TransitionFluentBuilder<StateFluentBuilder<TOuterBuilder>>> TransitionFluentBuilderFactory { private get; [UsedImplicitly] init; }

	public TOuterBuilder EndState()
	{
		BuiltAction(Builder.Build());

		return OuterBuilder;
	}

	public StateFluentBuilder<TOuterBuilder> SetId(string id) => SetId((Identifier) id);

	public StateFluentBuilder<TOuterBuilder> SetId(IIdentifier id)
	{
		Infra.Requires(id);

		Builder.SetId(id);

		return this;
	}

	public StateFluentBuilder<TOuterBuilder> SetInitial(params string[] initial)
	{
		Infra.RequiresNonEmptyCollection(initial);

		var builder = ImmutableArray.CreateBuilder<IIdentifier>(initial.Length);

		foreach (var s in initial)
		{
			builder.Add((Identifier) s);
		}

		Builder.SetInitial(builder.MoveToImmutable());

		return this;
	}

	public StateFluentBuilder<TOuterBuilder> SetInitial(params IIdentifier[] initial)
	{
		Infra.RequiresNonEmptyCollection(initial);

		Builder.SetInitial(initial.ToImmutableArray());

		return this;
	}

	public StateFluentBuilder<TOuterBuilder> SetInitial(ImmutableArray<string> initial)
	{
		Infra.RequiresNonEmptyCollection(initial);

		Builder.SetInitial(ImmutableArray.CreateRange<string, IIdentifier>(initial, id => (Identifier) id));

		return this;
	}

	public StateFluentBuilder<TOuterBuilder> SetInitial(ImmutableArray<IIdentifier> initial)
	{
		Infra.RequiresNonEmptyCollection(initial);

		Builder.SetInitial(initial);

		return this;
	}

	private StateFluentBuilder<TOuterBuilder> AddOnEntry(IExecutableEntity action)
	{
		Builder.AddOnEntry(new OnEntryEntity { Action = ImmutableArray.Create(action) });

		return this;
	}

	public StateFluentBuilder<TOuterBuilder> AddOnEntry(Action action) => AddOnEntry(RuntimeAction.GetAction(action));

	public StateFluentBuilder<TOuterBuilder> AddOnEntryAsync(Func<ValueTask> action) => AddOnEntry(RuntimeAction.GetAction(action));

	private StateFluentBuilder<TOuterBuilder> AddOnExit(IExecutableEntity action)
	{
		Builder.AddOnExit(new OnExitEntity { Action = ImmutableArray.Create(action) });

		return this;
	}

	public StateFluentBuilder<TOuterBuilder> AddOnExit(Action action) => AddOnExit(RuntimeAction.GetAction(action));

	public StateFluentBuilder<TOuterBuilder> AddOnExitAsync(Func<ValueTask> action) => AddOnExit(RuntimeAction.GetAction(action));

	public InitialFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginInitial() => InitialFluentBuilderFactory(this, Builder.SetInitial);

	public StateFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginState() => StateFluentBuilderFactory(this, Builder.AddState);

	public ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginParallel() => ParallelFluentBuilderFactory(this, Builder.AddParallel);

	public FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginFinal() => FinalFluentBuilderFactory(this, Builder.AddFinal);

	public HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginHistory() => HistoryFluentBuilderFactory(this, Builder.AddHistory);

	public StateFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginState(string id) => BeginState((Identifier) id);

	public StateFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginState(IIdentifier id) => StateFluentBuilderFactory(this, Builder.AddState).SetId(id);

	public ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginParallel(string id) => BeginParallel((Identifier) id);

	public ParallelFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginParallel(IIdentifier id) => ParallelFluentBuilderFactory(this, Builder.AddParallel).SetId(id);

	public FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginFinal(string id) => BeginFinal((Identifier) id);

	public FinalFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginFinal(IIdentifier id) => FinalFluentBuilderFactory(this, Builder.AddFinal).SetId(id);

	public HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginHistory(string id) => BeginHistory((Identifier) id);

	public HistoryFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginHistory(IIdentifier id) => HistoryFluentBuilderFactory(this, Builder.AddHistory).SetId(id);

	public TransitionFluentBuilder<StateFluentBuilder<TOuterBuilder>> BeginTransition() => TransitionFluentBuilderFactory(this, Builder.AddTransition);

	public StateFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, string target) => AddTransition(eventDescriptor, (Identifier) target);

	public StateFluentBuilder<TOuterBuilder> AddTransition(EventDescriptor eventDescriptor, IIdentifier target) => BeginTransition().SetEvent(eventDescriptor).SetTarget(target).EndTransition();

	public StateFluentBuilder<TOuterBuilder> AddTransition(Func<bool> condition, string target) => AddTransition(condition, (Identifier) target);

	public StateFluentBuilder<TOuterBuilder> AddTransition(Func<bool> condition, IIdentifier target) => BeginTransition().SetConditionFunc(condition).SetTarget(target).EndTransition();
}