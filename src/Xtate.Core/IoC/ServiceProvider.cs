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
using System.Threading;
using System.Threading.Tasks;
using Empty = System.ValueTuple;

namespace Xtate.Core.IoC;

public interface IDelegateFactory
{
	Delegate? GetDelegate<T>();
}

public class ServiceProvider : IServiceProvider, IServiceScopeFactory, ITypeKeyAction, IDisposable, IAsyncDisposable
{
	private readonly CancellationTokenSource              _disposeTokenSource  = new();
	private readonly WeakReferenceCollection              _instancesForDispose = new();
	private readonly Cache<TypeKey, ImplementationEntry?> _services;
	private readonly SingletonContainer                   _singletonContainer;
	private readonly ServiceProvider?                     _sourceServiceProvider;

	private int _disposed;

	public ServiceProvider(IServiceCollection services)
	{
		Infra.Requires(services);

		_sourceServiceProvider = default;
		_singletonContainer = new SingletonContainer();
		_services = new Cache<TypeKey, ImplementationEntry?>(GroupServices(services));

		InitializationHandler = GetService<IInitializationHandler>();
	}

	protected ServiceProvider(ServiceProvider sourceServiceProvider, IServiceCollection? additionalServices = default)
	{
		Infra.Requires(sourceServiceProvider);

		_sourceServiceProvider = sourceServiceProvider;
		_singletonContainer = sourceServiceProvider._singletonContainer;
		_singletonContainer.AddReference();
		_services = new Cache<TypeKey, ImplementationEntry?>(GroupServices(sourceServiceProvider, additionalServices));

		InitializationHandler = GetService<IInitializationHandler>();
	}

	public IInitializationHandler? InitializationHandler { get; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	#endregion

	#region Interface IServiceProvider

	public void TypedAction<T>(TypeKey typeKey) => _services.TryAdd(typeKey, CreateEntries<T>(typeKey));

	public ImplementationEntry? GetImplementationEntry(TypeKey typeKey)
	{
		CheckIfDisposed();

		if (_services.TryGetValue(typeKey, out var entry))
		{
			return entry;
		}

		if (typeKey is SimpleTypeKey simpleKey)
		{
			return _services.GetOrAdd(typeKey, CopyEntries(simpleKey));
		}

		Infra.Requires(typeKey);

		typeKey.DoTypedAction(this);

		_services.TryGetValue(typeKey, out entry);

		return entry;
	}

	public CancellationToken DisposeToken => _disposeTokenSource.Token;

	#endregion

	#region Interface IServiceScopeFactory

	public IServiceScope CreateScope() => new ServiceProviderScope(this);

	public IServiceScope CreateScope(Action<IServiceCollection> configureServices)
	{
		Infra.Requires(configureServices);

		var additionalServices = new ServiceCollection();
		configureServices(additionalServices);

		return new ServiceProviderScope(this, additionalServices);
	}

#endregion

	private IEnumerable<KeyValuePair<TypeKey, ImplementationEntry?>> GroupServices(IServiceCollection services)
	{
		const int internalServicesCount = 3; /*IInitializationHandler, IServiceProvider, IServiceScopeFactory*/

		var groupedServices = new Dictionary<TypeKey, ImplementationEntry?>(services.Count + internalServicesCount);

		AddForwarding(groupedServices, static (_, _) => AsyncInitializationHandler.Instance);

		foreach (var registration in services)
		{
			AddRegistration(groupedServices, sourceServiceProvider: default, registration);
		}

		AddForwarding(groupedServices, static (serviceProvider, _) => serviceProvider);
		AddForwarding(groupedServices, static (serviceProvider, _) => (IServiceScopeFactory) serviceProvider);

		return groupedServices;
	}

	private void AddForwarding<T>(Dictionary<TypeKey, ImplementationEntry?> services, Func<IServiceProvider, Empty, T> evaluator) =>
		AddRegistration(services, sourceServiceProvider: default, new ServiceEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, evaluator));

	private IEnumerable<KeyValuePair<TypeKey, ImplementationEntry?>> GroupServices(ServiceProvider sourceServiceProvider, IServiceCollection? services)
	{
		if (services?.Count is not ({ } count and > 0))
		{
			return Array.Empty<KeyValuePair<TypeKey, ImplementationEntry?>>();
		}

		var groupedServices = new Dictionary<TypeKey, ImplementationEntry?>(count);

		foreach (var registration in services)
		{
			AddRegistration(groupedServices, sourceServiceProvider, registration);
		}

		return groupedServices;
	}

	private void AddRegistration(Dictionary<TypeKey, ImplementationEntry?> services, ServiceProvider? sourceServiceProvider, in ServiceEntry service)
	{
		var key = service.Key as SimpleTypeKey ?? ((GenericTypeKey)service.Key).DefinitionKey;

		if (!services.TryGetValue(key, out var lastEntry))
		{
			if (sourceServiceProvider?.GetImplementationEntry2(key) is { } sourceEntry)
			{
				foreach (var entry in sourceEntry.AsChain())
				{
					entry.CreateNew2(this).AddToChain(ref lastEntry);
				}
			}
		}

		CreateImplementationEntry(service).AddToChain(ref lastEntry);
		services[key] = lastEntry;
	}

	private ImplementationEntry? GetImplementationEntry2(SimpleTypeKey typeKey)
	{
		if (!_services.TryGetValue(typeKey, out var entry))
		{
			entry = _services.GetOrAdd(typeKey, CopyEntries(typeKey));
		}

		return entry;
	}

	private T? GetService<T>() => GetImplementationEntry2((SimpleTypeKey) TypeKey.ServiceKey<T, Empty>()) is { } entry ? entry.GetOptionalServiceSync<T, Empty>(default) : default;

	public void RegisterInstanceForDispose<T>(T? instance)
	{
		if (Disposer.IsDisposable(instance) && (instance is not ServiceProvider serviceProvider || serviceProvider != this))
		{
			AddForDispose(instance);
		}
	}

	public void RegisterSingletonInstanceForDispose<T>(T? instance)
	{
		if (Disposer.IsDisposable(instance) && (instance is not ServiceProvider serviceProvider || serviceProvider != this))
		{
			_singletonContainer.AddForDispose(instance);
		}
	}

	private ImplementationEntry? CopyEntries(SimpleTypeKey typeKey)
	{
		ImplementationEntry? lastEntry = default;

		if (_sourceServiceProvider?.GetImplementationEntry2(typeKey) is { } sourceEntry)
		{
			foreach (var entry in sourceEntry.AsChain())
			{
				entry.CreateNew2(this).AddToChain(ref lastEntry);
			}
		}

		return lastEntry;
	}

	private ImplementationEntry? CreateEntries<T>(TypeKey typeKey)
	{
		if (typeKey is not GenericTypeKey genericTypeKey)
		{
			Infra.Fail();

			return null;
		}

		ImplementationEntry? lastEntry = default;

		if (GetImplementationEntry2(genericTypeKey.DefinitionKey) is { } genericEntry)
		{
			foreach (var entry in genericEntry.AsChain())
			{
				if (entry.Factory is not Func<IDelegateFactory> delegateFactoryFunc)
				{
					entry.CreateNew2(this).AddToChain(ref lastEntry);
				}
				else if (delegateFactoryFunc().GetDelegate<T>() is { } factory)
				{
					entry.CreateNew2(this, factory).AddToChain(ref lastEntry);
				}
			}
		}

		return lastEntry;
	}

	private ImplementationEntry CreateImplementationEntry(in ServiceEntry service) =>
		service.InstanceScope switch
		{
			InstanceScope.Singleton  => new SingletonImplementationEntry(this, service.Factory),
			InstanceScope.Scoped     => new ScopedImplementationEntry(this, service.Factory),
			InstanceScope.Transient  => new TransientImplementationEntry(this, service.Factory),
			InstanceScope.Forwarding => new ForwardingImplementationEntry(this, service.Factory),
			_                        => throw Infra.Unexpected<Exception>(service.InstanceScope)
		};

	private void AddForDispose(object instance)
	{
		CheckIfDisposed();

		_instancesForDispose.Put(instance);
	}

	~ServiceProvider() => Dispose(false);

	protected virtual void Dispose(bool disposing)
	{
		if (Interlocked.Exchange(ref _disposed, value: 1) == 0)
		{
			var isLastReference = _singletonContainer.RemoveReference();

			if (disposing)
			{
				_disposeTokenSource.Cancel();

				while (_instancesForDispose.TryTake(out var instance))
				{
					Disposer.Dispose(instance);
				}

				if (isLastReference)
				{
					_singletonContainer.Dispose();
				}

				_disposeTokenSource.Dispose();
			}
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (Interlocked.Exchange(ref _disposed, value: 1) == 0)
		{
			_disposeTokenSource.Cancel();

			while (_instancesForDispose.TryTake(out var instance))
			{
				await Disposer.DisposeAsync(instance).ConfigureAwait(false);
			}

			if (_singletonContainer.RemoveReference())
			{
				await _singletonContainer.DisposeAsync().ConfigureAwait(false);
			}

			_disposeTokenSource.Dispose();
		}
	}

	private void CheckIfDisposed()
	{
		if (_disposed != 0)
		{
			throw new ObjectDisposedException(nameof(ServiceProvider));
		}
	}

	private sealed class SingletonContainer : IDisposable, IAsyncDisposable
	{
		private readonly WeakReferenceCollection _instancesForDispose = new();
		private          int                     _disposed;

		private int _referenceCount;

	#region Interface IAsyncDisposable

		public async ValueTask DisposeAsync()
		{
			if (Interlocked.CompareExchange(ref _disposed, value: 1, comparand: 0) == 0)
			{
				while (_instancesForDispose.TryTake(out var instance))
				{
					await Disposer.DisposeAsync(instance).ConfigureAwait(false);
				}
			}
		}

	#endregion

	#region Interface IDisposable

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposed, value: 1, comparand: 0) == 0)
			{
				while (_instancesForDispose.TryTake(out var instance))
				{
					Disposer.Dispose(instance);
				}
			}
		}

	#endregion

		public void AddReference() => Interlocked.Increment(ref _referenceCount);

		public bool RemoveReference() => Interlocked.Decrement(ref _referenceCount) == 0;

		public void AddForDispose(object instance)
		{
			CheckIfDisposed();

			_instancesForDispose.Put(instance);
		}

		private void CheckIfDisposed()
		{
			if (Volatile.Read(ref _disposed) != 0)
			{
				throw new ObjectDisposedException(nameof(ServiceProvider));
			}
		}
	}
}