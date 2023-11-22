#region Copyright © 2019-2022 Sergii Artemenko

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
using System.Threading.Tasks;

namespace Xtate.Core.IoC;

public class ScopedImplementationEntry : ImplementationEntry
{
	private readonly ServiceProvider _serviceProvider;
	private readonly object          _syncRoot = new();
	private          object?         _instance;

	public ScopedImplementationEntry(ServiceProvider serviceProvider, object factory) : base(factory) => _serviceProvider = serviceProvider;

	private ScopedImplementationEntry(ServiceProvider serviceProvider, ScopedImplementationEntry sourceEntry) : base(sourceEntry) => _serviceProvider = serviceProvider;

	protected override IServiceProvider ServiceProvider => _serviceProvider;

	internal override ImplementationEntry CreateNew2(ServiceProvider serviceProvider) => new ScopedImplementationEntry(serviceProvider, this);

	internal override ImplementationEntry CreateNew2(ServiceProvider serviceProvider, object factory) => new ScopedImplementationEntry(serviceProvider, factory);

	protected override ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) where T : default
	{
		lock (_syncRoot)
		{
			if (_instance is not Task<T?> task)
			{
				if (ArgumentType.TypeOf<TArg>().IsEmpty)
				{
					task = base.ExecuteFactory<T, TArg>(argument).AsTask();

					RegisterTaskInstanceForDispose(task).Forget();

					_instance = task;
				}
				else
				{
					task = ExecuteFactoryWithArg<T, TArg>(argument);
				}
			}

			return new ValueTask<T?>(task);
		}
	}

	private Task<T?> ExecuteFactoryWithArg<T, TArg>(TArg argument)
	{
		if (_instance is not Dictionary<ValueTuple<TArg>, Task<T?>> dictionary)
		{
			// ValueTuple<TArg> used instead of TArg as TKey type in Dictionary to support NULLs as key value
			_instance = dictionary = new Dictionary<ValueTuple<TArg>, Task<T?>>();
		}

		if (!dictionary.TryGetValue(ValueTuple.Create(argument), out var task))
		{
			task = base.ExecuteFactory<T, TArg>(argument).AsTask();

			RegisterTaskInstanceForDispose(task).Forget();

			dictionary.Add(ValueTuple.Create(argument), task);
		}

		return task;
	}

	protected override T ExecuteFactorySync<T, TArg>(TArg argument) where T : default =>
		throw new DependencyInjectionException(Resources.Exception_SingletonScopedTypesDoesNotSupportedForSyncronousInstantination);

	private async ValueTask RegisterTaskInstanceForDispose<T>(Task<T?> task)
	{
		var instance = await task.ConfigureAwait(false);

		try
		{
			_serviceProvider.RegisterInstanceForDispose(instance);
		}
		catch
		{
			await Disposer.DisposeAsync(instance).ConfigureAwait(false);

			throw;
		}
	}
}