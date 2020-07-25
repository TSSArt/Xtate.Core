namespace Xtate
{
	internal sealed class DefaultObject : IObject
	{
		public static readonly IObject Null = new DefaultObject(null);

		private readonly object? _value;

		public DefaultObject(object? value) => _value = value;

	#region Interface IObject

		public object? ToObject() => _value;

	#endregion
	}
}