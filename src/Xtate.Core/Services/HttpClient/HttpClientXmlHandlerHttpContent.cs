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

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Xtate.Service;

public class HttpClientXmlHandlerHttpContent : HttpContent
{
	private readonly DataModelValue _value;

	public HttpClientXmlHandlerHttpContent(DataModelValue value, string contentType)
	{
		if (string.IsNullOrEmpty(contentType)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(contentType));

		_value = value;

		Headers.ContentType = new MediaTypeHeaderValue(contentType);
	}

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => DataModelConverter.ToXmlAsync(stream, _value);

	protected override bool TryComputeLength(out long length)
	{
		length = 0;

		return false;
	}
}