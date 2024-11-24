// Copyright © 2019-2024 Sergii Artemenko
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

public class TaskMonitor
{
	public required ILogger<TaskMonitor> Logger { protected get; [UsedImplicitly] init; }

	public void Run(Func<Task> factory) => _ = Logging<ValueTuple>(factory, argument: default);

	public void Run<TArg>(Func<TArg, Task> factory, TArg argument) => _ = Logging(factory, argument);

	public void Run<TResult>(Func<Task<TResult>> factory) => _ = Logging<TResult, ValueTuple>(factory, argument: default);

	public void Run<TResult, TArg>(Func<TArg, Task<TResult>> factory, TArg argument) => _ = Logging<TResult, TArg>(factory, argument);

	public void Run(Func<ValueTask> factory) => _ = Logging<ValueTuple>(factory, argument: default);

	public void Run<TArg>(Func<TArg, ValueTask> factory, TArg argument) => _ = Logging(factory, argument);

	public void Run<TResult>(Func<ValueTask<TResult>> factory) => _ = Logging<TResult, ValueTuple>(factory, argument: default);

	public void Run<TResult, TArg>(Func<TArg, ValueTask<TResult>> factory, TArg argument) => _ = Logging<TResult, TArg>(factory, argument);

	public Task RunAndWait(Func<Task> factory, CancellationToken token) =>
		token.CanBeCanceled ? token.IsCancellationRequested ? Task.FromCanceled(token) : Logging<ValueTuple>(factory, argument: default).WaitAsync(token) : factory();

	public Task RunAndWait<TArg>(Func<TArg, Task> factory, TArg argument, CancellationToken token) =>
		token.CanBeCanceled ? token.IsCancellationRequested ? Task.FromCanceled(token) : Logging(factory, argument).WaitAsync(token) : factory(argument);

	public Task<TResult> RunAndWait<TResult>(Func<Task<TResult>> factory, CancellationToken token) =>
		token.CanBeCanceled ? token.IsCancellationRequested ? Task.FromCanceled<TResult>(token) : Logging<TResult, ValueTuple>(factory, argument: default).WaitAsync(token) : factory();

	public Task<TResult> RunAndWait<TResult, TArg>(Func<TArg, Task<TResult>> factory, TArg argument, CancellationToken token) =>
		token.CanBeCanceled ? token.IsCancellationRequested ? Task.FromCanceled<TResult>(token) : Logging<TResult, TArg>(factory, argument).WaitAsync(token) : factory(argument);

	public ValueTask RunAndWait(Func<ValueTask> factory, CancellationToken token) =>
		token.CanBeCanceled ? new ValueTask(token.IsCancellationRequested ? Task.FromCanceled(token) : Logging<ValueTuple>(factory, argument: default).WaitAsync(token)) : factory();

	public ValueTask RunAndWait<TArg>(Func<TArg, ValueTask> factory, TArg argument, CancellationToken token) =>
		token.CanBeCanceled ? new ValueTask(token.IsCancellationRequested ? Task.FromCanceled(token) : Logging(factory, argument).WaitAsync(token)) : factory(argument);

	public ValueTask<TResult> RunAndWait<TResult>(Func<ValueTask<TResult>> factory, CancellationToken token) =>
		token.CanBeCanceled
			? new ValueTask<TResult>(token.IsCancellationRequested ? Task.FromCanceled<TResult>(token) : Logging<TResult, ValueTuple>(factory, argument: default).WaitAsync(token))
			: factory();

	public ValueTask<TResult> RunAndWait<TResult, TArg>(Func<TArg, ValueTask<TResult>> factory, TArg argument, CancellationToken token) =>
		token.CanBeCanceled
			? new ValueTask<TResult>(token.IsCancellationRequested ? Task.FromCanceled<TResult>(token) : Logging<TResult, TArg>(factory, argument).WaitAsync(token))
			: factory(argument);

	private async Task Logging<TArg>(Delegate factory, TArg argument)
	{
		try
		{
			switch (factory)
			{
				case Func<ValueTask> func:
					await func().ConfigureAwait(false);

					break;

				case Func<TArg, ValueTask> func:
					await func(argument).ConfigureAwait(false);

					break;

				case Func<Task> func:
					await func().ConfigureAwait(false);

					break;

				case Func<TArg, Task> func:
					await func(argument).ConfigureAwait(false);

					break;

				default:
					throw Infra.Unmatched(factory);
			}

			await TaskCompletedSuccessfully().ConfigureAwait(false);
		}
		catch (OperationCanceledException ex)
		{
			await TaskCancelled(ex).ConfigureAwait(false);

			throw;
		}
		catch (Exception ex)
		{
			await TaskFailed(ex).ConfigureAwait(false);

			throw;
		}
	}

	private async Task<TResult> Logging<TResult, TArg>(Delegate factory, TArg argument)
	{
		try
		{
			var result = factory switch
						 {
							 Func<Task<TResult>> func            => await func().ConfigureAwait(false),
							 Func<TArg, Task<TResult>> func      => await func(argument).ConfigureAwait(false),
							 Func<ValueTask<TResult>> func       => await func().ConfigureAwait(false),
							 Func<TArg, ValueTask<TResult>> func => await func(argument).ConfigureAwait(false),
							 _                                   => throw Infra.Unmatched(factory)
						 };

			await TaskCompletedSuccessfully().ConfigureAwait(false);

			return result;
		}
		catch (OperationCanceledException ex)
		{
			await TaskCancelled(ex).ConfigureAwait(false);

			throw;
		}
		catch (Exception ex)
		{
			await TaskFailed(ex).ConfigureAwait(false);

			throw;
		}
	}

	protected virtual ValueTask TaskCancelled(OperationCanceledException ex) => Logger.Write(Level.Warning, eventId: 1, Resources.Message_TaskWasCanceled, ex);

	protected virtual ValueTask TaskFailed(Exception ex) => Logger.Write(Level.Error, eventId: 2, Resources.Message_TaskFailed, ex);

	protected virtual ValueTask TaskCompletedSuccessfully() => default;
}