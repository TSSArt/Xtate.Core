// Copyright © 2019-2024 Sergii Artemenko
// 
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

using System.Threading.Channels;

namespace Xtate.Persistence;

internal sealed class StateMachinePersistedController : StateMachineRuntimeController, IStorageProvider
{
	private const string ControllerStateKey = "cs";
	private const int    ExternalEventsKey  = 0;

	private readonly ChannelPersistingController<IEvent> _channelPersistingController;
	private readonly CancellationToken                   _stopToken;
	private readonly SemaphoreSlim                       _storageLock = new(initialCount: 0, maxCount: 1);
	private readonly IStorageProvider                    _storageProvider;

	private bool                   _disposed;
	private ITransactionalStorage? _storage;

	[Obsolete]
	public StateMachinePersistedController(SessionId sessionId,
										   IStateMachineOptions? options,
										   IStateMachine? stateMachine,
										   Uri? stateMachineLocation,
										   IStateMachineHost stateMachineHost,
										   IStorageProvider storageProvider,
										   TimeSpan? idlePeriod,
										   InterpreterOptions defaultOptions

		//SecurityContext securityContext,
		//								   DeferredFinalizer finalizer
	)
		: base(sessionId, options, stateMachine, stateMachineLocation, stateMachineHost, idlePeriod, defaultOptions)
	{
		_storageProvider = storageProvider;
		_stopToken = defaultOptions.StopToken;

		_channelPersistingController = new ChannelPersistingController<IEvent>(base.EventChannel);
	}

	protected override Channel<IEvent> EventChannel => _channelPersistingController;

#region Interface IStorageProvider

	ValueTask<ITransactionalStorage> IStorageProvider.GetTransactionalStorage(string? partition, string key)
	{
		if (partition is not null) throw new ArgumentException(Resources.Exception_PartitionArgumentShouldBeNull, nameof(partition));

		return _storageProvider.GetTransactionalStorage(SessionId.Value, key);
	}

	ValueTask IStorageProvider.RemoveTransactionalStorage(string? partition, string key)
	{
		if (partition is not null) throw new ArgumentException(Resources.Exception_PartitionArgumentShouldBeNull, nameof(partition));

		return _storageProvider.RemoveTransactionalStorage(SessionId.Value, key);
	}

	ValueTask IStorageProvider.RemoveAllTransactionalStorage(string? partition)
	{
		if (partition is not null) throw new ArgumentException(Resources.Exception_PartitionArgumentShouldBeNull, nameof(partition));

		return _storageProvider.RemoveAllTransactionalStorage(SessionId.Value);
	}

#endregion

	protected override async ValueTask DisposeAsyncCore()
	{
		if (_disposed)
		{
			return;
		}

		_storageLock.Dispose();

		_channelPersistingController.Dispose();

		if (_storage is { } storage)
		{
			await storage.DisposeAsync().ConfigureAwait(false);
		}

		_disposed = true;

		await base.DisposeAsyncCore().ConfigureAwait(false);
	}

	protected override async ValueTask Initialize()
	{
		await base.Initialize().ConfigureAwait(false);

		_storage = await _storageProvider.GetTransactionalStorage(SessionId.Value, ControllerStateKey /*, _stopToken*/).ConfigureAwait(false);

		_channelPersistingController.Initialize(new Bucket(_storage).Nested(ExternalEventsKey), bucket => new EventObject(bucket), _storageLock, token => _storage.CheckPoint(level: 0));

		_storageLock.Release();
	}
}