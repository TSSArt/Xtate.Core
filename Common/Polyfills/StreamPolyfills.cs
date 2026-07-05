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

#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1

// ReSharper disable once CheckNamespace
namespace System.IO;

internal static class StreamPolyfills
{
	extension(Stream stream)
	{
		public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext) => new(stream, continueOnCapturedContext);

		public ValueTask DisposeAsync()
		{
			stream.Dispose();

			return ValueTask.CompletedTask;
		}
	}

	public readonly struct ConfiguredAwaitable(Stream stream, bool continueOnCapturedContext)
	{
		public ConfiguredValueTaskAwaitable DisposeAsync() => stream.DisposeAsync().ConfigureAwait(continueOnCapturedContext);
	}
}

#endif