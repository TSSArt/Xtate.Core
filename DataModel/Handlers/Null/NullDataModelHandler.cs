// Copyright © 2019-2024 Sergii Artemenko
// 
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

namespace Xtate.DataModel.Null;

public class NullDataModelHandler : DataModelHandlerBase
{
	public required IErrorProcessorService<NullDataModelHandler> NullErrorProcessorService { private get; [UsedImplicitly] init; }

	public required Func<IConditionExpression, IIdentifier, NullConditionExpressionEvaluator> NullConditionExpressionEvaluatorFactory { private get; [UsedImplicitly] init; }

	protected override void Visit(ref IForEach forEach) => NullErrorProcessorService.AddError(forEach, Resources.ErrorMessage_ForEachNotSupportedForNull);

	protected override void Visit(ref IScript script) => NullErrorProcessorService.AddError(script, Resources.ErrorMessage_ScriptingNotSupportedForNull);

	protected override void Visit(ref IDataModel dataModel) => NullErrorProcessorService.AddError(dataModel, Resources.ErrorMessage_DataModelNotSupportedForNull);

	protected override void Visit(ref IDoneData doneData) => NullErrorProcessorService.AddError(doneData, Resources.ErrorMessage_DoneDataNotSupportedForNull);

	protected override void Visit(ref IValueExpression expression) => NullErrorProcessorService.AddError(expression, Resources.ErrorMessage_ValueExpressionNotSupportedForNull);

	protected override void Visit(ref ILocationExpression expression) => NullErrorProcessorService.AddError(expression, Resources.ErrorMessage_LocationExpressionNotSupportedForNull);

	protected override void Visit(ref IConditionExpression conditionExpression)
	{
		base.Visit(ref conditionExpression);

		var expression = conditionExpression.Expression!;

		if (!expression.StartsWith(value: @"In(", StringComparison.Ordinal) || !expression.EndsWith(value: @")", StringComparison.Ordinal))
		{
			NullErrorProcessorService.AddError(conditionExpression, Resources.ErrorMessage_IncorrectConditionExpressionForNull);

			return;
		}

		var state = expression[3..^1].Trim();

		if (Identifier.TryCreate(state, out var inState))
		{
			conditionExpression = NullConditionExpressionEvaluatorFactory(conditionExpression, inState);
		}
		else
		{
			NullErrorProcessorService.AddError(conditionExpression, Resources.ErrorMessage_IncorrectConditionExpression);
		}
	}
}