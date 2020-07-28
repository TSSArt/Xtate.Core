#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Xml;
using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class SimpleTypeNodeAdapter : NodeAdapter
	{
		public override XPathNodeType GetNodeType(in DataModelXPathNavigator.Node node) => XPathNodeType.Text;

		public override string GetValue(in DataModelXPathNavigator.Node node) =>
				node.DataModelValue.Type switch
				{
						DataModelValueType.Undefined => string.Empty,
						DataModelValueType.Null => string.Empty,
						DataModelValueType.String => node.DataModelValue.AsString(),
						DataModelValueType.Number => XmlConvert.ToString(node.DataModelValue.AsNumber()),
						DataModelValueType.Boolean => node.DataModelValue.AsBoolean() ? "true" : "false",
						DataModelValueType.DateTime => DateTimeToXmlString(node.DataModelValue.AsDateTime()),
						_ => Infrastructure.UnexpectedValue<string>()
				};

		private static string DateTimeToXmlString(in DataModelDateTime dttm) =>
				dttm.Type switch
				{
						DataModelDateTimeType.DateTime => XmlConvert.ToString(dttm.ToDateTime(), XmlDateTimeSerializationMode.RoundtripKind),
						DataModelDateTimeType.DateTimeOffset => XmlConvert.ToString(dttm.ToDateTimeOffset()),
						_ => Infrastructure.UnexpectedValue<string>()
				};

		public override string GetLocalName(in DataModelXPathNavigator.Node node) => "#text";

		public override int GetBufferSizeForValue(in DataModelXPathNavigator.Node node) =>
				node.DataModelValue.Type switch
				{
						DataModelValueType.Undefined => 0,
						DataModelValueType.Null => 0,
						DataModelValueType.String => node.DataModelValue.AsString().Length,
						DataModelValueType.Number => 24, // -1.2345678901234567e+123 (G17)
						DataModelValueType.DateTime => 33, // YYYY-MM-DDThh:mm:ss.1234567+hh:mm (DateTime with Offset)
						DataModelValueType.Boolean => 5, // 'false' - longest value
						_ => Infrastructure.UnexpectedValue<int>()
				};

		public override int WriteValueToSpan(in DataModelXPathNavigator.Node node, in Span<char> span)
		{
			return node.DataModelValue.Type switch
			{
					DataModelValueType.Undefined => 0,
					DataModelValueType.Null => 0,
					DataModelValueType.String => WriteString(node.DataModelValue.AsString(), span),
					DataModelValueType.Number => WriteString(XmlConvert.ToString(node.DataModelValue.AsNumber()), span),
					DataModelValueType.DateTime => WriteDataModelDateTime(node.DataModelValue.AsDateTime(), span),
					DataModelValueType.Boolean => WriteString(node.DataModelValue.AsBoolean() ? "true" : "false", span),
					_ => Infrastructure.UnexpectedValue<int>()
			};

			static int WriteDataModelDateTime(in DataModelDateTime val, in Span<char> span) =>
					val.Type switch
					{
							DataModelDateTimeType.DateTime => WriteString(XmlConvert.ToString(val.ToDateTime(), XmlDateTimeSerializationMode.RoundtripKind), span),
							DataModelDateTimeType.DateTimeOffset => WriteString(XmlConvert.ToString(val.ToDateTimeOffset()), span),
							_ => Infrastructure.UnexpectedValue<int>()
					};

			static int WriteString(string val, in Span<char> span)
			{
				val.AsSpan().CopyTo(span);
				return val.Length;
			}
		}
	}
}