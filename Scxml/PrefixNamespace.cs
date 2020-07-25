using System;

namespace Xtate.Scxml
{
	public readonly struct PrefixNamespace : IEquatable<PrefixNamespace>
	{
		public PrefixNamespace(string prefix, string ns)
		{
			Prefix = prefix;
			Namespace = ns;
		}

		public string Prefix { get; }

		public string Namespace { get; }

	#region Interface IEquatable<PrefixNamespace>

		public bool Equals(PrefixNamespace other) => Prefix == other.Prefix && Namespace == other.Namespace;

	#endregion

		public override bool Equals(object? obj) => obj is PrefixNamespace other && Equals(other);

		public override int GetHashCode() => unchecked((Prefix.GetHashCode() * 397) ^ Namespace.GetHashCode());

		public static bool operator ==(PrefixNamespace left, PrefixNamespace right) => left.Equals(right);

		public static bool operator !=(PrefixNamespace left, PrefixNamespace right) => !left.Equals(right);
	}
}