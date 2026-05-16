// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.IoC;

namespace Xtate.Core;

public readonly struct TransformArgs<T, TArg, TNewArg>
{
	private readonly IServiceCollection _services;

	private readonly Delegate _transform;

	public TransformArgs(bool synchronous, IServiceCollection services, Delegate transform)
	{
		_services = services;
		_transform = transform;

		if (services.IsRegistered(TypeKey.ImplementationKey<ServiceFactoryNewArgsAsync<T, TArg, TNewArg>, ValueTuple>()))
		{
			return;
		}

		if (!synchronous)
		{
			services.AddFactory<ServiceFactoryNewArgsAsync<T, TArg, TNewArg>>().For<T, TNewArg>();
		}
		else if (!services.IsRegistered(TypeKey.ImplementationKey<ServiceFactoryNewArgsSync<T, TArg, TNewArg>, ValueTuple>()))
		{
			services.AddFactorySync<ServiceFactoryNewArgsSync<T, TArg, TNewArg>>().For<T, TNewArg>();
		}
	}

	public TransformArgs<T, TArg, TNewArg> IfAncestor<TAncestor>() where TAncestor : notnull
	{
		switch (_transform)
		{
			case Func<TNewArg, TArg> transformSync:
				_services.AddForwarding(sp => sp.GetRequiredServiceSync<AncestorTracker>().IsAncestorTypeEquals(level: 2, typeof(TAncestor))
											? (IArgsTransformer<T, TArg, TNewArg>) new ArgsTransformerSync<T, TArg, TNewArg>(transformSync)
											: null!);

				break;

			case Func<TNewArg, ValueTask<TArg>> transformAsync:
				_services.AddForwarding(sp => sp.GetRequiredServiceSync<AncestorTracker>().IsAncestorTypeEquals(level: 2, typeof(TAncestor))
											? (IArgsTransformer<T, TArg, TNewArg>) new ArgsTransformerAsync<T, TArg, TNewArg>(transformAsync)
											: null!);

				break;
		}

		return this;
	}
}