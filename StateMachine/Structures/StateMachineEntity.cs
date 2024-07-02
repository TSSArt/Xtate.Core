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

namespace Xtate.Core;

public struct StateMachineEntity : IStateMachine, IVisitorEntity<StateMachineEntity, IStateMachine>, IAncestorProvider, IDebugEntityId
{
	internal object? Ancestor;

	#region Interface IAncestorProvider

	readonly object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IDebugEntityId

	readonly FormattableString IDebugEntityId.EntityId => @$"{Name}";

#endregion

#region Interface IStateMachine

	public string?                      DataModelType { get; set; }
	public IInitial?                    Initial       { get; set; }
	public string?                      Name          { get; set; }
	public BindingType                  Binding       { get; set; }
	public ImmutableArray<IStateEntity> States        { get; set; }
	public IDataModel?                  DataModel     { get; set; }
	public IExecutableEntity?           Script        { get; set; }

#endregion

#region Interface IVisitorEntity<StateMachineEntity,IStateMachine>

	public void Init(IStateMachine source)
	{
		Ancestor = source ?? throw new ArgumentNullException(nameof(source));
		Name = source.Name;
		Initial = source.Initial;
		DataModelType = source.DataModelType;
		Binding = source.Binding;
		States = source.States;
		DataModel = source.DataModel;
		Script = source.Script;
	}

	readonly bool IVisitorEntity<StateMachineEntity, IStateMachine>.RefEquals(ref StateMachineEntity other) =>
		Binding == other.Binding &&
		States == other.States &&
		ReferenceEquals(Name, other.Name) &&
		ReferenceEquals(DataModel, other.DataModel) &&
		ReferenceEquals(DataModelType, other.DataModelType) &&
		ReferenceEquals(Initial, other.Initial) &&
		ReferenceEquals(Script, other.Script);

#endregion
}