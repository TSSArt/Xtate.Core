namespace Xtate.DataModel;

public abstract class ScriptEvaluator(IScript script) : IScript, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => script;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IScript

	public virtual IScriptExpression? Content => script.Content;

	public virtual IExternalScriptExpression? Source => script.Source;

#endregion
}