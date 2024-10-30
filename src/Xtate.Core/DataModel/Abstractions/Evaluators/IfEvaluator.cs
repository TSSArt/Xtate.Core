namespace Xtate.DataModel;

public abstract class IfEvaluator(IIf iif) : IIf, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => iif;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IIf

	public virtual ImmutableArray<IExecutableEntity> Action => iif.Action;

	public virtual IConditionExpression? Condition => iif.Condition;

#endregion
}