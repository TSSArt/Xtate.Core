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

using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class XmlnsXmlNodeAdapter : NodeAdapter
	{
		public override XPathNodeType GetNodeType() => XPathNodeType.Namespace;

		public override string GetValue(in DataModelXPathNavigator.Node node) => @"http://www.w3.org/XML/1998/namespace";

		public override string GetLocalName(in DataModelXPathNavigator.Node node) => @"xml";
	}
}