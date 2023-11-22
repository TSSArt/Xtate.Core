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

namespace Xtate.Core
{
	public class LazyAsync<T> where T : class
	{
		private Func<T>? _factory;

		volatile private object? _state;

		private T? _value;

		public LazyAsync(Func<T>? valueFactory)
		{
			_factory = valueFactory;
			_state = new object();
		}

		public T Value => _state == null ? _value! : CreateValue();

		private void ViaFactory()
		{
			var factory = _factory;
			
			if (factory == null)
			{
				throw new InvalidOperationException(@"SR.Lazy_Value_RecursiveCallsToValue");
			}

			_factory = null;

			_value = factory();
			_state = null;
		}

		private T CreateValue()
		{
			var state = _state;
			if (state != null)
			{
				lock (state)
				{
					if (ReferenceEquals(_state, state))
					{
						ViaFactory();
					}
				}
			}

			return Value;
		}
	}
}