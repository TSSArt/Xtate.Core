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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.IoC;

namespace Xtate.Core
{
	public interface IInterpreterModel
	{
		StateMachineNode                  Root                   { get; }
		//ImmutableArray<DataModelNode>     DataModelList          { get; }
		ImmutableDictionary<int, IEntity> EntityMap { get; }
	}

	public class InterpreterModel : IInterpreterModel, IAsyncInitialization
	{
		private readonly InterpreterModelBuilder     _interpreterModelBuilder;
		private readonly AsyncInit<StateMachineNode> _stateMachineNodeAsyncInit;

		[Obsolete]
		public InterpreterModel(StateMachineNode root,
								int maxConfigurationLength,
								ImmutableDictionary<int, IEntity> entityMap,
								ImmutableArray<DataModelNode> dataModelList)
		{
			MaxConfigurationLength = maxConfigurationLength;
			EntityMap = entityMap;
			DataModelList = dataModelList;
		}

		public InterpreterModel(InterpreterModelBuilder interpreterModelBuilder) 
		{
			_interpreterModelBuilder = interpreterModelBuilder;
			_stateMachineNodeAsyncInit = AsyncInit.RunAfter(_interpreterModelBuilder, builder => builder.Build2(null));
		}

		public StateMachineNode Root => _stateMachineNodeAsyncInit.Value;

		public int MaxConfigurationLength { get; }

		public ImmutableDictionary<int, IEntity> EntityMap { get; }

		public ImmutableArray<DataModelNode> DataModelList  { get; }

		public virtual Task Initialization => _stateMachineNodeAsyncInit.Task;
	}
}