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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.CustomAction
{
	[PublicAPI]
	public class DynamicCustomActionFactory : DynamicFactory<ICustomActionFactory>, ICustomActionFactory
	{
		public DynamicCustomActionFactory(bool throwOnError = true) : base(throwOnError) { }

	#region Interface ICustomActionFactory

		public async ValueTask<ICustomActionFactoryActivator?> TryGetActivator(IFactoryContext factoryContext,
																			   string ns,
																			   string name,
																			   CancellationToken token)
		{
			var factories = await GetFactories(factoryContext, ns, token).ConfigureAwait(false);

			foreach (var factory in factories)
			{
				var activator = await factory.TryGetActivator(factoryContext, ns, name, token).ConfigureAwait(false);

				if (activator is not null)
				{
					return activator;
				}
			}

			return null;
		}

	#endregion

		protected virtual Uri CustomActionNamespaceToUri(string customActionNamespace) => new(customActionNamespace, UriKind.RelativeOrAbsolute);

		protected sealed override Uri KeyToUri(object key) => CustomActionNamespaceToUri((string) key);
	}
}