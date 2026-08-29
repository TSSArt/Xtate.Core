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

using Xtate.Class;
using Xtate.DataModel;
using Xtate.DataTypes;
using Xtate.ExternalServices;
using Xtate.Interpreter;
using Xtate.IoC.Tools;
using Xtate.Scxml;
using Xtate.StateMachine;
using Xtate.TaskMonitor;

namespace Xtate.StateMachineHost.Services;

[InstantiatedByIoC]
public class StateMachineExternalService : ExternalServiceBase, IDisposable, IAsyncDisposable
{
	[InstantiatedByIoC]
	public class Provider() : ExternalServiceProviderBase<StateMachineExternalService>(Const.ScxmlServiceTypeId, Const.ScxmlServiceAliasTypeId);

	private bool _disposed;

	public InvokeId InvokeId { get; private init; } = null!;

	public SessionId SessionId { get; private init; } = null!;

	public required Deferred<IStateMachineScopeManager> StateMachineScopeManager { private get; [SetByIoC] init; }

	public required IStateMachineLocation StateMachineLocation { private get; [SetByIoC] init; }

	public required IStateMachineCollection StateMachineCollection { private get; [SetByIoC] init; }

	public required IStateMachineSessionId ParentStateMachineSessionId { private get; [SetByIoC] init; }

	public SessionId ParentSessionId => ParentStateMachineSessionId.SessionId;

	public FullUri Type => ExternalServiceType.Type;

	[SetByIoC]
	public required IExternalServiceInvokeId ExternalServiceInvokeId
	{
		init
		{
			InvokeId = value.InvokeId;
			SessionId = new InvokeSessionId(InvokeId.UniqueId);
		}
	}

	public required IExternalServiceType ExternalServiceType { private get; [SetByIoC] init; }

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
		if (!_disposed)
		{
			using var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, DestroyToken);

			await StateMachineCollection.Dispatch(SessionId, incomingEvent, combinedToken.Token).ConfigureAwait(false);
		}
	}

	protected virtual StateMachineClass GetInvokedStateMachineClass()
	{
		StateMachineClass? stateMachineClass = null;

		if ((RawContent ?? Content.AsStringOrDefault()) is { } scxml)
		{
			stateMachineClass = new ScxmlStringInvokedStateMachine(scxml)
								{
									SessionId = SessionId,
									ParentSessionId = ParentSessionId,
									InvokeId = InvokeId,
									Type = Type,
									Location = StateMachineLocation.Location!,
									Arguments = Parameters
								};
		}

		if (Source is not null)
		{
			stateMachineClass = new LocationInvokedStateMachine(StateMachineLocation.Location, Source)
								{
									SessionId = SessionId,
									ParentSessionId = ParentSessionId,
									InvokeId = InvokeId,
									Type = Type,
									Arguments = Parameters
								};
		}

		Infra.NotNull(stateMachineClass);

		return stateMachineClass;
	}

	protected override async ValueTask<DataModelValue> Execute()
	{
		var stateMachineScopeManager = await StateMachineScopeManager().ConfigureAwait(false);

		return await stateMachineScopeManager.Execute(GetInvokedStateMachineClass(), SecurityContextType.InvokedService).ConfigureAwait(false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !_disposed)
		{
			_disposed = true;

			StateMachineCollection.Destroy(SessionId).Forget(TaskMonitor);
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (!_disposed)
		{
			_disposed = true;

			await StateMachineCollection.Destroy(SessionId).ConfigureAwait(false);
		}
	}

	private class InvokeSessionId(UniqueInvokeId uniqueInvokeId) : SessionId
	{
		protected override string GenerateId() => uniqueInvokeId.Value;
	}
}
