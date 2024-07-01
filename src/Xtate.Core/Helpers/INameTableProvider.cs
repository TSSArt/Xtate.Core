<<<<<<< Updated upstream
﻿using System.Xml;
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

<<<<<<<< Updated upstream:src/Xtate.Core/src/Xtate.Core/Helpers/IAsyncInitialization.cs
using System.Threading.Tasks;

namespace Xtate.Core
{
	//TODO: Delete
	public interface IAsyncInitializationOld
	{
		Task Initialization { get; }
	}
========
using System.Xml;
>>>>>>> Stashed changes

namespace Xtate.Core;

public interface INameTableProvider
{
	NameTable GetNameTable();
<<<<<<< Updated upstream
=======
>>>>>>>> Stashed changes:src/Xtate.Core/src/Xtate.Core/Helpers/INameTableProvider.cs
>>>>>>> Stashed changes
}