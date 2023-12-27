#region Copyright © 2019-2023 Sergii Artemenko

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

namespace Xtate.Core;

internal static class AncestorProviderExtensions
{
	public static T As<T>(this object entity) where T : notnull
	{
		if (entity.Is<T>(out var result))
		{
			return result;
		}

		throw new InvalidCastException(Res.Format(Resources.Exception_TypeCantBeFound, typeof(T).Name, entity.GetType().Name));
	}

	public static bool Is<T>(this object? entity) => entity.Is<T>(out _);

	public static bool Is<T>(this object? entity,
							 [NotNullWhen(true)] [MaybeNullWhen(false)]
							 out T value)
	{
		while (true)
		{
			switch (entity)
			{
				case null:
					value = default!;
					return false;

				case AncestorContainer { Value: T ancestorValue }:
					value = ancestorValue;
					return true;

				case T tValue:
					value = tValue;
					return true;

				case IAncestorProvider provider:
					entity = provider.Ancestor;
					break;

				default:
					value = default!;
					return false;
			}
		}
	}

	public static ImmutableArray<TDestination> AsArrayOf<TSource, TDestination>(this ImmutableArray<TSource> array, bool emptyArrayIfDefault = false) where TDestination : notnull
	{
		if (array.IsDefault)
		{
			return emptyArrayIfDefault ? [] : default;
		}

		return ImmutableArray.CreateRange(array, item => item is not null ? item.As<TDestination>() : default!);
	}
}