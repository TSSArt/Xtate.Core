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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.Service
{
	public abstract class ServiceFactoryBase : IServiceFactory
	{
		private Activator? _activator;

	#region Interface IServiceFactory

		public ValueTask<IServiceFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, Uri type, CancellationToken token)
		{
			_activator ??= CreateActivator();

			return new ValueTask<IServiceFactoryActivator?>(_activator.CanHandle(type) ? _activator : null);
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
			private readonly Dictionary<Uri, Delegate> _creators = new(FullUriComparer.Instance);

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

			public ValueTask<IService> CreateService(IFactoryContext factoryContext, Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication, CancellationToken token)
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
						return creator(factoryContext, baseUri, invokeData, serviceCommunication, token);

					default:
						return Infrastructure.UnexpectedValue<ValueTask<IService>>(_creators[invokeData.Type]?.GetType());
				}
			}
		}

		private class Activator : IServiceFactoryActivator
		{
			private readonly Catalog _catalog;

			public Activator(Catalog catalog) => _catalog = catalog;

		#region Interface IServiceFactoryActivator

			public ValueTask<IService> StartService(IFactoryContext factoryContext, Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication, CancellationToken token)
			{
				if (invokeData is null) throw new ArgumentNullException(nameof(invokeData));

				Infrastructure.Assert(CanHandle(invokeData.Type));

				return _catalog.CreateService(factoryContext, baseUri, invokeData, serviceCommunication, token);
			}

		#endregion

			public bool CanHandle(Uri type) => _catalog.CanHandle(type);
		}
	}
}