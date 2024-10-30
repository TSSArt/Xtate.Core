namespace Xtate.DataModel;

public abstract class ContentBodyEvaluator(IContentBody contentBody) : IContentBody, IObjectEvaluator, IStringEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => contentBody;

#endregion

#region Interface IContentBody

	public virtual string? Value => contentBody.Value;

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();

#endregion

#region Interface IStringEvaluator

	public virtual ValueTask<string> EvaluateString() => new(Value ?? string.Empty);

#endregion
}