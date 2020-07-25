using System;
using System.Collections.Generic;

namespace Xtate
{
	internal class FullUriComparer : IEqualityComparer<Uri>
	{
		public static readonly FullUriComparer Instance = new FullUriComparer();

	#region Interface IEqualityComparer<Uri>

		public bool Equals(Uri? x, Uri? y) => x == y && GetSafeFragment(x) == GetSafeFragment(y);

		public int GetHashCode(Uri uri) => (uri.GetHashCode() * 397) ^ GetSafeFragment(uri).GetHashCode();

	#endregion

		private static string GetSafeFragment(Uri? uri) => uri != null && uri.IsAbsoluteUri ? uri.Fragment : string.Empty;
	}
}