<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2023 Sergii Artemenko
// 
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
#endregion

using System;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class ExternalDataExpressionEvaluator : IExternalDataExpression, IResourceEvaluator, IAncestorProvider
{
	private readonly IExternalDataExpression _externalDataExpression;

	protected ExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression)
	{
		Infra.Requires(externalDataExpression);

		_externalDataExpression = externalDataExpression;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _externalDataExpression;
=======
namespace Xtate.DataModel;

public abstract class ExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression) : IExternalDataExpression, IObjectEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => externalDataExpression;
>>>>>>> Stashed changes

#endregion

#region Interface IExternalDataExpression

<<<<<<< Updated upstream
	public virtual Uri? Uri => _externalDataExpression.Uri;

#endregion

#region Interface IResourceEvaluator

	public abstract ValueTask<IObject> EvaluateObject(Resource resource);
=======
	public virtual Uri? Uri => externalDataExpression.Uri;

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();
>>>>>>> Stashed changes

#endregion
}

<<<<<<< Updated upstream
public class DefaultExternalDataExpressionEvaluator : ExternalDataExpressionEvaluator
{
	public DefaultExternalDataExpressionEvaluator(IExternalDataExpression externalDataExpression) : base(externalDataExpression) { }

	public required Func<ValueTask<DataConverter>> DataConverterFactory { private get; init; }

	public override async ValueTask<IObject> EvaluateObject(Resource resource) => await ParseToDataModel(resource).ConfigureAwait(false);

	protected virtual async ValueTask<DataModelValue> ParseToDataModel(Resource resource)
	{
		Infra.Requires(resource);

=======
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
>>>>>>> Stashed changes
		var dataConverter = await DataConverterFactory().ConfigureAwait(false);

		return await dataConverter.FromContent(resource).ConfigureAwait(false);
	}
}