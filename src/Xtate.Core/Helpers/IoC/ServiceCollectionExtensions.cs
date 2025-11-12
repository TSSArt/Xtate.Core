// Copyright © 2019-2025 Sergii Artemenko
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

namespace Xtate.IoC;

public static class ServiceCollectionExtensions
{
    private class ConfigureSync<T>(Action<T> setOptions) : IConfigureOptions<T>
    {
    #region Interface IConfigureOptions<T>

        public ValueTask Configure(T options)
        {
            setOptions(options);

            return ValueTask.CompletedTask;
        }

    #endregion
    }

    private class ConfigureAsync<T>(Func<T, ValueTask> setOptions) : IConfigureOptions<T>
    {
    #region Interface IConfigureOptions<T>

        public ValueTask Configure(T options) => setOptions(options);

    #endregion
    }

    extension(IServiceCollection services)
    {
        public void Configure<T>(Action<T> setOptions) => services.AddConstant<IConfigureOptions<T>>(new ConfigureSync<T>(setOptions));

        public void Configure<T>(Func<T, ValueTask> setOptions) => services.AddConstant<IConfigureOptions<T>>(new ConfigureAsync<T>(setOptions));
    }
}