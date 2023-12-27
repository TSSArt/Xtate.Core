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

public abstract class AsyncInit<T>
{
	private T? _value;

	public abstract Task Task { get; }

	public T Value => Task.Status == TaskStatus.RanToCompletion ? _value! : throw new InfrastructureException(Resources.ErrorMessage_Not_initialized);

	protected void SetValue(T value) => _value = value;
}

public static class AsyncInit
{
	/// <summary>
	///     Runs delegate <paramref name="init" /> immediately. If no asynchronous operations in <paramref name="init" /> after
	///     exit object is completely initialized.
	///     Caution: while executing <paramref name="init" /> delegate object cam be partially initialized. Consider method
	///     <see cref="RunAfter{T,TArg}(TArg, Func{TArg,ValueTask{T}})" /> to make sure object fully initialized including
	///     required properties.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="init">Initialization action</param>
	/// <returns></returns>
	public static AsyncInit<T> RunNow<T>(Func<ValueTask<T>> init) => new InitNow<T>(init);

	/// <summary>
	///     Runs delegate <paramref name="init" /> immediately. If no asynchronous operations in <paramref name="init" /> after
	///     exit object is completely initialized.
	///     Caution: while executing <paramref name="init" /> delegate object cam be partially initialized. Consider method
	///     <see cref="RunAfter{T,TArg}(TArg, Func{TArg,ValueTask{T}})" /> to make sure object fully initialized including
	///     required properties.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TArg"></typeparam>
	/// <param name="arg">Argument</param>
	/// <param name="init">Initialization action</param>
	/// <returns></returns>
	public static AsyncInit<T> RunNow<T, TArg>(TArg arg, Func<TArg, ValueTask<T>> init) => new InitNow<T, TArg>(arg, init);

	/// <summary>
	///     Runs delegate
	///     <param name="init">init</param>
	///     after completing constructors and setting up required fields and properties.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TArg"></typeparam>
	/// <param name="arg">Argument</param>
	/// <param name="init">Initialization action</param>
	/// <returns></returns>
	public static AsyncInit<T> RunAfter<T, TArg>(TArg arg, Func<TArg, ValueTask<T>> init) => new InitAfter<T, TArg>(arg, init);

	private sealed class InitNow<T> : AsyncInit<T>
	{
		public InitNow(Func<ValueTask<T>> func)
		{
			Infra.Requires(func);

			Task = Init(func);
		}

		public override Task Task { get; }

		private async Task Init(Func<ValueTask<T>> func) => SetValue(await func().ConfigureAwait(false));
	}

	private sealed class InitNow<T, TArg> : AsyncInit<T>
	{
		public InitNow(TArg arg, Func<TArg, ValueTask<T>> func)
		{
			Infra.Requires(func);

			Task = Init(arg, func);
		}

		public override Task Task { get; }

		private async Task Init(TArg arg, Func<TArg, ValueTask<T>> func) => SetValue(await func(arg).ConfigureAwait(false));
	}

	private sealed class InitAfter<T, TArg>(TArg arg, Func<TArg, ValueTask<T>> func) : AsyncInit<T>
	{
		private Task?                    _task;

		public override Task Task
		{
			get
			{
				if (_task is { } task)
				{
					return task;
				}

				lock (this)
				{
					return _task ??= Init();
				}
			}
		}

		private async Task Init() => SetValue(await func(arg).ConfigureAwait(false));
	}
}