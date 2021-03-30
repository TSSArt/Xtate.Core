#region Copyright © 2019-2021 Sergii Artemenko

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
using Xtate.Core;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultExternalDataExpressionEvaluator : IExternalDataExpression, IResourceEvaluator, IAncestorProvider
	{
		private readonly IExternalDataExpression _externalDataExpression;

		public DefaultExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression) => _externalDataExpression = externalDataExpression;

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _externalDataExpression;

	#endregion

	#region Interface IExternalDataExpression

		public Uri? Uri => _externalDataExpression.Uri;

	#endregion

	#region Interface IResourceEvaluator

		public virtual async ValueTask<IObject> EvaluateObject(IExecutionContext executionContext, Resource resource, CancellationToken token) =>
			await ParseToDataModel(executionContext, resource, token).ConfigureAwait(false);

	#endregion

		protected virtual async ValueTask<DataModelValue> ParseToDataModel(IExecutionContext executionContext, Resource resource, CancellationToken token)
		{
			if (resource is null) throw new ArgumentNullException(nameof(resource));

			return await resource.GetContent(token).ConfigureAwait(false) is { } content
				? DataConverter.FromContent(content, resource.ContentType)
				: DataModelValue.Null;
		}
	}
}