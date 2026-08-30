using System.Collections.Concurrent;
using System.Threading;

namespace Xtate.Core.Test.Exhaustive.Infrastructure;

internal sealed class OperationWatchdog(int maximumOperations)
{
	private int _operations;

	public int Operations => _operations;

	public void Tick()
	{
		if (maximumOperations < 1) throw new ArgumentOutOfRangeException(nameof(maximumOperations));
		var count = checked(++_operations);
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
		if (string.IsNullOrEmpty(category)) throw new ArgumentException("A category is required.", nameof(category));
		if (value is null) throw new ArgumentNullException(nameof(value));
		_entries.Add(new Entry(_sequence++, category, value));
	}

	public string[] Snapshot() => _entries.Select(static entry => $"{entry.Sequence}:{entry.Category}:{entry.Value}").ToArray();

	internal sealed record Entry(long Sequence, string Category, string Value);
}

/// <summary>Small deterministic primitives shared by the exhaustive suite.</summary>
internal sealed class VirtualScheduler : IDisposable
{
	private readonly List<ScheduledWork> _pending = [];
	private long _sequence;
	private bool _disposed;

	public long NowMilliseconds { get; private set; }

	public int PendingCount => _pending.Count;

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

	public void Dispose()
	{
		_disposed = true;
		foreach (var work in _pending) work.Dispose();
		_pending.Clear();
	}

	private bool TryTakeNextDueAtOrBefore(long milliseconds, [NotNullWhen(true)] out ScheduledWork? result)
	{
		ScheduledWork? selected = null;

		foreach (var work in _pending)
		{
			if (work.Due > milliseconds || selected is not null && (work.Due > selected.Due || work.Due == selected.Due && work.Sequence > selected.Sequence))
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
		public void Dispose() => Interlocked.Exchange(ref _action, null);
		public void Run() => Interlocked.Exchange(ref _action, null)?.Invoke();
	}
}

internal sealed class ResourceLedger : IDisposable
{
	private readonly ConcurrentDictionary<string, int> _live = new(StringComparer.Ordinal);

	public IDisposable Track(string kind)
	{
		if (string.IsNullOrEmpty(kind)) throw new ArgumentException("A resource kind is required.", nameof(kind));
		_live.AddOrUpdate(kind, 1, static (_, count) => checked(count + 1));
		return new Lease(this, kind);
	}

	public void AssertEmpty()
	{
		var leaks = _live.Where(static pair => pair.Value != 0).ToArray();
		if (leaks.Length != 0)
		{
			throw new InvalidOperationException("Leaked resources: " + string.Join(", ", leaks.Select(static pair => $"{pair.Key}={pair.Value}")));
		}
	}

	public void Dispose() => AssertEmpty();

	private void Release(string kind) => _live.AddOrUpdate(kind, 0, static (_, count) => count - 1);

	private sealed class Lease(ResourceLedger ledger, string kind) : IDisposable
	{
		private ResourceLedger? _ledger = ledger;
		public void Dispose() => Interlocked.Exchange(ref _ledger, null)?.Release(kind);
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
