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
using Xtate.Core;

namespace Xtate.Builder
{
	public class DataBuilder : BuilderBase, IDataBuilder
	{
		private IValueExpression?        _expression;
		private string?                  _id;
		private IInlineContent?          _inlineContent;
		private IExternalDataExpression? _source;

		public DataBuilder(IErrorProcessor errorProcessor, object? ancestor) : base(errorProcessor, ancestor) { }

	#region Interface IDataBuilder

		public IData Build() => new DataEntity { Ancestor = Ancestor, Id = _id, Source = _source, Expression = _expression, InlineContent = _inlineContent };

		public void SetId(string id)
		{
			if (string.IsNullOrEmpty(id)) throw new ArgumentException(Resources.Exception_ValueCannotBeNullOrEmpty, nameof(id));

			_id = id;
		}

		public void SetSource(IExternalDataExpression source) => _source = source ?? throw new ArgumentNullException(nameof(source));

		public void SetExpression(IValueExpression expression) => _expression = expression ?? throw new ArgumentNullException(nameof(expression));

		public void SetInlineContent(IInlineContent inlineContent) => _inlineContent = inlineContent ?? throw new ArgumentNullException(nameof(inlineContent));

	#endregion
	}
}