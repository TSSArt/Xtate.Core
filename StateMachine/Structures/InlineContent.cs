namespace Xtate
{
	public struct InlineContent : IInlineContent, IVisitorEntity<InlineContent, IInlineContent>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IInlineContent

		public string? Value { get; set; }

	#endregion

	#region Interface IVisitorEntity<InlineContent,IInlineContent>

		void IVisitorEntity<InlineContent, IInlineContent>.Init(IInlineContent source)
		{
			Ancestor = source;
			Value = source.Value;
		}

		bool IVisitorEntity<InlineContent, IInlineContent>.RefEquals(ref InlineContent other) => ReferenceEquals(Value, other.Value);

	#endregion
	}
}