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

namespace Xtate.Service;

public abstract class ServiceFactoryBase : IServiceFactory
{
	private Activator? _activator;

#region Interface IServiceFactory

	public ValueTask<IServiceFactoryActivator?> TryGetActivator(Uri type)
	{
		_activator ??= CreateActivator();

		return new ValueTask<IServiceFactoryActivator?>(_activator.CanHandle(type) ? _activator : null);
	}

<<<<<<< Updated upstream
		public ValueTask<IServiceFactoryActivator?> TryGetActivator(ServiceLocator serviceLocator, Uri type, CancellationToken token)
=======
#endregion

	private Activator CreateActivator()
	{
		var catalog = new Catalog();

		Register(catalog);

		return new Activator(catalog);
	}

	protected abstract void Register(IServiceCatalog catalog);

	private class Catalog : IServiceCatalog
	{
		private readonly Dictionary<Uri, Delegate> _creators = new(FullUriComparer.Instance);

#region Interface IServiceCatalog

		public void Register(string type, IServiceCatalog.Creator creator)
>>>>>>> Stashed changes
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
			if (creator is null) throw new ArgumentNullException(nameof(creator));

			_creators.Add(new Uri(type, UriKind.RelativeOrAbsolute), creator);
		}

		public void Register(string type, IServiceCatalog.ServiceCreator creator)
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
			if (creator is null) throw new ArgumentNullException(nameof(creator));

			_creators.Add(new Uri(type, UriKind.RelativeOrAbsolute), creator);
		}

		public void Register(string type, IServiceCatalog.ServiceCreatorAsync creator)
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
			if (creator is null) throw new ArgumentNullException(nameof(creator));

<<<<<<< Updated upstream
		#region Interface IServiceCatalog

			public void Register(string type, IServiceCatalog.Creator creator)
			{
				if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
				if (creator is null) throw new ArgumentNullException(nameof(creator));

				_creators.Add(new Uri(type, UriKind.RelativeOrAbsolute), creator);
			}

			public void Register(string type, IServiceCatalog.ServiceCreator creator)
			{
				if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
				if (creator is null) throw new ArgumentNullException(nameof(creator));

				_creators.Add(new Uri(type, UriKind.RelativeOrAbsolute), creator);
			}

			public void Register(string type, IServiceCatalog.ServiceCreatorAsync creator)
			{
				if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
				if (creator is null) throw new ArgumentNullException(nameof(creator));

				_creators.Add(new Uri(type, UriKind.RelativeOrAbsolute), creator);
			}

		#endregion

			public bool CanHandle(Uri type) => _creators.ContainsKey(type);

			public ValueTask<IService> CreateService(ServiceLocator serviceLocator,
													 Uri? baseUri,
													 InvokeData invokeData,
													 IServiceCommunication serviceCommunication,
													 CancellationToken token)
			{
				switch (_creators[invokeData.Type])
				{
					case IServiceCatalog.Creator creator:
						var service = creator();

						service.Start(baseUri, invokeData, serviceCommunication);

						return new ValueTask<IService>(service);

					case IServiceCatalog.ServiceCreator creator:
						return new ValueTask<IService>(creator(baseUri, invokeData, serviceCommunication));

					case IServiceCatalog.ServiceCreatorAsync creator:
						return creator(serviceLocator, baseUri, invokeData, serviceCommunication, token);

					default:
						return Infra.Unexpected<ValueTask<IService>>(_creators[invokeData.Type].GetType());
				}
			}
=======
			_creators.Add(new Uri(type, UriKind.RelativeOrAbsolute), creator);
>>>>>>> Stashed changes
		}

#endregion

		public bool CanHandle(Uri type) => _creators.ContainsKey(type);

		public ValueTask<IService> CreateService(Uri? baseUri,
												 InvokeData invokeData,
												 IServiceCommunication serviceCommunication)
		{
			switch (_creators[invokeData.Type])
			{
				case IServiceCatalog.Creator creator:
					var service = creator();

					service.Start(baseUri, invokeData, serviceCommunication);

					return new ValueTask<IService>(service);

				case IServiceCatalog.ServiceCreator creator:
					return new ValueTask<IService>(creator(baseUri, invokeData, serviceCommunication));

				case IServiceCatalog.ServiceCreatorAsync creator:
					return creator(baseUri, invokeData, serviceCommunication);

				default:
					return Infra.Unexpected<ValueTask<IService>>(_creators[invokeData.Type].GetType());
			}
		}
	}

	private class Activator(Catalog catalog) : IServiceFactoryActivator
	{

		#region Interface IServiceFactoryActivator

<<<<<<< Updated upstream
			public ValueTask<IService> StartService(ServiceLocator serviceLocator,
													Uri? baseUri,
													InvokeData invokeData,
													IServiceCommunication serviceCommunication,
													CancellationToken token)
			{
				if (invokeData is null) throw new ArgumentNullException(nameof(invokeData));
=======
		public ValueTask<IService> StartService(Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication)
		{
			if (invokeData is null) throw new ArgumentNullException(nameof(invokeData));
>>>>>>> Stashed changes

			Infra.Assert(CanHandle(invokeData.Type));

<<<<<<< Updated upstream
				return _catalog.CreateService(serviceLocator, baseUri, invokeData, serviceCommunication, token);
			}

		#endregion

			public bool CanHandle(Uri type) => _catalog.CanHandle(type);
=======
			return catalog.CreateService(baseUri, invokeData, serviceCommunication);
>>>>>>> Stashed changes
		}

#endregion

		public bool CanHandle(Uri type) => catalog.CanHandle(type);
	}
}