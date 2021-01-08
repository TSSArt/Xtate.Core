#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Core;

namespace Xtate.CustomAction
{
	public class DestroyAction : ICustomActionExecutor
	{
		private const string SessionId     = "sessionId";
		private const string SessionIdExpr = "sessionIdExpr";

		private readonly string?               _sessionId;
		private readonly IExpressionEvaluator? _sessionIdExpression;

		public DestroyAction(ICustomActionContext access, XmlReader xmlReader)
		{
			if (access is null) throw new ArgumentNullException(nameof(access));
			if (xmlReader is null) throw new ArgumentNullException(nameof(xmlReader));

			var sessionIdExpression = xmlReader.GetAttribute(SessionIdExpr);
			_sessionId = xmlReader.GetAttribute(SessionId);

			if (_sessionId is { Length: 0 })
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_SessionId_could_not_be_empty);
			}

			if (_sessionId is not null && sessionIdExpression is not null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_sessionId__and__sessionIdExpr__attributes_should_not_be_assigned_in_Start_element);
			}

			if (_sessionId is null && sessionIdExpression is null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_sessionId__or__sessionIdExpr__must_be_specified);
			}

			if (sessionIdExpression is not null)
			{
				_sessionIdExpression = access.RegisterValueExpression(sessionIdExpression, ExpectedValueType.String);
			}
		}

	#region Interface ICustomActionExecutor

		public async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			var host = GetHost(executionContext);
			var sessionId = await GetSessionId(executionContext, token).ConfigureAwait(false);

			if (sessionId is { Length: 0 })
			{
				throw new ProcessorException(Resources.Exception_SessionId_could_not_be_empty);
			}

			await host.DestroyStateMachine(Xtate.SessionId.FromString(sessionId), token).ConfigureAwait(false);
		}

	#endregion

		private static IHost GetHost(IExecutionContext executionContext)
		{
			if (executionContext.RuntimeItems[typeof(IHost)] is IHost host)
			{
				return host;
			}

			throw new ProcessorException(Resources.Exception_Can_t_get_access_to_IHost_interface);
		}

		private async ValueTask<string> GetSessionId(IExecutionContext executionContext, CancellationToken token)
		{
			if (_sessionId is not null)
			{
				return _sessionId;
			}

			if (_sessionIdExpression is not null)
			{
				var val = await _sessionIdExpression.Evaluate(executionContext, token).ConfigureAwait(false);

				return val.AsString();
			}

			return Infrastructure.Fail<string>();
		}
	}
}