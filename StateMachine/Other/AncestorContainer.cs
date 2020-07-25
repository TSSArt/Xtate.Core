namespace Xtate
{
	internal class AncestorContainer : IAncestorProvider
	{
		private readonly object? _ancestor;

		public AncestorContainer(object value, object? ancestor)
		{
			Value = value;
			_ancestor = ancestor;
		}

		public object Value { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _ancestor;

	#endregion
	}
}