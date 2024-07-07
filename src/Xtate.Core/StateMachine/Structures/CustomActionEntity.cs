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

public struct CustomActionEntity : ICustomAction, IVisitorEntity<CustomActionEntity, ICustomAction>, IAncestorProvider
{
	internal object? Ancestor;

#region Interface IAncestorProvider

	readonly object? IAncestorProvider.Ancestor => Ancestor;

#endregion

#region Interface ICustomAction

	public string? XmlNamespace { get; set; }

	public string? XmlName { get; set; }

	public string? Xml { get; set; }

	public ImmutableArray<ILocationExpression> Locations { get; set; }

	public ImmutableArray<IValueExpression> Values { get; set; }

#endregion

#region Interface IVisitorEntity<CustomActionEntity,ICustomAction>

	void IVisitorEntity<CustomActionEntity, ICustomAction>.Init(ICustomAction source)
	{
		Ancestor = source;
		XmlNamespace = source.XmlNamespace;
		XmlName = source.XmlName;
		Xml = source.Xml;
		Locations = source.Locations;
		Values = source.Values;
	}

	readonly bool IVisitorEntity<CustomActionEntity, ICustomAction>.RefEquals(ref CustomActionEntity other) =>
		ReferenceEquals(XmlNamespace, other.XmlNamespace) &&
		ReferenceEquals(XmlName, other.XmlName) &&
		ReferenceEquals(Xml, other.Xml) &&
		Locations == other.Locations &&
		Values == other.Values;

#endregion
}