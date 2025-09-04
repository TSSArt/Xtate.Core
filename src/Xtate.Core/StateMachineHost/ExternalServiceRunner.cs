// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.ExternalService;

public class ExternalServiceRunner : IExternalServiceRunner
{
    private readonly AsyncInit _execute;

    private readonly InvokeId _invokeId;

    public ExternalServiceRunner(IExternalServiceInvokeId externalServiceInvokeId)
    {
        _invokeId = externalServiceInvokeId.InvokeId;
        _execute = AsyncInit.Run(Execute);
    }

    public required IExternalService ExternalService { private get; [UsedImplicitly] init; }

    public required DataConverter DataConverter { private get; [UsedImplicitly] init; }

    public required IExternalCommunication ExternalCommunication { private get; [UsedImplicitly] init; }

#region Interface IExternalServiceRunner

    public ValueTask WaitForCompletion() => new(_execute.Task);

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
            var outgoingEvent = CreateEventFromException(ex);
            var sendStatus = await ExternalCommunication.TrySend(outgoingEvent).ConfigureAwait(false);
            Infra.Assert(sendStatus == SendStatus.Sent);
        }
    }

    private EventEntity CreateEventFromResult(DataModelValue result) =>
        new() { Name = EventName.GetDoneInvokeName(_invokeId), Data = result, Type = Const.ScxmlIoProcessorId, Target = Const.ParentTarget };

    private EventEntity CreateEventFromException(Exception ex) =>
        new() { Name = EventName.ErrorExecution, Data = DataConverter.FromException(ex), Type = Const.ScxmlIoProcessorId, Target = Const.ParentTarget };
}