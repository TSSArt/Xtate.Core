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

using Xtate.DataModel;
using Xtate.Persistence;

namespace Xtate.Core;

public sealed class StateMachineNode : StateEntityNode, IStateMachine, IAncestorProvider, IDebugEntityId
{
	private readonly IStateMachine _stateMachine;

	public StateMachineNode(DocumentIdNode documentIdNode, IStateMachine stateMachine) : base(documentIdNode)
	{
		Infra.Requires(stateMachine);
		Infra.Requires(stateMachine.Initial);

		_stateMachine = stateMachine;

		Initial = stateMachine.Initial.As<InitialNode>();
		ScriptEvaluator = stateMachine.Script?.As<ScriptNode>();
		DataModel = stateMachine.DataModel?.As<DataModelNode>();
		States = stateMachine.States.AsArrayOf<IStateEntity, StateEntityNode>(true);

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

	protected override void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.StateMachineNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.Add(Key.Name, Name);
		bucket.Add(Key.DataModelType, DataModelType);
		bucket.Add(Key.Binding, Binding);
		bucket.AddEntity(Key.Script, Script);
		bucket.AddEntity(Key.DataModel, DataModel);
		bucket.AddEntity(Key.Initial, Initial);
		bucket.AddEntityList(Key.States, _stateMachine.States);
	}
}