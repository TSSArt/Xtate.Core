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
using System.ComponentModel;
using Xtate.Annotations;
using Xtate.Core;

namespace Xtate.Builder
{
	[PublicAPI]
	public class StateMachineBuilder : BuilderBase, IStateMachineBuilder
	{
		private BindingType                           _bindingType;
		private IDataModel?                           _dataModel;
		private string?                               _dataModelType;
		private ImmutableArray<IIdentifier>           _initialId;
		private bool                                  _injectOptions;
		private string?                               _name;
		private StateMachineOptions                   _options;
		private IScript?                              _script;
		private ImmutableArray<IStateEntity>.Builder? _states;

		public StateMachineBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IStateMachineBuilder

		public IStateMachine Build()
		{
			var initial = !_initialId.IsDefaultOrEmpty ? (IInitial) new InitialEntity { Transition = new TransitionEntity { Target = _initialId } } : null;

			var ancestor = _injectOptions ? new AncestorContainer(_options, Ancestor) : Ancestor;

			return new StateMachineEntity
				   {
						   Ancestor = ancestor, Name = _name, Initial = initial, DataModelType = _dataModelType,
						   Binding = _bindingType, States = _states?.ToImmutable() ?? default, DataModel = _dataModel, Script = _script
				   };
		}

		public void SetInitial(ImmutableArray<IIdentifier> initialId)
		{
			if (initialId.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCannotBeEmptyList, nameof(initialId));

			_initialId = initialId;
		}

		public void SetName(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

			_name = name;
			_options.Name = name;
			_injectOptions = true;
		}

		public void SetBindingType(BindingType bindingType) => _bindingType = bindingType;

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

		public void AddFinal(IFinal final)
		{
			if (final is null) throw new ArgumentNullException(nameof(final));

			(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(final);
		}

		public void SetDataModel(IDataModel dataModel) => _dataModel = dataModel ?? throw new ArgumentNullException(nameof(dataModel));

		public void SetScript(IScript script) => _script = script ?? throw new ArgumentNullException(nameof(script));

		public void SetDataModelType(string dataModelType) => _dataModelType = dataModelType ?? throw new ArgumentNullException(nameof(dataModelType));

		public void SetPersistenceLevel(PersistenceLevel persistenceLevel)
		{
			if (persistenceLevel < PersistenceLevel.None || persistenceLevel > PersistenceLevel.ExecutableAction)
			{
				throw new InvalidEnumArgumentException(nameof(persistenceLevel), (int) persistenceLevel, typeof(PersistenceLevel));
			}

			_options.PersistenceLevel = persistenceLevel;
			_injectOptions = true;
		}

		public void SetSynchronousEventProcessing(bool value)
		{
			_options.SynchronousEventProcessing = value;
			_injectOptions = true;
		}

		public void SetExternalQueueSize(int size)
		{
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

			_options.ExternalQueueSize = size;
			_injectOptions = true;
		}

		public void SetUnhandledErrorBehaviour(UnhandledErrorBehaviour unhandledErrorBehaviour)
		{
			if (unhandledErrorBehaviour < UnhandledErrorBehaviour.DestroyStateMachine || unhandledErrorBehaviour > UnhandledErrorBehaviour.IgnoreError)
			{
				throw new InvalidEnumArgumentException(nameof(unhandledErrorBehaviour), (int) unhandledErrorBehaviour, typeof(UnhandledErrorBehaviour));
			}

			_options.UnhandledErrorBehaviour = unhandledErrorBehaviour;
			_injectOptions = true;
		}

	#endregion
	}
}