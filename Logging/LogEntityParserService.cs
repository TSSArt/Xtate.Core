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
	public required IAsyncEnumerable<IEntityParserProvider<TSource>> Providers { private get; [UsedImplicitly] init; }

#region Interface IEntityParserHandler<TSource>

	public async IAsyncEnumerable<LoggingParameter> EnumerateProperties<T>(T entity)
	{
		await foreach (var provider in Providers.ConfigureAwait(false))
		{
			if (provider.TryGetEntityParserHandler(entity) is { } handler)
			{
				await foreach (var property in handler.EnumerateProperties(entity).ConfigureAwait(false))
				{
					yield return property;
				}

				yield break;
			}
		}

		throw new InvalidOperationException(Res.Format(Resources.Exception_CantFindEntityParser, typeof(T)));
	}

#endregion
}