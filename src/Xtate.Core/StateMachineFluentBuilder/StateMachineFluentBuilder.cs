﻿// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Builder;

public class StateMachineFluentBuilder
{
	public required IStateMachineBuilder Builder { private get; [UsedImplicitly] init; }

	public required Func<StateMachineFluentBuilder, Action<IState>, StateFluentBuilder<StateMachineFluentBuilder>> StateFluentBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<StateMachineFluentBuilder, Action<IParallel>, ParallelFluentBuilder<StateMachineFluentBuilder>> ParallelFluentBuilderFactory { private get; [UsedImplicitly] init; }

	public required Func<StateMachineFluentBuilder, Action<IFinal>, FinalFluentBuilder<StateMachineFluentBuilder>> FinalFluentBuilderFactory { private get; [UsedImplicitly] init; }

	public IStateMachine Build()
	{
		Builder.SetDataModelType(@"runtime");

		return Builder.Build();
	}

	public StateMachineFluentBuilder SetInitial(params string[] initial)
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

	public StateMachineFluentBuilder SetInitial(params IIdentifier[] initial)
	{
		Infra.RequiresNonEmptyCollection(initial);

		Builder.SetInitial([.. initial]);

		return this;
	}

	public StateMachineFluentBuilder SetInitial(ImmutableArray<string> initial)
	{
		Infra.RequiresNonEmptyCollection(initial);

		Builder.SetInitial(ImmutableArray.CreateRange<string, IIdentifier>(initial, id => (Identifier) id));

		return this;
	}

	public StateMachineFluentBuilder SetInitial(ImmutableArray<IIdentifier> initial)
	{
		Infra.RequiresNonEmptyCollection(initial);

		Builder.SetInitial(initial);

		return this;
	}

	public StateFluentBuilder<StateMachineFluentBuilder> BeginState() => StateFluentBuilderFactory(this, Builder.AddState);

	public ParallelFluentBuilder<StateMachineFluentBuilder> BeginParallel() => ParallelFluentBuilderFactory(this, Builder.AddParallel);

	public FinalFluentBuilder<StateMachineFluentBuilder> BeginFinal() => FinalFluentBuilderFactory(this, Builder.AddFinal);

	public StateFluentBuilder<StateMachineFluentBuilder> BeginState(string id) => BeginState((Identifier) id);

	public StateFluentBuilder<StateMachineFluentBuilder> BeginState(IIdentifier id) => StateFluentBuilderFactory(this, Builder.AddState).SetId(id);

	public ParallelFluentBuilder<StateMachineFluentBuilder> BeginParallel(string id) => BeginParallel((Identifier) id);

	public ParallelFluentBuilder<StateMachineFluentBuilder> BeginParallel(IIdentifier id) => ParallelFluentBuilderFactory(this, Builder.AddParallel).SetId(id);

	public FinalFluentBuilder<StateMachineFluentBuilder> BeginFinal(string id) => BeginFinal((Identifier) id);

	public FinalFluentBuilder<StateMachineFluentBuilder> BeginFinal(IIdentifier id) => FinalFluentBuilderFactory(this, Builder.AddFinal).SetId(id);

	public StateMachineFluentBuilder SetPersistenceLevel(PersistenceLevel persistenceLevel)
	{
		Builder.SetPersistenceLevel(persistenceLevel);

		return this;
	}

	public StateMachineFluentBuilder SetSynchronousEventProcessing(bool value)
	{
		Builder.SetSynchronousEventProcessing(value);

		return this;
	}

	public StateMachineFluentBuilder SetExternalQueueSize(int size)
	{
		Builder.SetExternalQueueSize(size);

		return this;
	}
}