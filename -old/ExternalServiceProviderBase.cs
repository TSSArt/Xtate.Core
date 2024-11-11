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

namespace Xtate.ExternalService;

[Obsolete("Use ExternalServiceProviderBase<TService>")]
public abstract class ExternalServiceProviderBase : IExternalServiceProvider
{
	private Activator? _activator;

#region Interface IExternalServiceProvider

	public IExternalServiceActivator? TryGetActivator(FullUri type)
	{
		_activator ??= CreateActivator();

		return _activator.CanHandle(type) ? _activator : null;
	}

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
		private readonly Dictionary<FullUri, Delegate> _creators = new();

	#region Interface IServiceCatalog

		public void Register(string type, IServiceCatalog.Creator creator)
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
			if (creator is null) throw new ArgumentNullException(nameof(creator));

			_creators.Add(new FullUri(type), creator);
		}

		public void Register(string type, IServiceCatalog.ServiceCreator creator)
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
			if (creator is null) throw new ArgumentNullException(nameof(creator));

			_creators.Add(new FullUri(type), creator);
		}

		public void Register(string type, IServiceCatalog.ServiceCreatorAsync creator)
		{
			if (string.IsNullOrEmpty(type)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(type));
			if (creator is null) throw new ArgumentNullException(nameof(creator));

			_creators.Add(new FullUri(type), creator);
		}

	#endregion

		public bool CanHandle(FullUri type) => _creators.ContainsKey(type);

		public ValueTask<IExternalService> CreateService(Uri? baseUri,
														 InvokeData invokeData,
														 IServiceCommunication serviceCommunication)
		{
			switch (_creators[invokeData.Type])
			{
				case IServiceCatalog.Creator creator:
					var service = creator();

					//service.Start(baseUri, invokeData, serviceCommunication);

					return new ValueTask<IExternalService>(service);

				case IServiceCatalog.ServiceCreator creator:
					return new ValueTask<IExternalService>(creator(baseUri, invokeData, serviceCommunication));

				case IServiceCatalog.ServiceCreatorAsync creator:
					return creator(baseUri, invokeData, serviceCommunication);

				default:
					throw Infra.Unmatched(_creators[invokeData.Type].GetType());
			}
		}
	}

	private class Activator(Catalog catalog) : IExternalServiceActivator
	{
	#region Interface IExternalServiceActivator

		public ValueTask<IExternalService> Create() => throw new NotImplementedException();

	#endregion

		public ValueTask<IExternalService> StartService(Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication)
		{
			if (invokeData is null) throw new ArgumentNullException(nameof(invokeData));

			Infra.Assert(CanHandle(invokeData.Type));

			return default; //catalog.CreateService(baseUri, invokeData, serviceCommunication);
		}

		public bool CanHandle(FullUri type) => catalog.CanHandle(type);
	}
}