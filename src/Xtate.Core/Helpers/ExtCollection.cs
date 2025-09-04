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

public class ExtCollection<TValue1, TValue2>(IEqualityComparer<TValue1>? comparer1 = default, IEqualityComparer<TValue2>? comparer2 = default)
    : IReadOnlyCollection<(TValue1, TValue2)> where TValue1 : notnull where TValue2 : class
{
    private readonly IEqualityComparer<TValue1> _comparer1 = comparer1 ?? EqualityComparer<TValue1>.Default;

    private readonly IEqualityComparer<TValue2> _comparer2 = comparer2 ?? EqualityComparer<TValue2>.Default;

    private int _count;

    private ConcurrentDictionary<TValue1, object>? _dictionary;

#region Interface IEnumerable

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion

#region Interface IEnumerable<(TValue1, TValue2)>

    public IEnumerator<(TValue1, TValue2)> GetEnumerator()
    {
        if (_dictionary is not { } dictionary)
        {
            yield break;
        }

        foreach (var (key, value) in dictionary)
        {
            if (value is ImmutableList<TValue2> list)
            {
                foreach (var value2 in list)
                {
                    yield return (key, value2);
                }
            }
            else
            {
                yield return (key, (TValue2)value);
            }
        }
    }

#endregion

#region Interface IReadOnlyCollection<(TValue1, TValue2)>

    public int Count => _count;

#endregion

    private ConcurrentDictionary<TValue1, object> GetDictionary()
    {
        if (_dictionary is { } dictionary)
        {
            return dictionary;
        }

        dictionary = new ConcurrentDictionary<TValue1, object>(_comparer1);

        return Interlocked.CompareExchange(ref _dictionary, dictionary, comparand: default) ?? dictionary;
    }

    public bool TryRemoveGroup(TValue1 value, [MaybeNullWhen(false)] out IEnumerable<TValue2> values)
    {
        if (_dictionary is { } dictionary)
        {
            if (dictionary.TryRemove(value, out var obj))
            {
                if (obj is ImmutableList<TValue2> list)
                {
                    values = list;

                    Interlocked.Add(ref _count, -list.Count);

                    return true;
                }

                values = Enumerable.Repeat((TValue2)obj, count: 1);

                Interlocked.Decrement(ref _count);

                return true;
            }
        }

        values = default;

        return false;
    }

    public bool TryTake([MaybeNullWhen(false)] out TValue1 value1, [MaybeNullWhen(false)] out TValue2 value2)
    {
        while (_dictionary is { IsEmpty: false } dictionary)
        {
            foreach (var (key, value) in dictionary)
            {
                value1 = key;

                if (TryUpdateOrRemove(out value2))
                {
                    Interlocked.Decrement(ref _count);

                    return true;
                }

                continue;

                bool TryUpdateOrRemove(out TValue2 value2)
                {
                    if (value is ImmutableList<TValue2> list)
                    {
                        value2 = list[0];

                        var newList = list.RemoveAt(0);

                        return dictionary.TryUpdate(key, newList.Count > 1 ? newList : newList[0], list);
                    }

                    value2 = (TValue2)value;

                    return dictionary.TryRemove(new KeyValuePair<TValue1, object>(key, value2));
                }
            }
        }

        value1 = default;
        value2 = default;

        return false;
    }

    public void Add(TValue1 value1, TValue2 value2)
    {
        GetDictionary()
            .AddOrUpdate(
                value1,
                static (_, value) => value,
                static (_, obj, value) => obj is ImmutableList<TValue2> list ? list.Add(value) : [(TValue2)obj, value],
                value2);

        Interlocked.Increment(ref _count);
    }

    public bool Remove(TValue1 value1, TValue2 value2)
    {
        if (_dictionary is not { } dictionary)
        {
            return false;
        }

        while (dictionary.TryGetValue(value1, out var obj))
        {
            if (obj is ImmutableList<TValue2> list)
            {
                var newList = list.Remove(value2, _comparer2);

                if (dictionary.TryUpdate(value1, newList.Count > 1 ? newList : newList[0], list))
                {
                    Interlocked.Decrement(ref _count);

                    return true;
                }
            }
            else
            {
                if (_comparer2.Equals((TValue2)obj, value2))
                {
                    if (dictionary.TryRemove(new KeyValuePair<TValue1, object>(value1, obj)))
                    {
                        Interlocked.Decrement(ref _count);

                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        return false;
    }
}