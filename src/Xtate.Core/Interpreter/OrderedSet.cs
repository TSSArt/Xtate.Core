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

public sealed class OrderedSet<T> : List<T>
{
	public delegate void ChangedHandler(ChangedAction action, T? item);

	public enum ChangedAction
	{
		Add,

		Clear,

		Delete
	}

	public bool IsEmpty => Count == 0;

	public event ChangedHandler? Changed;

	public void AddIfNotExists(T item)
	{
		if (!Contains(item))
		{
			base.Add(item);

			Changed?.Invoke(ChangedAction.Add, item);
		}
	}

	public new void Add(T item)
	{
		base.Add(item);

		Changed?.Invoke(ChangedAction.Add, item);
	}

	public new void Clear()
	{
		base.Clear();

		Changed?.Invoke(ChangedAction.Clear, item: default);
	}

	public bool IsMember(T item) => Contains(item);

	public void Delete(T item)
	{
		Remove(item);

		Changed?.Invoke(ChangedAction.Delete, item);
	}

	public List<T> ToSortedList(IComparer<T> comparer)
	{
		var list = new List<T>(this);
		list.Sort(comparer);

		return list;
	}

	public List<T> ToFilteredSortedList(Predicate<T> predicate, IComparer<T> comparer)
	{
		var list = FindAll(predicate);
		list.Sort(comparer);

		return list;
	}

	public List<T> ToFilteredList<TArg>(Func<T, TArg, bool> predicate, TArg arg)
	{
		Infra.Requires(predicate);

		var list = new List<T>();

		foreach (var item in this)
		{
			if (predicate(item, arg))
			{
				list.Add(item);
			}
		}

		return list;
	}
}