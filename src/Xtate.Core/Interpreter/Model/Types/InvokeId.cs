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

public sealed class UniqueInvokeId : InvokeId
{
    internal UniqueInvokeId(InvokeId? invokeId, IIdentifier stateId) : base(stateId) => InvokeId = invokeId ?? this;

    internal UniqueInvokeId(string uniqueInvokeId) : base(uniqueInvokeId) => InvokeId = this;

    public InvokeId InvokeId { get; }
}

[Serializable]
public class InvokeId : ServiceId, IEquatable<InvokeId>
{
    private readonly IIdentifier? _stateId;

    private InvokeId(IIdentifier stateId, string invokeId) : base(invokeId)
    {
        _stateId = stateId;
        UniqueId = new UniqueInvokeId(this, _stateId);
    }

    private InvokeId(string invokeId, string uniqueInvokeId) : base(invokeId) => UniqueId = new UniqueInvokeId(uniqueInvokeId);

    private protected InvokeId(IIdentifier stateId)
    {
        _stateId = stateId;
        UniqueId = (UniqueInvokeId)this;
    }

    private protected InvokeId(string uniqueInvokeId) : base(uniqueInvokeId) => UniqueId = (UniqueInvokeId)this;

    public override string ServiceType => nameof(InvokeId);

    public UniqueInvokeId UniqueId { get; }

#region Interface IEquatable<InvokeId>

    public bool Equals(InvokeId? other) => FastEqualsNoTypeCheck(other);

#endregion

    protected override string GenerateId() => IdGenerator.NewInvokeId(_stateId!.Value, GetHashCode());

    public override int GetHashCode() => base.GetHashCode();

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is InvokeId other && Equals(other));

    public static InvokeId New(IIdentifier stateId, [Localizable(false)] string? invokeId) => invokeId is null ? new UniqueInvokeId(invokeId: default, stateId) : new InvokeId(stateId, invokeId);

    public static InvokeId FromString([Localizable(false)] string uniqueInvokeId) => new UniqueInvokeId(uniqueInvokeId);

    public static InvokeId FromString([Localizable(false)] string invokeId, [Localizable(false)] string uniqueInvokeId) =>
        invokeId == uniqueInvokeId ? new UniqueInvokeId(uniqueInvokeId) : new InvokeId(invokeId, uniqueInvokeId);
}