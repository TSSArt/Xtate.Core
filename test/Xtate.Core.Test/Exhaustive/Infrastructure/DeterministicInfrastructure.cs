// Copyright © 2019-2026 Sergii Artemenko
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

using System.Threading;

namespace Xtate.Core.Test.Exhaustive.Infrastructure;

internal sealed class OperationWatchdog(int maximumOperations)
{
	public int Operations { get; private set; }

	public void Tick()
	{
		if (maximumOperations < 1) throw new ArgumentOutOfRangeException(nameof(maximumOperations));

		var count = checked(++Operations);

		if (count > maximumOperations) throw new TimeoutException($"Deterministic operation budget of {maximumOperations} was exceeded.");
	}
}

internal sealed class OrderedTrace
{
	private readonly List<Entry> _entries = [];

	private long _sequence;

	public IReadOnlyList<Entry> Entries => _entries;

	public void Record(string category, string value)
	{
		if (string.IsNullOrEmpty(category)) throw new ArgumentException(message: @"A category is required.", nameof(category));
		if (value is null) throw new ArgumentNullException(nameof(value));

		_entries.Add(new Entry(_sequence++, category, value));
	}

	public string[] Snapshot() => [.. _entries.Select(static entry => $"{entry.Sequence}:{entry.Category}:{entry.Value}")];

	internal sealed record Entry(long Sequence, string Category, string Value);
}

/// <summary>Small deterministic primitives shared by the exhaustive suite.</summary>
internal sealed class VirtualScheduler : IDisposable
{
	private readonly List<ScheduledWork> _pending = [];

	private bool _disposed;

	private long _sequence;

	private long NowMilliseconds { get; set; }

	public int PendingCount => _pending.Count;

#region Interface IDisposable

	public void Dispose()
	{
		_disposed = true;

		foreach (var work in _pending)
		{
			work.Dispose();
		}

		_pending.Clear();
	}

#endregion

	public IDisposable Schedule(long delayMilliseconds, Action action)
	{
		if (delayMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(delayMilliseconds));
		if (_disposed) throw new ObjectDisposedException(nameof(VirtualScheduler));
		if (action is null) throw new ArgumentNullException(nameof(action));

		var work = new ScheduledWork(action, checked(NowMilliseconds + delayMilliseconds), _sequence++);
		_pending.Add(work);

		return work;
	}

	public void AdvanceBy(long milliseconds)
	{
		if (milliseconds < 0) throw new ArgumentOutOfRangeException(nameof(milliseconds));

		AdvanceTo(checked(NowMilliseconds + milliseconds));
	}

	public void AdvanceTo(long milliseconds)
	{
		if (milliseconds < 0) throw new ArgumentOutOfRangeException(nameof(milliseconds));
		if (milliseconds < NowMilliseconds) throw new ArgumentOutOfRangeException(nameof(milliseconds));

		while (TryTakeNextDueAtOrBefore(milliseconds, out var work))
		{
			NowMilliseconds = work.Due;
			work.Run();
		}

		NowMilliseconds = milliseconds;
	}

	private bool TryTakeNextDueAtOrBefore(long milliseconds, [NotNullWhen(true)] out ScheduledWork? result)
	{
		ScheduledWork? selected = null;

		foreach (var work in _pending)
		{
			if (work.Due > milliseconds || (selected is not null && (work.Due > selected.Due || (work.Due == selected.Due && work.Sequence > selected.Sequence))))
			{
				continue;
			}

			selected = work;
		}

		if (selected is null)
		{
			result = null;

			return false;
		}

		_pending.Remove(selected);
		result = selected;

		return true;
	}

	private sealed class ScheduledWork(Action action, long due, long sequence) : IDisposable
	{
		private Action? _action = action;

		public long Due { get; } = due;

		public long Sequence { get; } = sequence;

	#region Interface IDisposable

		public void Dispose() => Interlocked.Exchange(ref _action, value: null);

	#endregion

		public void Run() => Interlocked.Exchange(ref _action, value: null)?.Invoke();
	}
}

internal sealed class ResourceLedger : IDisposable
{
	private readonly ConcurrentDictionary<string, int> _live = new(StringComparer.Ordinal);

#region Interface IDisposable

	public void Dispose() => AssertEmpty();

#endregion

	public IDisposable Track(string kind)
	{
		if (string.IsNullOrEmpty(kind)) throw new ArgumentException(message: @"A resource kind is required.", nameof(kind));

		_live.AddOrUpdate(kind, addValue: 1, static (_, count) => checked(count + 1));

		return new Lease(this, kind);
	}

	public void AssertEmpty()
	{
		var leaks = _live.Where(static pair => pair.Value != 0).ToArray();

		if (leaks.Length != 0)
		{
			throw new InvalidOperationException("Leaked resources: " + string.Join(separator: ", ", leaks.Select(static pair => $"{pair.Key}={pair.Value}")));
		}
	}

	private void Release(string kind) => _live.AddOrUpdate(kind, addValue: 0, static (_, count) => count - 1);

	private sealed class Lease(ResourceLedger ledger, string kind) : IDisposable
	{
		private ResourceLedger? _ledger = ledger;

	#region Interface IDisposable

		public void Dispose() => Interlocked.Exchange(ref _ledger, value: null)?.Release(kind);

	#endregion
	}
}

internal static class WeakReferenceProbe
{
	public static WeakReference ObserveCollectedObject()
	{
		var reference = CreateReference();

		for (var attempt = 0; attempt < 4 && reference.IsAlive; attempt++)
		{
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
			GC.WaitForPendingFinalizers();
		}

		return reference;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference CreateReference() => new(new object());
}
