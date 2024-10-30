namespace Xtate.DataModel;

public abstract class ForEachEvaluator(IForEach forEach) : IForEach, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => forEach;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IForEach

	public virtual IValueExpression? Array => forEach.Array;

	public virtual ILocationExpression? Item => forEach.Item;

	public virtual ILocationExpression? Index => forEach.Index;

	public virtual ImmutableArray<IExecutableEntity> Action => forEach.Action;

#endregion
}