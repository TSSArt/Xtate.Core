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

using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.ExternalServices;
using Xtate.Interpreter;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.TaskMonitor;

namespace Xtate.StateMachineHost.Services;

[InstantiatedByIoC]
public class StateMachineExternalService : ExternalServiceBase, IDisposable, IAsyncDisposable
{
	[InstantiatedByIoC]
	public class Provider() : ExternalServiceProviderBase<StateMachineExternalService>(Const.ScxmlServiceTypeId, Const.ScxmlServiceAliasTypeId);

	private SessionId? _sessionId;

	public required IStateMachineScopeManager StateMachineScopeManager { private get; [SetByIoC] init; }

	public required IStateMachineLocation StateMachineLocation { private get; [SetByIoC] init; }

	public required IStateMachineCollection StateMachineCollection { private get; [SetByIoC] init; }

	public required ITaskMonitor TaskMonitor { private get; [SetByIoC] init; }

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

	protected override async ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (_sessionId is { } sessionId)
		{
			using var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, DestroyToken);

			await StateMachineCollection.Dispatch(sessionId, incomingEvent, combinedToken.Token).ConfigureAwait(false);
		}
	}

	protected override ValueTask<DataModelValue> Execute()
	{
		var scxml = RawContent ?? Content.AsStringOrDefault();

		if (scxml is not null)
		{
			var stateMachineClass = new ScxmlStringChildStateMachine(scxml)
									{
										ParentEventDispatcher = this,
										Location = StateMachineLocation.Location!,
										Arguments = Parameters
									};

			_sessionId = stateMachineClass.SessionId;

			return StateMachineScopeManager.Execute(stateMachineClass, SecurityContextType.InvokedService);
		}

		if (Source is not null)
		{
			var stateMachineClass = new LocationChildStateMachine(StateMachineLocation.Location, Source)
									{
										ParentEventDispatcher = this,
										Arguments = Parameters
									};

			_sessionId = stateMachineClass.SessionId;

			return StateMachineScopeManager.Execute(stateMachineClass, SecurityContextType.InvokedService);
		}

		throw Infra.Fail<Exception>();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && _sessionId is { } sessionId)
		{
			_sessionId = null;

			StateMachineCollection.Destroy(sessionId).Forget(TaskMonitor);
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (_sessionId is { } sessionId)
		{
			_sessionId = null;

			await StateMachineCollection.Destroy(sessionId).ConfigureAwait(false);
		}
	}
}