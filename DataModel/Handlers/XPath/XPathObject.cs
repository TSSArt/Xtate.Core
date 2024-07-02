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

using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Xtate.DataModel.XPath;

public class XPathObject(object value) : IObject
{
	private readonly object _value = value switch
	{
		XPathNodeIterator => value,
		string => value,
		double => value,
		bool => value,
		_ => Infra.Unexpected<object>(value)
	};

	public XPathObjectType Type =>
		_value switch
		{
			XPathNodeIterator => XPathObjectType.NodeSet,
			double            => XPathObjectType.Number,
			string            => XPathObjectType.String,
			bool              => XPathObjectType.Boolean,
			_                 => Infra.Unexpected<XPathObjectType>(_value)
		};

#region Interface IObject

	public object? ToObject() =>
		_value switch
		{
			XPathNodeIterator iterator => ToObject(iterator),
			_                          => _value
		};

#endregion

	private static string GetFirstStringValue(XPathNodeIterator iterator)
	{
		iterator = iterator.Clone();

		if (iterator.MoveNext() && iterator.Current is { } first)
		{
			return first.Value;
		}

		return string.Empty;
	}

	public int AsInteger() =>
		_value switch
		{
			XPathNodeIterator iterator => XmlConvert.ToInt32(GetFirstStringValue(iterator)),
			string value               => XmlConvert.ToInt32(value),
			double value               => (int) value,
			bool value                 => value ? 1 : 0,
			_                          => Infra.Unexpected<int>(_value)
		};

	public string AsString() =>
		_value switch
		{
			XPathNodeIterator iterator => GetFirstStringValue(iterator),
			string value               => value,
			double value               => XmlConvert.ToString(value),
			bool value                 => XmlConvert.ToString(value),
			_                          => Infra.Unexpected<string>(_value)
		};

	public bool AsBoolean() =>
		_value switch
		{
			XPathNodeIterator iterator => XmlConvert.ToBoolean(GetFirstStringValue(iterator)),
			string value               => XmlConvert.ToBoolean(value),
			double value               => value != 0,
			bool value                 => value,
			_                          => Infra.Unexpected<bool>(_value)
		};

	public XPathNodeIterator AsIterator() => ((XPathNodeIterator) _value).Clone();

	private static object? ToObject(XPathNodeIterator iterator)
	{
		var length = 0;
		var count = 0;
		string? result = default;

		foreach (DataModelXPathNavigator navigator in iterator)
		{
			switch (navigator.NodeType)
			{
				case XPathNodeType.Element:
					return ToDataModelObject(iterator);

				case XPathNodeType.Text:
					count ++;
					if (navigator.DataModelValue.AsStringOrDefault() is { } str)
					{
						length += str.Length;
						result = str;
					}

					break;

				default:
					return Infra.Unexpected<DataModelList>(navigator.NodeType);
			}
		}

		if (count == 0)
		{
			return null;
		}

		if (length == 0)
		{
			return string.Empty;
		}

		Infra.NotNull(result);

		if (result.Length == length)
		{
			return result;
		}

		var sb = new StringBuilder(length);

		foreach (DataModelXPathNavigator navigator in iterator)
		{
			if (navigator.DataModelValue.AsStringOrDefault() is { } str)
			{
				sb.Append(str);
			}
		}

		return sb.ToString();
	}

	private static DataModelList ToDataModelObject(XPathNodeIterator iterator)
	{
		var list = new DataModelList();

		foreach (DataModelXPathNavigator navigator in iterator)
		{
			switch (navigator.NodeType)
			{
				case XPathNodeType.Element:
					var key = XmlConverter.NsNameToKey(navigator.NamespaceURI, navigator.LocalName);
					list.Add(key, navigator.DataModelValue.CloneAsWritable(), navigator.Metadata?.DeepClone(DataModelAccess.Writable));
					break;

				case XPathNodeType.Text:
					list.Add(key: default, navigator.DataModelValue.CloneAsWritable(), metadata: default);
					break;

				default:
					return Infra.Unexpected<DataModelList>(navigator.NodeType);
			}
		}

		return list;
	}
}