namespace Xtate.DataModel.Runtime
{
	internal sealed class RuntimeDataModelHandler : DataModelHandlerBase
	{
		public const string DataModelType = "runtime";

		public static readonly IDataModelHandlerFactory Factory = new DataModelHandlerFactory();

		private RuntimeDataModelHandler(IErrorProcessor errorProcessor) : base(errorProcessor) { }

		protected override void Visit(ref IScript script) => AddErrorMessage(script, Resources.ErrorMessage_ScriptingNotSupportedInRuntimeDataModel);

		protected override void Visit(ref IDataModel dataModel) => AddErrorMessage(dataModel, Resources.ErrorMessage_DataModelNotSupportedInRuntime);

		protected override void Visit(ref IExecutableEntity executableEntity)
		{
			if (!(executableEntity is RuntimeAction) && !(executableEntity is RuntimePredicate))
			{
				AddErrorMessage(executableEntity, Resources.ErrorMessage_RuntimeActionAndPredicateOnlyAllowed);
			}
		}

		private class DataModelHandlerFactory : IDataModelHandlerFactory
		{
		#region Interface IDataModelHandlerFactory

			public bool CanHandle(string dataModelType) => dataModelType == DataModelType;

			public IDataModelHandler CreateHandler(IErrorProcessor errorProcessor) => new RuntimeDataModelHandler(errorProcessor);

		#endregion
		}
	}
}