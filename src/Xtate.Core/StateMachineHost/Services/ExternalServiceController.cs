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
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.IoC;
using Xtate.Logging;
using Xtate.Scxml;
using Xtate.StateMachine;

namespace Xtate.StateMachineHost.Services;

[InstantiatedByIoC]
public class ExternalServiceController : IExternalServiceController
{
	private readonly AsyncInit<ExternalServiceController> _execute = new(controller => controller.Execute());

	public required IExternalService ExternalService { private get; [SetByIoC] init; }

	public required DataConverter DataConverter { private get; [SetByIoC] init; }

	public required IExternalCommunication ExternalCommunication { private get; [SetByIoC] init; }

	public required ILogger<ExternalServiceController> Logger { private get; [SetByIoC] init; }

	public required IExternalServiceInvokeId ExternalServiceInvokeId { private get; [SetByIoC] init; }

#region Interface IExternalServiceController

	public ValueTask WaitForCompletion() => AsyncInit.For(this).Run(_execute);

	public virtual async ValueTask Dispatch(IIncomingEvent incomingEvent, CancellationToken token)
	{
		if (ExternalService is not IEventDispatcher eventDispatcher)
		{
			return;
		}

		if (incomingEvent is not IncomingEvent)
		{
			incomingEvent = new IncomingEvent(incomingEvent);
		}

		await eventDispatcher.Dispatch(incomingEvent, token).ConfigureAwait(false);
	}

#endregion

	protected virtual ValueTask<DataModelValue> GetResult() => ExternalService.GetResult();

	protected virtual async ValueTask Execute()
	{
		try
		{
			var outgoingEvent = CreateEventFromResult(await GetResult().ConfigureAwait(false));
			var sendStatus = await ExternalCommunication.TrySend(outgoingEvent).ConfigureAwait(false);
			Infra.Assert(sendStatus == SendStatus.Sent);
		}
		catch (Exception ex)
		{
			await HandleExecutionException(ex).ConfigureAwait(false);
		}
	}

	protected virtual async ValueTask HandleExecutionException(Exception exception)
	{
		try
		{
			var outgoingEvent = CreateEventFromException(exception);
			var sendStatus = await ExternalCommunication.TrySend(outgoingEvent).ConfigureAwait(false);
			Infra.Assert(sendStatus == SendStatus.Sent);
		}
		catch (Exception ex)
		{
			await Logger.Write(Level.Error, eventId: 1, Resources.Message_ServiceExecutionError, exception).ConfigureAwait(false);
			await Logger.Write(Level.Error, eventId: 2, Resources.Message_ErrorOnSendingErrorToParent, ex).ConfigureAwait(false);
		}
	}

	private EventEntity CreateEventFromResult(DataModelValue result) =>
		new() { Name = EventName.GetDoneInvokeName(ExternalServiceInvokeId.InvokeId), Data = result, Type = Const.ScxmlIoProcessorId, Target = Const.ParentTarget };

	private EventEntity CreateEventFromException(Exception ex) =>
		new() { Name = EventName.ErrorExecution, Data = DataConverter.FromException(ex), Type = Const.ScxmlIoProcessorId, Target = Const.ParentTarget };
}
