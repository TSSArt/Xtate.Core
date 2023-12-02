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

using System.IO;
using Xtate.IoC;

namespace Xtate.Core.Test;

public class ServiceProviderDebugger(TextWriter writer) : IServiceProviderDebugger
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

	public void BeforeFactory(TypeKey serviceKey)
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
	}

	public void FactoryCalled(TypeKey serviceKey)
	{
		var stat = GetStat(serviceKey);
		stat.FactoryCalled();

		writer.Write($" {{ #{stat.InstancesCreated} ");
		_factoryCalled = true;
		_noFactory = false;
	}

	public void AfterFactory(TypeKey serviceKey)
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
		public TypeKey TypeKey { get; } = key;
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