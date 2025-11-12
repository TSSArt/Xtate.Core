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

namespace Xtate;

public static class Disposer
{
    public static bool IsDisposable<T>([NotNullWhen(true)] T instance) => instance is IDisposable or IAsyncDisposable;

	public static void Dispose<T>(T instance)
    {
        if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public static ValueTask DisposeAsync<T>(T instance)
    {
        switch (instance)
        {
            case IAsyncDisposable asyncDisposable:
                return asyncDisposable.DisposeAsync();

            case IDisposable disposable:
                disposable.Dispose();

                return ValueTask.CompletedTask;

            default:
                return ValueTask.CompletedTask;
        }
    }
}