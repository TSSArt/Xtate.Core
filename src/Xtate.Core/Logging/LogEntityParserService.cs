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

public class LogEntityParserService<TSource> : IEntityParserHandler<TSource>
{
	public required ServiceList<IEntityParserProvider<TSource>> Providers { private get; [UsedImplicitly] init; }

#region Interface IEntityParserHandler<TSource>

	public IEnumerable<LoggingParameter> EnumerateProperties<T>(T entity)
	{
		foreach (var provider in Providers)
		{
			if (provider.TryGetEntityParserHandler(entity) is { } handler)
			{
				return handler.EnumerateProperties(entity);
			}
		}

		throw new InvalidOperationException(Res.Format(Resources.Exception_CantFindEntityParser, typeof(T)));
	}

#endregion
}