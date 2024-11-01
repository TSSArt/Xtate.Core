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

namespace Xtate.ExternalService.HttpClient;

public abstract class HttpClientMimeTypeHandler
{
	protected static bool ContentTypeEquals(string? contentTypeA, string? contentTypeB)
	{
		if (string.IsNullOrEmpty(contentTypeA) || string.IsNullOrEmpty(contentTypeB))
		{
			return false;
		}

		var lengthA = contentTypeA.IndexOf(';');

		if (lengthA < 0)
		{
			lengthA = contentTypeA.Length;
		}

		var lengthB = contentTypeB.IndexOf(';');

		if (lengthB < 0)
		{
			lengthB = contentTypeB.Length;
		}

		return lengthA == lengthB && string.Compare(contentTypeA, indexA: 0, contentTypeB, indexB: 0, lengthA, StringComparison.OrdinalIgnoreCase) == 0;
	}

	protected static void AppendAcceptHeader(WebRequest webRequest, string contentType)
	{
		if (string.IsNullOrEmpty(contentType)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(contentType));

		if (webRequest is HttpWebRequest httpWebRequest)
		{
			var acceptHeaderValue = httpWebRequest.Accept;

			AppendAcceptHeader(ref acceptHeaderValue, contentType);

			httpWebRequest.Accept = acceptHeaderValue;
		}
	}

	protected static void AppendAcceptHeader(ref string? acceptHeaderValue, string contentType)
	{
		if (string.IsNullOrEmpty(contentType)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(contentType));

		if (acceptHeaderValue is not { Length: > 0 } accept)
		{
			acceptHeaderValue = contentType;

			return;
		}

		var state = 0;
		var start = 0;
		var length = contentType.Length;

		for (var i = 0; i < accept.Length; i ++)
		{
			switch (state)
			{
				case 0:
					if (char.IsWhiteSpace(accept[i]))
					{
						continue;
					}

					start = i;
					state = 1;
					goto case 1;

				case 1:
					if (accept[i] is not ',' and not ';')
					{
						continue;
					}

					if (length == i - start && string.Compare(accept, start, contentType, indexB: 0, length, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return;
					}

					state = 2;
					goto case 1;

				case 2:
					if (accept[i] != ',')
					{
						continue;
					}

					state = 0;

					continue;
			}
		}

		if (state == 1 && length == accept.Length - start && string.Compare(accept, start, contentType, indexB: 0, length, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return;
		}

		acceptHeaderValue = acceptHeaderValue + @", " + contentType;
	}

	public virtual void PrepareRequest(WebRequest webRequest,
									   string? contentType,
									   DataModelList parameters,
									   DataModelValue value) { }

	public virtual HttpContent? TryCreateHttpContent(WebRequest webRequest,
													 string? contentType,
													 DataModelList parameters,
													 DataModelValue value) =>
		default;

	public virtual ValueTask<DataModelValue?> TryParseResponseAsync(WebResponse webResponse, DataModelList parameters, CancellationToken token) => default;
}