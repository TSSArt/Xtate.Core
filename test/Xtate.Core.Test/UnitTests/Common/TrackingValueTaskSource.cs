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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Threading;
using System.Threading.Tasks.Sources;

namespace Xtate.Core.Test.UnitTests.Common;

internal sealed class TrackingValueTaskSource<T> : IValueTaskSource<T>, IValueTaskSource
{
	private ManualResetValueTaskSourceCore<T> _core;

	private int _consumptions;

	public int Consumptions => Volatile.Read(ref _consumptions);

	public int Registrations { get; private set; }

	public TaskCompletionSource Registered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

	public ValueTask ValueTask => new(this, _core.Version);

	public ValueTask<T> GenericValueTask => new(this, _core.Version);

	public void SetResult(T value) => _core.SetResult(value);

	public void SetException(Exception exception) => _core.SetException(exception);

	public T GetResult(short token)
	{
		Interlocked.Increment(ref _consumptions);

		return _core.GetResult(token);
	}

	void IValueTaskSource.GetResult(short token) => GetResult(token);

	public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);

	public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
	{
		Registrations++;
		_core.OnCompleted(continuation, state, token, flags);
		Registered.TrySetResult();
	}
}
