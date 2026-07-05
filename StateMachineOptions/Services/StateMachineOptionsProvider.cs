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

using Xtate.Interpreter;
using Xtate.IoC.Options;
using Xtate.Persistence;
using Xtate.Scxml;
using Xtate.StateMachineHost.Services;

namespace Xtate.StateMachineOptions.Services;

[InstantiatedByIoC]
public class StateMachineOptionsProvider(IOptions<StateMachineOptions> options) : IDestroyOnIdleTimeout, IUnhandledErrorBehaviour, IPersistenceOptions, IXIncludeOptions
{
	private readonly StateMachineOptions _options = options.Value;

#region Interface IDestroyOnIdleTimeout

	TimeSpan IDestroyOnIdleTimeout.IdleTimeout => _options.DestroyOnIdleTimeout;

#endregion

#region Interface IPersistenceOptions

	PersistenceLevel IPersistenceOptions.PersistenceLevel => _options.PersistenceLevel;

#endregion

#region Interface IUnhandledErrorBehaviour

	UnhandledErrorBehaviour IUnhandledErrorBehaviour.Behaviour => _options.UnhandledErrorBehaviour;

#endregion

#region Interface IXIncludeOptions

	bool IXIncludeOptions.XIncludeAllowed => _options.XIncludeAllowed;

	int IXIncludeOptions.MaxNestingLevel => _options.XIncludeMaxNestingLevel;

#endregion
}