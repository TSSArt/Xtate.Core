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
using System.Net.Mime;
using System.Text;
using Xtate.DataModel.Services;
using Xtate.DataTypes;

namespace Xtate.Http;

public class XmlHttpContent : HttpContent
{
	private const DataModelConverter.XmlOptions DefaultXmlOptions = DataModelConverter.XmlOptions.None;

	private readonly DataModelValue _value;

	public XmlHttpContent(DataModelValue value)
	{
		_value = value;

		Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Text.Xml) { CharSet = Encoding.UTF8.WebName };
	}

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => DataModelConverter.ToXmlAsync(stream, _value);

	protected override bool TryComputeLength(out long length)
	{
		length = 0;

		return false;
	}

#if NET5_0_OR_GREATER

	protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken) => DataModelConverter.ToXml(stream, _value);

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken token) => DataModelConverter.ToXmlAsync(stream, _value, DefaultXmlOptions, token);

#endif
}