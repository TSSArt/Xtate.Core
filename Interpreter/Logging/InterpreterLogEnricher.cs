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

public class InterpreterLogEnricher<TSource> : ILogEnricher<TSource>
{
	public required Func<ValueTask<IStateMachineSessionId>> StateMachineSessionIdFactory { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<IStateMachine>> StateMachineFactory { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<IStateMachineContext>> StateMachineContextFactory { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<IDataModelController>> DataModelControllerFactory { private get; [UsedImplicitly] init; }

	public required ILogger<TSource> Logger { private get; [UsedImplicitly] init; }

#region Interface ILogEnricher<TSource>

	public async IAsyncEnumerable<LoggingParameter> EnumerateProperties(Level level, int eventId)
	{
		var stateMachineSessionId = await StateMachineSessionIdFactory().ConfigureAwait(false);

		yield return new LoggingParameter(name: @"SessionId", stateMachineSessionId.SessionId);

		var stateMachine = await StateMachineFactory().ConfigureAwait(false);

		yield return new LoggingParameter(name: @"StateMachineName", stateMachine.Name);

		var stateMachineContext = await StateMachineContextFactory().ConfigureAwait(false);

		if (stateMachineContext.Configuration.Count > 0)
		{
			var activeStates = new DataModelList();
			foreach (var node in stateMachineContext.Configuration)
			{
				activeStates.Add(node.Id.Value);
			}

			activeStates.MakeDeepConstant();

			yield return new LoggingParameter(name: @"ActiveStates", activeStates);
		}

		if (Logger.IsEnabled(Level.Verbose))
		{
			var dataModelController = await DataModelControllerFactory().ConfigureAwait(false);

			yield return new LoggingParameter(name: @"DataModel", dataModelController.DataModel.AsConstant());
		}
	}

	public string Namespace => @"ctx";

#endregion
}