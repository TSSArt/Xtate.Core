using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xtate.IoC;

namespace Xtate;

internal class ServiceProviderDebugger(TextWriter writer) : IServiceProviderActions, IServiceProviderDataActions
{
	private readonly ConcurrentDictionary<TypeKey, Stat> _stats = new();
	private          bool                                _factoryCalled;
	private          int                                 _level = 1;
	private          bool                                _noFactory;
	private          int                                 _prevLevel;

#region Interface IServiceProviderDebugger

	public void RegisterService(ServiceEntry serviceEntry)
	{
		GetStat(serviceEntry.Key).RegisterService(serviceEntry);

		writer.WriteLine($"REG: {serviceEntry.InstanceScope,-10} - {serviceEntry.Key}");
	}

	public IServiceProviderDataActions RegisterServices() => this;

	public void ServiceRequesting<T, TArg>(TArg argument) => throw new NotSupportedException();

	public void ServiceRequested<T, TArg>(T? instance) => throw new NotSupportedException();

	public void FactoryCalling<T, TArg>(TArg argument) => throw new NotSupportedException();

	public void FactoryCalled<T, TArg>(T? instance) => throw new NotSupportedException();

	public IServiceProviderDataActions? ServiceRequesting(TypeKey serviceKey)
	{
		GetStat(serviceKey).BeforeFactory();

		if (_factoryCalled)
		{
			writer.WriteLine();

			_factoryCalled = false;
		}

		WriteIdent();
		writer.Write($@"GET: {serviceKey}");

		_level ++;

		_noFactory = true;

		return default;
	}

	public IServiceProviderDataActions? FactoryCalling(TypeKey serviceKey)
	{
		var stat = GetStat(serviceKey);
		stat.FactoryCalled();

		writer.Write($" {{ #{stat.InstancesCreated} ");
		_factoryCalled = true;
		_noFactory = false;

		return default;
	}

	public IServiceProviderDataActions? FactoryCalled(TypeKey typeKey) => default;

	public IServiceProviderDataActions? ServiceRequested(TypeKey serviceKey)
	{
		_level --;

		if (_noFactory)
		{
			writer.WriteLine(" - USE CACHED");
		}
		else
		{
			if (_factoryCalled)
			{
				writer.WriteLine('}');
				_factoryCalled = false;
			}
			else
			{
				WriteIdent();
				writer.WriteLine('}');
			}
		}

		_noFactory = false;

		GetStat(serviceKey).AfterFactory();

		return default;
	}

#endregion

	private void WriteIdent()
	{
		var padding = false;

		if (_level > 0)
		{
			for (var i = 1; i < _level; i ++)
			{
				WriteWithPadding('│');
			}

			WriteWithPadding(_level < _prevLevel ? '└' : '▶');

			_prevLevel = _level;
		}

		void WriteWithPadding(char ch)
		{
			if (padding)
			{
				writer.Write("  ");
			}

			padding = true;
			writer.Write(ch);
		}
	}

	private Stat GetStat(TypeKey serviceKey) => _stats.GetOrAdd(serviceKey, key => new Stat(key));

	public void Dump()
	{
		foreach (var pair in _stats.OrderByDescending(p => p.Value.InstancesCreated).ThenBy(p => p.Value.TypeKey.ToString()))
		{
			writer.WriteLine($"STAT: {pair.Value.TypeKey}:\t{pair.Value.InstancesCreated}");
		}
	}

	private class Stat(TypeKey key)
	{
		private int _deepLevel;

		public List<ServiceEntry> Registrations    { get; } = [];
		public TypeKey            TypeKey          { get; } = key;
		public int                InstancesCreated { get; private set; }

		public void BeforeFactory()
		{
			if (_deepLevel ++ > 100)
			{
				throw new DependencyInjectionException(@"Cycle reference detected in container configuration");
			}
		}

		public void AfterFactory() => _deepLevel --;

		public void FactoryCalled() => InstancesCreated ++;

		public void RegisterService(in ServiceEntry serviceEntry) => Registrations.Add(serviceEntry);
	}
}