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

using System;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel.Runtime;

public class RuntimeActionExecutor : IExecutableEntity, IExecEvaluator
{
	public required RuntimeAction Action { private get; init; }

	public required Func<ValueTask<RuntimeExecutionContext>> RuntimeExecutionContextFactory { private get; init; }

	public async ValueTask Execute()
	{
		var executionContext = await RuntimeExecutionContextFactory().ConfigureAwait(false);

		Xtate.Runtime.SetCurrentExecutionContext(executionContext);

		await Action.DoAction().ConfigureAwait(false);
	}
}

public abstract class RuntimeAction : IExecutableEntity
{
	public static RuntimeAction GetAction(Action action)
	{
		Infra.Requires(action);

		return new ActionSync(action);
	}

	public static RuntimeAction GetAction(Func<ValueTask> action)
	{
		Infra.Requires(action);

		return new ActionAsync(action);
	}

	public abstract ValueTask DoAction();

	private sealed class ActionSync : RuntimeAction
	{
		private readonly Action _action;
		
		public ActionSync(Action action) => _action = action;

		public override ValueTask DoAction()
		{
			_action();

			return default;
		}
	}

	private sealed class ActionAsync : RuntimeAction
	{
		private readonly Func<ValueTask> _action;
		
		public ActionAsync(Func<ValueTask> action) => _action = action;

		public override ValueTask DoAction() => _action();
	}
}