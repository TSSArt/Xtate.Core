namespace Xtate
{
	public struct ScriptEntity : IScript, IVisitorEntity<ScriptEntity, IScript>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IScript

		public IScriptExpression?         Content { get; set; }
		public IExternalScriptExpression? Source  { get; set; }

	#endregion

	#region Interface IVisitorEntity<ScriptEntity,IScript>

		void IVisitorEntity<ScriptEntity, IScript>.Init(IScript source)
		{
			Ancestor = source;
			Content = source.Content;
			Source = source.Source;
		}

		bool IVisitorEntity<ScriptEntity, IScript>.RefEquals(ref ScriptEntity other) =>
				ReferenceEquals(Content, other.Content) &&
				ReferenceEquals(Source, other.Source);

	#endregion
	}
}