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

using System.IO;
using System.Net;

namespace Xtate.IoProcessor;

internal sealed class HttpIoProcessor(IEventConsumer eventConsumer, Uri baseUri, IPEndPoint ipEndPoint)
    : HttpIoProcessorBase<HttpIoProcessorHost, HttpListenerContext>(eventConsumer, baseUri, ipEndPoint, Id, Alias, ErrorSuffix)
{
    private const string ErrorSuffix = @"Internal";

    private static readonly FullUri Id = new(@"http://www.w3.org/TR/scxml/#BasicHTTPEventProcessor");

    private static readonly FullUri Alias = new(@"http");

    protected override HttpIoProcessorHost CreateHost(IPEndPoint ipEndPoint) => new(ipEndPoint);

    protected override string GetPath(HttpListenerContext context) => context.Request.Url?.GetComponents(UriComponents.Path, UriFormat.Unescaped) ?? throw new NotSupportedException();

    protected override string? GetHeaderValue(HttpListenerContext context, string name) => context.Request.Headers[name];

    protected override IPAddress? GetRemoteAddress(HttpListenerContext context) => context.Request.RemoteEndPoint is { } endPoint ? endPoint.Address : default;

    protected override string? GetQueryString(HttpListenerContext context) => context.Request.Url?.GetComponents(UriComponents.Query, UriFormat.Unescaped);

    protected override Stream GetBody(HttpListenerContext context) => context.Request.InputStream;

    protected override string GetMethod(HttpListenerContext context) => context.Request.HttpMethod;
}