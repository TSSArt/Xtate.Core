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

using Xtate.DataModel;

namespace Xtate.ExternalService;

public class ExternalServiceBridge(
	InvokeId invokeId,
	InvokeData invokeData,
	IEventDispatcher eventDispatcher,
	IStateMachineSessionId stateMachineSessionId,
	IStateMachineLocation stateMachineLocation,
	ICaseSensitivity caseSensitivity)
	: IExternalServiceInvokeId,
	  IExternalServiceType,
	  IExternalServiceSource,
	  IExternalServiceParameters,
	  ICaseSensitivity,
	  IParentStateMachineSessionId,
	  IStateMachineSessionId,
	  IStateMachineLocation,
	  IParentEventDispatcher
{
	private FullUri? _origin;

	private FullUri Origin => _origin ??= new FullUri(Const.ScxmlIoProcessorInvokeIdPrefix + invokeId.Value);

	internal IEventDispatcher? EventDispatcher { get; set; }

#region Interface ICaseSensitivity

	public bool CaseInsensitive { get; } = caseSensitivity.CaseInsensitive;

#endregion

#region Interface IEventDispatcher

	public ValueTask Dispatch(IIncomingEvent incomingEvent) =>
		eventDispatcher.Dispatch(new IncomingEvent(incomingEvent) { Type = EventType.External, OriginType = Type, Origin = Origin, InvokeId = invokeId });

#endregion

#region Interface IExternalServiceInvokeId

	public InvokeId InvokeId => invokeId;

#endregion

#region Interface IExternalServiceParameters

	public DataModelValue Parameters { get; } = invokeData.Parameters;

#endregion

#region Interface IExternalServiceSource

	public Uri? Source { get; } = invokeData.Source;

	public string? RawContent { get; } = invokeData.RawContent;

	public DataModelValue Content { get; } = invokeData.Content;

#endregion

#region Interface IExternalServiceType

	public FullUri Type { get; } = invokeData.Type;

#endregion

#region Interface IParentStateMachineSessionId

	public SessionId SessionId { get; } = stateMachineSessionId.SessionId;

#endregion

#region Interface IStateMachineLocation

	public Uri? Location { get; } = stateMachineLocation.Location;

#endregion

	internal ValueTask IncomingEventHandler(IIncomingEvent incomingEvent)
	{
		if (EventDispatcher is not { } dispatcher)
		{
			return default;
		}

		return dispatcher.Dispatch(new IncomingEvent(incomingEvent));
	}
}