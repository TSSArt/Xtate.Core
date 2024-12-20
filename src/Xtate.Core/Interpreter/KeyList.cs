﻿// Copyright © 2019-2024 Sergii Artemenko
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

public sealed class KeyList<T> : IEnumerable<KeyValuePair<IEntity, List<T>>>
{
	public delegate void ChangeHandler(ChangedAction action, IEntity entity, List<T> list);

	public enum ChangedAction
	{
		Set
	}

	private readonly Dictionary<IEntity, List<T>> _dic = [];

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion

#region Interface IEnumerable<KeyValuePair<IEntity,List<T>>>

	IEnumerator<KeyValuePair<IEntity, List<T>>> IEnumerable<KeyValuePair<IEntity, List<T>>>.GetEnumerator() => GetEnumerator();

#endregion

	public event ChangeHandler? Changed;

	public void Set(IEntity entity, List<T> list)
	{
		_dic[entity] = list;

		Changed?.Invoke(ChangedAction.Set, entity, list);
	}

	public bool TryGetValue(IEntity entity, out List<T> list) => _dic.TryGetValue(entity, out list!);

	public Dictionary<IEntity, List<T>>.Enumerator GetEnumerator() => _dic.GetEnumerator();
}