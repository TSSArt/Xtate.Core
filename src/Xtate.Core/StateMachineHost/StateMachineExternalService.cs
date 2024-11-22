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

public class StateMachineExternalService : ExternalServiceBase, IEventDispatcher, IDisposable, IAsyncDisposable
{
	public class Provider() : ExternalServiceProviderBase<StateMachineExternalService>(Const.ScxmlServiceTypeId, Const.ScxmlServiceAliasTypeId);

	private SessionId? _sessionId;

	public required IStateMachineScopeManager StateMachineScopeManager { private get; [UsedImplicitly] init; }

	public required IStateMachineLocation StateMachineLocation { private get; [UsedImplicitly] init; }

	public required IStateMachineCollection StateMachineCollection { private get; [UsedImplicitly] init; }

	public required ITaskMonitor TaskMonitor { private get; [UsedImplicitly] init; }

	public required Func<Uri, DataModelValue, StateMachineClass> LocationStateMachineClassFactory { private get; [UsedImplicitly] init; }

	public required Func<string, Uri?, DataModelValue, StateMachineClass> ScxmlStateMachineClassFactory { private get; [UsedImplicitly] init; }

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);
		
		Dispose(false);
		
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);

		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IEventDispatcher

	public async ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (_sessionId is { } sessionId)
		{
			using var combinedToken = new CombinedToken(token, DestroyToken);

			await StateMachineCollection.Dispatch(sessionId, incomingEvent, combinedToken.Token).ConfigureAwait(false);
		}
	}

#endregion

	protected override ValueTask<DataModelValue> Execute()
	{
		var scxml = RawContent ?? Content.AsStringOrDefault();

		Infra.Assert(scxml is not null || Source is not null);

		var stateMachineClass = scxml is not null
			? ScxmlStateMachineClassFactory(scxml, StateMachineLocation.Location, Parameters)
			: LocationStateMachineClassFactory(StateMachineLocation.Location.CombineWith(Source!), Parameters);

		_sessionId = stateMachineClass.SessionId;

		return StateMachineScopeManager.Execute(stateMachineClass, SecurityContextType.InvokedService);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && _sessionId is { } sessionId)
		{
			_sessionId = default;

			StateMachineCollection.Destroy(sessionId).Forget(TaskMonitor);
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (_sessionId is { } sessionId)
		{
			_sessionId = default;

			await StateMachineCollection.Destroy(sessionId).ConfigureAwait(false);
		}
	}
}