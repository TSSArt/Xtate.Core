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

using Xtate.Persistence;

namespace Xtate.Core;

public sealed class CustomActionNode : ExecutableEntityNode, ICustomAction, IAncestorProvider
{
<<<<<<< Updated upstream
	public sealed class CustomActionNode : ExecutableEntityNode, ICustomAction, IAncestorProvider
=======
	private readonly ICustomAction _customAction;

	public CustomActionNode(DocumentIdNode documentIdNode, ICustomAction customAction) : base(documentIdNode, customAction)
>>>>>>> Stashed changes
	{
		Infra.NotNull(customAction.Xml);

		_customAction = customAction;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _customAction;

#endregion

#region Interface ICustomAction

	public string XmlNamespace => _customAction.XmlNamespace!;

	public string XmlName => _customAction.XmlName!;

	public string Xml => _customAction.Xml!;

	public ImmutableArray<ILocationExpression> Locations => _customAction.Locations;

	public ImmutableArray<IValueExpression> Values => _customAction.Values;

#endregion

	protected override void Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.CustomActionNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.Add(Key.Namespace, XmlNamespace);
		bucket.Add(Key.Name, XmlName);
		bucket.Add(Key.Content, Xml);
		bucket.AddEntityList(Key.LocationList, Locations);
		bucket.AddEntityList(Key.ValueList, Values);
	}
}