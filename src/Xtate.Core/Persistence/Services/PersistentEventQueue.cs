// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.DataModel;
using Xtate.Interpreter.Services;

namespace Xtate.Persistence.Services;

[InstantiatedByIoC]
public class PersistentEventQueue(ITransactionalStorage storage) : EventQueue(new PersistentChannel<IIncomingEvent>(storage, Store, Restore)), IAsyncDisposable
{
#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);

		GC.SuppressFinalize(this);
	}

#endregion

	private static void Store(Bucket bucket, IIncomingEvent incomingEvent)
	{
		if (incomingEvent is not PersistedIncomingEvent persistedIncomingEvent)
		{
			persistedIncomingEvent = new PersistedIncomingEvent(incomingEvent);
		}

		persistedIncomingEvent.Store(bucket);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			storage.Dispose();
		}

		base.Dispose(disposing);
	}

	protected virtual ValueTask DisposeAsyncCore() => storage.DisposeAsync();

	private static PersistedIncomingEvent Restore(Bucket bucket) => new(bucket);
}
