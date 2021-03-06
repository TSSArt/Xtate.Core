﻿#region Copyright © 2019-2021 Sergii Artemenko

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

		public CustomActionBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface ICustomActionBuilder

		public ICustomAction Build() => new CustomActionEntity { Ancestor = Ancestor, XmlNamespace = _ns, XmlName = _name, Xml = _xml };

		public void SetXml(string ns, string name, string xml)
		{
			_ns = ns ?? throw new ArgumentNullException(nameof(xml));
			_name = name ?? throw new ArgumentNullException(nameof(xml));
			_xml = xml ?? throw new ArgumentNullException(nameof(xml));
		}

	#endregion
	}
}