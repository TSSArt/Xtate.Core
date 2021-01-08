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

using System;
using System.Collections.Immutable;
using Xtate.Core;

namespace Xtate
{
	public interface IInvoke : IEntity
	{
		Uri?                                Type             { get; }
		IValueExpression?                   TypeExpression   { get; }
		Uri?                                Source           { get; }
		IValueExpression?                   SourceExpression { get; }
		string?                             Id               { get; }
		ILocationExpression?                IdLocation       { get; }
		ImmutableArray<ILocationExpression> NameList         { get; }
		bool                                AutoForward      { get; }
		ImmutableArray<IParam>              Parameters       { get; }
		IFinalize?                          Finalize         { get; }
		IContent?                           Content          { get; }
	}
}