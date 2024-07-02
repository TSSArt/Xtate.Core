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

using System.Collections.Specialized;
using System.Text;
using System.Text.Encodings.Web;

namespace Xtate.Core;

internal static class QueryStringHelper
{
	public static string AddQueryString(string uri, string name, string value)
	{
		var uriToBeAppended = uri;
		var anchorText = string.Empty;

		var anchorIndex = uri.IndexOf('#');
		if (anchorIndex >= 0)
		{
			anchorText = uri[anchorIndex..];
			uriToBeAppended = uri[..anchorIndex];
		}

		return new StringBuilder(uriToBeAppended)
			   .Append(uriToBeAppended.IndexOf('?') >= 0 ? '&' : '?')
			   .Append(UrlEncoder.Default.Encode(name))
			   .Append('=')
			   .Append(UrlEncoder.Default.Encode(value))
			   .Append(anchorText)
			   .ToString();
	}

	public static NameValueCollection ParseQuery(string? query)
	{
		var collection = new NameValueCollection();

		if (string.IsNullOrEmpty(query) || query == @"?")
		{
			return collection;
		}

		var scanIndex = 0;
		if (query[0] == '?')
		{
			scanIndex = 1;
		}

		var textLength = query.Length;
		var equalIndex = query.IndexOf('=');
		if (equalIndex < 0)
		{
			equalIndex = textLength;
		}

		while (scanIndex < textLength)
		{
			var delimiterIndex = query.IndexOf(value: '&', scanIndex);
			if (delimiterIndex < 0)
			{
				delimiterIndex = textLength;
			}

			if (equalIndex < delimiterIndex)
			{
				while (scanIndex != equalIndex && char.IsWhiteSpace(query[scanIndex]))
				{
					++ scanIndex;
				}

				var name = UnescapeDataString(query[scanIndex..equalIndex]);
				var value = UnescapeDataString(query[(equalIndex + 1)..delimiterIndex]);

				collection.Add(name, value);

				equalIndex = query.IndexOf(value: '=', delimiterIndex);
				if (equalIndex < 0)
				{
					equalIndex = textLength;
				}
			}
			else
			{
				if (delimiterIndex > scanIndex)
				{
					var name = UnescapeDataString(query[scanIndex..delimiterIndex]);
					collection.Add(name, string.Empty);
				}
			}

			scanIndex = delimiterIndex + 1;
		}

		return collection;
	}

	private static string UnescapeDataString(string value) => Uri.UnescapeDataString(value.Replace(oldChar: '+', newChar: ' '));
}