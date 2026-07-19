// Copyright © 2019-2026 Sergii Artemenko
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.Http;

namespace Xtate.ExternalServices.HttpClient.Services;

[InstantiatedByIoC]
public class HttpClientJsonHandler : HttpClientMimeTypeHandler
{
	private const string MediaTypeApplicationJson = "application/json";

	private static readonly MediaTypeWithQualityHeaderValue AcceptMediaType = new(MediaTypeApplicationJson, quality: 1);

	private static bool CanHandle(string? mediaType) => mediaType is not null && string.Equals(mediaType, MediaTypeApplicationJson, StringComparison.OrdinalIgnoreCase);

	public override void PrepareRequest(HttpRequestMessage request,
										ContentType? contentType,
										DataModelList parameters,
										DataModelValue value)
	{
		var headerValues = request.Headers.Accept;

		if (!headerValues.Contains(AcceptMediaType))
		{
			headerValues.Add(AcceptMediaType);
		}
	}

	public override HttpContent? TryCreateHttpContent(HttpRequestMessage request,
													  ContentType? contentType,
													  DataModelList parameters,
													  DataModelValue value) =>
		CanHandle(contentType?.MediaType) ? new JsonHttpContent(value) : null;

	public override async ValueTask<DataModelValue?> TryParseResponseAsync(HttpResponseMessage response, DataModelList parameters, CancellationToken token)
	{
		if (!CanHandle(response.Content.Headers.ContentType?.MediaType))
		{
			return null;
		}

		var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);

		await using (stream.ConfigureAwait(false))
		{
			return await DataModelConverter.FromJsonAsync(stream, token).ConfigureAwait(false);
		}
	}
}