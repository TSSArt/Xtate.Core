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

public abstract class AsyncInit<T>
{
	private T? _value;

	public abstract Task Task { get; }

	public T Value => Task.Status == TaskStatus.RanToCompletion ? _value! : throw new InvalidOperationException(Resources.ErrorMessage_Not_initialized);

	protected void SetValue(T value) => _value = value;
}

public static class AsyncInit
{
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
	public static AsyncInit<T> Run<T, TArg>(TArg arg, Func<TArg, ValueTask<T>> init) => new InitAfter<T, TArg>(arg, init);

	private sealed class InitAfter<T, TArg>(TArg arg, Func<TArg, ValueTask<T>> func) : AsyncInit<T>
	{
		private Task? _task;

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