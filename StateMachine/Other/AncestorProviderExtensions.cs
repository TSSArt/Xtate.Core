// Copyright © 2019-2026 Sergii Artemenko
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

namespace Xtate.Core;

public static class AncestorProviderExtensions
{
	extension<T>(T entity)
	{
		public Ancestor<T> UseAncestor => new(entity);
	}

	public static T As1<T>(this object entity) where T : notnull
	{
		Infra.Requires(entity);

		if (entity.UseAncestor.Is<T>(out var result))
		{
			return result;
		}

		throw new InvalidCastException(Res.Format(Resources.Exception_TypeCantBeFound, typeof(T).Name, entity.GetType().Name));
	}

	extension(object? entity)
	{
		public bool Is1<T>() => entity.Is1<T>(out _);

		public bool Is1<T>([NotNullWhen(true)] [MaybeNullWhen(false)] out T value)
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
	}

	extension<TSource>(ImmutableArray<TSource> array)
	{
		public AncestorArray<TSource> UseAncestor => new(array);
	}

	public readonly ref struct Ancestor<TEntity>(TEntity entity)
	{
		public bool Is<T>() => Is<T>(out _);

		public bool Is<T>([NotNullWhen(true)] [MaybeNullWhen(false)] out T value)
		{
			switch (entity)
			{
				case AncestorContainer { Value: T ancestorValue }:
					value = ancestorValue;

					return true;

				case T tValue:
					value = tValue;

					return true;

				case IAncestorProvider provider:
					return provider.Ancestor.UseAncestor.Is(out value);

				default:
					value = default!;

					return false;
			}
		}

		public T As<T>() where T : notnull
		{
			Infra.Requires(entity);

			if (Is<T>(out var result))
			{
				return result;
			}

			throw new InvalidCastException(Res.Format(Resources.Exception_TypeCantBeFound, typeof(T).Name, entity.GetType().Name));
		}
	}

	public readonly ref struct AncestorArray<T>(ImmutableArray<T> array)
	{
		public ImmutableArray<TDestination> ItemsAs<TDestination>(bool emptyArrayIfDefault = false) where TDestination : notnull
		{
			if (array.IsDefault)
			{
				return emptyArrayIfDefault ? [] : default;
			}

			return ImmutableArray.CreateRange(array, item => item is not null ? item.UseAncestor.As<TDestination>() : default!);
		}
	}
}