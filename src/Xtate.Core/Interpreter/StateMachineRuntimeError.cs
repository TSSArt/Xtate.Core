// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.Core;

public class StateMachineRuntimeError(ScopeObject scopeObject)
{
	private readonly object _owner = scopeObject.GetForType<Owner>();

	public bool IsDestroyError(Exception exception) => AsOwnedError<StateMachineDestroyedException>(exception) is not null;

	public bool IsPlatformError(Exception exception) => AsOwnedError<PlatformException>(exception) is not null;

	public bool IsCommunicationError(Exception exception, out SendId? sendId)
	{
		var communicationException = AsOwnedError<CommunicationException>(exception);
		
		sendId = communicationException?.SendId;

		return communicationException is not null;
	}

	private TException? AsOwnedError<TException>(Exception exception) where TException : OwnedXtateException
	{
		for (var ex = exception; ex is not null; ex = ex.InnerException)
		{
			if (ex is TException ownedXtateException)
			{
				if (ownedXtateException.IsOwnedBy(_owner))
				{
					return ownedXtateException;
				}

				break;
			}
		}

		return null;
	}

	public CommunicationException NoExternalConnections() => new(Resources.Exception_ExternalConnectionsDoesNotConfiguredForStateMachineInterpreter) { Owner = _owner };

	public CommunicationException CommunicationError(Exception innerException, SendId? sendId = null) => new(innerException, sendId) { Owner = _owner };

	public PlatformException PlatformError(string message) => new(message) { Owner = _owner };
	
	public PlatformException PlatformError(Exception innerException) => new(innerException) { Owner = _owner };

	public StateMachineDestroyedException LiveLockError() => new(Resources.Exception_LivelockDetected) { Reason = DestroyReason.LiveLock, Owner = _owner };

	public StateMachineDestroyedException QueueClosedError() => new(Resources.Exception_StateMachineExternalQueueHasBeenClosed) { Reason = DestroyReason.QueueClosed, Owner = _owner };

	public StateMachineDestroyedException DestroySignalError(Exception? innerException) =>
		new(Resources.Exception_StateMachineHasBeenDestroyed, innerException) { Reason = DestroyReason.DestroySignal, Owner = _owner };

	[UsedImplicitly]
	private class Owner;
}