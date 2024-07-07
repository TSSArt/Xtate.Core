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

namespace Xtate.Core;

public sealed class ServiceIdSet : IEnumerable<ServiceId>
{
	public delegate void ChangeHandler(ChangedAction action, ServiceId serviceId);

	public enum ChangedAction
	{
		Add,
		Remove
	}

	private readonly HashSet<ServiceId> _set = [];

	public int Count => _set.Count;

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion

#region Interface IEnumerable<ServiceId>

	IEnumerator<ServiceId> IEnumerable<ServiceId>.GetEnumerator() => GetEnumerator();

#endregion

	public event ChangeHandler? Changed;

	public void Remove(ServiceId serviceId)
	{
		if (_set.Remove(serviceId))
		{
			Changed?.Invoke(ChangedAction.Remove, serviceId);
		}
	}

	public void Add(ServiceId serviceId)
	{
		if (_set.Add(serviceId))
		{
			Changed?.Invoke(ChangedAction.Add, serviceId);
		}
	}

	public bool Contains(ServiceId serviceId) => _set.Contains(serviceId);

	public HashSet<ServiceId>.Enumerator GetEnumerator() => _set.GetEnumerator();
}