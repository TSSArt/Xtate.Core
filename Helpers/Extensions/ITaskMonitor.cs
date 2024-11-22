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

public interface ITaskMonitor
{
	/// <summary>
	///     Collects a ValueTask into a private pool and tracks its successful completion.
	/// </summary>
	/// <param name="valueTask">The ValueTask to collect.</param>
	ValueTask RunAsync(ValueTask valueTask); //TODO: delete1

	/// <summary>
	///     Collects a ValueTask of type T into a private pool and tracks its successful completion.
	/// </summary>
	/// <typeparam name="T">The type of the result produced by the ValueTask.</typeparam>
	/// <param name="valueTask">The ValueTask to collect.</param>
	ValueTask RunAsync<T>(ValueTask<T> valueTask);

	/// <summary>
	///     Collects a Task into a private pool and tracks its successful completion.
	/// </summary>
	/// <param name="task">The Task to collect.</param>
	ValueTask RunAsync(Task task);
}