#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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

namespace Xtate
{
	internal static class IdGenerator
	{
		public static string NewSendId(int hash) => NewGuidWithHash(hash);

		public static string NewSessionId(int hash) => NewGuidWithHash(hash);

		public static string NewInvokeUniqueId(int hash) => NewGuidWithHash(hash);

		public static string NewId(int hash) => NewGuidWithHash(hash);

#if NETSTANDARD2_1
		public static string NewInvokeId(string id, int hash) =>
				string.Create(46 + id.Length, (id, hash), (span, arg) =>
														  {
															  arg.id.AsSpan().CopyTo(span);
															  span[arg.id.Length] = '.';
															  span = span.Slice(arg.id.Length + 1);
															  Guid.NewGuid().TryFormat(span, out var pos, format: "D");
															  span[pos] = '-';
															  hash.TryFormat(span.Slice(pos + 1), out pos, format: "x8");
														  });

		private static string NewGuidWithHash(int hash) =>
				string.Create(length: 45, hash, (span, h) =>
												{
													Guid.NewGuid().TryFormat(span, out var pos, format: "D");
													span[pos] = '-';
													hash.TryFormat(span.Slice(pos + 1), out pos, format: "x8");
												});
#else
		public static string NewInvokeId(string id, int hash) => id + "." + Guid.NewGuid().ToString("D") + "-" + hash.ToString("x8");

		private static string NewGuidWithHash(int hash) => Guid.NewGuid().ToString("D") + "-" + hash.ToString("x8");
#endif
	}
}