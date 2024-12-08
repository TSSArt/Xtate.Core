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
public abstract class InvokeId : ServiceId, IEquatable<InvokeId>
{
	[Serializable]
	internal sealed class Static(string invokeId) : InvokeId(invokeId)
	{
		[ExcludeFromCodeCoverage]
		protected override string GenerateId() => throw new NotSupportedException();

		public override string? InvokeUniqueIdValue => default;
	}

	internal sealed class Execution : InvokeId
	{
		private readonly IIdentifier? _stateId;

		private string? _invokeUniqueId;

		public Execution(string invokeId) : base(invokeId) { }

		public Execution(IIdentifier stateId) => _stateId = stateId;

		public Execution(string invokeId, string invokeUniqueId) : base(invokeId) => _invokeUniqueId = invokeUniqueId;

		public override string InvokeUniqueIdValue
		{
			get
			{
				if (_invokeUniqueId is { } invokeUniqueId)
				{
					return invokeUniqueId;
				}

				var newInvokeUniqueId = IdGenerator.NewInvokeUniqueId(GetHashCode());
				invokeUniqueId = Interlocked.CompareExchange(ref _invokeUniqueId, newInvokeUniqueId, comparand: null) ?? newInvokeUniqueId;

				return invokeUniqueId;
			}
		}

		protected override string GenerateId()
		{
			Infra.NotNull(_stateId);

			return IdGenerator.NewInvokeId(_stateId.Value, GetHashCode());
		}

		public static bool Equals(Execution x, Execution y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			return x?._invokeUniqueId is { } a && y?._invokeUniqueId is { } b && a == b;
		}
	}

	private InvokeId() { }

	private InvokeId(string invokeId) : base(invokeId) { }

	public sealed override string ServiceType => nameof(InvokeId);

	public abstract string? InvokeUniqueIdValue { get; }

	public bool Equals(InvokeId? other) => FastEqualsNoTypeCheck(other) && (this is not Execution a || other is not Execution b || Execution.Equals(a, b));

	public override int GetHashCode() => base.GetHashCode();

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is InvokeId other && Equals(other));

	public static InvokeId FromString([Localizable(false)] string invokeId) => new Static(invokeId);

	public static InvokeId New(IIdentifier stateId, [Localizable(false)] string? invokeId) => invokeId is null ? new Execution(stateId) : new Execution(invokeId);

	public static InvokeId FromString([Localizable(false)] string invokeId, [Localizable(false)] string? invokeUniqueId) => invokeUniqueId is null ? new Static(invokeId) : new Execution(invokeId, invokeUniqueId);
}
