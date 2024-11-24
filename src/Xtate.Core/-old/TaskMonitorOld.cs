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

[Obsolete] //TODO:delete
public interface TaskMonitorOld
{
	[Obsolete]
	ValueTask RunAsync(ValueTask valueTask);

	[Obsolete]
	ValueTask RunAsync<T>(ValueTask<T> valueTask);

	[Obsolete]
	ValueTask RunAsync(Task task);

	void Run(Func<Task> factory);

	void Run<TArg>(Func<TArg, Task> factory, TArg argument);

	Task RunAndWait(Func<Task> factory, CancellationToken token);

	Task RunAndWait<TArg>(Func<TArg, Task> factory, TArg argument, CancellationToken token);

	Task<TResult> RunAndWait<TResult>(Func<Task<TResult>> factory, CancellationToken token);

	Task<TResult> RunAndWait<TResult, TArg>(Func<TArg, Task<TResult>> factory, TArg argument, CancellationToken token);

	ValueTask<TResult> RunAndWait<TResult>(Func<ValueTask<TResult>> factory, CancellationToken token);

	ValueTask<TResult> RunAndWait<TResult, TArg>(Func<TArg, ValueTask<TResult>> factory, TArg argument, CancellationToken token);
}