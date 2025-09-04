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

using Xtate.IoC;

namespace Xtate.Core.Test;

public static class ServiceCollectionExtensions
{
    public static Mock<T> AddMock<T>(this IServiceCollection services) where T : class
    {
        var mock = new Mock<T>();

        services.AddForwarding(_ => mock.Object);

        return mock;
    }

    public static Mock<T> AddMock<T>(this IServiceCollection services, Action<Mock<T>> configureMock) where T : class
    {
        var mock = new Mock<T>();

        configureMock(mock);

        services.AddForwarding(_ => mock.Object);

        return mock;
    }
}