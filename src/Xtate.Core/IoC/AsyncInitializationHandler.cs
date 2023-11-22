#region Copyright © 2019-2022 Sergii Artemenko

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

using System;
using System.Threading.Tasks;
using Xtate.IoC;

namespace Xtate.Core.IoC;

public sealed class AsyncInitializationHandler : IInitializationHandler
{
	public static readonly IInitializationHandler Instance = new AsyncInitializationHandler();

#region Interface IInitializationHandler

	bool IInitializationHandler.Initialize<T>(T instance) => Initialize(instance);

	Task IInitializationHandler.InitializeAsync<T>(T instance) => InitializeAsync(instance);

#endregion

	public static bool Initialize<T>(T instance) => instance is IAsyncInitialization;

	public static Task InitializeAsync<T>(T instance)
	{
		if (Base<T>.Instance is { } initializer)
		{
			return initializer.InitInternal(instance);
		}

		if (instance is IAsyncInitialization asyncInitialization)
		{
			return asyncInitialization.Initialization;
		}

		return Task.CompletedTask;
	}

	private abstract class Base<T>
	{
		public static readonly Base<T>? Instance;

		static Base()
		{
			if (typeof(IAsyncInitialization).IsAssignableFrom(typeof(T)))
			{
				Instance = (Base<T>?) Activator.CreateInstance(typeof(AsyncInit<>).MakeGenericType(typeof(T)));
			}
		}

		public abstract Task InitInternal(in T? instance);
	}

	private sealed class AsyncInit<T> : Base<T> where T : IAsyncInitialization
	{
		public override Task InitInternal(in T? instance) => instance is not null ? instance.Initialization : Task.CompletedTask;
	}
}