#region Copyright © 2019-2020 Sergii Artemenko

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

using System.Reflection;
using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public interface ITypeInfo
	{
		public string FullTypeName    { get; }
		public string AssemblyName    { get; }
		public string AssemblyVersion { get; }
	}

	[PublicAPI]
	public class TypeInfo<T> : ITypeInfo
	{
		private TypeInfo()
		{
			var type = typeof(T);
			var assembly = type.Assembly;

			FullTypeName = type.FullName ?? string.Empty;
			AssemblyName = assembly.GetName().Name ?? string.Empty;
			AssemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
		}

		public static ITypeInfo Instance { get; } = new TypeInfo<T>();

	#region Interface ITypeInfo

		public string FullTypeName    { get; }
		public string AssemblyName    { get; }
		public string AssemblyVersion { get; }

	#endregion
	}
}