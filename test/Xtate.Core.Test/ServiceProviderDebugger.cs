using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xtate.IoC;

namespace Xtate.Core.Test
{
public class ServiceProviderDebugger : IServiceProviderDebugger
{
	private          int                                 _prevLevel;
	private          int                                 _level = 1;
	private          bool                                _noFactory;
	private          bool                                _factoryCalled;
	private readonly ConcurrentDictionary<TypeKey, Stat> _stats = new();
	private readonly TextWriter                          _writer;

	public ServiceProviderDebugger(TextWriter writer) => _writer = writer;
		
#region Interface IServiceProviderDebugger

	public void RegisterService(ServiceEntry serviceEntry)
	{
		GetStat(serviceEntry.Key).RegisterService(serviceEntry);

		_writer.WriteLine($"REG: {serviceEntry.InstanceScope,-10} - {serviceEntry.Key}");
	}

	public void BeforeFactory(TypeKey serviceKey)
	{
		GetStat(serviceKey).BeforeFactory();

		if (_factoryCalled)
		{
			_writer.WriteLine();

			_factoryCalled = false;
		}

		WriteIdent();
		_writer.Write($@"GET: {serviceKey}");

		_level ++;

		_noFactory = true;
	}

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
				_writer.Write("  ");
			}

			padding = true;
			_writer.Write(ch);
		}
	}

	public void FactoryCalled(TypeKey serviceKey)
	{
		var stat = GetStat(serviceKey);
		stat.FactoryCalled();

		_writer.Write($" {{ #{stat.InstancesCreated} ");
		_factoryCalled = true;
		_noFactory = false;
	}

	public void AfterFactory(TypeKey serviceKey)
	{
		_level --;

		if (_noFactory)
		{
			_writer.WriteLine(" - USE CACHED");
		}
		else
		{
			if (_factoryCalled)
			{
				_writer.WriteLine('}');
				_factoryCalled = false;
			}
			else
			{
				WriteIdent();
				_writer.WriteLine('}');
			}
		}

		_noFactory = false;

		GetStat(serviceKey).AfterFactory();
	}

#endregion

	private Stat GetStat(TypeKey serviceKey) => _stats.GetOrAdd(serviceKey, key => new Stat(key));

	public void Dump()
	{
		foreach (var pair in _stats.OrderByDescending(p => p.Value.InstancesCreated).ThenBy(p => p.Value.TypeKey.ToString()))
		{
			_writer.WriteLine($"STAT: {pair.Value.TypeKey}:\t{pair.Value.InstancesCreated}");
		}
	}

	private class Stat
	{
		private int _deepLevel;

		public Stat(TypeKey key)
		{
			TypeKey = key;
		}

		public List<ServiceEntry> Registrations    { get; }              = new();
		public TypeKey            TypeKey          { get; }
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
}}
