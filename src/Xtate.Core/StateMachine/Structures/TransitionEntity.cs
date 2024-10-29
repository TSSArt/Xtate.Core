// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Core;

public struct TransitionEntity : ITransition, IVisitorEntity<TransitionEntity, ITransition>, IAncestorProvider
{
	internal object? Ancestor;

#region Interface IAncestorProvider

	readonly object? IAncestorProvider.Ancestor => Ancestor;

#endregion

#region Interface ITransition

	public ImmutableArray<IEventDescriptor> EventDescriptors { get; set; }

	public IConditionExpression? Condition { get; set; }

	public ImmutableArray<IIdentifier> Target { get; set; }

	public TransitionType Type { get; set; }

	public ImmutableArray<IExecutableEntity> Action { get; set; }

#endregion

#region Interface IVisitorEntity<TransitionEntity,ITransition>

	void IVisitorEntity<TransitionEntity, ITransition>.Init(ITransition source)
	{
		Ancestor = source;
		Action = source.Action;
		Condition = source.Condition;
		EventDescriptors = source.EventDescriptors;
		Target = source.Target;
		Type = source.Type;
	}

	readonly bool IVisitorEntity<TransitionEntity, ITransition>.RefEquals(ref TransitionEntity other) =>
		Type == other.Type &&
		Target == other.Target &&
		Action == other.Action &&
		EventDescriptors == other.EventDescriptors &&
		ReferenceEquals(Condition, other.Condition);

#endregion
}