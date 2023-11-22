#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xtate.Core;

namespace Xtate.CustomAction
{/*
	public class StartAction : CustomActionBase
	{
		private readonly Value    _url;
		private readonly Value    _sessionId;
		private readonly Location _output;

		public StartAction(XmlReader xmlReader)
		{
			_url = new StringValue(xmlReader.GetAttribute("urlExpr"), xmlReader.GetAttribute("url"));
			_sessionId = new StringValue(xmlReader.GetAttribute("sessionIdExpr"), xmlReader.GetAttribute("sessionId"));
			sessionId = new Location(xmlReader.GetAttribute("destination"));
		}

		public override IEnumerable<Value> GetValues() { yield return _input; }

		public override IEnumerable<Location> GetLocations() { yield return _output;}

		protected override DataModelValue Evaluate(IReadOnlyDictionary<string, DataModelValue> arguments) => base.Evaluate(arguments);

		protected override ValueTask<DataModelValue> EvaluateAsync(IReadOnlyDictionary<string, DataModelValue> arguments) => base.EvaluateAsync(arguments);

		public override async ValueTask Execute()
		{
			await _output.CopyFrom(_input);
		}
	}*/

	public class StartAction : CustomActionBase, IDisposable
	{
		private const string Url               = "url";
		private const string UrlExpr           = "urlExpr";
		private const string Trusted           = "trusted";
		private const string SessionId         = "sessionId";
		private const string SessionIdExpr     = "sessionIdExpr";
		private const string SessionIdLocation = "sessionIdLocation";

		private readonly DisposingToken        _disposingToken = new();
		private readonly ILocationAssigner?    _idLocation;
		private readonly string?               _sessionId;
		private readonly IExpressionEvaluator? _sessionIdExpression;
		private readonly bool                  _trusted;
		private readonly Uri?                  _url;
		private readonly IExpressionEvaluator? _urlExpression;

		public required Func<ValueTask<IExecutionContext>> ExecutionContextFactory { private get; init; }

		public StartAction(ICustomActionContext access, XmlReader xmlReader)
		{
			if (access is null) throw new ArgumentNullException(nameof(access));
			if (xmlReader is null) throw new ArgumentNullException(nameof(xmlReader));

			var url = xmlReader.GetAttribute(Url);
			var urlExpression = xmlReader.GetAttribute(UrlExpr);
			var trusted = xmlReader.GetAttribute(Trusted);
			var sessionIdExpression = xmlReader.GetAttribute(SessionIdExpr);
			var sessionIdLocation = xmlReader.GetAttribute(SessionIdLocation);
			_sessionId = xmlReader.GetAttribute(SessionId);
			_trusted = trusted is not null && XmlConvert.ToBoolean(trusted);

			if (url is null && urlExpression is null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_AtLeastOneUrlMustBeSpecified);
			}

			if (url is not null && urlExpression is not null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_UrlAndUrlExprAttributesShouldNotBeAssignedInStartElement);
			}

			if (url is not null && !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _url))
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_UrlHasInvalidURIFormat);
			}

			if (_sessionId is { Length: 0 })
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_SessionIdCouldNotBeEmpty);
			}

			if (_sessionId is not null && sessionIdExpression is not null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_SessionIdAndSessionIdExprAttributesShouldNotBeAssignedInStartElement);
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

		public async ValueTask Execute()
		{
			var executionContext = await ExecutionContextFactory().ConfigureAwait(false);

			var host = GetHost(executionContext);
			var baseUri = GetBaseUri(executionContext);
			var source = await GetSource().ConfigureAwait(false);

			if (source is null)
			{
				throw new ProcessorException(Resources.Exception_StartActionExecuteSourceNotSpecified);
			}

			var sessionId = await GetSessionId().ConfigureAwait(false);

			if (_sessionId is { Length: 0 })
			{
				throw new ProcessorException(Resources.Exception_SessionIdCouldNotBeEmpty);
			}

			var finalizer = new DeferredFinalizer();
			var securityContextType = _trusted ? SecurityContextType.NewTrustedStateMachine : SecurityContextType.NewStateMachine;
			var securityContext = executionContext.SecurityContext.CreateNested(securityContextType);

			await using (finalizer.ConfigureAwait(false))
			{
				await host.StartStateMachineAsync(sessionId, new StateMachineOrigin(source, baseUri), parameters: default, securityContextType: securityContextType, finalizer: finalizer, _disposingToken.Token).ConfigureAwait(false);
			}

			if (_idLocation is not null)
			{
				await _idLocation.Assign(sessionId).ConfigureAwait(false);
			}
		}

	#endregion

		private static Uri? GetBaseUri(IExecutionContext executionContext)
		{
			var value = executionContext.DataModel[key: @"_x", caseInsensitive: false]
										.AsListOrEmpty()[key: @"host", caseInsensitive: false]
										.AsListOrEmpty()[key: @"location", caseInsensitive: false]
										.AsStringOrDefault();

			return value is not null ? new Uri(value, UriKind.RelativeOrAbsolute) : null;
		}

		private static IHost GetHost(IExecutionContext executionContext)
		{
			if (executionContext.RuntimeItems[typeof(IHost)] is IHost host)
			{
				return host;
			}

			throw new ProcessorException(Resources.Exception_CantGetAccessToIHostInterface);
		}

		private async ValueTask<Uri?> GetSource()
		{
			if (_url is not null)
			{
				return _url;
			}

			if (_urlExpression is not null)
			{
				var value = await _urlExpression.Evaluate().ConfigureAwait(false);

				return new Uri(value.AsString(), UriKind.RelativeOrAbsolute);
			}

			return Infra.Fail<Uri>();
		}

		private async ValueTask<SessionId> GetSessionId()
		{
			if (_sessionId is not null)
			{
				return Xtate.SessionId.FromString(_sessionId);
			}

			if (_sessionIdExpression is not null)
			{
				var value = await _sessionIdExpression.Evaluate().ConfigureAwait(false);

				return Xtate.SessionId.FromString(value.AsString());
			}

			return Xtate.SessionId.New();
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