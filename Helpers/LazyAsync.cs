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

public class LazyAsync<T>(Func<T>? valueFactory) where T : class
{
	private volatile object? _state = new object();

	private T? _value;

	public T Value => _state == null ? _value! : CreateValue();

	private void ViaFactory()
	{
		var factory = valueFactory;

		if (factory == null)
		{
			throw new InvalidOperationException(@"SR.Lazy_Value_RecursiveCallsToValue");
		}

		valueFactory = null;

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