using Xtate.Persistence;

namespace Xtate
{
	internal sealed class LocationExpressionNode : ILocationExpression, IStoreSupport, IAncestorProvider
	{
		private readonly LocationExpression _locationExpression;

		public LocationExpressionNode(in LocationExpression locationExpression)
		{
			Infrastructure.Assert(locationExpression.Expression != null);

			_locationExpression = locationExpression;
		}

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _locationExpression.Ancestor;

	#endregion

	#region Interface ILocationExpression

		public string Expression => _locationExpression.Expression!;

	#endregion

	#region Interface IStoreSupport

		void IStoreSupport.Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.LocationExpressionNode);
			bucket.Add(Key.Expression, Expression);
		}

	#endregion
	}
}