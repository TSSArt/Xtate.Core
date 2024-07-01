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

using System.IO;
using Xtate.IoC;

namespace Xtate.Core;

public static class ResourceLoaderExtensions
{
<<<<<<< Updated upstream
	public static void RegisterResource(this IServiceCollection services)
	{
		if (services.IsRegistered<Resource, Stream>())
		{
			return;
		}

		services.AddSharedImplementation<DefaultIoBoundTask>(SharedWithin.Scope).For<IIoBoundTask>();
		services.AddTypeSync<Resource, Stream>();
	}

=======
>>>>>>> Stashed changes
	public static void RegisterResourceLoaders(this IServiceCollection services)
	{
		if (services.IsRegistered<ResourceLoaderService>())
		{
			return;
		}

		services.RegisterFileResourceLoader();
		services.RegisterResxResourceLoader();
		services.RegisterWebResourceLoader();

		services.AddImplementation<ResourceLoaderService>().For<ResourceLoaderService>().For<IResourceLoader>();
	}

<<<<<<< Updated upstream
	public static void RegisterFileResourceLoader(this IServiceCollection services)
=======
	private static void RegisterResource(this IServiceCollection services)
	{
		if (services.IsRegistered<Resource, Stream>())
		{
			return;
		}

		services.AddSharedImplementation<DefaultIoBoundTask>(SharedWithin.Scope).For<IIoBoundTask>();
		services.AddTypeSync<Resource, Stream>();
	}

	private static void RegisterFileResourceLoader(this IServiceCollection services)
>>>>>>> Stashed changes
	{
		if (services.IsRegistered<FileResourceLoader>())
		{
			return;
		}

		services.RegisterResource();
		services.AddSharedImplementation<FileResourceLoaderProvider>(SharedWithin.Container).For<IResourceLoaderProvider>();
		services.AddImplementation<FileResourceLoader>().For<FileResourceLoader>().For<IResourceLoader>();
	}

<<<<<<< Updated upstream
	public static void RegisterResxResourceLoader(this IServiceCollection services)
=======
	private static void RegisterResxResourceLoader(this IServiceCollection services)
>>>>>>> Stashed changes
	{
		if (services.IsRegistered<ResxResourceLoader>())
		{
			return;
		}

		services.RegisterResource();
		services.AddSharedImplementation<ResxResourceLoaderProvider>(SharedWithin.Container).For<IResourceLoaderProvider>();
		services.AddImplementation<ResxResourceLoader>().For<ResxResourceLoader>().For<IResourceLoader>();
	}

<<<<<<< Updated upstream
	public static void RegisterWebResourceLoader(this IServiceCollection services)
=======
	private static void RegisterWebResourceLoader(this IServiceCollection services)
>>>>>>> Stashed changes
	{
		if (services.IsRegistered<WebResourceLoader>())
		{
			return;
		}

		services.RegisterResource();
		services.AddSharedImplementation<WebResourceLoaderProvider>(SharedWithin.Container).For<IResourceLoaderProvider>();
		services.AddImplementation<WebResourceLoader>().For<WebResourceLoader>().For<IResourceLoader>();
	}
}