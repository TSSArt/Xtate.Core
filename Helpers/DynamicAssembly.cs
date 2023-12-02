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
using System.Reflection;
using System.Runtime.Loader;

namespace Xtate.Core;

[Obsolete]

//TODO: delete
public sealed class DynamicAssembly : IDisposable
{
	internal static readonly object    AssemblyCacheKey = new();
	private                  Assembly? _assembly;

	private Context? _context;

	public DynamicAssembly(Stream stream)
	{
		_context = new Context();
		_assembly = _context.LoadFromStream(stream);
	}

	public Assembly Assembly => _assembly ?? throw new ObjectDisposedException(nameof(DynamicAssembly));

#region Interface IDisposable

	public void Dispose()
	{
		_context?.Unload();
		_assembly = null;
		_context = null;
	}

#endregion

	private class Context : AssemblyLoadContext
	{
		public Context() : base(isCollectible: true) { }
	}
}