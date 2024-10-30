namespace Xtate.DataModel;

public abstract class RaiseEvaluator(IRaise raise) : IRaise, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => raise;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface IRaise

	public virtual IOutgoingEvent? OutgoingEvent => raise.OutgoingEvent;

#endregion
}