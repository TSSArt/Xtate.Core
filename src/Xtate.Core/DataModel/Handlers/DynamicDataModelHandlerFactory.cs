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

<<<<<<< Updated upstream
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;
using Xtate.IoC;
using IServiceProvider = Xtate.IoC.IServiceProvider;
=======
using System.Globalization;
using Xtate.IoC;
>>>>>>> Stashed changes

namespace Xtate.DataModel;

public class DynamicDataModelHandlerProvider : IDataModelHandlerProvider
{
<<<<<<< Updated upstream

	public class DynamicDataModelHandlerProvider : IDataModelHandlerProvider
	{
		private readonly IDataModelTypeToUriConverter _dataModelTypeToUriConverter;
		private readonly IAssemblyContainerProvider   _assemblyContainerProvider;

		public DynamicDataModelHandlerProvider(IDataModelTypeToUriConverter dataModelTypeToUriConverter, IAssemblyContainerProvider assemblyContainerProvider)
		{
			_dataModelTypeToUriConverter = dataModelTypeToUriConverter;
			_assemblyContainerProvider = assemblyContainerProvider;
		}

		public async ValueTask<IDataModelHandler?> TryGetDataModelHandler(string? dataModelType)
		{
			var uri = _dataModelTypeToUriConverter.GetUri(dataModelType);
			var serviceProvider = await _assemblyContainerProvider.GetContainer(uri).ConfigureAwait(false);
			var  dataModelHandlerService = await serviceProvider.GetRequiredService<IDataModelHandlerService>().ConfigureAwait(false);

			return await dataModelHandlerService.GetDataModelHandler(dataModelType).ConfigureAwait(false);
		}
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


		public ValueTask<IServiceProvider> GetContainer(Uri uri)
		{
			return default; // TODO: implement
		}
	}
	//TODO: uncomment
	/*
	[PublicAPI]
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
=======
	public required Func<Uri, IAssemblyContainerProvider> AssemblyContainerProviderFactory { private get; [UsedImplicitly] init; }

	public required IDataModelTypeToUriConverter DataModelTypeToUriConverter { private get; [UsedImplicitly] init; }

#region Interface IDataModelHandlerProvider

	public async ValueTask<IDataModelHandler?> TryGetDataModelHandler(string? dataModelType)
	{
		if (dataModelType is null)
		{
			return default;
>>>>>>> Stashed changes
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
	public virtual Uri GetUri(string dataModelType)
	{
		var uriString = string.Format(CultureInfo.InvariantCulture, uriFormat, dataModelType);

		return new Uri(uriString, UriKind.RelativeOrAbsolute);
	}
}

public interface IAssemblyContainerProvider
{
	IAsyncEnumerable<IDataModelHandlerProvider> GetDataModelHandlerProviders();
}

public class AssemblyContainerProvider : IAsyncInitialization, IAssemblyContainerProvider, IDisposable
{
	private readonly Uri                                   _uri;

	public required  IServiceScopeFactory                  ServiceScopeFactory    { private get; [UsedImplicitly] init; }
	public required  Func<Uri, ValueTask<DynamicAssembly>> DynamicAssemblyFactory { private get; [UsedImplicitly] init; }

	private readonly AsyncInit<IServiceScope> _asyncInitServiceScope;

	public AssemblyContainerProvider(Uri uri)
	{
		_uri = uri;
		_asyncInitServiceScope = AsyncInit.Run(this, acp => acp.CreateServiceScope());
	}

	public Task Initialization => _asyncInitServiceScope.Task;

	private async ValueTask<IServiceScope> CreateServiceScope()
	{
		var dynamicAssembly = await DynamicAssemblyFactory(_uri).ConfigureAwait(false);

		return ServiceScopeFactory.CreateScope(dynamicAssembly.Register);
	}

	public virtual IAsyncEnumerable<IDataModelHandlerProvider> GetDataModelHandlerProviders()
	{
		return _asyncInitServiceScope.Value.ServiceProvider.GetServices<IDataModelHandlerProvider>();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_asyncInitServiceScope.Value.Dispose();
		}

		public IDataModelHandler GetDataModelHandler(string dataModel)
		{
			throw new NotImplementedException();
		}
	}
<<<<<<< Updated upstream
	*/
}
=======

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
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
>>>>>>> Stashed changes
