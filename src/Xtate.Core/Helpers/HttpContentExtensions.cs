#region Copyright © 2019-2021 Sergii Artemenko

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

#if !NET6_0_OR_GREATER
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate.Core
{
	[PublicAPI]
	public static class HttpContentExtensions
	{
		public static Task CopyToAsync(this HttpContent httpContent, Stream stream, CancellationToken token)
		{
			if (httpContent is null) throw new ArgumentNullException(nameof(httpContent));

			return httpContent.CopyToAsync(stream.InjectCancellationToken(token));
		}
	}
}
#endif