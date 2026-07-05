// Copyright © 2019-2026 Sergii Artemenko
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
using Xtate.DataTypes;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Model;
using Xtate.Persistence;
using Xtate.Persistence.Services;

namespace Xtate.Core;

public class NoStateMachineContext : IStateMachinePersistenceContext
{
	public readonly InMemoryStorage StateStorage = new(false);

#region Interface IStateMachineContext

	public EntityQueue<IIncomingEvent> InternalQueue { get; } = [];

	public DataModelList DataModel => [];

	public OrderedSet<StateEntityNode> Configuration { get; } = [];

	public OrderedSet<StateEntityNode> StatesToInvoke { get; } = [];

	public InvokeIdSet ActiveInvokes { get; } = [];

	public KeyList<StateEntityNode> HistoryValue { get; } = [];

	public DataModelValue DoneData { get; set; }

#endregion

#region Interface IStateMachinePersistenceContext

	public Bucket GetStateBucket() => new(StateStorage);

	public ValueTask CheckPoint(int level) => default;

	public ValueTask Shrink() => default;

#endregion
}