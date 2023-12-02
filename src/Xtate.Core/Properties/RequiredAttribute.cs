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

#if !NET6_0_OR_GREATER || NET6_0
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
	internal sealed class RequiredMemberAttribute : Attribute;

	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	internal sealed class CompilerFeatureRequiredAttribute : Attribute
	{
		public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;

		public string FeatureName { get; }

		public string? Language { get; init; }
	}
}

namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
	internal sealed class SetsRequiredMembersAttribute : Attribute;
}
#endif