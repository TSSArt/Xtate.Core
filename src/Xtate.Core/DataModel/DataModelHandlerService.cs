// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.DataModel;

namespace Xtate.Core;

public class DataModelHandlerService : IDataModelHandlerService
{
	public required IAsyncEnumerable<IDataModelHandlerProvider> DataModelHandlerProviders { private get; [UsedImplicitly] init; }

	public required IErrorProcessorService<DataModelHandlerService> ErrorProcessorService { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<UnknownDataModelHandler>> UnknownDataModelHandlerFactory { private get; [UsedImplicitly] init; }

#region Interface IDataModelHandlerService

	public virtual async ValueTask<IDataModelHandler> GetDataModelHandler(string? dataModelType)
	{
		await foreach (var dataModelHandlerProvider in DataModelHandlerProviders.ConfigureAwait(false))
		{
			if (await dataModelHandlerProvider.TryGetDataModelHandler(dataModelType).ConfigureAwait(false) is { } dataModelHandler)
			{
				return dataModelHandler;
			}
		}

		ErrorProcessorService.AddError(entity: null, Res.Format(Resources.ErrorMessage_CantFindDataModelHandlerFactoryForDataModelType, dataModelType));

		return await UnknownDataModelHandlerFactory().ConfigureAwait(false);
	}

#endregion
}