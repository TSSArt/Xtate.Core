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

public class InterpreterInfoLogEnricher<TSource> : ILogEnricher<TSource>
{
	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

#region Interface ILogEnricher<TSource>

	public IEnumerable<LoggingParameter> EnumerateProperties()
	{
		yield return new LoggingParameter(name: @"SessionId", StateMachineSessionId.SessionId);
	}

	public string Namespace => @"ctx";

	public Level Level => Level.Info;

#endregion
}

public class InterpreterDebugLogEnricher<TSource> : ILogEnricher<TSource>
{
	public required IStateMachine StateMachine { private get; [UsedImplicitly] init; }

	public required IStateMachineContext StateMachineContext { private get; [UsedImplicitly] init; }

#region Interface ILogEnricher<TSource>

	public IEnumerable<LoggingParameter> EnumerateProperties()
	{
		if (!string.IsNullOrEmpty(StateMachine.Name))
		{
			yield return new LoggingParameter(name: @"StateMachineName", StateMachine.Name);
		}

		if (StateMachineContext.Configuration.Count > 0)
		{
			var activeStates = new DataModelList();

			foreach (var node in StateMachineContext.Configuration)
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

public class InterpreterVerboseLogEnricher<TSource> : ILogEnricher<TSource>
{
	public required IDataModelController DataModelController { private get; [UsedImplicitly] init; }

#region Interface ILogEnricher<TSource>

	public IEnumerable<LoggingParameter> EnumerateProperties()
	{
		yield return new LoggingParameter(name: @"DataModel", DataModelController.DataModel.AsConstant());
	}

	public string Namespace => @"ctx";

	public Level Level => Level.Verbose;

#endregion
}