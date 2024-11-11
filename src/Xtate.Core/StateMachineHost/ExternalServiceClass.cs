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

using Xtate.DataModel;
using Xtate.IoC;

namespace Xtate.ExternalService;

public class ExternalServiceClass(
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
	  IStateMachineSessionId,
	  IStateMachineLocation,
	  IParentEventDispatcher
{
	public void AddServices(IServiceCollection services)
	{
		services.AddConstant<IStateMachineSessionId>(this);
		services.AddConstant<IStateMachineLocation>(this);
		services.AddConstant<ICaseSensitivity>(this);
		services.AddConstant<IExternalServiceInvokeId>(this);
		services.AddConstant<IExternalServiceType>(this);
		services.AddConstant<IExternalServiceSource>(this);
		services.AddConstant<IExternalServiceParameters>(this);
		services.AddConstant<IParentEventDispatcher>(this);
	}

	private FullUri? _origin;

	internal IEventDispatcher? EventDispatcher { get; set; }

#region Interface ICaseSensitivity

	bool ICaseSensitivity.CaseInsensitive { get; } = caseSensitivity.CaseInsensitive;

#endregion

#region Interface IEventDispatcher

	/// <summary>
	/// Dispatches the event to the parent session.
	/// </summary>
	/// <param name="incomingEvent"></param>
	/// <returns></returns>
	ValueTask IEventDispatcher.Dispatch(IIncomingEvent incomingEvent)
	{
		var origin = _origin ??= new FullUri(Const.ScxmlIoProcessorInvokeIdPrefix + invokeId.Value);

		var evt = new IncomingEvent(incomingEvent) { Type = EventType.External, OriginType = invokeData.Type, Origin = origin, InvokeId = invokeId };

		return eventDispatcher.Dispatch(evt);
	}

#endregion

#region Interface IExternalServiceInvokeId

	InvokeId IExternalServiceInvokeId.InvokeId => invokeId;

#endregion

#region Interface IExternalServiceParameters

	DataModelValue IExternalServiceParameters.Parameters { get; } = invokeData.Parameters;

#endregion

#region Interface IExternalServiceSource

	Uri? IExternalServiceSource.Source => invokeData.Source;

	string? IExternalServiceSource.RawContent => invokeData.RawContent;

	DataModelValue IExternalServiceSource.Content => invokeData.Content;

#endregion

#region Interface IExternalServiceType

	FullUri IExternalServiceType.Type => invokeData.Type;

#endregion

#region Interface IStateMachineLocation

	Uri? IStateMachineLocation.Location { get; } = stateMachineLocation.Location;

#endregion

#region Interface IStateMachineSessionId

	SessionId IStateMachineSessionId.SessionId { get; } = stateMachineSessionId.SessionId;

#endregion

	/// <summary>
	/// Dispatches the event to the external service.
	/// </summary>
	/// <param name="incomingEvent"></param>
	/// <returns></returns>
	internal ValueTask IncomingEventHandler(IIncomingEvent incomingEvent)
	{
		if (EventDispatcher is null)
		{
			return default;
		}

		if(incomingEvent is not IncomingEvent)
		{
			incomingEvent = new IncomingEvent(incomingEvent);
		}

		return EventDispatcher.Dispatch(incomingEvent);
	}
}