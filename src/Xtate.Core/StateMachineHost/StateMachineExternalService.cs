﻿// Copyright © 2019-2024 Sergii Artemenko
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

using Xtate.ExternalService;

namespace Xtate.Core;

public class StateMachineExternalService : ExternalServiceBase, IEventDispatcher
{
	public class Provider() : ExternalServiceProviderBase<StateMachineExternalService>(Const.ScxmlServiceTypeId, Const.ScxmlServiceAliasTypeId);

	private readonly SessionId _sessionId = SessionId.New();

	public required IStateMachineScopeManager StateMachineScopeManager { private get; [UsedImplicitly] init; }

	public required IStateMachineLocation StateMachineLocation { private get; [UsedImplicitly] init; }
	
	public required IEventDispatcher EventDispatcher { private get; [UsedImplicitly] init; }

	public required TaskCollector TaskCollector { private get; [UsedImplicitly] init; }

#region Interface IEventDispatcher

	public ValueTask Dispatch(IIncomingEvent incomingEvent) => throw new NotImplementedException(); //TODO:

#endregion

	protected override ValueTask<DataModelValue> Execute()
	{
		var scxml = RawContent ?? Content.AsStringOrDefault();

		Infra.Assert(scxml is not null || Source is not null);

		var stateMachineClass = scxml is not null
			? (StateMachineClass) new ScxmlStringChildStateMachine(scxml)
								  {
									  SessionId = _sessionId,
									  Location = StateMachineLocation.Location!,
									  Arguments = Parameters,
									  ParentEventDispatcher = EventDispatcher
								  }
			: new LocationChildStateMachine(StateMachineLocation.Location.CombineWith(Source!))
			  {
				  SessionId = _sessionId,
				  Arguments = Parameters,
				  ParentEventDispatcher = EventDispatcher
			  };

		return StateMachineScopeManager.Execute(stateMachineClass, SecurityContextType.InvokedService);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			TaskCollector.Collect(StateMachineScopeManager.DestroyStateMachine(_sessionId));
		}

		base.Dispose(disposing);
	}

	protected override async ValueTask DisposeAsyncCore()
	{
		await StateMachineScopeManager.DestroyStateMachine(_sessionId).ConfigureAwait(false);

		await base.DisposeAsyncCore().ConfigureAwait(false);
	}
}