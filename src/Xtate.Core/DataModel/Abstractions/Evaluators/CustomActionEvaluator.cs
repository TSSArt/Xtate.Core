namespace Xtate.DataModel;

public abstract class CustomActionEvaluator(ICustomAction customAction) : ICustomAction, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => customAction;

#endregion

#region Interface ICustomAction

	public virtual string? XmlNamespace => customAction.XmlNamespace;

	public virtual string? XmlName => customAction.XmlName;

	public virtual string? Xml => customAction.Xml;

	public virtual ImmutableArray<ILocationExpression> Locations => customAction.Locations;

	public virtual ImmutableArray<IValueExpression> Values => customAction.Values;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion
}