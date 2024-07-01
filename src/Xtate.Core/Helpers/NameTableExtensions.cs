<<<<<<< Updated upstream
﻿using Xtate.IoC;
=======
﻿#region Copyright © 2019-2023 Sergii Artemenko

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

using Xtate.IoC;
>>>>>>> Stashed changes

namespace Xtate.Core;

public static class NameTableExtensions
{
<<<<<<< Updated upstream
=======
<<<<<<<< Updated upstream:src/Xtate.Core/src/Xtate.Core/Interpreter/Interfaces/IResourceLoaderFactory.cs
	public interface IResourceLoaderFactory
	{
		ValueTask<IResourceLoaderFactoryActivator?> TryGetActivator(Uri uri);
========
>>>>>>> Stashed changes
	public static void RegisterNameTable(this IServiceCollection services)
	{
		if (services.IsRegistered<INameTableProvider>())
		{
			return;
		}

		services.AddSharedImplementationSync<NameTableProvider>(SharedWithin.Scope).For<INameTableProvider>();
<<<<<<< Updated upstream
=======
>>>>>>>> Stashed changes:src/Xtate.Core/src/Xtate.Core/Helpers/NameTableExtensions.cs
>>>>>>> Stashed changes
	}
}