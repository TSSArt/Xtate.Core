﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using Xtate.Core;

namespace Xtate.Builder
{
	public class LogBuilder : BuilderBase, ILogBuilder
	{
		private IValueExpression? _expression;
		private string?           _label;

		public LogBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface ILogBuilder

		public ILog Build() => new LogEntity { Ancestor = Ancestor, Label = _label, Expression = _expression };

		public void SetLabel(string label)
		{
			if (string.IsNullOrEmpty(label)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(label));

			_label = label;
		}

		public void SetExpression(IValueExpression expression) => _expression = expression ?? throw new ArgumentNullException(nameof(expression));

	#endregion
	}
}