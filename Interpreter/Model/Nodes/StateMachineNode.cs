﻿using System;
using System.Collections.Immutable;

namespace TSSArt.StateMachine
{
	internal sealed class StateMachineNode : StateEntityNode, IStateMachine, IAncestorProvider, IDebugEntityId
	{
		private readonly StateMachineEntity _stateMachine;

		public StateMachineNode(in DocumentIdRecord documentIdNode, in StateMachineEntity stateMachine) : base(documentIdNode, GetChildNodes(stateMachine.Initial, stateMachine.States))
		{
			Infrastructure.Assert(stateMachine.Initial != null);

			_stateMachine = stateMachine;
			Initial = stateMachine.Initial.As<InitialNode>();
			ScriptEvaluator = stateMachine.Script?.As<ScriptNode>();
			DataModel = stateMachine.DataModel?.As<DataModelNode>();
		}

		public override DataModelNode? DataModel { get; }

		public InitialNode     Initial         { get; }
		public IExecEvaluator? ScriptEvaluator { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _stateMachine.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{Name}(#{DocumentId})";

	#endregion

	#region Interface IStateMachine

		public BindingType        Binding       => _stateMachine.Binding;
		public string?            Name          => _stateMachine.Name;
		public string?            DataModelType => _stateMachine.DataModelType;
		public IExecutableEntity? Script        => _stateMachine.Script;

		IDataModel? IStateMachine.                 DataModel => _stateMachine.DataModel;
		IInitial? IStateMachine.                   Initial   => _stateMachine.Initial;
		ImmutableArray<IStateEntity> IStateMachine.States    => _stateMachine.States;

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
}