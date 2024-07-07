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

using System.Globalization;
using Xtate.IoC;

namespace Xtate.DataModel;

public class DynamicDataModelHandlerProvider : IDataModelHandlerProvider
{
	public required Func<Uri, IAssemblyContainerProvider> AssemblyContainerProviderFactory { private get; [UsedImplicitly] init; }

	public required IDataModelTypeToUriConverter DataModelTypeToUriConverter { private get; [UsedImplicitly] init; }

#region Interface IDataModelHandlerProvider

	public async ValueTask<IDataModelHandler?> TryGetDataModelHandler(string? dataModelType)
	{
		if (dataModelType is null)
		{
			return default;
		}

		var uri = DataModelTypeToUriConverter.GetUri(dataModelType);

		var providers = AssemblyContainerProviderFactory(uri).GetDataModelHandlerProviders();

		await foreach (var dataModelHandlerProvider in providers.ConfigureAwait(false))
		{
			if (await dataModelHandlerProvider.TryGetDataModelHandler(dataModelType).ConfigureAwait(false) is { } dataModelHandler)
			{
				return dataModelHandler;
			}
		}

		return default;
	}

#endregion
}

public interface IDataModelTypeToUriConverter
{
	Uri GetUri(string dataModelType);
}

public class DataModelTypeToUriConverter(string uriFormat) : IDataModelTypeToUriConverter
{
#region Interface IDataModelTypeToUriConverter

	public virtual Uri GetUri(string dataModelType)
	{
		var uriString = string.Format(CultureInfo.InvariantCulture, uriFormat, dataModelType);

		return new Uri(uriString, UriKind.RelativeOrAbsolute);
	}

#endregion
}

public interface IAssemblyContainerProvider
{
	IAsyncEnumerable<IDataModelHandlerProvider> GetDataModelHandlerProviders();
}

public class AssemblyContainerProvider : IAsyncInitialization, IAssemblyContainerProvider, IDisposable
{
	private readonly AsyncInit<IServiceScope> _asyncInitServiceScope;
	private readonly Uri                      _uri;

	public AssemblyContainerProvider(Uri uri)
	{
		_uri = uri;
		_asyncInitServiceScope = AsyncInit.Run(this, acp => acp.CreateServiceScope());
	}

	public required IServiceScopeFactory                  ServiceScopeFactory    { private get; [UsedImplicitly] init; }
	public required Func<Uri, ValueTask<DynamicAssembly>> DynamicAssemblyFactory { private get; [UsedImplicitly] init; }

#region Interface IAssemblyContainerProvider

	public virtual IAsyncEnumerable<IDataModelHandlerProvider> GetDataModelHandlerProviders() => _asyncInitServiceScope.Value.ServiceProvider.GetServices<IDataModelHandlerProvider>();

#endregion

#region Interface IAsyncInitialization

	public Task Initialization => _asyncInitServiceScope.Task;

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

	private async ValueTask<IServiceScope> CreateServiceScope()
	{
		var dynamicAssembly = await DynamicAssemblyFactory(_uri).ConfigureAwait(false);

		return ServiceScopeFactory.CreateScope(dynamicAssembly.Register);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_asyncInitServiceScope.Value.Dispose();
		}
	}
}

//TODO:Delete
/*
public class DynamicDataModelHandlerFactory : DynamicFactory
{
	private readonly string? _uriFormat;

	protected DynamicDataModelHandlerFactory(bool throwOnError) : base(throwOnError) { }

	public DynamicDataModelHandlerFactory(string format, bool throwOnError = true) : base(throwOnError) => _uriFormat = format ?? throw new ArgumentNullException(nameof(format));

#region Interface IDataModelHandlerFactory

	public async ValueTask<IDataModelHandlerFactoryActivator?> TryGetActivator(string dataModelType, CancellationToken token)
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