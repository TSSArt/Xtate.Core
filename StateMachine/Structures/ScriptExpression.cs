namespace Xtate
{
	public struct ScriptExpression : IScriptExpression, IVisitorEntity<ScriptExpression, IScriptExpression>, IAncestorProvider
	{
		internal object? Ancestor;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => Ancestor;

	#endregion

	#region Interface IScriptExpression

		public string? Expression { get; set; }

	#endregion

	#region Interface IVisitorEntity<ScriptExpression,IScriptExpression>

		void IVisitorEntity<ScriptExpression, IScriptExpression>.Init(IScriptExpression source)
		{
			Ancestor = source;
			Expression = source.Expression;
		}

		bool IVisitorEntity<ScriptExpression, IScriptExpression>.RefEquals(ref ScriptExpression other) => ReferenceEquals(Expression, other.Expression);

	#endregion
	}
}