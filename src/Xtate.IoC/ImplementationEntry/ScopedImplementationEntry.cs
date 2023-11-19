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

internal sealed class ScopedImplementationEntry : ImplementationEntry
{
	private readonly ServiceProvider _serviceProvider;
	private readonly object          _syncRoot = new();
	private          object?         _instance;

	public ScopedImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(factory) => _serviceProvider = serviceProvider;

	private ScopedImplementationEntry(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(sourceEntry) => _serviceProvider = serviceProvider;

	protected override IServiceProvider ServiceProvider => _serviceProvider;

	internal override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new ScopedImplementationEntry(serviceProvider, this);

	internal override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new ScopedImplementationEntry(serviceProvider, factory);

	[ExcludeFromCodeCoverage]
	private async ValueTask<T?> ExecuteFactoryInternal<T, TArg>(TArg argument)
	{
		var instance = await base.ExecuteFactory<T, TArg>(argument).ConfigureAwait(false);

		try
		{
			_serviceProvider.RegisterInstanceForDispose(instance);

			return instance;
		}
		catch
		{
			await Disposer.DisposeAsync(instance).ConfigureAwait(false);

			throw;
		}
	}

	protected override ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) where T : default
	{
		lock (_syncRoot)
		{
			if (_instance is not Task<T?> task)
			{
				if (ArgumentType.TypeOf<TArg>().IsEmpty)
				{
					_instance = task = ExecuteFactoryInternal<T, TArg>(argument).AsTask();
				}
				else
				{
					task = ExecuteFactoryWithArg<T, TArg>(argument);
				}
			}

			return new ValueTask<T?>(task);
		}
	}

	protected override T? ExecuteFactorySync<T, TArg>(TArg argument) where T : default
	{
		EnsureSynchronousContext<T, TArg>();

		var valueTask = ExecuteFactory<T, TArg>(argument);

		Infra.Assert(valueTask.IsCompleted);

		return valueTask.Result;
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
			task = ExecuteFactoryInternal<T, TArg>(argument).AsTask();

			dictionary.Add(ValueTuple.Create(argument), task);
		}

		return task;
	}
}