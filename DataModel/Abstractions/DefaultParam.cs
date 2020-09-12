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

namespace Xtate.DataModel
{
	public sealed class DefaultParam : IParam, IAncestorProvider, IDebugEntityId
	{
		private readonly ParamEntity _param;

		public DefaultParam(in ParamEntity param)
		{
			Infrastructure.NotNull(param.Name);

			_param = param;
			ExpressionEvaluator = param.Expression?.As<IObjectEvaluator>();
			LocationEvaluator = param.Location?.As<ILocationEvaluator>();
		}

		public IObjectEvaluator?   ExpressionEvaluator { get; }
		public ILocationEvaluator? LocationEvaluator   { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _param.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		FormattableString IDebugEntityId.EntityId => @$"{_param.Name}";

	#endregion

	#region Interface IParam

		public string               Name       => _param.Name!;
		IValueExpression? IParam.   Expression => _param.Expression;
		ILocationExpression? IParam.Location   => _param.Location;

	#endregion
	}
}