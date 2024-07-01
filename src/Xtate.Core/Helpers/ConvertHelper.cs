<<<<<<< Updated upstream
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Xtate.Core
{
	internal static class ConvertHelper<TFrom, TTo>
	{
		public static readonly Func<TFrom, TTo> Convert = GetConverter();

		private static Func<TFrom, TTo> GetConverter()
		{
			var arg = Expression.Parameter(typeof(TFrom));

			return Expression.Lambda<Func<TFrom, TTo>>(arg, arg).Compile();
		}
	}}
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

namespace Xtate.Core;

internal static class ConvertHelper<TFrom, TTo>
{
	public static readonly Func<TFrom, TTo> Convert = GetConverter();

	private static Func<TFrom, TTo> GetConverter()
	{
		var arg = Expression.Parameter(typeof(TFrom));
		var body = Expression.Convert(arg, typeof(TTo));

		return Expression.Lambda<Func<TFrom, TTo>>(body, arg).Compile();
	}
}
>>>>>>> Stashed changes
