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
using Xtate.IoProcessor;

namespace Xtate.Core;

public class StateMachineContext : IStateMachineContext
{
    public required ICaseSensitivity CaseSensitivity { private get; [UsedImplicitly] init; }

	public required IStateMachine StateMachine { private get; [UsedImplicitly] init; }

	public required ServiceList<IIoProcessor> IoProcessors { private get; [UsedImplicitly] init; }

	public required ServiceList<IXDataModelProperty> XDataModelProperties { private get; [UsedImplicitly] init; }

	public required IStateMachineSessionId StateMachineSessionId { private get; [UsedImplicitly] init; }

#region Interface IStateMachineContext

    [field: AllowNull, MaybeNull]
    public DataModelList DataModel => field ??= CreateDataModel();

	public OrderedSet<StateEntityNode> Configuration { get; } = [];

	public KeyList<StateEntityNode> HistoryValue { get; } = [];

	public EntityQueue<IIncomingEvent> InternalQueue { get; } = [];

	public OrderedSet<StateEntityNode> StatesToInvoke { get; } = [];

	public InvokeIdSet ActiveInvokes { get; } = [];

	public DataModelValue DoneData { get; set; }

#endregion

	private DataModelList CreateDataModel()
	{
		var dataModel = new DataModelList(CaseSensitivity.CaseInsensitive);

		dataModel.AddInternal(key: @"_name", StateMachine.Name, DataModelAccess.ReadOnly);
		dataModel.AddInternal(key: @"_sessionid", StateMachineSessionId.SessionId, DataModelAccess.Constant);
		dataModel.AddInternal(key: @"_event", value: default, DataModelAccess.ReadOnly);
		dataModel.AddInternal(key: @"_ioprocessors", LazyValue.Create(this, ctx => ctx.GetIoProcessors()), DataModelAccess.Constant);
		dataModel.AddInternal(key: @"_x", LazyValue.Create(this, ctx => ctx.GetPlatform()), DataModelAccess.Constant);

		return dataModel;
	}

	private DataModelValue GetPlatform()
	{
		if (XDataModelProperties.Count == 0)
		{
			return DataModelList.Empty;
		}

		var list = new DataModelList(DataModelAccess.ReadOnly, CaseSensitivity.CaseInsensitive);

		foreach (var property in XDataModelProperties)
		{
			list.AddInternal(property.Name, property.Value, DataModelAccess.Constant);
		}

		return list;
	}

	private DataModelValue GetIoProcessors()
	{
		if (IoProcessors.Count == 0)
		{
			return DataModelList.Empty;
		}

		var caseInsensitive = CaseSensitivity.CaseInsensitive;

		var list = new DataModelList(DataModelAccess.ReadOnly, caseInsensitive);

		foreach (var ioProcessor in IoProcessors)
		{
			var value = new DataModelList(DataModelAccess.ReadOnly, caseInsensitive);
			value.AddInternal(key: @"location", ioProcessor.GetTarget(StateMachineSessionId.SessionId)?.ToString(), DataModelAccess.Constant);

			list.AddInternal(ioProcessor.Id.ToString(), value, DataModelAccess.Constant);
		}

		return list;
	}
}