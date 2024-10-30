namespace Xtate.DataModel;

public abstract class InlineContentEvaluator(IInlineContent inlineContent) : IInlineContent, IObjectEvaluator, IStringEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => inlineContent;

#endregion

#region Interface IInlineContent

	public virtual string? Value => inlineContent.Value;

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();

#endregion

#region Interface IStringEvaluator

	public virtual ValueTask<string> EvaluateString() => new(Value ?? string.Empty);

#endregion
}