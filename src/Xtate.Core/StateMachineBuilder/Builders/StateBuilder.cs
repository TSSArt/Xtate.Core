// Copyright © 2019-2025 Sergii Artemenko
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

public class StateBuilder : BuilderBase, IStateBuilder
{
    private IDataModel? _dataModel;

    private ImmutableArray<IHistory>.Builder? _historyStates;

    private IIdentifier? _id;

    private IInitial? _initial;

    private ImmutableArray<IIdentifier> _initialId;

    private ImmutableArray<IInvoke>.Builder? _invokeList;

    private ImmutableArray<IOnEntry>.Builder? _onEntryList;

    private ImmutableArray<IOnExit>.Builder? _onExitList;

    private ImmutableArray<IStateEntity>.Builder? _states;

    private ImmutableArray<ITransition>.Builder? _transitions;

    public required IErrorProcessorService<StateBuilder> ErrorProcessorService { private get; [UsedImplicitly] init; }

#region Interface IStateBuilder

    public IState Build()
    {
        if (!_initialId.IsDefaultOrEmpty)
        {
            if (_initial is not null)
            {
                ErrorProcessorService.AddError(Ancestor, Resources.ErrorMessage_InitialAttributeAndInitialStateCantBeUsedAtTheSameTimeInStateElement);
            }

            _initial = new InitialEntity { Transition = new TransitionEntity { Target = _initialId } };
        }

        return new StateEntity
               {
                   Ancestor = Ancestor, Id = _id, Initial = _initial, States = _states?.ToImmutable() ?? default, HistoryStates = _historyStates?.ToImmutable() ?? default,
                   Transitions = _transitions?.ToImmutable() ?? default, DataModel = _dataModel, OnEntry = _onEntryList?.ToImmutable() ?? default,
                   OnExit = _onExitList?.ToImmutable() ?? default, Invoke = _invokeList?.ToImmutable() ?? default
               };
    }

    public void SetId(IIdentifier id)
    {
        Infra.Requires(id);

        _id = id;
    }

    public void SetInitial(ImmutableArray<IIdentifier> initialId)
    {
        Infra.RequiresNonEmptyCollection(initialId);

        _initialId = initialId;
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

    public void SetInitial(IInitial initial)
    {
        Infra.Requires(initial);

        _initial = initial;
    }

    public void AddFinal(IFinal final)
    {
        Infra.Requires(final);

        (_states ??= ImmutableArray.CreateBuilder<IStateEntity>()).Add(final);
    }

    public void AddHistory(IHistory history)
    {
        Infra.Requires(history);

        (_historyStates ??= ImmutableArray.CreateBuilder<IHistory>()).Add(history);
    }

    public void AddTransition(ITransition transition)
    {
        Infra.Requires(transition);

        (_transitions ??= ImmutableArray.CreateBuilder<ITransition>()).Add(transition);
    }

    public void AddOnEntry(IOnEntry onEntry)
    {
        Infra.Requires(onEntry);

        (_onEntryList ??= ImmutableArray.CreateBuilder<IOnEntry>()).Add(onEntry);
    }

    public void AddOnExit(IOnExit onExit)
    {
        Infra.Requires(onExit);

        (_onExitList ??= ImmutableArray.CreateBuilder<IOnExit>()).Add(onExit);
    }

    public void AddInvoke(IInvoke invoke)
    {
        Infra.Requires(invoke);

        (_invokeList ??= ImmutableArray.CreateBuilder<IInvoke>()).Add(invoke);
    }

    public void SetDataModel(IDataModel dataModel)
    {
        Infra.Requires(dataModel);

        _dataModel = dataModel;
    }

#endregion
}