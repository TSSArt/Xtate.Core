using System;
using System.Collections.Generic;

namespace Xtate
{
	internal sealed class IdentifierEqualityComparer : IEqualityComparer<IIdentifier>
	{
		public static readonly IEqualityComparer<IIdentifier> Instance = new IdentifierEqualityComparer();

	#region Interface IEqualityComparer<IIdentifier>

		public bool Equals(IIdentifier x, IIdentifier y) => x.As<IEquatable<IIdentifier>>().Equals(y.As<IEquatable<IIdentifier>>());

		public int GetHashCode(IIdentifier obj) => obj.As<IEquatable<IIdentifier>>().GetHashCode();

	#endregion
	}
}