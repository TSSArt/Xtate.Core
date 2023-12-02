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
/*
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

	public required IHost                _host;
	public required IDataModelController _dataModelController;

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

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

	public override async ValueTask Execute()
	{
		var baseUri = GetBaseUri();
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

		await using (finalizer.ConfigureAwait(false))
		{
			await _host.StartStateMachineAsync(sessionId, new StateMachineOrigin(source, baseUri), parameters: default, securityContextType, finalizer, _disposingToken.Token).ConfigureAwait(false);
		}

		if (_idLocation is not null)
		{
			await _idLocation.Assign(sessionId).ConfigureAwait(false);
		}
	}

	private Uri? GetBaseUri()
	{
		var value = _dataModelController.DataModel[key: @"_x", caseInsensitive: false]
									   .AsListOrEmpty()[key: @"host", caseInsensitive: false]
									   .AsListOrEmpty()[key: @"location", caseInsensitive: false]
									   .AsStringOrDefault();

		return value is not null ? new Uri(value, UriKind.RelativeOrAbsolute) : null;
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
}