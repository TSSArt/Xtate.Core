using System;

namespace Xtate.DataModel.None
{
	internal sealed class NoneDataModelHandler : DataModelHandlerBase
	{
		public const string DataModelType = "none";

		public static readonly IDataModelHandlerFactory Factory = new DataModelHandlerFactory();

		private NoneDataModelHandler(IErrorProcessor errorProcessor) : base(errorProcessor) { }

		protected override void Visit(ref IForEach forEach) => AddErrorMessage(forEach, Resources.ErrorMesasge_ForEachNotSupportedForNone);

		protected override void Visit(ref IScript script) => AddErrorMessage(script, Resources.ErrorMesasge_ScriptingNotSupportedForNone);

		protected override void Visit(ref IDataModel dataModel) => AddErrorMessage(dataModel, Resources.ErrorMesasge_DataModelNotSupportedForNone);

		protected override void Visit(ref IDoneData doneData) => AddErrorMessage(doneData, Resources.ErrorMesasge_DoneDataNotSupportedForNone);

		protected override void Visit(ref IValueExpression expression) => AddErrorMessage(expression, Resources.ErrorMesasge_ValueExpressionNotSupportedForNone);

		protected override void Visit(ref ILocationExpression expression) => AddErrorMessage(expression, Resources.ErrorMesasge_LocationExpressionNotSupportedForNone);

		protected override void Build(ref IConditionExpression conditionExpression, ref ConditionExpression properties)
		{
			base.Build(ref conditionExpression, ref properties);

			var expression = properties.Expression!;

			if (!expression.StartsWith(value: @"In(", StringComparison.Ordinal) || !expression.EndsWith(value: @")", StringComparison.Ordinal))
			{
				AddErrorMessage(conditionExpression, Resources.ErrorMesasge_IncorrectConditionExpressionForNone);

				return;
			}

			var state = expression.Substring(startIndex: 3, expression.Length - 4).Trim();

			if (Identifier.TryCreate(state, out var inState))
			{
				conditionExpression = new NoneConditionExpressionEvaluator(properties, inState);
			}
			else
			{
				AddErrorMessage(conditionExpression, Resources.ErrorMesasge_IncorrectConditionExpression);
			}
		}

		private class DataModelHandlerFactory : IDataModelHandlerFactory
		{
		#region Interface IDataModelHandlerFactory

			public bool CanHandle(string dataModelType) => dataModelType == DataModelType;

			public IDataModelHandler CreateHandler(IErrorProcessor errorProcessor) => new NoneDataModelHandler(errorProcessor);

		#endregion
		}
	}
}