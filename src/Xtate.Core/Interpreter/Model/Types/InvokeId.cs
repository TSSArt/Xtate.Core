﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Collections.Generic;
using System.Threading;
using Xtate.Core;

namespace Xtate
{
	[PublicAPI]
	[Serializable]
	public sealed class InvokeId : ServiceId, IEquatable<InvokeId>
	{
		internal static readonly IEqualityComparer<InvokeId> InvokeUniqueIdComparer = new InvokeUniqueIdEqualityComparer();

		private readonly IIdentifier? _stateId;

		private string? _invokeUniqueId;

		private InvokeId(IIdentifier stateId) => _stateId = stateId;

		private InvokeId(string invokeId) : base(invokeId) { }

		private InvokeId(string invokeId, string invokeUniqueId) : base(invokeId) => _invokeUniqueId = invokeUniqueId;

		public string InvokeUniqueIdValue
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

	#region Interface IEquatable<InvokeId>

		public bool Equals(InvokeId? other) => SameTypeEquals(other);

	#endregion

		protected override string GenerateId()
		{
			Infra.NotNull(_stateId);

			return IdGenerator.NewInvokeId(_stateId.Value, GetHashCode());
		}

		public static InvokeId New(IIdentifier stateId, string? invokeId) => invokeId is null ? new InvokeId(stateId) : new InvokeId(invokeId);

		public static InvokeId FromString(string invokeId) => new(invokeId);

		public static InvokeId FromString(string invokeId, string invokeUniqueId) => new(invokeId, invokeUniqueId);

		internal sealed class InvokeUniqueIdEqualityComparer : IEqualityComparer<InvokeId>
		{
		#region Interface IEqualityComparer<InvokeId>

			public bool Equals(InvokeId? x, InvokeId? y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				return x?._invokeUniqueId is { } a && y?._invokeUniqueId is { } b && a == b;
			}

			public int GetHashCode(InvokeId obj)
			{
				if (obj is null) throw new ArgumentNullException(nameof(obj));

				if (obj._invokeUniqueId is { } id)
				{
					return TryGetHashFromId(id, out var hash) ? hash : id.GetHashCode();
				}

				return obj.GetHashCode();
			}

		#endregion
		}
	}
}