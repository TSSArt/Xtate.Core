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

using System.Net;
using System.Net.Http;
using System.Net.Mime;

namespace Xtate.ExternalService.HttpClient;

public class HttpClientXmlHandler : HttpClientMimeTypeHandler
{
    private const string MediaTypeTextXml = "text/xml";

    private const string MediaTypeApplicationXml = "application/xml";

    private HttpClientXmlHandler() { }

    public static HttpClientMimeTypeHandler Instance { get; } = new HttpClientXmlHandler();

    private static bool CanHandle([NotNullWhen(true)] string? contentType)
    {
        if (contentType is not { Length: > 0 })
        {
            return false;
        }

        const StringComparison ct = StringComparison.OrdinalIgnoreCase;
        const string text = "text/";
        const string application = "application/";
        const string xml = "+xml";

        var mediaType = new ContentType(contentType).MediaType;

        return string.Equals(mediaType, MediaTypeApplicationXml, ct)
               || string.Equals(mediaType, MediaTypeTextXml, ct)
               || ((mediaType.StartsWith(text, ct) || mediaType.StartsWith(application, ct)) && mediaType.EndsWith(xml, ct));
    }

    public override void PrepareRequest(WebRequest webRequest,
                                        string? contentType,
                                        DataModelList parameters,
                                        DataModelValue value) =>
        AppendAcceptHeader(webRequest, MediaTypeApplicationXml);

    public override HttpContent? TryCreateHttpContent(WebRequest webRequest,
                                                      string? contentType,
                                                      DataModelList parameters,
                                                      DataModelValue value) =>
        CanHandle(contentType) ? new HttpClientXmlHandlerHttpContent(value, contentType) : default;

    public override async ValueTask<DataModelValue?> TryParseResponseAsync(WebResponse webResponse, DataModelList parameters, CancellationToken token)
    {
        if (webResponse is null) throw new ArgumentNullException(nameof(webResponse));

        if (!CanHandle(webResponse.ContentType))
        {
            return default;
        }

        var stream = webResponse.GetResponseStream();

        Infra.NotNull(stream);

        XtateCore.Use();

        await using (stream.ConfigureAwait(false))
        {
            return await DataModelConverter.FromJsonAsync(stream, token).ConfigureAwait(false);
        }
    }
}