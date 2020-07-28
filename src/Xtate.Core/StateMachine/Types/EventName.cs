#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;

namespace Xtate
{
	public static class EventName
	{
		private const char Dot = '.';

		private static readonly IIdentifier DoneIdentifier   = (Identifier) @"done";
		private static readonly IIdentifier StateIdentifier  = (Identifier) @"state";
		private static readonly IIdentifier ErrorIdentifier  = (Identifier) @"error";
		private static readonly IIdentifier InvokeIdentifier = (Identifier) @"invoke";

		public static readonly ImmutableArray<IIdentifier> ErrorExecution     = ImmutableArray.Create(ErrorIdentifier, (Identifier) @"execution");
		public static readonly ImmutableArray<IIdentifier> ErrorCommunication = ImmutableArray.Create(ErrorIdentifier, (Identifier) @"communication");
		public static readonly ImmutableArray<IIdentifier> ErrorPlatform      = ImmutableArray.Create(ErrorIdentifier, (Identifier) @"platform");

		internal static ImmutableArray<IIdentifier> GetDoneStateNameParts(IIdentifier id) => GetNameParts(DoneIdentifier, StateIdentifier, id.Value);

		internal static ImmutableArray<IIdentifier> GetDoneInvokeNameParts(InvokeId invokeId) => GetNameParts(DoneIdentifier, InvokeIdentifier, invokeId.Value);

		private static ImmutableArray<IIdentifier> GetNameParts(IIdentifier id1, IIdentifier id2, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var invokeIdPartCount = GetCount(name);
			var parts = new IIdentifier[2 + invokeIdPartCount];

			parts[0] = id1;
			parts[1] = id2;

			SetParts(parts.AsSpan(start: 2, invokeIdPartCount), name);

			return ImmutableArray.Create(parts);
		}

		private static void SetParts(Span<IIdentifier> span, string? id)
		{
			if (id == null)
			{
				return;
			}

			var pos = 0;
			int pos2;
			var index = 0;

			while ((pos2 = id.IndexOf(Dot, pos)) >= 0)
			{
				span[index ++] = (Identifier) id.Substring(pos, pos2 - pos);

				pos = pos2 + 1;
			}

			span[index] = (Identifier) id.Substring(pos);
		}

		private static int GetCount(string? id)
		{
			if (id == null)
			{
				return 0;
			}

			var count = 1;
			var pos = 0;

			while ((pos = id.IndexOf(Dot, pos) + 1) > 0)
			{
				count ++;
			}

			return count;
		}

		public static string ToName(ImmutableArray<IIdentifier> nameParts)
		{
			if (nameParts.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCantBeEmpty, nameof(nameParts));

			return string.Join(separator: @".", nameParts.Select(p => p.Value));
		}

		public static void WriteXml(XmlWriter writer, ImmutableArray<IIdentifier> nameParts)
		{
			if (writer == null) throw new ArgumentNullException(nameof(writer));
			if (nameParts.IsDefaultOrEmpty) throw new ArgumentException(Resources.Exception_ValueCantBeEmpty, nameof(nameParts));

			var writeDelimiter = false;
			foreach (var part in nameParts)
			{
				if (writeDelimiter)
				{
					writer.WriteString(@".");
				}

				writer.WriteString(part.Value);

				writeDelimiter = true;
			}
		}

		public static ImmutableArray<IIdentifier> ToParts(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));

			if (name == null) throw new ArgumentNullException(nameof(name));

			var length = GetCount(name);

			if (length == 0)
			{
				throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));
			}

			var parts = new IIdentifier[length];

			SetParts(parts, name);

			return ImmutableArray.Create(parts);
		}

		public static bool IsError(ImmutableArray<IIdentifier> nameParts) => !nameParts.IsDefaultOrEmpty && nameParts[0] == ErrorIdentifier;
	}
}