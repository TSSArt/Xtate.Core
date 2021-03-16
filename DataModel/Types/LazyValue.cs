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
using System.Threading;

namespace Xtate.Core
{
	[PublicAPI]
	public class LazyValue : Lazy<DataModelValue>, ILazyValue
	{
		public LazyValue(Func<DataModelValue> factory) : base(factory, LazyThreadSafetyMode.PublicationOnly) { }

	#region Interface ILazyValue

		DataModelValue ILazyValue.Value => Value;

	#endregion

		public static implicit operator DataModelValue(LazyValue lazyValue) => new(lazyValue);

		public DataModelValue ToDataModelValue() => this;
	}

	[PublicAPI]
	public class LazyValue<TArg> : Lazy<DataModelValue>, ILazyValue
	{
		public LazyValue(Func<TArg, DataModelValue> factory, TArg arg) : base(() => factory(arg), LazyThreadSafetyMode.PublicationOnly) { }

	#region Interface ILazyValue

		DataModelValue ILazyValue.Value => Value;

	#endregion

		public static implicit operator DataModelValue(LazyValue<TArg> lazyValue) => new(lazyValue);

		public DataModelValue ToDataModelValue() => this;
	}

	[PublicAPI]
	public class LazyValue<TArg1, TArg2> : Lazy<DataModelValue>, ILazyValue
	{
		public LazyValue(Func<TArg1, TArg2, DataModelValue> factory, TArg1 arg1, TArg2 arg2) : base(() => factory(arg1, arg2), LazyThreadSafetyMode.PublicationOnly) { }

	#region Interface ILazyValue

		DataModelValue ILazyValue.Value => Value;

	#endregion

		public static implicit operator DataModelValue(LazyValue<TArg1, TArg2> lazyValue) => new(lazyValue);

		public DataModelValue ToDataModelValue() => this;
	}
}