// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.Ancestor;
using Xtate.Ancestor.Extensions;
using Xtate.DataModel;
using Xtate.StateMachine;
using Xtate.StateMachine.Internal;

namespace Xtate.Interpreter.Model;

[InstantiatedByIoC]
public class StateMachineNode : StateEntityNode, IStateMachine, IAncestorProvider, IDebugEntityId
{
	private readonly IStateMachine _stateMachine;

	public StateMachineNode(DocumentIdNode documentIdNode, IStateMachine stateMachine) : base(documentIdNode)
	{
		Infra.Requires(stateMachine);
		Infra.Requires(stateMachine.Initial);

		_stateMachine = stateMachine;

		Initial = stateMachine.Initial.UseAncestor.As<InitialNode>();
		ScriptEvaluator = stateMachine.Script?.UseAncestor.As<ScriptNode>();
		DataModel = stateMachine.DataModel?.UseAncestor.As<DataModelNode>();
		States = stateMachine.States.UseAncestor.ItemsAs<StateEntityNode>(true);

		Register(Initial);
		Register(States);
	}

	public override DataModelNode? DataModel { get; }

	public override ImmutableArray<StateEntityNode> States { get; }

	public InitialNode Initial { get; }

	public IExecEvaluator? ScriptEvaluator { get; }

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _stateMachine;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{Name}(#{DocumentId})";

#endregion

#region Interface IStateMachine

	public BindingType Binding => _stateMachine.Binding;

	public string? Name => _stateMachine.Name;

	public string? DataModelType => _stateMachine.DataModelType;

	public IExecutableEntity? Script => _stateMachine.Script;

	IDataModel? IStateMachine.DataModel => DataModel;

	IInitial IStateMachine.Initial => Initial;

	ImmutableArray<IStateEntity> IStateMachine.States => ImmutableArray<IStateEntity>.CastUp(States);

#endregion

}