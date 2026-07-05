// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.DataTypes;
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.Logging.Provider;
using Xtate.StateMachine;

namespace Xtate.Interpreter.Services;

public class InterpreterDebugLogEnricher<TSource> : ILogEnricher<TSource>
{
	public required Safe<IStateMachine> StateMachine { private get; [SetByIoC] init; }

	public required Safe<IStateMachineContext> StateMachineContext { private get; [SetByIoC] init; }

#region Interface ILogEnricher<TSource>

	public IEnumerable<LoggingParameter> EnumerateProperties()
	{
		if (StateMachine()?.Name is { Length: > 0 } name)
		{
			yield return new LoggingParameter(name: @"StateMachineName", name);
		}

		if (StateMachineContext()?.Configuration is { Count: > 0 } configuration)
		{
			var activeStates = new DataModelList();

			foreach (var node in configuration)
			{
				activeStates.Add(node.Id.Value);
			}

			activeStates.MakeDeepConstant();

			yield return new LoggingParameter(name: @"ActiveStates", activeStates);
		}
	}

	public string Namespace => @"ctx";

	public Level Level => Level.Debug;

#endregion
}