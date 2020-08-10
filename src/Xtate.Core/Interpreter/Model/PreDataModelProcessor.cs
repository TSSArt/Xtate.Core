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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.CustomAction;

namespace Xtate
{
	internal sealed class PreDataModelProcessor : StateMachineVisitor
	{
		private readonly IErrorProcessor _errorProcessor;
		private readonly IFactoryContext _factoryContext;

		private Dictionary<ICustomAction, CustomActionDispatcher>? _customActionDispatchers;

		private bool _postProcess;

		public PreDataModelProcessor(IErrorProcessor errorProcessor, IFactoryContext factoryContext)
		{
			_errorProcessor = errorProcessor;
			_factoryContext = factoryContext;
		}

		public async ValueTask PreProcessStateMachine(IStateMachine stateMachine, ImmutableArray<ICustomActionFactory> customActionProviders, CancellationToken token)
		{
			Visit(ref stateMachine);

			if (_customActionDispatchers != null)
			{
				foreach (var pair in _customActionDispatchers)
				{
					await pair.Value.SetupExecutor(customActionProviders, token).ConfigureAwait(false);
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
					_customActionDispatchers.Add(entity, new CustomActionDispatcher(_errorProcessor, entity, _factoryContext));
				}
			}
			else
			{
				Infrastructure.Assert(_customActionDispatchers != null);

				entity = _customActionDispatchers[entity];
			}
		}
	}
}