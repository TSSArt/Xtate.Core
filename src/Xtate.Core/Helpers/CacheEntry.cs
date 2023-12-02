#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

namespace Xtate.Core;

internal class CacheEntry<T>
{
	private readonly T?             _value;
	private readonly WeakReference? _weakReference;

	private int _refCount;

	public CacheEntry([DisallowNull] T value, ValueOptions options)
	{
		Options = options;
		_refCount = 1;

		if (IsWeakReference)
		{
			_weakReference = new WeakReference(value);
		}
		else
		{
			_value = value;
		}
	}

	public ValueOptions Options { get; }

	private bool IsWeakReference => (Options & ValueOptions.WeakRef) != 0;

	private bool DisposeRequired => (Options & ValueOptions.Dispose) != 0;

	private bool IsThreadSafe => (Options & ValueOptions.ThreadSafe) != 0;

	public bool TryGetValue([NotNullWhen(true)] out T? value)
	{
		if (_value is not null)
		{
			value = _value;

			return true;
		}

		if (_weakReference is { Target: T target })
		{
			value = target;

			return true;
		}

		value = default;

		return false;
	}

	public bool AddReference()
	{
		if (!IsThreadSafe)
		{
			if (_refCount == 0)
			{
				return false;
			}

			++ _refCount;

			return true;
		}

		while (true)
		{
			var refCount = _refCount;

			if (refCount == 0)
			{
				return false;
			}

			if (Interlocked.CompareExchange(ref _refCount, refCount + 1, refCount) == refCount)
			{
				return true;
			}
		}
	}

	public async ValueTask<bool> RemoveReference()
	{
		if (!IsThreadSafe)
		{
			Infra.Assert(_refCount > 0);

			if (-- _refCount > 0)
			{
				return false;
			}
		}
		else
		{
			while (true)
			{
				var refCount = _refCount;

				Infra.Assert(refCount > 0);

				if (Interlocked.CompareExchange(ref _refCount, refCount - 1, refCount) == refCount)
				{
					if (refCount > 1)
					{
						return false;
					}

					break;
				}
			}
		}

		if (DisposeRequired && TryGetValue(out var value))
		{
			await Disposer.DisposeAsync(value).ConfigureAwait(false);
		}

		return true;
	}
}