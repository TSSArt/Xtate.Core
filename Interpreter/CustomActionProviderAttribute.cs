using System;

namespace Xtate.CustomAction
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class CustomActionProviderAttribute : Attribute
	{
		public CustomActionProviderAttribute(string @namespace)
		{
			if (string.IsNullOrEmpty(@namespace)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(@namespace));

			Namespace = @namespace;
		}

		public string Namespace { get; }
	}
}