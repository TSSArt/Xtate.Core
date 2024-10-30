namespace Xtate.DataModel;

public abstract class CancelEvaluator(ICancel cancel) : ICancel, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => cancel;

#endregion

#region Interface ICancel

	public virtual string? SendId => cancel.SendId;

	public virtual IValueExpression? SendIdExpression => cancel.SendIdExpression;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion
}