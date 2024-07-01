<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2024 Sergii Artemenko
// 
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
#endregion

using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class AssignEvaluator : IAssign, IExecEvaluator, IAncestorProvider
{
	private readonly IAssign _assign;

	protected AssignEvaluator(IAssign assign)
	{
		Infra.Requires(assign);

		_assign = assign;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _assign;
=======
namespace Xtate.DataModel;

public abstract class AssignEvaluator(IAssign assign) : IAssign, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => assign;
>>>>>>> Stashed changes

#endregion

#region Interface IAssign

<<<<<<< Updated upstream
	public virtual ILocationExpression? Location      => _assign.Location;
	public virtual IValueExpression?    Expression    => _assign.Expression;
	public virtual IInlineContent?      InlineContent => _assign.InlineContent;
	public virtual string?              Type          => _assign.Type;
	public virtual string?              Attribute     => _assign.Attribute;
=======
	public virtual ILocationExpression? Location      => assign.Location;
	public virtual IValueExpression?    Expression    => assign.Expression;
	public virtual IInlineContent?      InlineContent => assign.InlineContent;
	public virtual string?              Type          => assign.Type;
	public virtual string?              Attribute     => assign.Attribute;
>>>>>>> Stashed changes

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion
}

public class DefaultAssignEvaluator : AssignEvaluator
{
<<<<<<< Updated upstream
	public DefaultAssignEvaluator(IAssign assign) : base(assign)
	{
		Infra.NotNull(assign.Location);

		LocationEvaluator = assign.Location.As<ILocationEvaluator>();
		ExpressionEvaluator = assign.Expression?.As<IObjectEvaluator>();
		InlineContentEvaluator = assign.InlineContent?.As<IObjectEvaluator>();
	}

	public ILocationEvaluator LocationEvaluator      { get; }
	public IObjectEvaluator?  ExpressionEvaluator    { get; }
	public IObjectEvaluator?  InlineContentEvaluator { get; }

	public override async ValueTask Execute()
	{
		var value = await EvaluateRightValue().ConfigureAwait(false);

		await LocationEvaluator.SetValue(value).ConfigureAwait(false);
	}

	protected virtual ValueTask<IObject> EvaluateRightValue()
	{
		if (ExpressionEvaluator is not null)
		{
			return ExpressionEvaluator.EvaluateObject();
		}

		if (InlineContentEvaluator is not null)
		{
			return InlineContentEvaluator.EvaluateObject();
		}

		return new ValueTask<IObject>(DefaultObject.Null);
=======
	private readonly ILocationEvaluator _locationEvaluator;
	private readonly IObjectEvaluator   _valueEvaluator;

	public DefaultAssignEvaluator(IAssign assign) : base(assign)
	{
		var valueEvaluator = base.Expression?.As<IObjectEvaluator>() ?? base.InlineContent?.As<IObjectEvaluator>();
		Infra.NotNull(valueEvaluator);
		_valueEvaluator = valueEvaluator;

		var locationEvaluator = base.Location?.As<ILocationEvaluator>();
		Infra.NotNull(locationEvaluator);
		_locationEvaluator = locationEvaluator;
	}

	public override async ValueTask Execute()
	{
		var value = await _valueEvaluator.EvaluateObject().ConfigureAwait(false);

		await _locationEvaluator.SetValue(value).ConfigureAwait(false);
>>>>>>> Stashed changes
	}
}