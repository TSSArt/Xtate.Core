﻿// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.Builder;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public class FinalizeBuilder : BuilderBase, IFinalizeBuilder
{
	private ImmutableArray<IExecutableEntity>.Builder? _actions;

#region Interface IFinalizeBuilder

	public IFinalize Build() => new FinalizeEntity { Ancestor = Ancestor, Action = _actions?.ToImmutable() ?? default };

	public void AddAction(IExecutableEntity action)
	{
		Infra.Requires(action);

		(_actions ??= ImmutableArray.CreateBuilder<IExecutableEntity>()).Add(action);
	}

#endregion
}