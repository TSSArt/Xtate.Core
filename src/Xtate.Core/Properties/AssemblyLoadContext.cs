#region Copyright © 2019-2021 Sergii Artemenko

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

#if NET461 || NETSTANDARD2_0
using System.IO;
using System.Reflection;
using Xtate;
using Xtate.Core;

namespace System.Runtime.Loader
{
	[PublicAPI]
	internal class AssemblyLoadContext
	{
		public AssemblyLoadContext(bool isCollectible) => IsCollectible = isCollectible;

		public bool IsCollectible { get; }

		public void Unload() => Infra.Assert(IsCollectible);

		public Assembly LoadFromStream(Stream stream)
		{
			Infra.Assert(IsCollectible);

			if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var segment) && segment.Offset == 0 && segment.Count == memoryStream.Length)
			{
				return Assembly.Load(segment.Array);
			}

			return Assembly.Load(stream.ReadToEndAsync(default).SynchronousGetResult());
		}
	}
}
#endif