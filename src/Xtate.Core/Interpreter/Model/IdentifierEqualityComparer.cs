﻿#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Collections.Generic;

namespace Xtate
{
	internal sealed class IdentifierEqualityComparer : IEqualityComparer<IIdentifier>
	{
		public static readonly IEqualityComparer<IIdentifier> Instance = new IdentifierEqualityComparer();

	#region Interface IEqualityComparer<IIdentifier>

		public bool Equals(IIdentifier? x, IIdentifier? y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x is null || y is null)
			{
				return false;
			}

			return x.As<IEquatable<IIdentifier>>().Equals(y.As<IEquatable<IIdentifier>>());
		}

		public int GetHashCode(IIdentifier obj) => obj.As<IEquatable<IIdentifier>>().GetHashCode();

	#endregion
	}
}