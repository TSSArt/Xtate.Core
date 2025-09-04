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

public class StateMachineStatus : IStateMachineStatus, INotifyStateChanged
{
    private readonly TaskCompletionSource _acceptedTcs = new();

#region Interface INotifyStateChanged

    public virtual ValueTask OnChanged(StateMachineInterpreterState state)
    {
        CurrentState = state;

        if (state == StateMachineInterpreterState.Accepted)
        {
            _acceptedTcs.TrySetResult();
        }

        return default;
    }

#endregion

#region Interface IStateMachineStatus

    public Task WhenAccepted() => _acceptedTcs.Task;

    public void Completed()
    {
        _acceptedTcs.TrySetResult();
    }

    public void Failed(Exception exception)
    {
        _acceptedTcs.TrySetException(exception);
    }

    public void Cancelled(CancellationToken token)
    {
        _acceptedTcs.TrySetCanceled(token);
    }

    public StateMachineInterpreterState CurrentState { get; private set; } = StateMachineInterpreterState.Initializing;

#endregion
}