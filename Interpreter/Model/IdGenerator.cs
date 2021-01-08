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

using System;
using System.ComponentModel;
using System.Text;
using Xtate.Annotations;

namespace Xtate.Core
{
	[PublicAPI]
	internal static class IdGenerator
	{
		public static string NewSendId(int hash) => NewGuidWithHash(hash);

		public static string NewSessionId(int hash) => NewGuidWithHash(hash);

		public static string NewInvokeUniqueId(int hash) => NewGuidWithHash(hash);

		public static string NewId(int hash) => NewGuidWithHash(hash);

#if NET461 || NETSTANDARD2_0
		public static string NewInvokeId([Localizable(false)] string id, int hash) =>
				new StringBuilder(id.Length + 33)
						.Append(id)
						.Append('.')
						.Append(Guid.NewGuid().ToString("N"), startIndex: 0, count: 24)
						.Append(hash.ToString(@"x8")).ToString();

		private static string NewGuidWithHash(int hash) =>
				new StringBuilder(32)
						.Append(Guid.NewGuid().ToString("N"), startIndex: 0, count: 24)
						.Append(hash.ToString(@"x8")).ToString();
#else
		[UsedImplicitly]
		private static void IgnoreIt(StringBuilder _) { }

		public static string NewInvokeId([Localizable(false)] string id, int hash) =>
				string.Create(33 + id.Length, (id, hash), static(span, arg) =>
														  {
															  arg.id.AsSpan().CopyTo(span);
															  span[arg.id.Length] = '.';
															  span = span[(arg.id.Length + 1)..];
															  Guid.NewGuid().TryFormat(span, out var pos, format: @"N");
															  arg.hash.TryFormat(span[(pos - 8)..], out pos, format: @"x8");
														  });

		private static string NewGuidWithHash(int hash) =>
				string.Create(length: 32, hash, static(span, h) =>
												{
													Guid.NewGuid().TryFormat(span, out var pos, format: @"N");
													h.TryFormat(span[(pos - 8)..], out pos, format: @"x8");
												});
#endif
	}
}