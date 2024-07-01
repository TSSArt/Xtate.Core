// Copyright © 2019-2023 Sergii Artemenko
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

using System.ComponentModel;
using System.IO;
using System.Xml;

namespace Xtate.CustomAction;

public abstract class CustomActionProvider<TCustomAction>([Localizable(false)] string ns, [Localizable(false)] string name)
	: ICustomActionProvider, ICustomActionActivator where TCustomAction : CustomActionBase
{
	private readonly NameTable _nameTable = default!;

	public required Func<XmlReader, TCustomAction> CustomActionFactory { private get; [UsedImplicitly] init; }

	[UsedImplicitly]
	public required INameTableProvider? NameTableProvider
	{
		init
		{
			Infra.Requires(value);

			_nameTable = value.GetNameTable();

			ns = _nameTable.Add(ns);
			name = _nameTable.Add(name);
		}
	}

#region Interface ICustomActionActivator

	public virtual CustomActionBase Activate(string xml)
	{
		using var stringReader = new StringReader(xml);

		var nsManager = new XmlNamespaceManager(_nameTable);
		var context = new XmlParserContext(_nameTable, nsManager, xmlLang: null, xmlSpace: default);

		using var xmlReader = XmlReader.Create(stringReader, settings: null, context);

		xmlReader.MoveToContent();

		Infra.Assert(xmlReader.NamespaceURI == ns);
		Infra.Assert(xmlReader.LocalName == name);

		return CustomActionFactory(xmlReader);
	}

#endregion

#region Interface ICustomActionProvider

	public virtual ICustomActionActivator? TryGetActivator(string ns1, string name1) => ns == ns1 && name1 == name ? this : default;

#endregion
}