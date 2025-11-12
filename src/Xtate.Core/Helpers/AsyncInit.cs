// Copyright © 2019-2025 Sergii Artemenko
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

public abstract class AsyncInit<T> : AsyncInit
{
    private T? _value;

    public T Value => Task.Status == TaskStatus.RanToCompletion ? _value! : throw new InvalidOperationException(Resources.ErrorMessage_Not_initialized);

    protected void SetValue(T value) => _value = value;
}

public abstract class AsyncInit
{
    public abstract Task Task { get; }

    public AsyncInit Then(AsyncInit asyncInit) => new Wrapper(this, asyncInit);

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

    /// <summary>
    ///     Runs delegate
    ///     <param name="init">init</param>
    ///     after completing constructors and setting up required fields and properties.
    /// </summary>
    /// <param name="init">Initialization action</param>
    /// <returns></returns>
    public static AsyncInit Run(Func<ValueTask> init) => new InitAfter(init);

    private sealed class InitAfter<T, TArg>(TArg arg, Func<TArg, ValueTask<T>> func) : AsyncInit<T>
    {
        public override Task Task
        {
            get
            {
                if (field is { } task)
                {
                    return task;
                }

                lock (this)
                {
                    return field ??= Init();
                }
            }
        }

        private async Task Init() => SetValue(await func(arg).ConfigureAwait(false));
    }

    private sealed class InitAfter(Func<ValueTask> func) : AsyncInit
    {
        public override Task Task
        {
            get
            {
                if (field is { } task)
                {
                    return task;
                }

                lock (this)
                {
                    return field ??= Init();
                }
            }
        }

        private Task Init() => func().AsTask();
    }

    private sealed class Wrapper(AsyncInit asyncInit1, AsyncInit asyncInit2) : AsyncInit
    {
        public override Task Task
        {
            get
            {
                if (field is { } task)
                {
                    return task;
                }

                lock (this)
                {
                    return field ??= Init();
                }
            }
        }

        private async Task Init()
        {
            await asyncInit1.Task.ConfigureAwait(false);
            await asyncInit2.Task.ConfigureAwait(false);
        }
    }
}