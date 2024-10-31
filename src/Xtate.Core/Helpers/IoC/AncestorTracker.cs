// Copyright © 2019-2024 Sergii Artemenko
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

using System.Collections.Concurrent;
using Xtate.IoC;

namespace Xtate.Core;

public class AncestorTracker : IServiceProviderActions, IServiceProviderDataActions
{
	private static readonly ConcurrentBag<Container> ContainerPool = [];

	private readonly AsyncLocal<Container?> _local = new();

#region Interface IServiceProviderActions

	public IServiceProviderDataActions? RegisterServices() => default;

	public IServiceProviderDataActions? ServiceRequesting(TypeKey typeKey) => default;

	public IServiceProviderDataActions? ServiceRequested(TypeKey typeKey) => default;

	public IServiceProviderDataActions? FactoryCalling(TypeKey typeKey) => typeKey.IsEmptyArg ? this : default;

	public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey) => typeKey.IsEmptyArg ? this : default;

#endregion

#region Interface IServiceProviderDataActions

	[ExcludeFromCodeCoverage]
	public void RegisterService(ServiceEntry serviceEntry) { }

	[ExcludeFromCodeCoverage]
	public void ServiceRequesting<T, TArg>(TArg argument) { }

	[ExcludeFromCodeCoverage]
	public void ServiceRequested<T, TArg>(T? instance) { }

	public void FactoryCalling<T, TArg>(TArg argument) => CurrentContainer().Add((typeof(T), default));

	public void FactoryCalled<T, TArg>(T? instance)
	{
		var container = CurrentContainer();

		for (var i = 0; i < container.Count; i ++)
		{
			var (type, ancestor) = container[i];

			if (type == typeof(T))
			{
				container[i] = default;

				if (ancestor is AncestorFactory<T> ancestorFactory)
				{
					ancestorFactory.SetValue(instance);
				}
			}
		}

		container.RemoveAll(static p => p.Type is null);

		if (container.Count == 0)
		{
			_local.Value = default!;

			ContainerPool.Add(container);
		}
	}

#endregion

	private Container CurrentContainer()
	{
		if (_local.Value is { } container)
		{
			return container;
		}

		if (!ContainerPool.TryTake(out container))
		{
			container = [];
		}

		return _local.Value = container;
	}

	public bool TryCaptureAncestor(Type ancestorType, object ancestorFactory)
	{
		var container = CurrentContainer();

		for (var i = 0; i < container.Count; i ++)
		{
			var (type, ancestor) = container[i];

			if (type == ancestorType)
			{
				if (ancestor is null)
				{
					container[i] = (type, ancestorFactory);
				}
				else
				{
					container.Add((type, ancestorFactory));
				}

				return true;
			}
		}

		return false;
	}

	private class Container : List<(Type Type, object? Ancestor)>;
}