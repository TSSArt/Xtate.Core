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
using Xtate.Annotations;
using Xtate.Core;
using Xtate.CustomAction;
using Xtate.DataModel;
using Xtate.Service;

namespace Xtate
{
	[PublicAPI]
	public abstract class FactoryBase : IDataModelHandlerFactory, IServiceFactory, ICustomActionFactory
	{
		private List<ICustomActionFactory>?     _customActionFactories;
		private List<IDataModelHandlerFactory>? _dataModelHandlerFactories;
		private List<IServiceFactory>?          _serviceFactories;

	#region Interface ICustomActionFactory

		public async ValueTask<ICustomActionFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, string ns, string name, CancellationToken token)
		{
			if (_customActionFactories is not null)
			{
				foreach (var factory in _customActionFactories)
				{
					var activator = await factory.TryGetActivator(factoryContext, ns, name, token).ConfigureAwait(false);

					if (activator is not null)
					{
						return activator;
					}
				}
			}

			return null;
		}

	#endregion

	#region Interface IDataModelHandlerFactory

		public async ValueTask<IDataModelHandlerFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, string dataModelType, CancellationToken token)
		{
			if (_dataModelHandlerFactories is not null)
			{
				foreach (var factory in _dataModelHandlerFactories)
				{
					var activator = await factory.TryGetActivator(factoryContext, dataModelType, token).ConfigureAwait(false);

					if (activator is not null)
					{
						return activator;
					}
				}
			}

			return null;
		}

	#endregion

	#region Interface IServiceFactory

		public async ValueTask<IServiceFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, Uri type, CancellationToken token)
		{
			if (_serviceFactories is not null)
			{
				foreach (var factory in _serviceFactories)
				{
					var activator = await factory.TryGetActivator(factoryContext, type, token).ConfigureAwait(false);

					if (activator is not null)
					{
						return activator;
					}
				}
			}

			return null;
		}

	#endregion

		protected void Add(ICustomActionFactory customActionFactory) => (_customActionFactories ??= new List<ICustomActionFactory>()).Add(customActionFactory);

		protected void Add(IDataModelHandlerFactory dataModelHandlerFactory) => (_dataModelHandlerFactories ??= new List<IDataModelHandlerFactory>()).Add(dataModelHandlerFactory);

		protected void Add(IServiceFactory serviceFactory) => (_serviceFactories ??= new List<IServiceFactory>()).Add(serviceFactory);
	}
}