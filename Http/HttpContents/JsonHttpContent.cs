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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Xtate.DataModel.Services;
using Xtate.DataTypes;

namespace Xtate.Http;

public class JsonHttpContent : HttpContent
{
	private const string MediaTypeApplicationJson = "application/json";

	private const DataModelConverter.JsonOptions DefaultJsonOptions = DataModelConverter.JsonOptions.UndefinedToSkipOrNull;

	private readonly DataModelValue _value;

	public JsonHttpContent(DataModelValue value)
	{
		_value = value;

		Headers.ContentType = new MediaTypeHeaderValue(MediaTypeApplicationJson) { CharSet = Encoding.UTF8.WebName };
	}

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => DataModelConverter.ToJsonAsync(stream, _value, DefaultJsonOptions);

	protected override bool TryComputeLength(out long length)
	{
		length = 0;

		return false;
	}

#if NET5_0_OR_GREATER

	protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken) => DataModelConverter.ToJson(stream, _value, DefaultJsonOptions);

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken token) => DataModelConverter.ToJsonAsync(stream, _value, DefaultJsonOptions, token);

#endif
}