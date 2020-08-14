#region Copyright © 2019-2020 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Xtate.Persistence;

namespace Xtate
{
	internal sealed class LocationExpressionNode : ILocationExpression, IStoreSupport, IAncestorProvider
	{
		private readonly LocationExpression _locationExpression;

		public LocationExpressionNode(in LocationExpression locationExpression)
		{
			Infrastructure.NotNull(locationExpression.Expression);

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