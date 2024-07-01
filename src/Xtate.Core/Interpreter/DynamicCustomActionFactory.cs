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

//namespace Xtate.CustomAction;

//TODO: uncomment
/*

public class DynamicCustomActionFactory : DynamicFactory<ICustomActionFactory>, ICustomActionFactory
{
<<<<<<< Updated upstream
	//TODO: uncomment
	/*
	[PublicAPI]
	public class DynamicCustomActionFactory : DynamicFactory<ICustomActionFactory>, ICustomActionFactory
=======
	public DynamicCustomActionFactory(bool throwOnError = true) : base(throwOnError) { }

#region Interface ICustomActionFactory

	public async ValueTask<ICustomActionFactoryActivator?> TryGetActivator(ServiceLocator serviceLocator,
																		   string ns,
																		   string name,
																		   CancellationToken token)
>>>>>>> Stashed changes
	{
		var factories = await GetFactories(serviceLocator, CustomActionNamespaceToUri(ns), token).ConfigureAwait(false);

<<<<<<< Updated upstream
	#region Interface ICustomActionFactory

		public async ValueTask<ICustomActionFactoryActivator?> TryGetActivator(ServiceLocator serviceLocator,
																			   string ns,
																			   string name,
																			   CancellationToken token)
		{
			var factories = await GetFactories(serviceLocator, CustomActionNamespaceToUri(ns), token).ConfigureAwait(false);
=======
		foreach (var factory in factories)
		{
			var activator = await factory.TryGetActivator(serviceLocator, ns, name, token).ConfigureAwait(false);
>>>>>>> Stashed changes

			if (activator is not null)
			{
<<<<<<< Updated upstream
				var activator = await factory.TryGetActivator(serviceLocator, ns, name, token).ConfigureAwait(false);

				if (activator is not null)
				{
					return activator;
				}
=======
				return activator;
>>>>>>> Stashed changes
			}
		}

		return null;
	}
<<<<<<< Updated upstream
	*/
}
=======

#endregion

	protected virtual Uri CustomActionNamespaceToUri(string customActionNamespace) => new(customActionNamespace, UriKind.RelativeOrAbsolute);
}
*/
>>>>>>> Stashed changes
