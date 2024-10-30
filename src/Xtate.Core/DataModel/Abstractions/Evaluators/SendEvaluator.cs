namespace Xtate.DataModel;

public abstract class SendEvaluator(ISend send) : IExecEvaluator, ISend, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => send;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface ISend

	public virtual IContent? Content => send.Content;

	public virtual IValueExpression? DelayExpression => send.DelayExpression;

	public virtual int? DelayMs => send.DelayMs;

	public virtual string? EventName => send.EventName;

	public virtual IValueExpression? EventExpression => send.EventExpression;

	public virtual string? Id => send.Id;

	public virtual ILocationExpression? IdLocation => send.IdLocation;

	public virtual ImmutableArray<ILocationExpression> NameList => send.NameList;

	public virtual ImmutableArray<IParam> Parameters => send.Parameters;

	public virtual Uri? Target => send.Target;

	public virtual IValueExpression? TargetExpression => send.TargetExpression;

	public virtual Uri? Type => send.Type;

	public virtual IValueExpression? TypeExpression => send.TypeExpression;

#endregion
}