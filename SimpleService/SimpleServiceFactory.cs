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
	public sealed class SimpleServiceFactory<TService> : IServiceFactory where TService : SimpleServiceBase, new()
	{
		public static readonly IServiceFactory Instance = new SimpleServiceFactory<TService>();

		private readonly Uri? _alias;
		private readonly Uri  _type;

		private SimpleServiceFactory()
		{
			var serviceAttribute = typeof(TService).GetCustomAttribute<SimpleServiceAttribute>();

			if (serviceAttribute == null)
			{
				throw new InfrastructureException(Res.Format(Resources.Exception_ServiceAttribute_did_not_provided_for_type, typeof(TService)));
			}

			_type = new Uri(serviceAttribute.Type, UriKind.RelativeOrAbsolute);
			_alias = serviceAttribute.Alias != null ? new Uri(serviceAttribute.Alias, UriKind.RelativeOrAbsolute) : null;
		}

	#region Interface IServiceFactory

		ValueTask<bool> IServiceFactory.CanHandle(Uri type, Uri? source, CancellationToken token) =>
				new ValueTask<bool>(FullUriComparer.Instance.Equals(type, _type) || FullUriComparer.Instance.Equals(type, _alias));

		ValueTask<IService> IServiceFactory.StartService(Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication, CancellationToken token)
		{
			var service = new TService();

			service.Start(baseUri, invokeData, serviceCommunication);

			return new ValueTask<IService>(service);
		}

	#endregion
	}
}