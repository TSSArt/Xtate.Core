// Copyright © 2019-2024 Sergii Artemenko
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

public abstract class EntityParserBase<TSource, TEntity> : IEntityParserProvider<TSource>, IEntityParserHandler<TSource>
{
	public required ILogger<TSource> Logger { private get; [UsedImplicitly] init; }

#region Interface IEntityParserHandler<TSource>

	async IAsyncEnumerable<LoggingParameter> IEntityParserHandler<TSource>.EnumerateProperties<T>(T entity)
	{
		var entity2 = ConvertHelper<T, TEntity>.Convert(entity);

		if (EnumerateProperties(entity2) is { } enumerable)
		{
			foreach (var property in enumerable)
			{
				yield return property;
			}
		}

		if (EnumeratePropertiesAsync(entity2) is { } enumerableAsync)
		{
			await foreach (var property in enumerableAsync.ConfigureAwait(false))
			{
				yield return property;
			}
		}

		if (Logger.IsEnabled(Level.Verbose))
		{
			if (EnumerateVerboseProperties(entity2) is { } verboseEnumerable)
			{
				foreach (var property in verboseEnumerable)
				{
					yield return property;
				}
			}

			if (EnumerateVerbosePropertiesAsync(entity2) is { } verboseEnumerableAsync)
			{
				await foreach (var property in verboseEnumerableAsync.ConfigureAwait(false))
				{
					yield return property;
				}
			}
		}
	}

#endregion

#region Interface IEntityParserProvider<TSource>

	public virtual IEntityParserHandler<TSource>? TryGetEntityParserHandler<T>(T entity) => entity is TEntity ? this : default;

#endregion

	protected virtual IAsyncEnumerable<LoggingParameter>? EnumeratePropertiesAsync(TEntity entity) => default;

	protected virtual IEnumerable<LoggingParameter>? EnumerateProperties(TEntity entity) => default;

	protected virtual IAsyncEnumerable<LoggingParameter>? EnumerateVerbosePropertiesAsync(TEntity entity) => default;

	protected virtual IEnumerable<LoggingParameter>? EnumerateVerboseProperties(TEntity entity) => default;
}