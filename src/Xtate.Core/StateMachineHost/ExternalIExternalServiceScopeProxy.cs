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

namespace Xtate.Core;

public class ExternalIExternalServiceScopeProxy(
	InvokeId invokeId,
	InvokeData invokeData,
	IEventDispatcher eventDispatcher,
	IStateMachineSessionId stateMachineSessionId,
	IStateMachineLocation stateMachineLocation,
	ICaseSensitivity caseSensitivity)
	: IStateMachineInvokeId, IExternalServiceDefinition, ICaseSensitivity, IStateMachineSessionId, IStateMachineLocation, IEventDispatcher
{
#region Interface ICaseSensitivity

	public bool CaseInsensitive { get; } = caseSensitivity.CaseInsensitive;

#endregion

#region Interface IEventDispatcher

	public ValueTask Send(IEvent evt, CancellationToken token) => eventDispatcher.Send(evt, token);

#endregion

#region Interface IExternalServiceDefinition

	public Uri Type { get; } = invokeData.Type;

	public Uri? Source { get; } = invokeData.Source;

	public string? RawContent { get; } = invokeData.RawContent;

	public DataModelValue Content { get; } = invokeData.Content;

	public DataModelValue Parameters { get; } = invokeData.Parameters;

#endregion

#region Interface IStateMachineInvokeId

	public InvokeId InvokeId => invokeId;

#endregion

#region Interface IStateMachineLocation

	public Uri? Location { get; } = stateMachineLocation.Location;

#endregion

#region Interface IStateMachineSessionId

	public SessionId SessionId { get; } = stateMachineSessionId.SessionId;

#endregion
}