using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	internal static class ListExtensions
	{
		public static bool TrueForAll<T, TArg>(this List<T> list, Func<T, TArg, bool> predicate, TArg arg)
		{
			foreach (var item in list)
			{
				if (!predicate(item, arg))
				{
					return false;
				}
			}

			return true;
		}
		
		public static bool Exists<T, TArg>(this List<T> list, Func<T, TArg, bool> match, TArg arg)
		{
			foreach (var item in list)
			{
				if (match(item, arg))
				{
					return true;
				}
			}

			return false;
		}
		
		public static bool Any<T, TArg>(this ImmutableArray<T> array, Func<T, TArg, bool> predicate, TArg arg)
		{
			foreach (var item in array)
			{
				if (predicate(item, arg))
				{
					return true;
				}
			}

			return false;
		}
	}
}
