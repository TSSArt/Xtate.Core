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

using System;
using System.Reflection;

namespace Xtate.Core
{
	public interface ITypeInfo
	{
		public string FullTypeName    { get; }
		public string AssemblyName    { get; }
		public string AssemblyVersion { get; }
	}

	public class TypeInfoBase : ITypeInfo
	{
		public TypeInfoBase(Type type)
		{
			Infra.Requires(type);

			var assembly = type.Assembly;

			FullTypeName = type.FullName ?? string.Empty;
			AssemblyName = assembly.GetName().Name ?? string.Empty;
			AssemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
		}

	#region Interface ITypeInfo

		public string FullTypeName    { get; }
		public string AssemblyName    { get; }
		public string AssemblyVersion { get; }

	#endregion

	}

	public class TypeInfo<T> : TypeInfoBase
	{
		private TypeInfo() : base(typeof(T)) { }

		public static ITypeInfo Instance { get; } = new TypeInfo<T>();
	}
}