namespace Xtate.DataModel;

public abstract class ExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression) : IExternalDataExpression, IObjectEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => externalDataExpression;

#endregion

#region Interface IExternalDataExpression

	public virtual Uri? Uri => externalDataExpression.Uri;

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();

#endregion
}