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

namespace Xtate.DataModel.XPath;

internal static class XPathMetadata
{
	public const int ElementIndex             = 0;
	public const int ElementPrefixOffset      = 0;
	public const int ElementNamespaceOffset   = 1;
	public const int FirstAttributeOffset     = 2;
	public const int AttributeSegmentLength   = 4;
	public const int AttributeLocalNameOffset = 0;
	public const int AttributeValueOffset     = 1;
	public const int AttributePrefixOffset    = 2;
	public const int AttributeNamespaceOffset = 3;

	public const string Xmlns          = "xmlns";
	public const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

	public static string GetValue(DataModelList? metadata, int index, int offset)
	{
		if (metadata is null)
		{
			return string.Empty;
		}

		if (metadata.TryGet(index + offset, out var entry))
		{
			return entry.Value.AsStringOrDefault() ?? string.Empty;
		}

		return string.Empty;
	}
}