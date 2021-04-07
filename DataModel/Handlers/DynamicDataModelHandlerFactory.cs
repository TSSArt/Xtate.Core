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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DynamicDataModelHandlerFactory : DynamicFactory<IDataModelHandlerFactory>, IDataModelHandlerFactory
	{
		private readonly string? _uriFormat;

		protected DynamicDataModelHandlerFactory(bool throwOnError) : base(throwOnError) { }

		public DynamicDataModelHandlerFactory(string format, bool throwOnError = true) : base(throwOnError) => _uriFormat = format ?? throw new ArgumentNullException(nameof(format));

	#region Interface IDataModelHandlerFactory

		public async ValueTask<IDataModelHandlerFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, string dataModelType, CancellationToken token)
		{
			var factories = await GetFactories(factoryContext, DataModelTypeToUri(dataModelType), token).ConfigureAwait(false);

			foreach (var factory in factories)
			{
				var activator = await factory.TryGetActivator(factoryContext, dataModelType, token).ConfigureAwait(false);

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
	}
}