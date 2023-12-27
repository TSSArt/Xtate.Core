// Copyright © 2019-2023 Sergii Artemenko
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

using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Xtate.IoC;

namespace Xtate.Core;

public class DynamicAssembly : IDisposable, IAsyncInitialization, IServiceModule
{
	private readonly DisposingToken _disposingToken = new();
	private readonly Uri            _uri;

	private AsyncInit<ImmutableArray<IServiceModule>>? _asyncInitServiceModules;
	private Context?                                   _context;

	public DynamicAssembly(Uri uri)
	{
		_uri = uri;
		_asyncInitServiceModules = AsyncInit.RunAfter(this, static da => da.LoadAssemblyServiceModules());
	}

	public required IResourceLoader ResourceLoader { private get; [UsedImplicitly] init; }

#region Interface IAsyncInitialization

	public Task Initialization => _asyncInitServiceModules?.Task ?? Task.CompletedTask;

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IServiceModule

	public void Register(IServiceCollection servicesCollection)
	{
		var serviceModules = _asyncInitServiceModules?.Value ?? throw new ObjectDisposedException(nameof(DynamicAssembly));

		foreach (var serviceModule in serviceModules)
		{
			serviceModule.Register(servicesCollection);
		}
	}

#endregion

	private async ValueTask<ImmutableArray<IServiceModule>> LoadAssemblyServiceModules()
	{
		var resource = await ResourceLoader.Request(_uri).ConfigureAwait(false);
		await using (resource.ConfigureAwait(false))
		{
			var stream = await resource.GetStream(true).ConfigureAwait(false);
			await using (stream.ConfigureAwait(false))
			{
				var assembly = await LoadFromStream(stream).ConfigureAwait(false);

				return CreateServiceModules(assembly);
			}
		}
	}

	private static ImmutableArray<IServiceModule> CreateServiceModules(Assembly assembly)
	{
		var attributes = assembly.GetCustomAttributes(typeof(ServiceModuleAttribute), inherit: false);

		if (attributes.Length == 0)
		{
			return [];
		}

		var serviceModules = ImmutableArray.CreateBuilder<IServiceModule>(attributes.Length);

		foreach (ServiceModuleAttribute attribute in attributes)
		{
			serviceModules.Add((IServiceModule) Activator.CreateInstance(attribute.ServiceModuleType!)!);
		}

		return serviceModules.MoveToImmutable();
	}

	private async ValueTask<Assembly> LoadFromStream(Stream stream)
	{
		_context = new Context();

		if (stream is MemoryStream or UnmanagedMemoryStream)
		{
			return _context.LoadFromStream(stream);
		}

		using var memStream = new MemoryStream(new byte[stream.Length - stream.Position]);
		await stream.CopyToAsync(memStream, bufferSize: 81920, _disposingToken.Token).ConfigureAwait(false);
		memStream.Position = 0;

		return _context.LoadFromStream(memStream);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposingToken.Dispose();
			_context?.Unload();
			_asyncInitServiceModules = null;
			_context = null;
		}
	}

	private class Context : AssemblyLoadContext
	{
		public Context() : base(isCollectible: true) { }
	}
}