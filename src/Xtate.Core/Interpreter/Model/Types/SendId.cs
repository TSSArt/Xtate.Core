// Copyright © 2019-2024 Sergii Artemenko
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

using System.ComponentModel;

namespace Xtate;

[Serializable]
public sealed class SendId : LazyId, IEquatable<SendId>
{
	private SendId() { }

	private SendId(string value) : base(value) { }

#region Interface IEquatable<SendId>

	public bool Equals(SendId? other) => FastEqualsNoTypeCheck(other);

#endregion

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is SendId other && Equals(other));

	public override int GetHashCode() => base.GetHashCode();

	protected override string GenerateId() => IdGenerator.NewSendId(GetHashCode());

	public static SendId New() => new();

	[return: NotNullIfNotNull(nameof(value))]
	public static SendId? FromString([Localizable(false)] string? value) => value is not null ? new SendId(value) : null;
}