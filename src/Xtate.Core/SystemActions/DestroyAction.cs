#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using System.Xml;

namespace Xtate.CustomAction;

public class DestroyAction : CustomActionBase, IDisposable
{
	private const string SessionId     = "sessionId";
	private const string SessionIdExpr = "sessionIdExpr";

	private readonly DisposingToken        _disposingToken = new();
	private readonly string?               _sessionId;
	private readonly IExpressionEvaluator? _sessionIdExpression;

	public required IHost _host;

	public DestroyAction(ICustomActionContext access, XmlReader xmlReader)
	{
		if (access is null) throw new ArgumentNullException(nameof(access));
		if (xmlReader is null) throw new ArgumentNullException(nameof(xmlReader));

		var sessionIdExpression = xmlReader.GetAttribute(SessionIdExpr);
		_sessionId = xmlReader.GetAttribute(SessionId);

		if (_sessionId is { Length: 0 })
		{
			access.AddValidationError<StartAction>(Resources.ErrorMessage_SessionIdCouldNotBeEmpty);
		}

		if (_sessionId is not null && sessionIdExpression is not null)
		{
			access.AddValidationError<StartAction>(Resources.ErrorMessage_SessionIdAndSessionIdExprAttributesShouldNotBeAssignedInStartElement);
		}

		if (_sessionId is null && sessionIdExpression is null)
		{
			access.AddValidationError<StartAction>(Resources.ErrorMessage_SessionIdOrSessionIdExprMustBeSpecified);
		}

		if (sessionIdExpression is not null)
		{
			_sessionIdExpression = access.RegisterValueExpression(sessionIdExpression, ExpectedValueType.String);
		}
	}

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

	public override async ValueTask Execute()
	{
		var sessionId = await GetSessionId().ConfigureAwait(false);

		if (sessionId is { Length: 0 })
		{
			throw new ProcessorException(Resources.Exception_SessionIdCouldNotBeEmpty);
		}

		await _host.DestroyStateMachine(Xtate.SessionId.FromString(sessionId), _disposingToken.Token).ConfigureAwait(false);
	}

	private async ValueTask<string> GetSessionId()
	{
		if (_sessionId is not null)
		{
			return _sessionId;
		}

		if (_sessionIdExpression is not null)
		{
			var value = await _sessionIdExpression.Evaluate().ConfigureAwait(false);

			return value.AsString();
		}

		return Infra.Fail<string>();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();
		}
	}
}