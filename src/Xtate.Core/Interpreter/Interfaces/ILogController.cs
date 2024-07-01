<<<<<<< Updated upstream
﻿#region Copyright © 2019-2021 Sergii Artemenko
=======
﻿#region Copyright © 2019-2023 Sergii Artemenko
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
using System;
using System.Threading.Tasks;

namespace Xtate
{
	public enum LogLevel
	{
		Info,
		Warning,
		Error
	}


	public interface ILogController
	{
		bool IsEnabled { get; }

		ValueTask Log(string? message = default, DataModelValue arguments = default);

		//TODO:delete
		[Obsolete]
		ValueTask LogOld(LogLevel logLevel,
					  string? message = default,
					  DataModelValue arguments = default,
					  Exception? exception = default);
	}
=======
namespace Xtate;

public interface ILogController
{
	bool IsEnabled { get; }

	ValueTask Log(string? message = default, DataModelValue arguments = default);
>>>>>>> Stashed changes
}