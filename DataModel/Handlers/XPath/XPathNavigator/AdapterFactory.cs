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

using System;

namespace Xtate.DataModel.XPath
{
	internal static class AdapterFactory
	{
		public static readonly NodeAdapter XmlnsXmlNodeAdapter      = new XmlnsXmlNodeAdapter();
		public static readonly NodeAdapter AttributeNodeAdapter     = new AttributeNodeAdapter();
		public static readonly NodeAdapter KeyAttributeNodeAdapter  = new KeyAttributeNodeAdapter();
		public static readonly NodeAdapter TypeAttributeNodeAdapter = new TypeAttributeNodeAdapter();
		public static readonly NodeAdapter NamespaceNodeAdapter     = new NamespaceNodeAdapter();

		private static readonly NodeAdapter ListNodeAdapter           = new ListNodeAdapter();
		private static readonly NodeAdapter ListItemNodeAdapter       = new ListItemNodeAdapter();
		private static readonly NodeAdapter ItemNodeAdapter           = new ItemNodeAdapter();
		private static readonly NodeAdapter SimpleTypeItemNodeAdapter = new SimpleTypeItemNodeAdapter();
		private static readonly NodeAdapter SimpleTypeNodeAdapter     = new SimpleTypeNodeAdapter();

		public static NodeAdapter GetDefaultAdapter(in DataModelValue value) =>
				value.Type switch
				{
						DataModelValueType.Undefined => SimpleTypeNodeAdapter,
						DataModelValueType.Null => SimpleTypeNodeAdapter,
						DataModelValueType.String => SimpleTypeNodeAdapter,
						DataModelValueType.Number => SimpleTypeNodeAdapter,
						DataModelValueType.Boolean => SimpleTypeNodeAdapter,
						DataModelValueType.DateTime => SimpleTypeNodeAdapter,
						DataModelValueType.List => ListNodeAdapter,
						_ => throw GetNotSupportedException()
				};

		public static NodeAdapter GetItemAdapter(in DataModelList.Entry entry)
		{
			return entry.Value.Type switch
			{
					DataModelValueType.Undefined => ItemNodeAdapter,
					DataModelValueType.Null => ItemNodeAdapter,
					DataModelValueType.String => SimpleTypeItemNodeAdapter,
					DataModelValueType.Number => SimpleTypeItemNodeAdapter,
					DataModelValueType.Boolean => SimpleTypeItemNodeAdapter,
					DataModelValueType.DateTime => SimpleTypeItemNodeAdapter,
					DataModelValueType.List => ListItemNodeAdapter,
					_ => throw GetNotSupportedException()
			};
		}

		public static NodeAdapter? GetSimpleTypeAdapter(in DataModelValue value) =>
				value.Type switch
				{
						DataModelValueType.Undefined => null,
						DataModelValueType.Null => null,
						DataModelValueType.String when value.AsString().Length == 0 => null,
						DataModelValueType.String => SimpleTypeNodeAdapter,
						DataModelValueType.Number => SimpleTypeNodeAdapter,
						DataModelValueType.Boolean => SimpleTypeNodeAdapter,
						DataModelValueType.DateTime => SimpleTypeNodeAdapter,
						_ => throw GetNotSupportedException()
				};

		private static NotSupportedException GetNotSupportedException() => new NotSupportedException();
	}
}