// Copyright © 2019-2025 Sergii Artemenko
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

#if !NETCOREAPP2_0 && !NETCOREAPP2_1_OR_GREATER
using System.IO;
using System.Reflection;
using Xtate;

namespace System.Runtime.Loader
{
    /// <summary>
    /// Represents an assembly load context that can optionally be collectible.
    /// </summary>
    internal class AssemblyLoadContext(bool isCollectible)
    {
        /// <summary>
        /// Unloads the assembly load context if it is collectible.
        /// </summary>
        public void Unload() => Infra.Assert(isCollectible);

        /// <summary>
        /// Loads an assembly from the provided stream.
        /// </summary>
        /// <param name="stream">The stream containing the assembly data.</param>
        /// <returns>The loaded assembly.</returns>
        public Assembly LoadFromStream(Stream stream)
        {
            Infra.Assert(isCollectible);

            if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var segment) && segment.Offset == 0 && segment.Count == memoryStream.Length)
            {
                return Assembly.Load(segment.Array);
            }

            return Assembly.Load(stream.ReadToEnd(default));
        }
    }
}

#endif