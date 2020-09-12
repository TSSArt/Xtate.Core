#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Net.Mime;
using System.Text;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public class Resource
	{
		private byte[]? _bytes;
		private string? _content;

		public Resource(Uri uri, ContentType? contentType = default, DateTimeOffset? modifiedDate = default, string? content = default, byte[]? bytes = default)
		{
			Uri = uri;
			ContentType = contentType;
			ModifiedDate = modifiedDate;
			_content = content;
			_bytes = bytes;
		}

		public Uri             Uri          { get; }
		public ContentType?    ContentType  { get; }
		public DateTimeOffset? ModifiedDate { get; }

		public string? Content
		{
			get
			{
				if (_content is { })
				{
					return _content;
				}

				if (_bytes is { })
				{
					var encoding = !string.IsNullOrEmpty(ContentType?.CharSet) ? Encoding.GetEncoding(ContentType.CharSet) : Encoding.UTF8;

					_content = encoding.GetString(_bytes);
				}

				return _content;
			}
		}

		public byte[]? GetBytes()
		{
			if (_bytes is { })
			{
				return _bytes;
			}

			if (_content is { })
			{
				var encoding = !string.IsNullOrEmpty(ContentType?.CharSet) ? Encoding.GetEncoding(ContentType.CharSet) : Encoding.UTF8;

				_bytes = encoding.GetBytes(_content);
			}

			return _bytes;
		}
	}
}