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
using Xtate.Persistence;

namespace Xtate.Core
{
	internal sealed class CustomActionNode : ExecutableEntityNode, ICustomAction, IAncestorProvider
	{
		private readonly CustomActionEntity _entity;

		public CustomActionNode(in DocumentIdRecord documentIdNode, in CustomActionEntity entity) : base(documentIdNode, (ICustomAction?) entity.Ancestor)
		{
			Infrastructure.NotNull(entity.Xml);

			_entity = entity;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface ICustomAction

		public string XmlNamespace => _entity.XmlNamespace!;

		public string XmlName => _entity.XmlName!;

		public string Xml => _entity.Xml!;

		public ImmutableArray<ILocationExpression> Locations => _entity.Locations;

		public ImmutableArray<IValueExpression> Values => _entity.Values;

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
}