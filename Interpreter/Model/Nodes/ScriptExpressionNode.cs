using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ScriptExpressionNode : IScriptExpression, IStoreSupport, IAncestorProvider
	{
		private readonly ScriptExpression _scriptExpression;

		public ScriptExpressionNode(in ScriptExpression scriptExpression)
		{
			Infrastructure.Assert(scriptExpression.Expression != null);

			_scriptExpression = scriptExpression;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _scriptExpression.Ancestor;

	#endregion

	#region Interface IScriptExpression

		public string Expression => _scriptExpression.Expression!;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ScriptExpressionNode);
			bucket.Add(Key.Expression, Expression);
		}

	#endregion
	}
}