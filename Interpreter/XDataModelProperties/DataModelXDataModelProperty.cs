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

using Xtate.DataModel;

namespace Xtate.Core;

public class DataModelXDataModelProperty : IXDataModelProperty
{
	public required ICaseSensitivity CaseSensitivity { private get; [UsedImplicitly] init; }

	public required IDataModelHandler DataModelHandler { private get; [UsedImplicitly] init; }

	public required Func<Type, IAssemblyTypeInfo> TypeInfoFactory { private get; [UsedImplicitly] init; }

#region Interface IXDataModelProperty

	public string Name => @"datamodel";

	public DataModelValue Value => LazyValue.Create(this, static p => p.Factory());

#endregion

	private DataModelValue Factory()
	{
		var typeInfo = TypeInfoFactory(DataModelHandler.GetType());

		var dataModelHandlerList = new DataModelList(CaseSensitivity.CaseInsensitive)
								   {
									   { @"name", typeInfo.FullTypeName },
									   { @"assembly", typeInfo.AssemblyName },
									   { @"version", typeInfo.AssemblyVersion },
									   { @"vars", DataModelValue.FromObject(DataModelHandler.DataModelVars) }
								   };

		dataModelHandlerList.MakeDeepConstant();

		return dataModelHandlerList;
	}
}