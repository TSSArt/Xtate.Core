using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	internal class CacheEntry<T>
	{
		private readonly T?             _value1;
		private readonly WeakReference? _weakReference;
		private          int            _refCount;

		public CacheEntry([DisallowNull] T value, ValueOptions options)
		{
			if ((options & ValueOptions.WeakRef) != 0)
			{
				_weakReference = new WeakReference(value);
			}
			else
			{
				_value1 = value;
			}

			Options = options;
			_refCount = 1;
		}

		public ValueOptions Options { get; }

		public bool TryGetValue([NotNullWhen(true)] out T? value)
		{
			if (_value1 is not null)
			{
				value = _value1;

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
			if ((Options & ValueOptions.ThreadSafe) == 0)
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
				var val = _refCount;

				if (val == 0)
				{
					return false;
				}

				if (Interlocked.CompareExchange(ref _refCount, val + 1, val) == val)
				{
					return true;
				}
			}
		}

		public async ValueTask<bool> RemoveReference()
		{
			if ((Options & ValueOptions.ThreadSafe) == 0)
			{
				Infrastructure.Assert(_refCount > 0);

				if (-- _refCount > 0)
				{
					return false;
				}
			}
			else
			{
				while (true)
				{
					var val = _refCount;

					Infrastructure.Assert(val > 0);

					if (Interlocked.CompareExchange(ref _refCount, val - 1, val) == val)
					{
						if (val > 1)
						{
							return false;
						}

						break;
					}
				}
			}

			if ((Options & ValueOptions.Dispose) != 0 && TryGetValue(out var value))
			{
				switch (value)
				{
					case IAsyncDisposable asyncDisposable:
						await asyncDisposable.DisposeAsync().ConfigureAwait(false);
						break;

					case IDisposable disposable:
						disposable.Dispose();
						break;
				}
			}

			return true;
		}
	}
}