using System;
using System.Collections.Generic;
using System.Threading;

namespace Xtate
{
	[Serializable]
	public sealed class InvokeId : LazyId
	{
		public static readonly IEqualityComparer<InvokeId> InvokeUniqueIdComparer = new InvokeUniqueIdEqualityComparer();

		private readonly IIdentifier? _stateId;
		private          string?      _invokeUniqueId;

		private InvokeId(IIdentifier stateId) => _stateId = stateId;

		private InvokeId(string invokeId) : base(invokeId) { }

		private InvokeId(string invokeId, string invokeUniqueId) : base(invokeId) => _invokeUniqueId = invokeUniqueId;

		public string InvokeUniqueIdValue
		{
			get
			{
				var invokeUniqueId = _invokeUniqueId;

				if (invokeUniqueId == null)
				{
					var newInvokeUniqueId = IdGenerator.NewInvokeUniqueId(GetHashCode());

					invokeUniqueId = Interlocked.CompareExchange(ref _invokeUniqueId, newInvokeUniqueId, comparand: null) ?? newInvokeUniqueId;
				}

				return invokeUniqueId;
			}
		}

		protected override string GenerateId()
		{
			Infrastructure.Assert(_stateId != null);

			return IdGenerator.NewInvokeId(_stateId.Value, GetHashCode());
		}

		public static InvokeId New(IIdentifier stateId, string? invokeId) => invokeId == null ? new InvokeId(stateId) : new InvokeId(invokeId);

		public static InvokeId FromString(string invokeId) => new InvokeId(invokeId);

		public static InvokeId FromString(string invokeId, string invokeUniqueId) => new InvokeId(invokeId, invokeUniqueId);

		private sealed class InvokeUniqueIdEqualityComparer : IEqualityComparer<InvokeId>
		{
		#region Interface IEqualityComparer<InvokeId>

			public bool Equals(InvokeId? x, InvokeId? y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				return x?._invokeUniqueId != null && y?._invokeUniqueId != null && x._invokeUniqueId == y._invokeUniqueId;
			}

			public int GetHashCode(InvokeId obj)
			{
				if (obj == null) throw new ArgumentNullException(nameof(obj));

				var id = obj._invokeUniqueId;

				if (id == null)
				{
					return obj.GetHashCode();
				}

				return TryGetHashFromId(id, out var hash) ? hash : id.GetHashCode();
			}

		#endregion
		}
	}
}