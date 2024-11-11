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

using Xtate.ExternalService;

namespace Xtate.Core;

public class StateMachineExternalService : ExternalServiceBase, IEventDispatcher
{
	public class Provider() : ExternalServiceProviderBase<StateMachineExternalService>(Const.ScxmlServiceTypeId, Const.ScxmlServiceAliasTypeId);

	private SessionId? _sessionId;

	public required IStateMachineScopeManager StateMachineScopeManager { private get; [UsedImplicitly] init; }

	public required IStateMachineLocation StateMachineLocation { private get; [UsedImplicitly] init; }

	public required TaskCollector TaskCollector { private get; [UsedImplicitly] init; }

	public required Func<Uri, DataModelValue, StateMachineClass> LocationStateMachineClassFactory { private get; [UsedImplicitly] init; }

	public required Func<string, Uri?, DataModelValue, StateMachineClass> ScxmlStateMachineClassFactory { private get; [UsedImplicitly] init; }

#region Interface IEventDispatcher

	public ValueTask Dispatch(IIncomingEvent incomingEvent) => throw new NotImplementedException(); //TODO:

#endregion

	protected override ValueTask<DataModelValue> Execute()
	{
		var scxml = RawContent ?? Content.AsStringOrDefault();

		Infra.Assert(scxml is not null || Source is not null);

		var stateMachineClass = scxml is not null
			? ScxmlStateMachineClassFactory(scxml, StateMachineLocation.Location, Parameters)
			: LocationStateMachineClassFactory(StateMachineLocation.Location.CombineWith(Source), Parameters);

		_sessionId = stateMachineClass.SessionId;

		return StateMachineScopeManager.Execute(stateMachineClass, SecurityContextType.InvokedService);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && _sessionId is not null)
		{
			TaskCollector.Collect(StateMachineScopeManager.DestroyStateMachine(_sessionId));
		}

		base.Dispose(disposing);
	}

	protected override async ValueTask DisposeAsyncCore()
	{
		if (_sessionId is not null)
		{
			await StateMachineScopeManager.DestroyStateMachine(_sessionId).ConfigureAwait(false);
		}

		await base.DisposeAsyncCore().ConfigureAwait(false);
	}
}