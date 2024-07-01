<<<<<<< Updated upstream
﻿using System;
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
=======
﻿#region Copyright © 2019-2023 Sergii Artemenko

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
>>>>>>> Stashed changes

	public void RegisterService(ServiceEntry serviceEntry)
	{
		GetStat(serviceEntry.Key).RegisterService(serviceEntry);

<<<<<<< Updated upstream
		_writer.WriteLine($"REG: {serviceEntry.InstanceScope,-10} - {serviceEntry.Key}");
=======
		writer.WriteLine($"REG: {serviceEntry.InstanceScope,-10} - {serviceEntry.Key}");
>>>>>>> Stashed changes
	}

	public void BeforeFactory(TypeKey serviceKey)
	{
		GetStat(serviceKey).BeforeFactory();

		if (_factoryCalled)
		{
<<<<<<< Updated upstream
			_writer.WriteLine();
=======
			writer.WriteLine();
>>>>>>> Stashed changes

			_factoryCalled = false;
		}

		WriteIdent();
<<<<<<< Updated upstream
		_writer.Write($@"GET: {serviceKey}");
=======
		writer.Write($@"GET: {serviceKey}");
>>>>>>> Stashed changes

		_level ++;

		_noFactory = true;
	}

<<<<<<< Updated upstream
=======
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

>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
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

=======
				writer.Write("  ");
			}

			padding = true;
			writer.Write(ch);
		}
	}

>>>>>>> Stashed changes
	private Stat GetStat(TypeKey serviceKey) => _stats.GetOrAdd(serviceKey, key => new Stat(key));

	public void Dump()
	{
		foreach (var pair in _stats.OrderByDescending(p => p.Value.InstancesCreated).ThenBy(p => p.Value.TypeKey.ToString()))
		{
<<<<<<< Updated upstream
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
=======
			writer.WriteLine($"STAT: {pair.Value.TypeKey}:\t{pair.Value.InstancesCreated}");
		}
	}

	private class Stat(TypeKey key)
	{
		private int _deepLevel;

		public List<ServiceEntry> Registrations    { get; } = [];
		public TypeKey TypeKey { get; } = key;
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
}}
=======
}
>>>>>>> Stashed changes
