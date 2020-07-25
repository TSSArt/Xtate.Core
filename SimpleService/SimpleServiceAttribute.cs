using System;

namespace Xtate.Service
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class SimpleServiceAttribute : Attribute
	{
		public SimpleServiceAttribute(string type)
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_Value_cannot_be_null_or_empty_, nameof(type));

			Type = type;
		}

		public string Type { get; }

		public string? Alias { get; set; }
	}
}