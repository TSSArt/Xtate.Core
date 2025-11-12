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

namespace Xtate.Core;

[InstantiatedByIoC]
public class OptionsAsyncImpl<T> : IOptionsAsync<T>
{
    private ValueTask<T>? _valueTask;

    public required Func<ValueTask<T>> DefaultInstanceFactory { private get; [SetByIoC] init; }

    public required IAsyncEnumerable<IConfigureOptions<T>> Configurators { private get; [SetByIoC] init; }

#region Interface IOptionsAsync<T>

    public ValueTask<T> GetValue() => _valueTask ??= Factory().Preserve();

#endregion

    private async ValueTask<T> Factory()
    {
        var instance = await DefaultInstanceFactory().ConfigureAwait(false);

        await foreach (var configureOptions in Configurators.ConfigureAwait(false))
        {
            await configureOptions.Configure(instance).ConfigureAwait(false);
        }

        return instance;
    }
}