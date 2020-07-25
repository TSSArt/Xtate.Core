using System;
using System.Diagnostics.CodeAnalysis;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public sealed class Identifier : LazyId, IIdentifier, IEquatable<IIdentifier>
	{
		private Identifier() { }

		private Identifier(string val) : base(val) { }

	#region Interface IEquatable<IIdentifier>

		public bool Equals(IIdentifier other) => Equals((object) other);

	#endregion

		public static explicit operator Identifier(string val) => FromString(val);

		public static Identifier FromString(string val)
		{
			if (string.IsNullOrEmpty(val)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(val));

			foreach (var ch in val)
			{
				if (char.IsWhiteSpace(ch))
				{
					throw new ArgumentException(Resources.Exception_IdentifierCannotContainsWhitespace, nameof(val));
				}
			}

			return new Identifier(val);
		}

		public static bool TryCreate(string? val, [NotNullWhen(true)] [MaybeNullWhen(false)]
									 out Identifier? identifier)
		{
			if (string.IsNullOrEmpty(val))
			{
				identifier = null;

				return false;
			}

			foreach (var ch in val)
			{
				if (char.IsWhiteSpace(ch))
				{
					identifier = null;

					return false;
				}
			}

			identifier = new Identifier(val);

			return true;
		}

		protected override string GenerateId() => IdGenerator.NewId(GetHashCode());

		public static IIdentifier New() => new Identifier();
	}
}