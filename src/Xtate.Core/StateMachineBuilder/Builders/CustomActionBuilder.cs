#region Copyright © 2019-2021 Sergii Artemenko

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

using System;
using Xtate.Core;

namespace Xtate.Builder
{
	public class CustomActionBuilder : BuilderBase, ICustomActionBuilder
	{
		private string? _name;
		private string? _ns;
		private string? _xml;

	#region Interface ICustomActionBuilder

		public ICustomAction Build() => new CustomActionEntity { Ancestor = Ancestor, XmlNamespace = _ns, XmlName = _name, Xml = _xml };

		public void SetXml(string ns, string name, string xml)
		{
			Infra.Requires(xml);
			Infra.Requires(name);
			Infra.Requires(ns);

			_ns = ns;
			_name = name;
			_xml = xml;
		}

	#endregion
	}
}