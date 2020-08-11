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

namespace Xtate
{
	public class LazyValue : ILazyValue
	{
		private Func<DataModelValue>? _factory;
		private DataModelValue        _value;

		public LazyValue(Func<DataModelValue> factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

	#region Interface ILazyValue

		public DataModelValue Value
		{
			get
			{
				var factory = _factory;
				if (factory is null)
				{
					return _value;
				}

				var value = factory();

				_factory = null;
				_value = value;

				return value;
			}
		}

	#endregion

		public static implicit operator DataModelValue(LazyValue lazyValue) => new DataModelValue(lazyValue);

		public DataModelValue ToDataModelValue() => this;
	}

	public class LazyValue<TArg> : ILazyValue
	{
		private readonly TArg                        _arg;
		private          Func<TArg, DataModelValue>? _factory;
		private          DataModelValue              _value;

		public LazyValue(Func<TArg, DataModelValue> factory, TArg arg)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_arg = arg;
		}

	#region Interface ILazyValue

		public DataModelValue Value
		{
			get
			{
				var factory = _factory;
				if (factory is null)
				{
					return _value;
				}

				var value = factory(_arg);

				_factory = null;
				_value = value;

				return value;
			}
		}

	#endregion

		public static implicit operator DataModelValue(LazyValue<TArg> lazyValue) => new DataModelValue(lazyValue);

		public DataModelValue ToDataModelValue() => this;
	}

	public class LazyValue<TArg1, TArg2> : ILazyValue
	{
		private readonly TArg1                               _arg1;
		private readonly TArg2                               _arg2;
		private          Func<TArg1, TArg2, DataModelValue>? _factory;
		private          DataModelValue                      _value;

		public LazyValue(Func<TArg1, TArg2, DataModelValue> factory, TArg1 arg1, TArg2 arg2)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_arg1 = arg1;
			_arg2 = arg2;
		}

	#region Interface ILazyValue

		public DataModelValue Value
		{
			get
			{
				var factory = _factory;
				if (factory is null)
				{
					return _value;
				}

				var value = factory(_arg1, _arg2);

				_factory = null;
				_value = value;

				return value;
			}
		}

	#endregion

		public static implicit operator DataModelValue(LazyValue<TArg1, TArg2> lazyValue) => new DataModelValue(lazyValue);

		public DataModelValue ToDataModelValue() => this;
	}
}