// Copyright © 2019-2023 Sergii Artemenko
// 
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

public class DefaultExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression) : ExternalDataExpressionEvaluator(externalDataExpression)
{
	public required Func<ValueTask<DataConverter>>          DataConverterFactory        { private get; [UsedImplicitly] init; }
	public required Func<ValueTask<IStateMachineLocation?>> StateMachineLocationFactory { private get; [UsedImplicitly] init; }
	public required Func<ValueTask<IResourceLoader>>        ResourceLoaderFactory       { private get; [UsedImplicitly] init; }

	public override async ValueTask<IObject> EvaluateObject()
	{
		var uri = await GetUri().ConfigureAwait(false);

		var resourceLoader = await ResourceLoaderFactory().ConfigureAwait(false);
		var resource = await resourceLoader.Request(uri).ConfigureAwait(false);

		await using (resource.ConfigureAwait(false))
		{
			return await ParseToDataModel(resource).ConfigureAwait(false);
		}
	}

	protected virtual async ValueTask<Uri> GetUri()
	{
		var relativeUri = base.Uri;
		Infra.NotNull(relativeUri);

		var stateMachineLocation = await StateMachineLocationFactory().ConfigureAwait(false);
		var baseUri = stateMachineLocation?.Location;

		return baseUri.CombineWith(relativeUri);
	}

	protected virtual async ValueTask<DataModelValue> ParseToDataModel(Resource resource)
	{
		var dataConverter = await DataConverterFactory().ConfigureAwait(false);

		return await dataConverter.FromContent(resource).ConfigureAwait(false);
	}
}