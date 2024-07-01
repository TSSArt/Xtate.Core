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

<<<<<<< Updated upstream
using System;
using System.Xml;
using Xtate.Core;
=======
using System.Xml;
>>>>>>> Stashed changes
using Xtate.Scxml;

namespace Xtate.DataModel.XPath;

public class XPathXmlParserContextFactory
{
<<<<<<< Updated upstream
	public required INameTableProvider? NameTableProvider { private get; init; }
=======
<<<<<<<< Updated upstream:src/Xtate.Core/src/Xtate.Core/Interpreter/Model/IdentifierEqualityComparer.cs
========
	public required INameTableProvider? NameTableProvider { private get; [UsedImplicitly] init; }
>>>>>>> Stashed changes

	public XmlParserContext CreateContext(object entity)
	{
		var nameTable = NameTableProvider?.GetNameTable() ?? new NameTable();

		var namespaceManager = new XmlNamespaceManager(nameTable);

		if (entity.Is<IXmlNamespacesInfo>(out var namespacesInfo))
		{
			foreach (var prefixUri in namespacesInfo.Namespaces)
			{
				namespaceManager.AddNamespace(prefixUri.Prefix, prefixUri.Namespace);
			}
		}

		return new XmlParserContext(nameTable, namespaceManager, xmlLang: null, XmlSpace.None);
	}
<<<<<<< Updated upstream
=======
>>>>>>>> Stashed changes:src/Xtate.Core/src/Xtate.Core/DataModel/Handlers/XPath/XPathXmlParserContextFactory.cs
>>>>>>> Stashed changes
}