// Copyright © 2019-2026 Sergii Artemenko
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

#if !NETCOREAPP2_0 && !NETCOREAPP2_1_OR_GREATER && !NETSTANDARD2_1

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

internal static class KeyValuePairPolyfills
{
	extension<TKey, TValue>(KeyValuePair<TKey, TValue> keyValuePair)
	{
		public void Deconstruct(out TKey key, out TValue value)
		{
			key = keyValuePair.Key;
			value = keyValuePair.Value;
		}
	}
}

#endif