namespace Xtate.DataModel;

public abstract class AssignEvaluator(IAssign assign) : IAssign, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => assign;

#endregion

#region Interface IAssign

	public virtual ILocationExpression? Location => assign.Location;

	public virtual IValueExpression? Expression => assign.Expression;

	public virtual IInlineContent? InlineContent => assign.InlineContent;

	public virtual string? Type => assign.Type;

	public virtual string? Attribute => assign.Attribute;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion
}