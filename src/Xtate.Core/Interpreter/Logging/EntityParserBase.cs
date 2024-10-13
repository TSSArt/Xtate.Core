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
	public required Ancestor<ILogger<TSource>> Logger { private get; [UsedImplicitly] init; }

#region Interface IEntityParserHandler<TSource>

	IEnumerable<LoggingParameter> IEntityParserHandler<TSource>.EnumerateProperties<T>(T entity) => EnumerateProperties(ConvertHelper<T, TEntity>.Convert(entity));

#endregion

#region Interface IEntityParserProvider<TSource>

	IEntityParserHandler<TSource>? IEntityParserProvider<TSource>.TryGetEntityParserHandler<T>(T entity) => entity is TEntity ? this : default;

#endregion

	protected bool IsVerboseLogging => Logger().IsEnabled(Level.Verbose);

	protected abstract IEnumerable<LoggingParameter> EnumerateProperties(TEntity entity);
}