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
				if (factory == null)
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
				if (factory == null)
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
				if (factory == null)
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