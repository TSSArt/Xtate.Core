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

using System.Net;
using System.Net.Http;
using System.Text;

namespace Xtate.Service;

public class HttpClientFormUrlEncodedHandler : HttpClientMimeTypeHandler
{
	private const string MediaTypeApplicationFormUrlEncoded = "application/x-www-form-urlencoded";

	private HttpClientFormUrlEncodedHandler() { }

	public static HttpClientMimeTypeHandler Instance { get; } = new HttpClientFormUrlEncodedHandler();

	public override HttpContent? TryCreateHttpContent(WebRequest webRequest,
													  string? contentType,
													  DataModelList parameters,
													  DataModelValue value)
	{
		if (!ContentTypeEquals(contentType, MediaTypeApplicationFormUrlEncoded))
		{
			return default;
		}

		var list = value.AsListOrEmpty();

		var pairs = DataModelConverter.IsObject(list)
			? from pair in list.KeyValues select (Name: pair.Key, Value: pair.Value.AsStringOrDefault())
			: from item in list select (Name: item.AsListOrEmpty()["name"].AsStringOrDefault(), Value: item.AsListOrEmpty()["value"].AsStringOrDefault());

		var forms = from pair in pairs
					where !string.IsNullOrEmpty(pair.Name) && pair.Value is not null
					select new KeyValuePair<string?, string?>(pair.Name, pair.Value);

		return new FormUrlEncodedContent(forms);
	}

	public override async ValueTask<DataModelValue?> TryParseResponseAsync(WebResponse webResponse, DataModelList parameters, CancellationToken token)
	{
		if (webResponse is null) throw new ArgumentNullException(nameof(webResponse));

		if (!ContentTypeEquals(webResponse.ContentType, MediaTypeApplicationFormUrlEncoded))
		{
			return default;
		}

		var stream = webResponse.GetResponseStream();

		Infra.NotNull(stream);

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