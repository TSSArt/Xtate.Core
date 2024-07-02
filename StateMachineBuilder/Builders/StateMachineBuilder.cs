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

namespace Xtate.Builder;

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
		Infra.RequiresNonEmptyCollection(initialId);

		_initialId = initialId;
	}

	public void SetName(string name)
	{
		Infra.RequiresNonEmptyString(name);

		_name = name;
		_options.Name = name;
		_injectOptions = true;
	}

	public void SetBindingType(BindingType bindingType)
	{
		Infra.RequiresValidEnum(bindingType);

		_bindingType = bindingType;
	}

	public void AddState(IState state)
	{
		Infra.Requires(state);

		(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(state);
	}

	public void AddParallel(IParallel parallel)
	{
		Infra.Requires(parallel);

		(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(parallel);
	}

	public void AddFinal(IFinal final)
	{
		Infra.Requires(final);

		(_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(final);
	}

	public void SetDataModel(IDataModel dataModel)
	{
		Infra.Requires(dataModel);

		_dataModel = dataModel;
	}

	public void SetScript(IScript script)
	{
		Infra.Requires(script);

		_script = script;
	}

	public void SetDataModelType(string dataModelType)
	{
		Infra.Requires(dataModelType);

		_dataModelType = dataModelType;
	}

	public void SetPersistenceLevel(PersistenceLevel persistenceLevel)
	{
		Infra.RequiresValidEnum(persistenceLevel);

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
		Infra.RequiresNonNegative(size);

		_options.ExternalQueueSize = size;
		_injectOptions = true;
	}

	public void SetUnhandledErrorBehaviour(UnhandledErrorBehaviour unhandledErrorBehaviour)
	{
		Infra.RequiresValidEnum(unhandledErrorBehaviour);

		_options.UnhandledErrorBehaviour = unhandledErrorBehaviour;
		_injectOptions = true;
	}

#endregion
}