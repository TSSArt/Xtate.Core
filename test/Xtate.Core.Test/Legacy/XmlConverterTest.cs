#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Legacy
{
	[TestClass]
	public class XmlConverterTest
	{
		[TestMethod]
		public void ToXmlTest()
		{
			var writer = new StringWriter();
			var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
													 {
															 Indent = true
													 });

			xmlWriter.WriteStartElement("v");

			//	xmlWriter.WriteAttributeString(prefix: @"xmlns", localName: @"xs", ns: null, "http://www.w3.org/2001/XMLSchema");
			//	xmlWriter.WriteAttributeString(prefix: @"xmlns", localName: @"xsi", ns: null, "http://www.w3.org/2001/XMLSchema-instance");
			xmlWriter.WriteStartElement("inv");

			//XmlConverter.WriteValue(xmlWriter, new DataModelValue(5.5));
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.Flush();

			//xmlWriter.

			var _ = writer.ToString();
		}
	}
}