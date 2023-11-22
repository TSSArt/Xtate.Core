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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Core.IoC;

#if DEBUG

// Adding support of cycle reference check
public abstract partial class ImplementationEntry
{
	private readonly AsyncLocal<Counter> _nestedCount = new();

	partial void Enter()
	{
		if (_nestedCount.Value is not { } counter)
		{
			_nestedCount.Value = counter = new Counter();
		}

		if (counter.Value ++ > 20)
		{
			throw new DependencyInjectionException(@"Cycle reference detected in container configuration");
		}
	}

	partial void Exit() => _nestedCount.Value!.Value --;

	private class Counter
	{
		public int Value;
	}
}
#endif

public abstract partial class ImplementationEntry
{
	private DelegateEntry?       _delegateEntry;
	private ImplementationEntry  _nextEntry;
	private ImplementationEntry? _previousEntry;

	protected ImplementationEntry(object factory)
	{
		Infra.Requires(factory);

		Factory = factory;
		_nextEntry = this;
	}

	protected ImplementationEntry(ImplementationEntry sourceImplementationEntry)
	{
		Infra.Requires(sourceImplementationEntry);

		Factory = sourceImplementationEntry.Factory;
		_delegateEntry = sourceImplementationEntry._delegateEntry;
		_nextEntry = this;
	}

	public object Factory { get; }

	protected abstract IServiceProvider ServiceProvider { get; }

	internal abstract ImplementationEntry CreateNew2(ServiceProvider serviceProvider);

	internal abstract ImplementationEntry CreateNew2(ServiceProvider serviceProvider, object factory);

	public void AddToChain([NotNull] ref ImplementationEntry? lastEntry)
	{
		Infra.Assert(_previousEntry is null);
		Infra.Assert(ReferenceEquals(_nextEntry, this));

		if (lastEntry is not null)
		{
			_nextEntry = lastEntry._nextEntry;
			_previousEntry = lastEntry;
			lastEntry._nextEntry = this;
		}

		lastEntry = this;
	}

	public Chain AsChain() => new(this);

	partial void Enter();

	partial void Exit();

	public async ValueTask<T> GetRequiredService<T, TArg>(TArg argument) where T : notnull
	{
		Enter();

		var instance = await ExecuteFactory<T, TArg>(argument).ConfigureAwait(false);

		Exit();

		var initTask = instance is not null && IsAsyncInitializationHandlerUsed
			? AsyncInitializationHandler.InitializeAsync(instance)
			: CustomInitialize(ServiceProvider, instance);

		await initTask.ConfigureAwait(false);

		return instance;

		static async Task CustomInitialize(IServiceProvider serviceProvider, [NotNull]T? obj)
		{
			if (obj is null)
			{
				throw MissedServiceException<T, TArg>();
			}

			if (serviceProvider.InitializationHandler is { } initializationHandler)
			{
				if (initializationHandler.Initialize(obj))
				{
					await initializationHandler.InitializeAsync(obj).ConfigureAwait(false);
				}
			}
		}
	}
	
	public async ValueTask<T?> GetOptionalService<T, TArg>(TArg argument)
	{
		Enter();

		var instance = await ExecuteFactory<T, TArg>(argument).ConfigureAwait(false);

		Exit();

		var initTask = IsAsyncInitializationHandlerUsed
			? AsyncInitializationHandler.InitializeAsync(instance)
			: CustomInitialize(ServiceProvider, instance);

		await initTask.ConfigureAwait(false);

		return instance;

		static async Task CustomInitialize(IServiceProvider serviceProvider, T? obj)
		{
			if (serviceProvider.InitializationHandler is { } initializationHandler)
			{
				if (initializationHandler.Initialize(obj))
				{
					await initializationHandler.InitializeAsync(obj).ConfigureAwait(false);
				}
			}
		}
	}

	public T GetRequiredServiceSync<T, TArg>(TArg argument) where T : notnull
	{
		Enter();

		var instance = ExecuteFactorySync<T, TArg>(argument);

		Exit();

		if (instance is not null && IsAsyncInitializationHandlerUsed)
		{
			if (AsyncInitializationHandler.Initialize(instance))
			{
				throw TypeUsedInSynchronousInstantiationException<T>();
			}
		}
		else
		{
			CustomInitialize(ServiceProvider, instance);
		}

		return instance;

		static void CustomInitialize(IServiceProvider serviceProvider, [NotNull]T? instance)
		{
			if (instance is null)
			{
				throw MissedServiceException<T, TArg>();
			}

			if (serviceProvider.InitializationHandler is { } initializationHandler)
			{
				if (initializationHandler.Initialize(instance))
				{
					throw TypeUsedInSynchronousInstantiationException<T>();
				}
			}
		}
	}

	public T? GetOptionalServiceSync<T, TArg>(TArg argument)
	{
		Enter();

		var instance = ExecuteFactorySync<T, TArg>(argument);

		Exit();

		if (IsAsyncInitializationHandlerUsed)
		{
			if (AsyncInitializationHandler.Initialize(instance))
			{
				throw TypeUsedInSynchronousInstantiationException<T>();
			}
		}
		else
		{
			CustomInitialize(ServiceProvider, instance);
		}

		return instance;

		static void CustomInitialize(IServiceProvider serviceProvider, T? instance)
		{
			if (serviceProvider.InitializationHandler is { } initializationHandler)
			{
				if (initializationHandler.Initialize(instance))
				{
					throw TypeUsedInSynchronousInstantiationException<T>();
				}
			}
		}
	}

	public static ValueTask<T> MissedServiceExceptionTask<T, TArg>() => new(Task.FromException<T>(MissedServiceException<T, TArg>()));

	public static DependencyInjectionException MissedServiceException<T, TArg>() =>
		ArgumentType.TypeOf<TArg>().IsEmpty
			? throw new DependencyInjectionException(Res.Format(Resources.Exception_ServiceMissedInContainer, typeof(T)))
			: throw new DependencyInjectionException(Res.Format(Resources.Exception_ServiceArgMissedInContainer, typeof(T), ArgumentType.TypeOf<TArg>()));

	private bool IsAsyncInitializationHandlerUsed => ReferenceEquals(ServiceProvider.InitializationHandler, AsyncInitializationHandler.Instance);

	private static Exception TypeUsedInSynchronousInstantiationException<T>() => new DependencyInjectionException(Res.Format(Resources.Exception_TypeUsedInSynchronousInstantiation, typeof(T)));

	protected virtual ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) =>
		Factory switch
		{
			Func<IServiceProvider, TArg, ValueTask<T?>> factory    => factory(ServiceProvider, argument),
			Func<IServiceProvider, TArg, T?> factory               => new ValueTask<T?>(factory(ServiceProvider, argument)),
			Func<IServiceProvider, T, TArg, ValueTask<T?>> factory => GetDecoratorAsync(factory, argument),
			Func<IServiceProvider, T, TArg, T?> factory            => GetDecoratorAsync(factory, argument),
			_                                                      => throw Infra.Unexpected<Exception>(Factory)
		};

	protected virtual T? ExecuteFactorySync<T, TArg>(TArg argument) =>
		Factory switch
		{
			Func<IServiceProvider, TArg, T?> factory       => factory(ServiceProvider, argument),
			Func<IServiceProvider, T, TArg, T?>            => throw new DependencyInjectionException(Resources.Exception_ServiceNotAvailableInSynchronousContext),
			Func<IServiceProvider, TArg, ValueTask<T?>>    => throw new DependencyInjectionException(Resources.Exception_ServiceNotAvailableInSynchronousContext),
			Func<IServiceProvider, T, TArg, ValueTask<T?>> => throw new DependencyInjectionException(Resources.Exception_ServiceNotAvailableInSynchronousContext),
			_                                              => throw Infra.Unexpected<Exception>(Factory)
		};

	private async ValueTask<T?> GetDecoratorAsync<T, TArg>(Func<IServiceProvider, T, TArg, ValueTask<T?>> factory, TArg argument) =>
		_previousEntry is not null && await _previousEntry.GetOptionalService<T, TArg>(argument).ConfigureAwait(false) is { } decoratedService
			? await factory(ServiceProvider, decoratedService, argument).ConfigureAwait(false)
			: default;

	private async ValueTask<T?> GetDecoratorAsync<T, TArg>(Func<IServiceProvider, T, TArg, T?> factory, TArg argument) =>
		_previousEntry is not null && await _previousEntry.GetOptionalService<T, TArg>(argument).ConfigureAwait(false) is { } decoratedService
			? factory(ServiceProvider, decoratedService, argument)
			: default;

	public async IAsyncEnumerable<T> GetServices<T, TArg>(TArg argument)
	{
		foreach (var entry in AsChain())
		{
			var result = await entry.GetOptionalService<T, TArg>(argument).ConfigureAwait(false);

			if (result is not null)
			{
				yield return result;
			}
		}
	}

	public TDelegate GetServicesDelegate<T, TArg, TDelegate>() where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is ServicesDelegateEntry<TDelegate> servicesDelegateEntry)
			{
				return servicesDelegateEntry.Delegate;
			}
		}

		var newDelegate = ArgumentType.CastFunc<TDelegate>(new Func<TArg, IAsyncEnumerable<T>>(GetServices<T, TArg>));
		_delegateEntry = new ServicesDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	public TDelegate GetRequiredServiceDelegate<T, TArg, TDelegate>() where T : notnull where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is RequiredServiceDelegateEntry<TDelegate> requiredServiceDelegateEntry)
			{
				return requiredServiceDelegateEntry.Delegate;
			}
		}

		var newDelegate = ArgumentType.CastFunc<TDelegate>(new Func<TArg, ValueTask<T>>(GetRequiredService<T, TArg>));
		_delegateEntry = new RequiredServiceDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	public TDelegate GetOptionalServiceDelegate<T, TArg, TDelegate>() where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is OptionalServiceDelegateEntry<TDelegate> optionalServiceDelegateEntry)
			{
				return optionalServiceDelegateEntry.Delegate;
			}
		}

		var newDelegate = ArgumentType.CastFunc<TDelegate>(new Func<TArg, ValueTask<T?>>(GetOptionalService<T, TArg>));
		_delegateEntry = new OptionalServiceDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	public TDelegate GetRequiredServiceSyncDelegate<T, TArg, TDelegate>() where T : notnull where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is RequiredServiceSyncDelegateEntry<TDelegate> requiredServiceSyncDelegateEntry)
			{
				return requiredServiceSyncDelegateEntry.Delegate;
			}
		}

		var newDelegate = ArgumentType.CastFunc<TDelegate>(new Func<TArg, T>(GetRequiredServiceSync<T, TArg>));
		_delegateEntry = new RequiredServiceSyncDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	public TDelegate GetOptionalServiceSyncDelegate<T, TArg, TDelegate>() where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is OptionalServiceSyncDelegateEntry<TDelegate> optionalServiceSyncDelegateEntry)
			{
				return optionalServiceSyncDelegateEntry.Delegate;
			}
		}

		var newDelegate = ArgumentType.CastFunc<TDelegate>(new Func<TArg, T?>(GetOptionalServiceSync<T, TArg>));
		_delegateEntry = new OptionalServiceSyncDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	public struct Chain : IEnumerable<ImplementationEntry>, IEnumerator<ImplementationEntry>
	{
		private readonly ImplementationEntry _lastEntry;

		public Chain(ImplementationEntry lastEntry) => _lastEntry = lastEntry;

	#region Interface IDisposable

		void IDisposable.Dispose() { }

	#endregion

	#region Interface IEnumerable

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion

	#region Interface IEnumerable<ImplementationEntry>

		IEnumerator<ImplementationEntry> IEnumerable<ImplementationEntry>.GetEnumerator() => GetEnumerator();

	#endregion

	#region Interface IEnumerator

		public bool MoveNext()
		{
			var ok = !ReferenceEquals(Current, _lastEntry);
			Current = (Current ?? _lastEntry)._nextEntry;

			return ok;
		}

		void IEnumerator.Reset() => Current = default!;

		object IEnumerator.Current => Current;

	#endregion

	#region Interface IEnumerator<ImplementationEntry>

		public ImplementationEntry Current { get; private set; } = default!;

	#endregion

		public Chain GetEnumerator()
		{
			Infra.Assert(_lastEntry._nextEntry._previousEntry is null); // Entry should be last entry in the chain

			return new(_lastEntry);
		}
	}

	private abstract record DelegateEntry
	{
		protected DelegateEntry(DelegateEntry? next) => Next = next;

		public DelegateEntry? Next { get; }
	}

	private record ServicesDelegateEntry<T>(T Delegate, DelegateEntry? Next) : DelegateEntry(Next);

	private record RequiredServiceDelegateEntry<T>(T Delegate, DelegateEntry? Next) : DelegateEntry(Next);

	private record OptionalServiceDelegateEntry<T>(T Delegate, DelegateEntry? Next) : DelegateEntry(Next);

	private record RequiredServiceSyncDelegateEntry<T>(T Delegate, DelegateEntry? Next) : DelegateEntry(Next);

	private record OptionalServiceSyncDelegateEntry<T>(T Delegate, DelegateEntry? Next) : DelegateEntry(Next);
}