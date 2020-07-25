using System;

namespace Xtate.DataModel.XPath
{
	internal static class AdapterFactory
	{
		public static readonly NodeAdapter SimpleTypeNodeAdapter = new SimpleTypeNodeAdapter();
		public static readonly NodeAdapter XmlnsXmlNodeAdapter   = new XmlnsXmlNodeAdapter();
		public static readonly NodeAdapter AttributeNodeAdapter  = new AttributeNodeAdapter();
		public static readonly NodeAdapter NamespaceNodeAdapter  = new NamespaceNodeAdapter();

		private static readonly NodeAdapter ListNodeAdapter           = new ListNodeAdapter();
		private static readonly NodeAdapter ListItemNodeAdapter       = new ListItemNodeAdapter();
		private static readonly NodeAdapter ItemNodeAdapter           = new ItemNodeAdapter();
		private static readonly NodeAdapter SimpleTypeItemNodeAdapter = new SimpleTypeItemNodeAdapter();

		public static NodeAdapter GetDefaultAdapter(in DataModelValue value) =>
				value.Type switch
				{
						DataModelValueType.Undefined => SimpleTypeNodeAdapter,
						DataModelValueType.Null => SimpleTypeNodeAdapter,
						DataModelValueType.String => SimpleTypeNodeAdapter,
						DataModelValueType.Number => SimpleTypeNodeAdapter,
						DataModelValueType.Boolean => SimpleTypeNodeAdapter,
						DataModelValueType.DateTime => SimpleTypeNodeAdapter,
						DataModelValueType.Object => ListNodeAdapter,
						DataModelValueType.Array => ListNodeAdapter,
						_ => throw GetNotSupportedException()
				};

		public static NodeAdapter GetItemAdapter(in DataModelValue value) =>
				value.Type switch
				{
						DataModelValueType.Undefined => ItemNodeAdapter,
						DataModelValueType.Null => ItemNodeAdapter,
						DataModelValueType.String => SimpleTypeItemNodeAdapter,
						DataModelValueType.Number => SimpleTypeItemNodeAdapter,
						DataModelValueType.Boolean => SimpleTypeItemNodeAdapter,
						DataModelValueType.DateTime => SimpleTypeItemNodeAdapter,
						DataModelValueType.Object => ListItemNodeAdapter,
						DataModelValueType.Array => ListItemNodeAdapter,
						_ => throw GetNotSupportedException()
				};

		private static NotSupportedException GetNotSupportedException() => new NotSupportedException();
	}
}