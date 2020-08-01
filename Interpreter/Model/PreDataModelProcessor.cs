#region Copyright © 2019-2020 Sergii Artemenko
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
// 
#endregion

using System.Collections.Immutable;
using Xtate.CustomAction;

namespace Xtate
{
	internal sealed class PreDataModelProcessor : StateMachineVisitor
	{
		private readonly ImmutableArray<ICustomActionFactory> _customActionProviders;
		private readonly IErrorProcessor                      _errorProcessor;

		public PreDataModelProcessor(IErrorProcessor errorProcessor, ImmutableArray<ICustomActionFactory> customActionProviders)
		{
			_errorProcessor = errorProcessor;
			_customActionProviders = customActionProviders;
		}

		public void Process(ref IExecutableEntity executableEntity)
		{
			Visit(ref executableEntity);
		}

		protected override void Build(ref ICustomAction customAction, ref CustomActionEntity customActionProperties)
		{
			base.Build(ref customAction, ref customActionProperties);

			var customActionDispatcher = new CustomActionDispatcher(_customActionProviders, _errorProcessor, customActionProperties);
			customActionDispatcher.SetupExecutor();

			customAction = customActionDispatcher;
		}
	}
}