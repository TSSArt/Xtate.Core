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