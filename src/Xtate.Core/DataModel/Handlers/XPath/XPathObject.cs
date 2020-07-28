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

using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Xtate.DataModel.XPath
{
	internal class XPathObject : IObject
	{
		private readonly object _value;

		public XPathObject(object value)
		{
			_value = value switch
			{
					XPathObject xPathObject => xPathObject._value,
					XPathNodeIterator _ => value,
					string _ => value,
					int _ => value,
					bool _ => value,
					_ => Infrastructure.UnexpectedValue<object>()
			};
		}

		public XPathObjectType Type =>
				_value switch
				{
						XPathNodeIterator _ => XPathObjectType.NodeSet,
						int _ => XPathObjectType.Integer,
						string _ => XPathObjectType.String,
						bool _ => XPathObjectType.Boolean,
						_ => Infrastructure.UnexpectedValue<XPathObjectType>()
				};

	#region Interface IObject

		public object? ToObject() =>
				_value switch
				{
						XPathNodeIterator iterator => ToArray(iterator),
						_ => _value
				};

	#endregion

		public int AsInteger() => (int) _value;

		public string AsString() => (string) _value;

		public bool AsBoolean() => (bool) _value;

		public XPathNodeIterator AsIterator() => (XPathNodeIterator) _value;

		private static IObject[] ToArray(XPathNodeIterator value)
		{
			var list = new List<IObject>();

			foreach (DataModelXPathNavigator navigator in value)
			{
				list.Add(navigator.DataModelValue);
			}

			return list.ToArray();
		}

		public static string ToString(object obj) =>
				obj switch
				{
						XPathNodeIterator iterator => ToString(iterator),
						int val => XmlConvert.ToString(val),
						string val => val,
						bool val => XmlConvert.ToString(val),
						_ => Infrastructure.UnexpectedValue<string>()
				};

		private static string ToString(XPathNodeIterator iterator)
		{
			var stringBuilder = new StringBuilder();

			foreach (XPathNavigator navigator in iterator)
			{
				stringBuilder.Append(navigator.Value);
			}

			return stringBuilder.ToString();
		}
	}
}