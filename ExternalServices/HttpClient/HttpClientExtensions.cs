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

using Xtate.ExternalService.HttpClient;

namespace Xtate;

public static class HttpClientExtensions
{
    public static StateMachineHostBuilder AddHttpClient(this StateMachineHostBuilder builder)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));

        //builder.AddServiceFactory(HttpClientServiceFactory.Create(HttpClientServiceOptions.CreateDefault()));

        return builder;
    }

    public static StateMachineHostBuilder AddHttpClient(this StateMachineHostBuilder builder, Action<HttpClientServiceOptions> setOptions)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (setOptions is null) throw new ArgumentNullException(nameof(setOptions));

        var options = HttpClientServiceOptions.CreateDefault();
        setOptions(options);

        //builder.AddServiceFactory(HttpClientServiceFactory.Create(options));

        return builder;
    }
}