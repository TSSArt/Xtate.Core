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

using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultExternalDataExpressionEvaluator : IExternalDataExpression, IResourceEvaluator, IAncestorProvider
	{
		private readonly ExternalDataExpression _externalDataExpression;

		public DefaultExternalDataExpressionEvaluator(in ExternalDataExpression externalDataExpression) => _externalDataExpression = externalDataExpression;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _externalDataExpression.Ancestor;

	#endregion

	#region Interface IExternalDataExpression

		public Uri? Uri => _externalDataExpression.Uri;

	#endregion

	#region Interface IResourceEvaluator

		public virtual ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, Resource resource, CancellationToken token)
		{
			Exception? parsingException = null;
			var parsedValue = ParseToDataModel(resource, ref parsingException);

			if (parsingException is not null)
			{
				Infrastructure.IgnoredException(parsingException);
			}

			return new ValueTask<IObject>(parsedValue);
		}

	#endregion

		protected virtual DataModelValue ParseToDataModel(Resource resource, ref Exception? parseException)
		{
			if (resource is null) throw new ArgumentNullException(nameof(resource));

			return resource.Content is { } content ? DataConverter.FromContent(content, resource.ContentType) : DataModelValue.Null;
		}
	}
}