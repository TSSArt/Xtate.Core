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
#if NET461 || NETSTANDARD2_0
using System.Threading.Tasks;

#endif

namespace Xtate
{
	[PublicAPI]
	public readonly struct ConfiguredStreamAwaitable
	{
		private readonly bool   _continueOnCapturedContext;
		private readonly Stream _stream;

		public ConfiguredStreamAwaitable(Stream stream, bool continueOnCapturedContext)
		{
			_stream = stream;
			_continueOnCapturedContext = continueOnCapturedContext;
		}

		public ConfiguredValueTaskAwaitable DisposeAsync()
		{
#if NET461 || NETSTANDARD2_0
			_stream.Dispose();

			return new ValueTask().ConfigureAwait(_continueOnCapturedContext);
#else
			return _stream.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
#endif
		}
	}
}