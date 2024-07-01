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

namespace Xtate.Core;

public class HostBasedSourceStateMachine(ISourceStateMachine sourceStateMachine, IHostBaseUri? hostBaseUri) : ISourceStateMachine
{
<<<<<<<< Updated upstream:src/Xtate.Core/src/Xtate.Core/Helpers/ServiceModuleAttribute.cs
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ServiceModuleAttribute : Attribute
	{
		public ServiceModuleAttribute(Type serviceModuleType) => ServiceModuleType = serviceModuleType;

		public Type? ServiceModuleType { get; }
	}
========

	#region Interface ISourceStateMachine

	public Uri Location => hostBaseUri?.HostBaseUri.CombineWith(sourceStateMachine.Location)!;

#endregion
>>>>>>>> Stashed changes:src/Xtate.Core/src/Xtate.Core/IoC/HostBasedSourceStateMachine.cs
}