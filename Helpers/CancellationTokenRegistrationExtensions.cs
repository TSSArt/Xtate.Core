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

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Core
{
	[PublicAPI]
	public static class CancellationTokenRegistrationExtensions
	{
		public static ConfiguredAwaitable ConfigureAwait(this CancellationTokenRegistration cancellationTokenRegistration, bool continueOnCapturedContext) =>
				new(cancellationTokenRegistration, continueOnCapturedContext);

#if NET461 || NETSTANDARD2_0
		public static ValueTask DisposeAsync(this CancellationTokenRegistration cancellationTokenRegistration)
		{
			cancellationTokenRegistration.Dispose();

			return default;
		}
#else
		[UsedImplicitly]
		internal static void IgnoreIt(ValueTask _) { }

#endif

		[PublicAPI]
		public struct ConfiguredAwaitable
		{
			private readonly CancellationTokenRegistration _cancellationTokenRegistration;
			private readonly bool                          _continueOnCapturedContext;

			public ConfiguredAwaitable(CancellationTokenRegistration cancellationTokenRegistration, bool continueOnCapturedContext)
			{
				_cancellationTokenRegistration = cancellationTokenRegistration;
				_continueOnCapturedContext = continueOnCapturedContext;
			}

			public ConfiguredValueTaskAwaitable DisposeAsync() => _cancellationTokenRegistration.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
		}
	}
}