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

using Xtate.Interpreter.Model;
using Xtate.Persistence.Extensions;
using Xtate.Persistence.Internal;
using Xtate.Persistence.Services;
using Xtate.StateMachine;

namespace Xtate.Persistence;

[InstantiatedByIoC]
public class PersistedCustomActionNode(DocumentIdNode documentIdNode, ICustomAction customAction) : CustomActionNode(documentIdNode, customAction), IStoreSupport
{
#region Interface IStoreSupport

	void IStoreSupport.Store(Bucket bucket)
	{
		bucket.Add(Key.TypeInfo, TypeInfo.CustomActionNode);
		bucket.Add(Key.DocumentId, DocumentId);
		bucket.Add(Key.Namespace, XmlNamespace);
		bucket.Add(Key.Name, XmlName);
		bucket.Add(Key.Content, Xml);
		bucket.AddEntityList(Key.LocationList, Locations);
		bucket.AddEntityList(Key.ValueList, Values);
	}

#endregion
}