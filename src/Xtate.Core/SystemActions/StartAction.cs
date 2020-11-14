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

namespace Xtate.CustomAction
{
	public class StartAction : ICustomActionExecutor
	{
		private const string Url               = "url";
		private const string UrlExpr           = "urlExpr";
		private const string SessionId         = "sessionId";
		private const string SessionIdExpr     = "sessionIdExpr";
		private const string SessionIdLocation = "sessionIdLocation";

		private readonly ILocationAssigner?    _idLocation;
		private readonly string?               _sessionId;
		private readonly IExpressionEvaluator? _sessionIdExpression;
		private readonly Uri?                  _url;
		private readonly IExpressionEvaluator? _urlExpression;

		public StartAction(XmlReader xmlReader, ICustomActionContext access)
		{
			if (xmlReader is null) throw new ArgumentNullException(nameof(xmlReader));
			if (access is null) throw new ArgumentNullException(nameof(access));

			var url = xmlReader.GetAttribute(Url);
			var urlExpression = xmlReader.GetAttribute(UrlExpr);
			var sessionIdExpression = xmlReader.GetAttribute(SessionIdExpr);
			var sessionIdLocation = xmlReader.GetAttribute(SessionIdLocation);
			_sessionId = xmlReader.GetAttribute(SessionId);

			if (url is null && urlExpression is null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_At_least_one_url_must_be_specified);
			}

			if (url is not null && urlExpression is not null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_url_and_urlExpr_attributes_should_not_be_assigned_in_Start_element);
			}

			if (url is not null && !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _url))
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_url__has_invalid_URI_format);
			}

			if (_sessionId is { Length: 0 })
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_SessionId_could_not_be_empty);
			}

			if (_sessionId is not null && sessionIdExpression is not null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_sessionId__and__sessionIdExpr__attributes_should_not_be_assigned_in_Start_element_);
			}

			if (urlExpression is not null)
			{
				_urlExpression = access.RegisterValueExpression(urlExpression, ExpectedValueType.String);
			}

			if (sessionIdExpression is not null)
			{
				_sessionIdExpression = access.RegisterValueExpression(sessionIdExpression, ExpectedValueType.String);
			}

			if (sessionIdLocation is not null)
			{
				_idLocation = access.RegisterLocationExpression(sessionIdLocation);
			}
		}

	#region Interface ICustomActionExecutor

		public async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			var host = GetHost(executionContext);
			var baseUri = GetBaseUri(executionContext);
			var source = await GetSource(executionContext, token).ConfigureAwait(false);

			if (source is null)
			{
				throw new ProcessorException(Resources.StartAction_Execute_Source_not_specified);
			}

			var sessionId = await GetSessionId(executionContext, token).ConfigureAwait(false);

			if (_sessionId is { Length: 0 })
			{
				throw new ProcessorException(Resources.Exception_SessionId_could_not_be_empty);
			}

			await host.StartStateMachineAsync(sessionId, new StateMachineOrigin(source, baseUri), parameters: default, token).ConfigureAwait(false);

			if (_idLocation is not null)
			{
				await _idLocation.Assign(executionContext, sessionId, token).ConfigureAwait(false);
			}
		}

	#endregion

		private static Uri? GetBaseUri(IExecutionContext executionContext)
		{
			var val = executionContext.DataModel[key: "_x", caseInsensitive: false]
									  .AsListOrEmpty()[key: "host", caseInsensitive: false]
									  .AsListOrEmpty()[key: "location", caseInsensitive: false]
									  .AsStringOrDefault();

			return val is not null ? new Uri(val, UriKind.RelativeOrAbsolute) : null;
		}

		private static IHost GetHost(IExecutionContext executionContext)
		{
			if (executionContext.RuntimeItems[typeof(IHost)] is IHost host)
			{
				return host;
			}

			throw new ProcessorException(Resources.Exception_Can_t_get_access_to_IHost_interface);
		}

		private async ValueTask<Uri?> GetSource(IExecutionContext executionContext, CancellationToken token)
		{
			if (_url is not null)
			{
				return _url;
			}

			if (_urlExpression is not null)
			{
				var val = await _urlExpression.Evaluate(executionContext, token).ConfigureAwait(false);

				return new Uri(val.AsString(), UriKind.RelativeOrAbsolute);
			}

			return Infrastructure.Fail<Uri>();
		}

		private async ValueTask<SessionId> GetSessionId(IExecutionContext executionContext, CancellationToken token)
		{
			if (_sessionId is not null)
			{
				return Xtate.SessionId.FromString(_sessionId);
			}

			if (_sessionIdExpression is not null)
			{
				var val = await _sessionIdExpression.Evaluate(executionContext, token).ConfigureAwait(false);

				return Xtate.SessionId.FromString(val.AsString());
			}

			return Xtate.SessionId.New();
		}
	}
}