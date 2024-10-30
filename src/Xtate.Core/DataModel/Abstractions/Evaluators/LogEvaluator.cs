namespace Xtate.DataModel;

public abstract class LogEvaluator(ILog log) : ILog, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => log;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface ILog

	public virtual IValueExpression? Expression => log.Expression;

	public virtual string? Label => log.Label;

#endregion
}