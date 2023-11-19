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
using Xtate.CustomAction;

namespace Xtate.Core
{
	public interface IPreDataModelProcessor
	{
		ValueTask PreProcessStateMachine(IStateMachine stateMachine, CancellationToken token);

		void      PostProcess(ref IExecutableEntity executableEntity);
	}

	public class PreDataModelProcessor : StateMachineVisitor, IPreDataModelProcessor
	{
		//private readonly InterpreterModelBuilder.Parameters _parameters;
		private         Dictionary<ICustomAction, CustomActionDispatcher>? _customActionDispatchers;
		private         bool                                               _postProcess;

		private ImmutableArray<ICustomActionFactory>   _CustomActionProviders;

		[Obsolete]
		//TODO:delete
		public PreDataModelProcessor(InterpreterModelBuilder.Parameters parameters)
		{
			_CustomActionProviders = parameters.CustomActionProviders;
		}

		public PreDataModelProcessor(
									 ) 
		{
		}

		public async ValueTask PreProcessStateMachine(IStateMachine stateMachine, CancellationToken token)
		{
			Visit(ref stateMachine);

			if (_customActionDispatchers is not null)
			{
				foreach (var pair in _customActionDispatchers)
				{
					await pair.Value.SetupExecutor(_CustomActionProviders, token).ConfigureAwait(false);
				}
			}

			_postProcess = true;
		}

		public void PostProcess(ref IExecutableEntity executableEntity)
		{
			Visit(ref executableEntity);
		}

		protected override void Visit(ref ICustomAction entity)
		{
			base.Visit(ref entity);

			if (!_postProcess)
			{
				_customActionDispatchers ??= new Dictionary<ICustomAction, CustomActionDispatcher>();

				if (!_customActionDispatchers.ContainsKey(entity))
				{
					var customActionDispatcher = new CustomActionDispatcher(null, entity, default); //TODO: ?
					_customActionDispatchers.Add(entity, customActionDispatcher);
				}
			}
			else
			{
				Infra.NotNull(_customActionDispatchers);

				entity = _customActionDispatchers[entity];
			}
		}
	}
}