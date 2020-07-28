#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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

using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.DataModel;

namespace Xtate
{
	public delegate void ExecutableAction(IExecutionContext executionContext);

	public delegate ValueTask ExecutableTask(IExecutionContext executionContext);

	public delegate ValueTask ExecutableCancellableTask(IExecutionContext executionContext, CancellationToken token);

	public class RuntimeAction : IExecutableEntity, IExecEvaluator
	{
		private readonly object _action;

		public RuntimeAction(ExecutableAction action) => _action = action ?? throw new ArgumentNullException(nameof(action));

		public RuntimeAction(ExecutableTask task) => _action = task ?? throw new ArgumentNullException(nameof(task));

		public RuntimeAction(ExecutableCancellableTask task) => _action = task ?? throw new ArgumentNullException(nameof(task));

	#region Interface IExecEvaluator

		public async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			switch (_action)
			{
				case ExecutableAction action:
					action(executionContext);
					break;

				case ExecutableTask task:
					await task(executionContext).ConfigureAwait(false);
					break;

				case ExecutableCancellableTask task:
					await task(executionContext, token).ConfigureAwait(false);
					break;
			}
		}

	#endregion
	}
}