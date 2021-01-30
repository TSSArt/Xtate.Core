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

namespace Xtate.Service
{
	public class HttpClientServiceFactory : ServiceFactoryBase
	{
		private readonly HttpClientServiceOptions _options;

		private HttpClientServiceFactory(HttpClientServiceOptions options) => _options = options;

		public static IServiceFactory Instance { get; } = new HttpClientServiceFactory(HttpClientServiceOptions.CreateDefault());

		protected override void Register(IServiceCatalog catalog)
		{
			if (catalog is null) throw new ArgumentNullException(nameof(catalog));

			catalog.Register(type: @"http://xtate.net/scxml/service/#HTTPClient", HttpClientServiceCreator);
			catalog.Register(type: @"http", HttpClientServiceCreator);
		}

		private ServiceBase HttpClientServiceCreator() => new HttpClientService(_options);

		public static IServiceFactory Create(HttpClientServiceOptions options) => new HttpClientServiceFactory(options);
	}
}