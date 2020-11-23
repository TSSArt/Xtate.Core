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

using System.Threading;
using System.Threading.Tasks;

namespace Xtate.DataModel.Runtime
{
	internal sealed class RuntimeDataModelHandler : DataModelHandlerBase
	{
		private const string DataModelType = "runtime";

		private RuntimeDataModelHandler(IErrorProcessor errorProcessor) : base(errorProcessor) { }

		public override ITypeInfo TypeInfo => TypeInfo<RuntimeDataModelHandler>.Instance;

		public static IDataModelHandlerFactory Factory { get; } = new DataModelHandlerFactory();

		protected override void Visit(ref IScript script) => AddErrorMessage(script, Resources.ErrorMessage_ScriptingNotSupportedInRuntimeDataModel);

		protected override void Visit(ref IDataModel dataModel) => AddErrorMessage(dataModel, Resources.ErrorMessage_DataModelNotSupportedInRuntime);

		protected override void Visit(ref IExecutableEntity executableEntity)
		{
			if (!(executableEntity is RuntimeAction) && !(executableEntity is RuntimePredicate))
			{
				AddErrorMessage(executableEntity, Resources.ErrorMessage_RuntimeActionAndPredicateOnlyAllowed);
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

			public ValueTask<IDataModelHandler> CreateHandler(IFactoryContext factoryContext, string dataModelType, IErrorProcessor errorProcessor, CancellationToken token)
			{
				Infrastructure.Assert(CanHandle(dataModelType));

				return new ValueTask<IDataModelHandler>(new RuntimeDataModelHandler(errorProcessor));
			}

		#endregion
		}
	}
}