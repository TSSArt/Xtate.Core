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

namespace Xtate.DataModel.Null
{
	internal sealed class NullDataModelHandler : DataModelHandlerBase
	{
		public const string DataModelType = "null";

		internal NullDataModelHandler(IErrorProcessor errorProcessor) : base(errorProcessor) { }

		public static IDataModelHandlerFactory Factory { get; } = new DataModelHandlerFactory();

		public override ITypeInfo TypeInfo => TypeInfo<NullDataModelHandler>.Instance;

		protected override void Visit(ref IForEach forEach) => AddErrorMessage(forEach, Resources.ErrorMessage_ForEachNotSupportedForNull);

		protected override void Visit(ref IScript script) => AddErrorMessage(script, Resources.ErrorMessage_ScriptingNotSupportedForNull);

		protected override void Visit(ref IDataModel dataModel) => AddErrorMessage(dataModel, Resources.ErrorMessage_DataModelNotSupportedForNull);

		protected override void Visit(ref IDoneData doneData) => AddErrorMessage(doneData, Resources.ErrorMessage_DoneDataNotSupportedForNull);

		protected override void Visit(ref IValueExpression expression) => AddErrorMessage(expression, Resources.ErrorMessage_ValueExpressionNotSupportedForNull);

		protected override void Visit(ref ILocationExpression expression) => AddErrorMessage(expression, Resources.ErrorMessage_LocationExpressionNotSupportedForNull);

		protected override void Visit(ref IConditionExpression conditionExpression)
		{
			base.Visit(ref conditionExpression);

			var expression = conditionExpression.Expression!;

			if (!expression.StartsWith(value: @"In(", StringComparison.Ordinal) || !expression.EndsWith(value: @")", StringComparison.Ordinal))
			{
				AddErrorMessage(conditionExpression, Resources.ErrorMessage_IncorrectConditionExpressionForNull);

				return;
			}

			var state = expression[3..^1].Trim();

			if (Identifier.TryCreate(state, out var inState))
			{
				conditionExpression = new NullConditionExpressionEvaluator(conditionExpression, inState);
			}
			else
			{
				AddErrorMessage(conditionExpression, Resources.ErrorMessage_IncorrectConditionExpression);
			}
		}

		private static bool CanHandle(string dataModelType) => dataModelType == DataModelType;

		private class DataModelHandlerFactory : IDataModelHandlerFactory
		{
		#region Interface IDataModelHandlerFactory

			public ValueTask<IDataModelHandlerFactoryActivator?> TryGetActivator(IFactoryContext factoryContext, string dataModelType, CancellationToken token) =>
				new(CanHandle(dataModelType) ? DataModelHandlerFactoryActivator.Instance : null);

		#endregion
		}

		private class DataModelHandlerFactoryActivator : IDataModelHandlerFactoryActivator
		{
			public static IDataModelHandlerFactoryActivator Instance { get; } = new DataModelHandlerFactoryActivator();

		#region Interface IDataModelHandlerFactoryActivator

			public ValueTask<IDataModelHandler> CreateHandler(IFactoryContext factoryContext,
															  string dataModelType,
															  IErrorProcessor errorProcessor,
															  CancellationToken token)
			{
				Infrastructure.Assert(CanHandle(dataModelType));

				return new ValueTask<IDataModelHandler>(new NullDataModelHandler(errorProcessor));
			}

		#endregion
		}
	}
}