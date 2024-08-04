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

namespace Xtate.IoC;

[MustDisposeResource]
public sealed class Container : IServiceProvider, IDisposable, IAsyncDisposable
{
	private readonly IServiceProvider _serviceProvider;

	private Container(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

#region Interface IAsyncDisposable

	public ValueTask DisposeAsync() => Disposer.DisposeAsync(_serviceProvider);

#endregion

#region Interface IDisposable

	public void Dispose() => Disposer.Dispose(_serviceProvider);

#endregion

#region Interface IServiceProvider

	public ImplementationEntry? GetImplementationEntry(TypeKey typeKey) => _serviceProvider.GetImplementationEntry(typeKey);

	public CancellationToken DisposeToken => _serviceProvider.DisposeToken;

	public IInitializationHandler? InitializationHandler => _serviceProvider.InitializationHandler;

	public IServiceProviderDebugger? Debugger => _serviceProvider.Debugger;

#endregion

	[MustDisposeResource]
	public static Container Create<TModule>() where TModule : IModule, new()
	{
		var services = new ServiceCollection();
		services.AddModule<TModule>();
		
		return new Container(services.BuildProvider());
	}
}

public interface IModule
{
	IServiceCollection Services { init; }

	void Register();
}

public abstract class Module : IModule
{
	protected IServiceCollection Services { get; private init; } = default!;

#region Interface IModule

	void IModule.Register()
	{
		AddModules();
		AddServices();
	}

	IServiceCollection IModule.Services { init => Services = value; }

#endregion

	protected abstract void AddModules();

	protected abstract void AddServices();

	protected void AddModule<TModule>() where TModule : IModule, new() => Services.AddModule<TModule>();
}

public static class ModuleExtensions
{
	public static void AddModule<TModule>(this IServiceCollection services) where TModule : IModule, new()
	{
		if (!IsRegistered(services, TypeKey.ImplementationKey<TModule, ValueTuple>()))
		{
			new TModule { Services = services }.Register();

			services.AddImplementation<TModule>().For<Module>();
		}
	}

	private static bool IsRegistered(IServiceCollection services, TypeKey key)
	{
		foreach (var entry in services)
		{
			if (entry.Key == key)
			{
				return true;
			}
		}

		return false;
	}
}