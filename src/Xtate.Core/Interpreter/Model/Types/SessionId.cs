// Copyright © 2019-2025 Sergii Artemenko
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
public sealed class SessionId : ServiceId, IEquatable<SessionId>
{
    private static readonly SessionId Empty = FromString(string.Empty);

    private SessionId() { }

    private SessionId(string value) : base(value) { }

    public override string ServiceType => nameof(SessionId);

#region Interface IEquatable<SessionId>

    public bool Equals(SessionId? other) => FastEqualsNoTypeCheck(other);

#endregion

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is SessionId other && Equals(other));

    public override int GetHashCode() => base.GetHashCode();

    protected override string GenerateId() => IdGenerator.NewSessionId(GetHashCode());

    public static SessionId New() => new();

    public static SessionId FromString([Localizable(false)] string value) => new(value);

    public static bool IsNullOrEmpty([NotNullWhen(false)] SessionId? sessionId) => sessionId is null || sessionId == Empty;
}