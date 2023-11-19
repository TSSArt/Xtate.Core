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

internal class AsyncInitializationHandler : IInitializationHandler
{
	public static readonly IInitializationHandler Instance = new AsyncInitializationHandler();

#region Interface IInitializationHandler

	bool IInitializationHandler.Initialize<T>(T instance) => Initialize(instance);

	Task IInitializationHandler.InitializeAsync<T>(T instance) => InitializeAsync(instance);

#endregion

	public static bool Initialize<T>(T instance) => instance is IAsyncInitialization;

	public static Task InitializeAsync<T>(T instance)
	{
		if (BaseAsyncInit<T>.AsyncInitInstance is { } initializer)
		{
			return initializer.InitInternal(instance);
		}

		if (instance is IAsyncInitialization asyncInitialization)
		{
			return asyncInitialization.Initialization;
		}

		return Task.CompletedTask;
	}

	private abstract class BaseAsyncInit<T>
	{
		public static readonly BaseAsyncInit<T>? AsyncInitInstance;

		static BaseAsyncInit()
		{
			if (typeof(IAsyncInitialization).IsAssignableFrom(typeof(T)))
			{
				AsyncInitInstance = typeof(AsyncInit<>).MakeGenericTypeExt(typeof(T)).CreateInstance<BaseAsyncInit<T>>();
			}
		}

		public abstract Task InitInternal(in T? instance);
	}

	private sealed class AsyncInit<T> : BaseAsyncInit<T> where T : IAsyncInitialization
	{
		public override Task InitInternal(in T? instance) => instance is not null ? instance.Initialization : Task.CompletedTask;
	}
}