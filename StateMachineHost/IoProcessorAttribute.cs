using System;

namespace Xtate.IoProcessor
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class IoProcessorAttribute : Attribute
	{
		public IoProcessorAttribute(string type)
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));

			Type = type;
		}

		public string Type { get; }

		public string? Alias { get; set; }
	}
}