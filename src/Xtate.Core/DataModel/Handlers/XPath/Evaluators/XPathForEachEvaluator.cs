#region Copyright © 2019-2023 Sergii Artemenko

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
using System.Threading.Tasks;

namespace Xtate.DataModel.XPath;

public class XPathForEachEvaluator : DefaultForEachEvaluator
{
	public XPathForEachEvaluator(IForEach forEach) : base(forEach) { }
	
	public required Func<ValueTask<XPathEngine>> EngineFactory { private get; init; }

	public override async ValueTask Execute()
	{
		var engine = await EngineFactory().ConfigureAwait(false);

		engine.EnterScope();

		try
		{
			await ItemEvaluator.DeclareLocalVariable().ConfigureAwait(false);

			if (IndexEvaluator is not null)
			{
				await IndexEvaluator.DeclareLocalVariable().ConfigureAwait(false);
			}

			await base.Execute().ConfigureAwait(false);
		}
		finally
		{
			engine.LeaveScope();
		}
	}
}