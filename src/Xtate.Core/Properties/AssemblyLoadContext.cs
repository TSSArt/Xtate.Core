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

#if !NET6_0_OR_GREATER

using System.IO;
using System.Reflection;
using Xtate;

namespace System.Runtime.Loader
{
	internal class AssemblyLoadContext(bool isCollectible)
	{
		public void Unload() => Infra.Assert(isCollectible);

		public Assembly LoadFromStream(Stream stream)
		{
			Infra.Assert(isCollectible);

			if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var segment) && segment.Offset == 0 && segment.Count == memoryStream.Length)
			{
				return Assembly.Load(segment.Array);
			}

			return Assembly.Load(stream.ReadToEndAsync(default).SynchronousGetResult());
		}
	}
}

#endif