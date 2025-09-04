// Copyright © 2019-2025 Sergii Artemenko
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

public class StartAction : AsyncAction
{
    public class Provider() : ActionProvider<StartAction>(ns: "http://xtate.net/scxml/system", name: "start");

    private readonly Location? _sessionIdLocation;

    private readonly StringValue? _sessionIdValue;

    private readonly bool _trusted;

    private readonly StringValue _urlValue;

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

    public required DisposeToken DisposeToken { private get; [UsedImplicitly] init; }

    public required Deferred<TaskMonitor> TaskMonitor { private get; [UsedImplicitly] init; }

    public required Deferred<IStateMachineLocation> StateMachineLocation { private get; [UsedImplicitly] init; }

    public required Deferred<IStateMachineScopeManager> StateMachineScopeManager { private get; [UsedImplicitly] init; }

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
        var location = await GetLocation().ConfigureAwait(false);
        var securityContextType = _trusted ? SecurityContextType.NewTrustedStateMachine : SecurityContextType.NewStateMachine;

        var locationStateMachine = new LocationStateMachine(location) { SessionId = sessionId };

        var taskMonitor = await TaskMonitor().ConfigureAwait(false);
        var stateMachineScopeManager = await StateMachineScopeManager().ConfigureAwait(false);
        await stateMachineScopeManager.Start(locationStateMachine, securityContextType).WaitAsync(taskMonitor, DisposeToken).ConfigureAwait(false);

        if (_sessionIdLocation is not null)
        {
            await _sessionIdLocation.SetValue(sessionId).ConfigureAwait(false);
        }
    }

    private async ValueTask<Uri> GetLocation()
    {
        var url = await _urlValue.GetValue().ConfigureAwait(false);

        if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            var baseUri = (await StateMachineLocation().ConfigureAwait(false))?.Location;

            return baseUri.CombineWith(uri);
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