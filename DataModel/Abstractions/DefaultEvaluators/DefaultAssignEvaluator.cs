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
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultAssignEvaluator : IAssign, IExecEvaluator, IAncestorProvider
	{
		private readonly AssignEntity _assign;

		public DefaultAssignEvaluator(in AssignEntity assign)
		{
			_assign = assign;

			Infrastructure.NotNull(assign.Location);

			LocationEvaluator = assign.Location.As<ILocationEvaluator>();
			ExpressionEvaluator = assign.Expression?.As<IObjectEvaluator>();
			InlineContentEvaluator = assign.InlineContent?.As<IObjectEvaluator>();
		}

		public ILocationEvaluator LocationEvaluator      { get; }
		public IObjectEvaluator?  ExpressionEvaluator    { get; }
		public IObjectEvaluator?  InlineContentEvaluator { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _assign.Ancestor;

	#endregion

	#region Interface IAssign

		public ILocationExpression Location      => _assign.Location!;
		public IValueExpression?   Expression    => _assign.Expression;
		public IInlineContent?     InlineContent => _assign.InlineContent;
		public string?             Type          => _assign.Type;
		public string?             Attribute     => _assign.Attribute;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			var value = await EvaluateRightValue(executionContext, token).ConfigureAwait(false);

			await LocationEvaluator.SetValue(value, GetCustomData(), executionContext, token).ConfigureAwait(false);
		}

	#endregion

		protected virtual object? GetCustomData() => null;

		private ValueTask<IObject> EvaluateRightValue(IExecutionContext executionContext, CancellationToken token)
		{
			if (ExpressionEvaluator is not null)
			{
				return ExpressionEvaluator.EvaluateObject(executionContext, token);
			}

			if (InlineContentEvaluator is not null)
			{
				return InlineContentEvaluator.EvaluateObject(executionContext, token);
			}

			return new ValueTask<IObject>(DefaultObject.Null);
		}
	}
}