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

using Xtate.IoC;

namespace Xtate.Core;

public class SafeFactory<T> : IAsyncInitialization
{
	private T? _value;

	public SafeFactory(Func<ValueTask<T?>> factory) => Initialization = Initialize(this, factory);

#region Interface IAsyncInitialization

	public Task Initialization { get; }

#endregion

	private static async Task Initialize(SafeFactory<T> safeFactory, Func<ValueTask<T?>> factory)
	{
		try
		{
			safeFactory._value = await factory().ConfigureAwait(false);
		}
		catch (DependencyInjectionException ex) when (ex.GetBaseException() is MissedServiceException)
		{
			// ignore
		}
	}

	private T? GetValue() => _value;

	[UsedImplicitly]
	public ValueTask<Safe<T>> GetValueFunc() => new(GetValue);
}