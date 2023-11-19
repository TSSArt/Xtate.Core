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

using System;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class ParamEvaluator : IParam, IParameterEvaluator, IAncestorProvider, IDebugEntityId
{
	private readonly IParam _param;

	protected ParamEvaluator(IParam param)
	{
		Infra.Requires(param);
		Infra.NotNull(param.Name);

		_param = param;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _param;

#endregion

#region Interface IDebugEntityId

	FormattableString IDebugEntityId.EntityId => @$"{_param.Name}";

#endregion

#region Interface IObjectEvaluator

	public abstract ValueTask<IObject> EvaluateObject();

#endregion

#region Interface IParam

	public virtual string               Name       => _param.Name!;
	public virtual IValueExpression?    Expression => _param.Expression;
	public virtual ILocationExpression? Location   => _param.Location;

#endregion
}

public class DefaultParamEvaluator : ParamEvaluator
{
	public DefaultParamEvaluator(IParam param) : base(param)
	{
		ExpressionEvaluator = param.Expression?.As<IObjectEvaluator>();
		LocationEvaluator = param.Location?.As<ILocationEvaluator>();
	}

	public IObjectEvaluator? ExpressionEvaluator { get; }

	public ILocationEvaluator? LocationEvaluator { get; }

	public override async ValueTask<IObject> EvaluateObject()
	{
		if (ExpressionEvaluator is not null)
		{
			return await ExpressionEvaluator.EvaluateObject().ConfigureAwait(false);
		}

		if (LocationEvaluator is not null)
		{
			return await LocationEvaluator.GetValue().ConfigureAwait(false);
		}

		return DefaultObject.Null;
	}
}