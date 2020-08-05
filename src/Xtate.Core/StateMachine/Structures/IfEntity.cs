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

using System.Collections.Immutable;

namespace Xtate
{
	public struct IfEntity : IIf, IVisitorEntity<IfEntity, IIf>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IIf

		public ImmutableArray<IExecutableEntity> Action    { get; set; }
		public IConditionExpression?             Condition { get; set; }

	#endregion

	#region Interface IVisitorEntity<IfEntity,IIf>

		void IVisitorEntity<IfEntity, IIf>.Init(IIf source)
		{
			Ancestor = source;
			Action = source.Action;
			Condition = source.Condition!;
		}

		bool IVisitorEntity<IfEntity, IIf>.RefEquals(ref IfEntity other) =>
				Action == other.Action &&
				ReferenceEquals(Condition, other.Condition);

	#endregion
	}
}