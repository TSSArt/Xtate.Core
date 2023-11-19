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

namespace Xtate.IoC;

[PublicAPI]
[SuppressMessage(category: "Performance", checkId: "CA1815:Override equals and operator equals on value types", Justification = "Never uses equality")]
public readonly struct DecoratorImplementation<TImplementation, TArg> where TImplementation : notnull
{
	private readonly InstanceScope      _instanceScope;
	private readonly IServiceCollection _serviceCollection;
	private readonly bool               _synchronous;

	public DecoratorImplementation(IServiceCollection serviceCollection, InstanceScope instanceScope, bool synchronous)
	{
		_serviceCollection = serviceCollection;
		_instanceScope = instanceScope;
		_synchronous = synchronous;
	}

	public void For<TService>() where TService : class
	{
		var factory = _synchronous
			? DecoratorSyncFactoryProvider<TImplementation, TService, TArg>.Delegate()
			: DecoratorAsyncFactoryProvider<TImplementation, TService, TArg>.Delegate();

		_serviceCollection.Add(new ServiceEntry(TypeKey.ServiceKey<TService, TArg>(), _instanceScope, factory));
	}
}