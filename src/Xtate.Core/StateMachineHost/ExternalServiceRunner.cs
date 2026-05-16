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
using Xtate.IoC;

namespace Xtate.ExternalService;

public class ExternalServiceRunner(IExternalServiceInvokeId externalServiceInvokeId) : IExternalServiceRunner
{
	private readonly AsyncInit<ExternalServiceRunner> _execute = new(runner => runner.Execute());

	private readonly InvokeId _invokeId = externalServiceInvokeId.InvokeId;

	public required IExternalService ExternalService { private get; [UsedImplicitly] init; }

	public required DataConverter DataConverter { private get; [UsedImplicitly] init; }

	public required IExternalCommunication ExternalCommunication { private get; [UsedImplicitly] init; }

	public required ILogger<ExternalServiceRunner> Logger { private get; [SetByIoC] init; }

#region Interface IExternalServiceRunner

	public ValueTask WaitForCompletion() => AsyncInit.For(this).Run(_execute);

#endregion

	protected virtual async ValueTask Execute()
	{
		try
		{
			var outgoingEvent = CreateEventFromResult(await ExternalService.GetResult().ConfigureAwait(false));
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
			await Logger.Write(Level.Error, eventId: 1, message: "Service Execution error.", exception).ConfigureAwait(false);
			await Logger.Write(Level.Error, eventId: 2, message: "Error on sending error to Parent.", ex).ConfigureAwait(false);
		}
	}

	private EventEntity CreateEventFromResult(DataModelValue result) =>
		new() { Name = EventName.GetDoneInvokeName(_invokeId), Data = result, Type = Const.ScxmlIoProcessorId, Target = Const.ParentTarget };

	private EventEntity CreateEventFromException(Exception ex) =>
		new() { Name = EventName.ErrorExecution, Data = DataConverter.FromException(ex), Type = Const.ScxmlIoProcessorId, Target = Const.ParentTarget };
}