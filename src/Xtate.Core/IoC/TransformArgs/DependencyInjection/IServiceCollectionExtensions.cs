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

using Xtate.IoC.TransformArgs.Internal;

namespace Xtate.IoC.TransformArgs.DependencyInjection;

internal static class IServiceCollectionExtensions
{
	extension(IServiceCollection services)
	{
		public ServiceSelectorSync<T, Empty> ForServiceSync<T>() where T : notnull => services.ForServiceSync<T, Empty>();

		public ServiceSelectorSync<T, TArg> ForServiceSync<T, TArg>() where T : notnull => new(services);

		public ServiceSelectorSync<T, (TArg1, TArg2)> ForServiceSync<T, TArg1, TArg2>() where T : notnull => services.ForServiceSync<T, (TArg1, TArg2)>();

		public ServiceSelectorSync<T, (TArg1, TArg2, TArg3)> ForServiceSync<T, TArg1, TArg2, TArg3>() where T : notnull => services.ForServiceSync<T, (TArg1, TArg2, TArg3)>();

		public ServiceSelectorSync<T, (TArg1, TArg2, TArg3, TArg4)> ForServiceSync<T, TArg1, TArg2, TArg3, TArg4>() where T : notnull => services.ForServiceSync<T, (TArg1, TArg2, TArg3, TArg4)>();

		public ServiceSelectorAsync<T, Empty> ForService<T>() where T : notnull => services.ForService<T, Empty>();

		public ServiceSelectorAsync<T, TArg> ForService<T, TArg>() where T : notnull => new(services);

		public ServiceSelectorAsync<T, (TArg1, TArg2)> ForService<T, TArg1, TArg2>() where T : notnull => services.ForService<T, (TArg1, TArg2)>();

		public ServiceSelectorAsync<T, (TArg1, TArg2, TArg3)> ForService<T, TArg1, TArg2, TArg3>() where T : notnull => services.ForService<T, (TArg1, TArg2, TArg3)>();

		public ServiceSelectorAsync<T, (TArg1, TArg2, TArg3, TArg4)> ForService<T, TArg1, TArg2, TArg3, TArg4>() where T : notnull => services.ForService<T, (TArg1, TArg2, TArg3, TArg4)>();
	}
}