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

#endregion

#region Interface IAssign

	public virtual ILocationExpression? Location      => _assign.Location;
	public virtual IValueExpression?    Expression    => _assign.Expression;
	public virtual IInlineContent?      InlineContent => _assign.InlineContent;
	public virtual string?              Type          => _assign.Type;
	public virtual string?              Attribute     => _assign.Attribute;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion
}

public class DefaultAssignEvaluator : AssignEvaluator
{
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
	}
}