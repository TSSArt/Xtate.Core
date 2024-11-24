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

using System.Xml;

namespace Xtate.CustomAction;

public class DestroyAction : AsyncAction
{
	public class Provider() : ActionProvider<DestroyAction>(ns: "http://xtate.net/scxml/system", name: "destroy");

	public required DisposeToken DisposeToken { private get; [UsedImplicitly] init; }
	
	public required TaskMonitor TaskMonitor { private get; [UsedImplicitly] init; }
	
	private readonly StringValue _sessionIdValue;

	public DestroyAction(XmlReader xmlReader, IErrorProcessorService<DestroyAction> errorProcessorService)
	{
		var sessionId = xmlReader.GetAttribute("sessionId");
		var sessionIdExpression = xmlReader.GetAttribute("sessionIdExpr");

		if (sessionId is { Length: 0 })
		{
			errorProcessorService.AddError(this, Resources.ErrorMessage_SessionIdCouldNotBeEmpty);
		}

		if (sessionId is not null && sessionIdExpression is not null)
		{
			errorProcessorService.AddError(this, Resources.ErrorMessage_SessionIdAndSessionIdExprAttributesShouldNotBeAssignedInStartElement);
		}

		if (sessionId is null && sessionIdExpression is null)
		{
			errorProcessorService.AddError(this, Resources.ErrorMessage_SessionIdOrSessionIdExprMustBeSpecified);
		}

		_sessionIdValue = new StringValue(sessionIdExpression, sessionId);
	}

	public required IStateMachineCollection StateMachineCollection { private get; [UsedImplicitly] init; }

	protected override IEnumerable<Value> GetValues() { yield return _sessionIdValue; }

	protected override async ValueTask Execute()
	{
		var sessionId = await GetSessionId().ConfigureAwait(false);

		await TaskMonitor.RunAndWait(static tuple => tuple.StateMachineCollection.Destroy(tuple.sessionId), (StateMachineCollection, sessionId), DisposeToken).ConfigureAwait(false);
	}

	private async ValueTask<SessionId> GetSessionId()
	{
		var sessionId = await _sessionIdValue.GetValue().ConfigureAwait(false);

		if (string.IsNullOrEmpty(sessionId))
		{
			throw new ProcessorException(Resources.Exception_SessionIdCouldNotBeEmpty);
		}

		return SessionId.FromString(sessionId);
	}
}