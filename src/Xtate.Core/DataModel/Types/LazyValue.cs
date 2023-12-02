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

public abstract class LazyValue : ILazyValue
{
	private volatile int _state;

	private DataModelValue _value;

#region Interface ILazyValue

	DataModelValue ILazyValue.Value
	{
		get
		{
			if (_state == 2)
			{
				return _value;
			}

			var newValue = Create();
			if (Interlocked.CompareExchange(ref _state, value: 1, comparand: 0) == 0)
			{
				_value = newValue;
				_state = 2;

				return _value;
			}

			SpinWait spinWait = default;
			while (_state != 2)
			{
				spinWait.SpinOnce();
			}

			return _value;
		}
	}

#endregion

	public static DataModelValue Create(Func<DataModelValue> factory) => new(new NoArg(factory));

	public static DataModelValue Create<TArg>(TArg arg, Func<TArg, DataModelValue> factory) => new(new OneArg<TArg>(factory, arg));

	public static DataModelValue Create<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, Func<TArg1, TArg2, DataModelValue> factory) => new(new TwoArgs<TArg1, TArg2>(factory, arg1, arg2));

	protected abstract DataModelValue Create();

	private class NoArg : LazyValue
	{
		private readonly Func<DataModelValue> _factory;

		public NoArg(Func<DataModelValue> factory) => _factory = factory;

		protected override DataModelValue Create() => _factory();
	}

	private class OneArg<TArg> : LazyValue
	{
		private readonly TArg                       _arg;
		private readonly Func<TArg, DataModelValue> _factory;

		public OneArg(Func<TArg, DataModelValue> factory, TArg arg)
		{
			_factory = factory;
			_arg = arg;
		}

		protected override DataModelValue Create() => _factory(_arg);
	}

	private class TwoArgs<TArg1, TArg2> : LazyValue
	{
		private readonly TArg1                              _arg1;
		private readonly TArg2                              _arg2;
		private readonly Func<TArg1, TArg2, DataModelValue> _factory;

		public TwoArgs(Func<TArg1, TArg2, DataModelValue> factory, TArg1 arg1, TArg2 arg2)
		{
			_factory = factory;
			_arg1 = arg1;
			_arg2 = arg2;
		}

		protected override DataModelValue Create() => _factory(_arg1, _arg2);
	}
}