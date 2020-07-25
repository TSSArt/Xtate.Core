using System;
using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ExternalDataExpressionNode : IExternalDataExpression, IStoreSupport, IAncestorProvider
	{
		private readonly ExternalDataExpression _externalDataExpression;

		public ExternalDataExpressionNode(in ExternalDataExpression externalDataExpression)
		{
			Infrastructure.Assert(externalDataExpression.Uri != null);

			_externalDataExpression = externalDataExpression;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _externalDataExpression.Ancestor;

	#endregion

	#region Interface IExternalDataExpression

		public Uri Uri => _externalDataExpression.Uri!;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ExternalDataExpressionNode);
			bucket.Add(Key.Uri, Uri);
		}

	#endregion
	}
}