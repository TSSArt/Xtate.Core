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

namespace Xtate.IoC;

internal sealed class TransientImplementationEntry : ImplementationEntry
{
	private readonly ServiceProvider _serviceProvider;

	public TransientImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(factory) => _serviceProvider = serviceProvider;

	private TransientImplementationEntry(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(sourceEntry) => _serviceProvider = serviceProvider;

	protected override IServiceProvider ServiceProvider => _serviceProvider;

	internal override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new TransientImplementationEntry(serviceProvider, this);

	internal override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new TransientImplementationEntry(serviceProvider, factory);

	[ExcludeFromCodeCoverage]
	protected override async ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) where T : default
	{
		var instance = await base.ExecuteFactory<T, TArg>(argument).ConfigureAwait(false);

		try
		{
			_serviceProvider.RegisterInstanceForDispose(instance);
		}
		catch
		{
			await Disposer.DisposeAsync(instance).ConfigureAwait(false);

			throw;
		}

		return instance;
	}

	protected override T? ExecuteFactorySync<T, TArg>(TArg argument) where T : default
	{
		var instance = base.ExecuteFactorySync<T, TArg>(argument);

		try
		{
			_serviceProvider.RegisterInstanceForDispose(instance);
		}
		catch
		{
			Disposer.Dispose(instance);

			throw;
		}

		return instance;
	}
}