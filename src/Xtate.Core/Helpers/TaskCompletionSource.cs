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

#if !NET5_0_OR_GREATER

namespace System.Threading.Tasks
{
	internal class TaskCompletionSource : TaskCompletionSource<ValueTuple>
	{
		public TaskCompletionSource() { }

		public TaskCompletionSource(object? state) : base(state) { }

		public TaskCompletionSource(object? state, TaskCreationOptions creationOptions) : base(state, creationOptions) { }

		public TaskCompletionSource(TaskCreationOptions creationOptions) : base(creationOptions) { }

		public new Task Task => base.Task;

		public void SetResult() => SetResult(default);

		public bool TrySetResult() => TrySetResult(default);
	}
}

#endif