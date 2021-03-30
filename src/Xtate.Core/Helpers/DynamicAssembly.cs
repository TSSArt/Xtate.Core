#region Copyright © 2019-2021 Sergii Artemenko

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
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Core
{
	internal sealed class DynamicAssembly : IAsyncDisposable, IEquatable<DynamicAssembly>
	{
		internal static readonly object AssemblyCacheKey = new();

		private readonly ulong                _hash0;
		private readonly ulong                _hash1;
		private readonly ulong                _hash2;
		private readonly ulong                _hash3;
		private readonly TaskFactory          _ioBoundTaskFactory;
		private readonly Lazy<Task<Assembly>> _lazyInitialization;
		private          byte[]?              _bytes;
		private          Context?             _context;

		public DynamicAssembly(TaskFactory ioBoundTaskFactory, byte[] bytes)
		{
			_ioBoundTaskFactory = ioBoundTaskFactory;
			_bytes = bytes;
			_lazyInitialization = new Lazy<Task<Assembly>>(Initialization, LazyThreadSafetyMode.ExecutionAndPublication);

			using var sha256Hash = SHA256.Create();
			var hash = sha256Hash.ComputeHash(bytes);
			_hash0 = BitConverter.ToUInt64(hash, startIndex: 0);
			_hash1 = BitConverter.ToUInt64(hash, startIndex: 8);
			_hash2 = BitConverter.ToUInt64(hash, startIndex: 16);
			_hash3 = BitConverter.ToUInt64(hash, startIndex: 24);
		}

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			return _context is not null
				? new ValueTask(_ioBoundTaskFactory.StartNew(static ctx => ((Context) ctx!).Unload(), _context))
				: default;
		}

	#endregion

	#region Interface IEquatable<DynamicAssembly>

		public bool Equals(DynamicAssembly? other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return _hash0 == other._hash0 && _hash1 == other._hash1 && _hash2 == other._hash2 && _hash3 == other._hash3;
		}

	#endregion

		private Task<Assembly> Initialization() => _ioBoundTaskFactory.StartNew(static obj => ((DynamicAssembly) obj!).LoadAssembly(), this);

		public ValueTask<Assembly> GetAssembly() => new(_lazyInitialization.Value);

		private Assembly LoadAssembly()
		{
			Infra.NotNull(_bytes);

			_context = new Context();

			var memoryStream = new MemoryStream(_bytes, index: 0, _bytes.Length, writable: false, publiclyVisible: true);
			var assembly = _context.LoadFromStream(memoryStream);

			_bytes = default;

			return assembly;
		}

		public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is DynamicAssembly other && Equals(other);

		public override int GetHashCode() => unchecked((int) _hash0);

		private class Context : AssemblyLoadContext
		{
			public Context() : base(isCollectible: true) { }
		}
	}
}