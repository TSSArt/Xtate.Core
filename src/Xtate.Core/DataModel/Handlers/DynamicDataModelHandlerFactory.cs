#region Copyright © 2019-2023 Sergii Artemenko

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

using Xtate.IoC;
using IServiceProvider = Xtate.IoC.IServiceProvider;

namespace Xtate.DataModel;

public class DynamicDataModelHandlerProvider : IDataModelHandlerProvider
{
	private readonly IAssemblyContainerProvider   _assemblyContainerProvider;
	private readonly IDataModelTypeToUriConverter _dataModelTypeToUriConverter;

	public DynamicDataModelHandlerProvider(IDataModelTypeToUriConverter dataModelTypeToUriConverter, IAssemblyContainerProvider assemblyContainerProvider)
	{
		_dataModelTypeToUriConverter = dataModelTypeToUriConverter;
		_assemblyContainerProvider = assemblyContainerProvider;
	}

#region Interface IDataModelHandlerProvider

	public async ValueTask<IDataModelHandler?> TryGetDataModelHandler(string? dataModelType)
	{
		var uri = _dataModelTypeToUriConverter.GetUri(dataModelType);
		var serviceProvider = await _assemblyContainerProvider.GetContainer(uri).ConfigureAwait(false);
		var dataModelHandlerService = await serviceProvider.GetRequiredService<IDataModelHandlerService>().ConfigureAwait(false);

		return await dataModelHandlerService.GetDataModelHandler(dataModelType).ConfigureAwait(false);
	}

#endregion
}

public interface IDataModelTypeToUriConverter
{
	Uri GetUri(string dataModelType);
}

public interface IAssemblyContainerProvider
{
	ValueTask<IServiceProvider> GetContainer(Uri uri);
}

public class AssemblyContainerProvider : IAssemblyContainerProvider
{
#region Interface IAssemblyContainerProvider

	public ValueTask<IServiceProvider> GetContainer(Uri uri) => default; // TODO: implement

#endregion
}

//TODO: uncomment
/*

public class DynamicDataModelHandlerFactory : DynamicFactory
{
	private readonly string? _uriFormat;

	protected DynamicDataModelHandlerFactory(bool throwOnError) : base(throwOnError) { }

	public DynamicDataModelHandlerFactory(string format, bool throwOnError = true) : base(throwOnError) => _uriFormat = format ?? throw new ArgumentNullException(nameof(format));

#region Interface IDataModelHandlerFactory

	public async ValueTask<IDataModelHandlerFactoryActivator?> TryGetActivator(ServiceLocator serviceLocator, string dataModelType, CancellationToken token)
	{
		var factories = await GetFactories(serviceLocator, DataModelTypeToUri(dataModelType), token).ConfigureAwait(false);

		foreach (var factory in factories)
		{
			var activator = await factory.TryGetActivator(serviceLocator, dataModelType, token).ConfigureAwait(false);

			if (activator is not null)
			{
				return activator;
			}
		}

		return null;
	}

#endregion

	protected virtual Uri DataModelTypeToUri(string dataModelType)
	{
		Infra.NotNull(_uriFormat);

		var uriString = string.Format(CultureInfo.InvariantCulture, _uriFormat, dataModelType);

		return new Uri(uriString, UriKind.RelativeOrAbsolute);
	}

	public IDataModelHandler GetDataModelHandler(string dataModel)
	{
		throw new NotImplementedException();
	}
}
*/