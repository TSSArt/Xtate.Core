#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Xtate
{
	internal static class AncestorProviderExtensions
	{
		[return: NotNull]
		public static T As<T>(this object entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (entity.Is<T>(out var result))
			{
				return result;
			}

			throw new InvalidCastException(Res.Format(Resources.Exception_TypeCantBeFound, typeof(T).Name, entity.GetType().Name));
		}

		public static bool Is<T>(this object? entity) => entity.Is<T>(out _);

		public static bool Is<T>(this object? entity, [NotNullWhen(true)] [MaybeNullWhen(false)]
								 out T value)
		{
			while (true)
			{
				switch (entity)
				{
					case AncestorContainer container when container.Value is T val:
						value = val;
						return true;

					case T val:
						value = val;
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

		public static ImmutableArray<TDestination> AsArrayOf<TSource, TDestination>(this ImmutableArray<TSource> array, bool emptyArrayIfDefault = false)
		{
			if (array.IsDefault)
			{
				return emptyArrayIfDefault ? ImmutableArray<TDestination>.Empty : default;
			}

			return ImmutableArray.CreateRange(array, item => item != null ? item.As<TDestination>() : default!);
		}
	}
}