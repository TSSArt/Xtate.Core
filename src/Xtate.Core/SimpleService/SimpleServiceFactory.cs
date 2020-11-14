#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Service
{
	public sealed class SimpleServiceFactory<TService> : IServiceFactory, IServiceFactoryActivator where TService : SimpleServiceBase, new()
	{
		public static IServiceFactory Instance { get; } = new SimpleServiceFactory<TService>();

		private readonly Uri? _alias;
		private readonly Uri  _type;

		private SimpleServiceFactory()
		{
			if (typeof(TService).GetCustomAttribute<SimpleServiceAttribute>() is { } serviceAttribute)
			{
				_type = new Uri(serviceAttribute.Type, UriKind.RelativeOrAbsolute);
				_alias = serviceAttribute.Alias is not null ? new Uri(serviceAttribute.Alias, UriKind.RelativeOrAbsolute) : null;

				return;
			}

			throw new InfrastructureException(Res.Format(Resources.Exception_ServiceAttribute_did_not_provided_for_type, typeof(TService)));
		}

	#region Interface IServiceFactory

		public ValueTask<IServiceFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, Uri type, CancellationToken token) =>
				new ValueTask<IServiceFactoryActivator?>(CanHandle(type) ? this : null);

	#endregion

	#region Interface IServiceFactoryActivator

		public ValueTask<IService> StartService(IFactoryContext factoryContext, Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication, CancellationToken token)
		{
			if (invokeData is null) throw new ArgumentNullException(nameof(invokeData));

			Infrastructure.Assert(CanHandle(invokeData.Type));

			var service = new TService();

			service.Start(baseUri, invokeData, serviceCommunication);

			return new ValueTask<IService>(service);
		}

	#endregion

		private bool CanHandle(Uri type) => FullUriComparer.Instance.Equals(type, _type) || FullUriComparer.Instance.Equals(type, _alias);
	}
}