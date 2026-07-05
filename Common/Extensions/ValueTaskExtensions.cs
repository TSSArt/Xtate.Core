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

using Xtate.TaskMonitor;

namespace Xtate;

internal static class ValueTaskExtensions
{
	extension(ValueTask valueTask)
	{
		public void Forget() => _ = valueTask.Preserve();

		public void Forget(ITaskMonitor taskMonitor) => taskMonitor.Forget(valueTask);

		public ValueTask WaitAsync(CancellationToken token) => valueTask.IsCompleted || !token.CanBeCanceled ? valueTask : new ValueTask(valueTask.AsTask().WaitAsync(token));

		public ValueTask WaitAsync(ITaskMonitor taskMonitor, CancellationToken token) => taskMonitor.WaitAsync(valueTask, token);
	}

	extension<TResult>(ValueTask<TResult> valueTask)
	{
		public void Forget() => _ = valueTask.Preserve();

		public void Forget(ITaskMonitor taskMonitor) => taskMonitor.Forget(valueTask);

		public ValueTask<TResult> WaitAsync(CancellationToken token) => valueTask.IsCompleted || !token.CanBeCanceled ? valueTask : new ValueTask<TResult>(valueTask.AsTask().WaitAsync(token));

		public ValueTask<TResult> WaitAsync(ITaskMonitor taskMonitor, CancellationToken token) => taskMonitor.WaitAsync(valueTask, token);
	}
}
