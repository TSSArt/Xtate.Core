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

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Xtate.DataModel.Services;
using Xtate.DataTypes;
using Xtate.IoProcessors.Http.Internal;
using Xtate.ResourceLoaders.Extensions;

namespace Xtate.ExternalServices.HttpClient.Services;

[InstantiatedByIoC]
public class HttpClientFormUrlEncodedHandler : HttpClientMimeTypeHandler
{
	private const string MediaTypeApplicationFormUrlEncoded = "application/x-www-form-urlencoded";

	private static readonly MediaTypeWithQualityHeaderValue AcceptMediaType = new(MediaTypeApplicationFormUrlEncoded, quality: 0.5);

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

	private static bool CanHandle(string? mediaType) => mediaType is not null && string.Equals(mediaType, MediaTypeApplicationFormUrlEncoded, StringComparison.OrdinalIgnoreCase);

	public override HttpContent? TryCreateHttpContent(HttpRequestMessage request,
													  ContentType? contentType,
													  DataModelList parameters,
													  DataModelValue value)
	{
		if (!CanHandle(contentType?.MediaType))
		{
			return null;
		}

		var list = value.AsListOrEmpty();

		var pairs = DataModelConverter.IsObject(list)
			? from pair in list.KeyValues select (Name: pair.Key, Value: pair.Value.AsStringOrDefault())
			: from item in list let pair = item.AsListOrEmpty() select (Name: pair["name"].AsStringOrDefault(), Value: pair["value"].AsStringOrDefault());

		var forms = from pair in pairs
					where !string.IsNullOrEmpty(pair.Name) && pair.Value is not null
					select new KeyValuePair<string?, string?>(pair.Name, pair.Value);

		return new FormUrlEncodedContent(forms);
	}

	public override async ValueTask<DataModelValue?> TryParseResponseAsync(HttpResponseMessage response, DataModelList parameters, CancellationToken token)
	{
		if (!CanHandle(response.Content.Headers.ContentType?.MediaType))
		{
			return null;
		}

		var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);

		await using (stream.ConfigureAwait(false))
		{
			var bytes = await stream.ReadToEndAsync(token).ConfigureAwait(false);

			var queryString = Encoding.ASCII.GetString(bytes);
			var collection = QueryStringHelper.ParseQuery(queryString);

			var list = new DataModelList();

			for (var i = 0; i < collection.Count; i ++)
			{
				if (collection.GetKey(i) is not { } key)
				{
					continue;
				}

				if (collection.GetValues(i) is { } values)
				{
					foreach (var value in values)
					{
						list.Add(key, value);
					}
				}
			}

			return list;
		}
	}
}