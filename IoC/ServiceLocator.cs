#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using Xtate.IoC;
using IServiceProvider = Xtate.IoC.IServiceProvider;

namespace Xtate;

//TODO: Replace ServiceLocator to DI
/// <summary>
///     temporary class
/// </summary>
public readonly struct ServiceLocator1
{
	public static readonly ServiceLocator1 Default;

	private readonly IServiceProvider _serviceProvider;

	static ServiceLocator1()
	{
		var services = new ServiceCollection();

		Container.Setup(services);

		Default = new ServiceLocator1(services.BuildProvider());
	}

	public ServiceLocator1(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

	public static ServiceLocator1 Create(Action<IServiceCollection> setup)
	{
		if (setup is null) throw new ArgumentNullException(nameof(setup));

		var services = new ServiceCollection();

		Container.Setup(services);

		setup(services);

		return new ServiceLocator1(services.BuildProvider());
	}

	public T GetService<T>() => _serviceProvider.GetRequiredService<T>().SynchronousGetResult();

	public Func<TArg, T> GetFactory<T, TArg>() => _serviceProvider.GetRequiredSyncFactory<T, TArg>();

	public T GetService<T, TArg>(TArg arg) => _serviceProvider.GetRequiredService<T, TArg>(arg).SynchronousGetResult();

	public T? GetOptionalService<T>() => _serviceProvider.GetOptionalService<T>().SynchronousGetResult();

	public IAsyncEnumerable<T> GetServices<T>() => _serviceProvider.GetServices<T>();
}