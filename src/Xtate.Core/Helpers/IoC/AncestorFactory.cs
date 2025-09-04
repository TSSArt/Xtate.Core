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

using Xtate.IoC;

namespace Xtate.Core;

public class AncestorFactory<T> : IAsyncInitialization
{
    private ValueTask<T> _task;

    public AncestorFactory(AncestorTracker tracker, Func<ValueTask<T>> factory) => _task = tracker.TryCaptureAncestor(typeof(T), this) ? default : factory().Preserve();

#region Interface IAsyncInitialization

    Task IAsyncInitialization.Initialization => _task.IsCompletedSuccessfully ? Task.CompletedTask : _task.AsTask();

#endregion

    [UsedImplicitly]
    public Ancestor<T> GetValueFunc() => GetValue;

    private T GetValue() => _task.Result ?? throw MissedServiceException.Create<T>();

    internal void SetValue(T? instance)
    {
        if (instance is null)
        {
            throw MissedServiceException.Create<T>();
        }

        _task = new ValueTask<T>(instance);
    }
}