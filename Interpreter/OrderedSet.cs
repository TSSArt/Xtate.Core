using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	internal sealed class OrderedSet<T> : List<T>
	{
		public delegate void ChangedHandler(ChangedAction action, [AllowNull] T item);

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

		public void Union(List<T> orderedSet)
		{
			if (orderedSet == null) throw new ArgumentNullException(nameof(orderedSet));

			foreach (var item in orderedSet)
			{
				AddIfNotExists(item);
			}
		}

		public bool HasIntersection(List<T> orderedSet)
		{
			if (orderedSet == null) throw new ArgumentNullException(nameof(orderedSet));

			foreach (var item in orderedSet)
			{
				if (Contains(item))
				{
					return true;
				}
			}

			return false;
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

		public List<T> ToFilteredList(Predicate<T> predicate) => FindAll(predicate);
	}
}