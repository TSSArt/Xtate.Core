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

public sealed class InvokeIdSet : IEnumerable<InvokeId>
{
    public delegate void ChangeHandler(ChangedAction action, InvokeId invokeId);

    public enum ChangedAction
    {
        Add,

        Remove
    }

    private readonly HashSet<UniqueInvokeId> _set = [];

    public int Count => _set.Count;

#region Interface IEnumerable

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion

#region Interface IEnumerable<InvokeId>

    public IEnumerator<InvokeId> GetEnumerator()
    {
        foreach (var uniqueInvokeId in _set)
        {
            yield return uniqueInvokeId.InvokeId;
        }
    }

#endregion

    public event ChangeHandler? Changed;

    public void Remove(InvokeId invokeId)
    {
        if (_set.Remove(invokeId.UniqueId))
        {
            Changed?.Invoke(ChangedAction.Remove, invokeId);
        }
    }

    public void Add(InvokeId invokeId)
    {
        if (_set.Add(invokeId.UniqueId))
        {
            Changed?.Invoke(ChangedAction.Add, invokeId);
        }
    }

    public bool Contains(InvokeId invokeId) => _set.Contains(invokeId.UniqueId);
}