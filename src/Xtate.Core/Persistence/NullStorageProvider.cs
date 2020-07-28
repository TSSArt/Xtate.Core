#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Persistence
{
	internal sealed class NullStorageProvider : IStorageProvider, ITransactionalStorage
	{
		public static readonly NullStorageProvider Instance = new NullStorageProvider();

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync() => default;

	#endregion

	#region Interface IDisposable

		public void Dispose() { }

	#endregion

	#region Interface IStorage

		public void Write(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value) { }

		public ReadOnlyMemory<byte> Read(ReadOnlySpan<byte> key) => ReadOnlyMemory<byte>.Empty;

	#endregion

	#region Interface IStorageProvider

		public ValueTask<ITransactionalStorage> GetTransactionalStorage(string? partition, string key, CancellationToken token) => new ValueTask<ITransactionalStorage>(Instance);

		public ValueTask RemoveTransactionalStorage(string? partition, string key, CancellationToken token) => default;

		public ValueTask RemoveAllTransactionalStorage(string? partition, CancellationToken token) => default;

	#endregion

	#region Interface ITransactionalStorage

		public ValueTask CheckPoint(int level, CancellationToken token) => default;

		public ValueTask Shrink(CancellationToken token) => default;

	#endregion
	}
}