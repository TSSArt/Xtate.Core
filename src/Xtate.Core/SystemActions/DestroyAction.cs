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

using System.Xml;

namespace Xtate.CustomAction;

public class DestroyAction : CustomActionBase, IDisposable
{
	private readonly DisposingToken _disposingToken = new();
	private readonly StringValue    _sessionIdValue;

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

	public required IHost Host { private get; [UsedImplicitly] init; }

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

<<<<<<< Updated upstream
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Core;

namespace Xtate.CustomAction
{
	public class DestroyAction : CustomActionBase, IDisposable
	{

		private const string SessionId     = "sessionId";
		private const string SessionIdExpr = "sessionIdExpr";

		private readonly DisposingToken        _disposingToken = new();
		private readonly string?               _sessionId;
		private readonly IExpressionEvaluator? _sessionIdExpression;

		public required Func<ValueTask<IExecutionContext>> ExecutionContextFactory { private get; init; }

		public DestroyAction(ICustomActionContext access, XmlReader xmlReader)
=======
	public override async ValueTask Execute()
	{
		var sessionId = await GetSessionId().ConfigureAwait(false);

		await Host.DestroyStateMachine(sessionId, _disposingToken.Token).ConfigureAwait(false);
	}

	private async ValueTask<SessionId> GetSessionId()
	{
		var sessionId = await _sessionIdValue.GetValue().ConfigureAwait(false);

		if (string.IsNullOrEmpty(sessionId))
>>>>>>> Stashed changes
		{
			throw new ProcessorException(Resources.Exception_SessionIdCouldNotBeEmpty);
		}

		return SessionId.FromString(sessionId);
	}

<<<<<<< Updated upstream
		public async ValueTask Execute()
		{
			var executionContext = await ExecutionContextFactory().ConfigureAwait(false);

			var host = GetHost(executionContext);
			var sessionId = await GetSessionId().ConfigureAwait(false);

			if (sessionId is { Length: 0 })
			{
				throw new ProcessorException(Resources.Exception_SessionIdCouldNotBeEmpty);
			}

			await host.DestroyStateMachine(Xtate.SessionId.FromString(sessionId), _disposingToken.Token).ConfigureAwait(false);
		}

	#endregion

		private static IHost GetHost(IExecutionContext executionContext)
		{
			if (executionContext.RuntimeItems[typeof(IHost)] is IHost host)
			{
				return host;
			}

			throw new ProcessorException(Resources.Exception_CantGetAccessToIHostInterface);
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
=======
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();
>>>>>>> Stashed changes
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_disposingToken.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}