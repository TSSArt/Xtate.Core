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

using System.IO;
using System.Runtime.CompilerServices;
using Xtate.Annotations;
#if !NET5_0
using System.Threading.Tasks;

#endif

namespace Xtate
{
	[PublicAPI]
	public static class StreamExtensions
	{
		public static ConfiguredStreamAwaitable ConfigureAwait(this Stream stream, bool continueOnCapturedContext) => new ConfiguredStreamAwaitable(stream, continueOnCapturedContext);
	}

	[PublicAPI]
	public readonly struct ConfiguredStreamAwaitable
	{
		private readonly Stream _stream;
		private readonly bool   _continueOnCapturedContext;

		public ConfiguredStreamAwaitable(Stream stream, bool continueOnCapturedContext)
		{
			_stream = stream;
			_continueOnCapturedContext = continueOnCapturedContext;
		}

#if NET5_0
		public ConfiguredValueTaskAwaitable DisposeAsync() => _stream.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
#else
		public ConfiguredValueTaskAwaitable DisposeAsync()
		{
			_stream.Dispose();

			return new ValueTask().ConfigureAwait(_continueOnCapturedContext);
		}
#endif
	}
}