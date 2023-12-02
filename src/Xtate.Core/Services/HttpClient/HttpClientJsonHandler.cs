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

using System.Net;
using System.Net.Http;

namespace Xtate.Service;

public class HttpClientJsonHandler : HttpClientMimeTypeHandler
{
	private const string MediaTypeApplicationJson = "application/json";

	private HttpClientJsonHandler() { }

	public static HttpClientMimeTypeHandler Instance { get; } = new HttpClientJsonHandler();

	public override void PrepareRequest(WebRequest webRequest,
										string? contentType,
										DataModelList parameters,
										DataModelValue value) =>
		AppendAcceptHeader(webRequest, MediaTypeApplicationJson);

	public override HttpContent? TryCreateHttpContent(WebRequest webRequest,
													  string? contentType,
													  DataModelList parameters,
													  DataModelValue value) =>
		ContentTypeEquals(contentType, MediaTypeApplicationJson) ? new HttpClientJsonHandlerHttpContent(value, MediaTypeApplicationJson) : default;

	public override async ValueTask<DataModelValue?> TryParseResponseAsync(WebResponse webResponse, DataModelList parameters, CancellationToken token)
	{
		if (webResponse is null) throw new ArgumentNullException(nameof(webResponse));

		if (!ContentTypeEquals(webResponse.ContentType, MediaTypeApplicationJson))
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