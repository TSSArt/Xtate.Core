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

#if !NET6_0_OR_GREATER

using Xtate;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks;

internal static class TaskPolyfills
{
	extension(Task task)
	{
		public Task WaitAsync(CancellationToken token) =>
			task.IsCompleted || !token.CanBeCanceled
				? task
				: token.IsCancellationRequested
					? Task.FromCanceled(token)
					: new WaitCompletionSource<Empty>(task, token).Task;
	}

	extension<TResult>(Task<TResult> task)
	{
		public Task<TResult> WaitAsync(CancellationToken token) =>
			task.IsCompleted || !token.CanBeCanceled
				? task
				: token.IsCancellationRequested
					? Task.FromCanceled<TResult>(token)
					: new WaitCompletionSource<TResult>(task, token).Task;
	}

	private sealed class WaitCompletionSource<TResult> : TaskCompletionSource<TResult>
	{
		private readonly CancellationToken _token;

		private CancellationTokenRegistration _registration;

		private Task? _source;

		public WaitCompletionSource(Task source, CancellationToken token)
		{
			_source = source;
			_token = token;

			// A single delegate replaces the continuation Task and does not capture the caller's context.
			source.ConfigureAwait(false).GetAwaiter().UnsafeOnCompleted(Complete);

			// Older targets lack UnsafeRegister. Neither callback needs an ExecutionContext snapshot.
			if (ExecutionContext.IsFlowSuppressed())
			{
				RegisterCancellation(token);
			}
			else
			{
				using (ExecutionContext.SuppressFlow())
				{
					RegisterCancellation(token);
				}
			}

			// Either callback can finish before Register returns and the field is assigned.
			if (Task.IsCompleted)
			{
				Cleanup();
			}
		}

		private void Cleanup() => _registration.Dispose();

		private void RegisterCancellation(CancellationToken token) => _registration = token.Register(Callback, this, useSynchronizationContext: false);

		private static void Callback(object state) => ((WaitCompletionSource<TResult>)state).Cancel();

		private void Complete()
		{
			var source = Interlocked.Exchange(ref _source, value: null);

			if (source?.Status is not { } status)
			{
				return;
			}

			switch (status)
			{
				case TaskStatus.RanToCompletion:
					TrySetResult(source is Task<TResult> resultTask ? resultTask.GetAwaiter().GetResult() : default!);

					break;

				case TaskStatus.Canceled:
					try
					{
						source.GetAwaiter().GetResult();
					}
					catch (OperationCanceledException exception)
					{
						TrySetCanceled(exception.CancellationToken);
					}

					break;

				case TaskStatus.Faulted:
					TrySetException(source.Exception!.InnerExceptions);

					break;

				default: throw Infra.Unmatched(status);
			}

			Cleanup();
		}

		private void Cancel()
		{
			if (TrySetCanceled(_token))
			{
				// Release the antecedent even if it never completes after the wait is canceled.
				Interlocked.Exchange(ref _source, value: null);
			}

			Cleanup();
		}
	}
}

#endif
