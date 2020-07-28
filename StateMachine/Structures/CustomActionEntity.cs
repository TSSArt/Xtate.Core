#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System.Collections.Immutable;

namespace Xtate
{
	public struct CustomActionEntity : ICustomAction, IVisitorEntity<CustomActionEntity, ICustomAction>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface ICustomAction

		public string? Xml { get; set; }

		public ImmutableArray<ILocationExpression> Locations { get; set; }

		public ImmutableArray<IValueExpression> Values { get; set; }

	#endregion

	#region Interface IVisitorEntity<CustomActionEntity,ICustomAction>

		void IVisitorEntity<CustomActionEntity, ICustomAction>.Init(ICustomAction source)
		{
			Ancestor = source;
			Xml = source.Xml;
			Locations = source.Locations;
			Values = source.Values;
		}

		bool IVisitorEntity<CustomActionEntity, ICustomAction>.RefEquals(ref CustomActionEntity other) =>
				ReferenceEquals(Xml, other.Xml) &&
				Locations == other.Locations &&
				Values == other.Values;

	#endregion
	}
}