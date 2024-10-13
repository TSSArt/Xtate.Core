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

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class StartActionProvider() : ActionProvider<StartAction>(ns: "http://xtate.net/scxml/system", name: "start");

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class StartAction : AsyncAction, IDisposable
{
	private readonly DisposingToken _disposingToken = new();
	private readonly Location?      _sessionIdLocation;
	private readonly StringValue?   _sessionIdValue;
	private readonly bool           _trusted;
	private readonly StringValue    _urlValue;

	public StartAction(XmlReader xmlReader, IErrorProcessorService<StartAction> errorProcessorService)
	{
		var url = xmlReader.GetAttribute("url");
		var urlExpression = xmlReader.GetAttribute("urlExpr");
		var sessionId = xmlReader.GetAttribute("sessionId");
		var sessionIdExpression = xmlReader.GetAttribute("sessionIdExpr");
		var sessionIdLocation = xmlReader.GetAttribute("sessionIdLocation");

		if (url is null && urlExpression is null)
		{
			errorProcessorService.AddError(this, Resources.ErrorMessage_AtLeastOneUrlMustBeSpecified);
		}

		if (url is not null && urlExpression is not null)
		{
			errorProcessorService.AddError(this, Resources.ErrorMessage_UrlAndUrlExprAttributesShouldNotBeAssignedInStartElement);
		}

		if (url is not null && !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _))
		{
			errorProcessorService.AddError(this, Resources.ErrorMessage_UrlHasInvalidURIFormat);
		}

		_urlValue = new StringValue(urlExpression, url);

		if (sessionId is { Length: 0 })
		{
			errorProcessorService.AddError(this, Resources.ErrorMessage_SessionIdCouldNotBeEmpty);
		}

		if (sessionId is not null && sessionIdExpression is not null)
		{
			errorProcessorService.AddError(this, Resources.ErrorMessage_SessionIdAndSessionIdExprAttributesShouldNotBeAssignedInStartElement);
		}

		if (sessionId is not null || sessionIdExpression is not null)
		{
			_sessionIdValue = new StringValue(sessionIdExpression, sessionId);
		}

		if (sessionIdLocation is not null)
		{
			_sessionIdLocation = new Location(sessionIdExpression);
		}

		_trusted = xmlReader.GetAttribute("trusted") is { } trusted && XmlConvert.ToBoolean(trusted);
	}

	public required Func<ValueTask<IStateMachineLocation?>> StateMachineLocationFactory { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<IHost>> HostFactory { private get; [UsedImplicitly] init; }

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();
		}
	}

	protected override IEnumerable<Location> GetLocations()
	{
		if (_sessionIdLocation is not null)
		{
			yield return _sessionIdLocation;
		}
	}

	protected override IEnumerable<Value> GetValues()
	{
		yield return _urlValue;

		if (_sessionIdValue is not null)
		{
			yield return _sessionIdValue;
		}
	}

	protected override async ValueTask Execute()
	{
		var sessionId = await GetSessionId().ConfigureAwait(false);
		var securityContextType = _trusted ? SecurityContextType.NewTrustedStateMachine : SecurityContextType.NewStateMachine;
		var source = await GetSource().ConfigureAwait(false);
		var stateMachineOrigin = new StateMachineOrigin(source, await GetBaseUri().ConfigureAwait(false));

		var host = await HostFactory().ConfigureAwait(false);

		await host.StartStateMachineAsync(sessionId, stateMachineOrigin, parameters: default, securityContextType, _disposingToken.Token).ConfigureAwait(false);

		if (_sessionIdLocation is not null)
		{
			await _sessionIdLocation.SetValue(sessionId).ConfigureAwait(false);
		}
	}

	private async ValueTask<Uri?> GetBaseUri() => (await StateMachineLocationFactory().ConfigureAwait(false))?.Location;

	//var value = dataModelController.DataModel["_x"].AsListOrEmpty()["host"].AsListOrEmpty()["location"].AsStringOrDefault();
	//return value is not null ? new Uri(value, UriKind.RelativeOrAbsolute) : null;
	private async ValueTask<Uri> GetSource()
	{
		var url = await _urlValue.GetValue().ConfigureAwait(false);

		if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
		{
			return uri;
		}

		throw new ProcessorException(Resources.Exception_StartActionExecuteSourceNotSpecified);
	}

	private async ValueTask<SessionId> GetSessionId()
	{
		if (_sessionIdValue is null)
		{
			return SessionId.New();
		}

		var sessionId = await _sessionIdValue.GetValue().ConfigureAwait(false);

		if (string.IsNullOrEmpty(sessionId))
		{
			throw new ProcessorException(Resources.Exception_SessionIdCouldNotBeEmpty);
		}

		return SessionId.FromString(sessionId);
	}
}