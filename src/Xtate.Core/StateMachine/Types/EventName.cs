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

using System.Xml;

namespace Xtate;


public static class EventName
{
	private const char Dot = '.';

	private static readonly IIdentifier DoneIdentifier     = Identifier.FromString("done");
	private static readonly IIdentifier StateIdentifier    = Identifier.FromString("state");
	private static readonly IIdentifier ErrorIdentifier    = Identifier.FromString("error");
	private static readonly IIdentifier InvokeIdentifier   = Identifier.FromString("invoke");
	private static readonly IIdentifier PlatformIdentifier = Identifier.FromString("platform");

	public static readonly ImmutableArray<IIdentifier> ErrorExecution     = [ErrorIdentifier, Identifier.FromString("execution")];
	public static readonly ImmutableArray<IIdentifier> ErrorCommunication = [ErrorIdentifier, Identifier.FromString("communication")];
	public static readonly ImmutableArray<IIdentifier> ErrorPlatform      = [ErrorIdentifier, Identifier.FromString("platform")];

	internal static ImmutableArray<IIdentifier> GetDoneStateNameParts(IIdentifier id) => GetNameParts(DoneIdentifier, StateIdentifier, id.Value);

	internal static ImmutableArray<IIdentifier> GetDoneInvokeNameParts(InvokeId invokeId) => GetNameParts(DoneIdentifier, InvokeIdentifier, invokeId.Value);

	private static ImmutableArray<IIdentifier> GetNameParts(IIdentifier id1, IIdentifier id2, string name)
	{
		Infra.Requires(name);

		var invokeIdPartCount = GetCount(name);
		var parts = new IIdentifier[2 + invokeIdPartCount];

		parts[0] = id1;
		parts[1] = id2;

		SetParts(parts.AsSpan(start: 2, invokeIdPartCount), name);

		return [..parts];
	}

	private static void SetParts(Span<IIdentifier> span, string? id)
	{
		if (id is null)
		{
			return;
		}

		var pos = 0;
		int pos2;
		var index = 0;

		while ((pos2 = id.IndexOf(Dot, pos)) >= 0)
		{
			span[index ++] = (Identifier) id[pos..pos2];

			pos = pos2 + 1;
		}

		span[index] = (Identifier) id[pos..];
	}

	private static int GetCount(string? id)
	{
		if (id is null)
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

	public static ImmutableArray<IIdentifier> GetErrorPlatform(string suffix)
	{
		Infra.Requires(suffix);

		var suffixPartCount = GetCount(suffix);
		var parts = new IIdentifier[2 + suffixPartCount];

		parts[0] = ErrorIdentifier;
		parts[1] = PlatformIdentifier;

		SetParts(parts.AsSpan(start: 2, suffixPartCount), suffix);

		return [..parts];
	}

	public static string? ToName(ImmutableArray<IIdentifier> nameParts)
	{
		if (nameParts.IsDefault)
		{
			return default;
		}

		if (nameParts.IsEmpty)
		{
			return string.Empty;
		}

		return string.Join(separator: @".", nameParts.Select(namePart => namePart.Value));
	}

	public static void WriteXml(XmlWriter writer, ImmutableArray<IIdentifier> nameParts)
	{
		if (nameParts.IsDefaultOrEmpty)
		{
			return;
		}

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
		if (name is null)
		{
			return default;
		}

		if (name.Length == 0)
		{
			return [];
		}

		var length = GetCount(name);

		if (length == 0)
		{
			throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(name));
		}

		var parts = new IIdentifier[length];

		SetParts(parts, name);

		return [..parts];
	}

	public static bool IsError(ImmutableArray<IIdentifier> nameParts) => !nameParts.IsDefaultOrEmpty && Identifier.EqualityComparer.Equals(nameParts[0], ErrorIdentifier);
}