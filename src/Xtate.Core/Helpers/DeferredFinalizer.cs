#region Copyright © 2019-2021 Sergii Artemenko

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

using System;
using System.Collections;
using System.Threading.Tasks;

namespace Xtate.Core
{
	/// <summary>
	///     Class makes sure that added objects/delegates will be disposed/called
	///     when <see cref="DeferredFinalizer" /> disposed (in case if <see cref="DefferFinalization()" /> wasn't called before
	///     disposing of <see cref="DeferredFinalizer" />) or at the moment when <see cref="ExecuteDeferredFinalization()" />
	///     called
	///     (in case if <see cref="DefferFinalization()" /> was called before disposing of <see cref="DeferredFinalizer" />)
	/// </summary>
	[PublicAPI]
	public sealed class DeferredFinalizer : IAsyncDisposable, IEnumerable
	{
		private const int EmbeddedCount = 4;

		private readonly DeferredFinalizer? _previous;

		private object?[]? _array;
		private int        _count;
		private bool       _deferred;
		private object?    _f0;
		private object?    _f1;
		private object?    _f2;
		private object?    _f3;

		public DeferredFinalizer() { }

		public DeferredFinalizer(DeferredFinalizer previous) => _previous = previous;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync() => !_deferred ? DisposeRecursive(0) : default;

	#endregion

	#region Interface IEnumerable

		public IEnumerator GetEnumerator() => throw new NotSupportedException();

	#endregion

		public async ValueTask ExecuteDeferredFinalization()
		{
			if (!_deferred)
			{
				return;
			}

			try
			{
				if (_previous is not null)
				{
					await _previous.ExecuteDeferredFinalization().ConfigureAwait(false);
				}
			}
			finally
			{
				await DisposeRecursive(0).ConfigureAwait(false);
			}
		}

		private async ValueTask DisposeRecursive(int index)
		{
			if (index >= _count)
			{
				return;
			}

			switch (ElementAt(index))
			{
				case null:
					break;

				case IAsyncDisposable asyncDisposable:
					try
					{
						await DisposeRecursive(index + 1).ConfigureAwait(false);
					}
					finally
					{
						ElementAt(index) = default;
						await asyncDisposable.DisposeAsync().ConfigureAwait(false);
					}

					break;

				case Func<ValueTask> func:
					try
					{
						await DisposeRecursive(index + 1).ConfigureAwait(false);
					}
					finally
					{
						ElementAt(index) = default;
						await func().ConfigureAwait(false);
					}

					break;

				case Func<object, ValueTask> func:
					try
					{
						await DisposeRecursive(index + 2).ConfigureAwait(false);
					}
					finally
					{
						var arg = ElementAt(index + 1);
						ElementAt(index) = ElementAt(index + 1) = default;
						await func(arg!).ConfigureAwait(false);
					}

					break;

				case Func<object, object, ValueTask> func:
					try
					{
						await DisposeRecursive(index + 3).ConfigureAwait(false);
					}
					finally
					{
						var arg1 = ElementAt(index + 1);
						var arg2 = ElementAt(index + 2);
						ElementAt(index) = ElementAt(index + 1) = ElementAt(index + 2) = default;
						await func(arg1!, arg2!).ConfigureAwait(false);
					}

					break;
			}

			_count = index;
		}

		public void Add(IAsyncDisposable asyncDisposable) => AddInternal(asyncDisposable);

		public void Add(Func<ValueTask> asyncAction) => AddInternal(asyncAction);

		public void Add(Func<object, ValueTask> asyncAction, object arg)
		{
			AddInternal(asyncAction);
			AddInternal(arg);
		}

		public void Add(Func<object, object, ValueTask> asyncAction, object arg1, object arg2)
		{
			AddInternal(asyncAction);
			AddInternal(arg1);
			AddInternal(arg2);
		}

		private void AddInternal(object obj)
		{
			if (obj is null) throw new ArgumentNullException(nameof(obj));

			ElementAt(_count ++) = obj;
		}

		private ref object? ElementAt(int index)
		{
			switch (index)
			{
				case 0: return ref _f0;
				case 1: return ref _f1;
				case 2: return ref _f2;
				case 3: return ref _f3;
				default:
#if DEBUG
					Infrastructure.Fail(@"Read DeferredFinalizer.ElementAt() source code.");
#endif

					// Code below used in Release mode to not allow DeferredFinalizer to fail if not enough space fields [_f#] is allocated.
					// If code fails here try to increase number of [_f#] fields.

					_array ??= new object?[4];

					var arrayIndex = index - EmbeddedCount;
					if (arrayIndex >= _array.Length)
					{
						var newArray = new object?[_array.Length * 2];
						Array.Copy(_array, newArray, _array.Length);
						_array = newArray;
					}

					return ref _array[arrayIndex];
			}
		}

		public void DefferFinalization()
		{
			for (var finalizer = this; finalizer is not null; finalizer = finalizer._previous)
			{
				finalizer._deferred = true;
			}
		}
	}
}