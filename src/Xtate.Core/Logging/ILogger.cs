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

public interface ILogger
{
	IFormatProvider? FormatProvider { get; }

	bool IsEnabled(Level level);
}

public interface ILogger<[UsedImplicitly] TSource> : ILogger
{
	ValueTask Write(Level level, int eventId, string? message);

	ValueTask Write<TEntity>(Level level,
							 int eventId,
							 string? message,
							 TEntity entity);

	ValueTask Write(Level level, int eventId, [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage);

	ValueTask Write<TEntity>(Level level,
							 int eventId,
							 [InterpolatedStringHandlerArgument("", "level")] LoggingInterpolatedStringHandler formattedMessage,
							 TEntity entity);
}