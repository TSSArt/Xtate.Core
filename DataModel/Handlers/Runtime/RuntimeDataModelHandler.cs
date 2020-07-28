#region Copyright © 2019-2020 Sergii Artemenko
// 
// This file is part of the Xtate project. <http://xtate.net>
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
// 
#endregion

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