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
using Xtate.Annotations;

namespace Xtate.Builder
{
	[PublicAPI]
	public class StateMachineFluentBuilder
	{
		private readonly IStateMachineBuilder _builder;
		private readonly IBuilderFactory      _factory;

		public StateMachineFluentBuilder(IBuilderFactory factory)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_builder = factory.CreateStateMachineBuilder(null);
			_builder.SetDataModelType(@"runtime");
		}

		public IStateMachine Build() => _builder.Build();

		public StateMachineFluentBuilder SetInitial(params string[] initial)
		{
			if (initial is null) throw new ArgumentNullException(nameof(initial));
			if (initial.Length == 0) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyCollection, nameof(initial));

			var builder = ImmutableArray.CreateBuilder<IIdentifier>(initial.Length);

			foreach (var s in initial)
			{
				builder.Add((Identifier) s);
			}

			_builder.SetInitial(builder.MoveToImmutable());

			return this;
		}

		public StateMachineFluentBuilder SetInitial(params IIdentifier[] initial)
		{
			if (initial is null) throw new ArgumentNullException(nameof(initial));
			if (initial.Length == 0) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyCollection, nameof(initial));

			_builder.SetInitial(initial.ToImmutableArray());

			return this;
		}

		public StateMachineFluentBuilder SetInitial(ImmutableArray<string> initial)
		{
			if (initial.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyList, nameof(initial));

			_builder.SetInitial(ImmutableArray.CreateRange<string, IIdentifier>(initial, id => (Identifier) id));

			return this;
		}

		public StateMachineFluentBuilder SetInitial(ImmutableArray<IIdentifier> initial)
		{
			if (initial.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeAnEmptyList, nameof(initial));

			_builder.SetInitial(initial);

			return this;
		}

		public StateFluentBuilder<StateMachineFluentBuilder> BeginState() => new StateFluentBuilder<StateMachineFluentBuilder>(_factory, this, _builder.AddState);

		public ParallelFluentBuilder<StateMachineFluentBuilder> BeginParallel() => new ParallelFluentBuilder<StateMachineFluentBuilder>(_factory, this, _builder.AddParallel);

		public FinalFluentBuilder<StateMachineFluentBuilder> BeginFinal() => new FinalFluentBuilder<StateMachineFluentBuilder>(_factory, this, _builder.AddFinal);

		public StateFluentBuilder<StateMachineFluentBuilder> BeginState(string id) => BeginState((Identifier) id);

		public StateFluentBuilder<StateMachineFluentBuilder> BeginState(IIdentifier id) => new StateFluentBuilder<StateMachineFluentBuilder>(_factory, this, _builder.AddState).SetId(id);

		public ParallelFluentBuilder<StateMachineFluentBuilder> BeginParallel(string id) => BeginParallel((Identifier) id);

		public ParallelFluentBuilder<StateMachineFluentBuilder> BeginParallel(IIdentifier id) => new ParallelFluentBuilder<StateMachineFluentBuilder>(_factory, this, _builder.AddParallel).SetId(id);

		public FinalFluentBuilder<StateMachineFluentBuilder> BeginFinal(string id) => BeginFinal((Identifier) id);

		public FinalFluentBuilder<StateMachineFluentBuilder> BeginFinal(IIdentifier id) => new FinalFluentBuilder<StateMachineFluentBuilder>(_factory, this, _builder.AddFinal).SetId(id);

		public StateMachineFluentBuilder SetPersistenceLevel(PersistenceLevel persistenceLevel)
		{
			_builder.SetPersistenceLevel(persistenceLevel);

			return this;
		}

		public StateMachineFluentBuilder SetSynchronousEventProcessing(bool value)
		{
			_builder.SetSynchronousEventProcessing(value);

			return this;
		}

		public StateMachineFluentBuilder SetExternalQueueSize(int size)
		{
			_builder.SetExternalQueueSize(size);

			return this;
		}
	}
}