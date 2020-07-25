namespace Xtate.DataModel.XPath
{
	internal static class XPathMetadata
	{
		public const int ElementNamespaceOffset   = 0;
		public const int ElementPrefixOffset      = 1;
		public const int AttributeNamespaceOffset = 0;
		public const int AttributePrefixOffset    = 1;

		public static string GetValue(DataModelList? metadata, int offset, int cursor = -1, string? key = null)
		{
			if (metadata == null)
			{
				return string.Empty;
			}

			while (metadata.NextEntry(ref cursor, out var entry))
			{
				if (entry.Key != key)
				{
					break;
				}

				if (offset -- == 0)
				{
					return entry.Value.AsStringOrDefault() ?? string.Empty;
				}
			}

			return string.Empty;
		}
	}
}