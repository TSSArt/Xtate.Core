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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Xtate
{
	internal class FactoryContext : IFactoryContext, IDisposable
	{
		private Dictionary<object, object?>? _dictionary;

		public FactoryContext(ImmutableArray<IResourceLoader> resourceLoaders) => ResourceLoaders = resourceLoaders;

	#region Interface IDisposable

		public void Dispose()
		{
			var dictionary = _dictionary;

			if (dictionary != null)
			{
				dictionary.Clear();
				_dictionary = null;
			}
		}

	#endregion

	#region Interface IFactoryContext

		public object? this[object key]
		{
			get
			{
				var dictionary = _dictionary;

				if (dictionary is null)
				{
					return null;
				}

				return dictionary.TryGetValue(key, out var value) ? value : null;
			}
			set
			{
				var dictionary = _dictionary;

				if (dictionary is null)
				{
					_dictionary = dictionary = new Dictionary<object, object?>();
				}

				dictionary[key] = value;
			}
		}

		public ImmutableArray<IResourceLoader> ResourceLoaders { get; }

	#endregion
	}
}