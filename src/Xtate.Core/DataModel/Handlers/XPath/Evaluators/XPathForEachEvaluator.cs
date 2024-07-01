<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2024 Sergii Artemenko
// 
>>>>>>> Stashed changes
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

namespace Xtate.DataModel.XPath;

<<<<<<< Updated upstream
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

=======
public class XPathForEachEvaluator : DefaultForEachEvaluator
{
	private readonly XPathLocationExpressionEvaluator? _indexEvaluator;
	private readonly XPathLocationExpressionEvaluator  _itemEvaluator;

	public XPathForEachEvaluator(IForEach forEach) : base(forEach)
	{
		var itemEvaluator = base.Item?.As<XPathLocationExpressionEvaluator>();
		Infra.NotNull(itemEvaluator);
		_itemEvaluator = itemEvaluator;

		_indexEvaluator = base.Index?.As<XPathLocationExpressionEvaluator>();
	}

	public required Func<ValueTask<XPathEngine>> EngineFactory { private get; [UsedImplicitly] init; }

	public override async ValueTask Execute()
	{
		var engine = await EngineFactory().ConfigureAwait(false);

>>>>>>> Stashed changes
		engine.EnterScope();

		try
		{
<<<<<<< Updated upstream
			await ItemEvaluator.DeclareLocalVariable().ConfigureAwait(false);

			if (IndexEvaluator is not null)
			{
				await IndexEvaluator.DeclareLocalVariable().ConfigureAwait(false);
=======
			await _itemEvaluator.DeclareLocalVariable().ConfigureAwait(false);

			if (_indexEvaluator is not null)
			{
				await _indexEvaluator.DeclareLocalVariable().ConfigureAwait(false);
>>>>>>> Stashed changes
			}

			await base.Execute().ConfigureAwait(false);
		}
		finally
		{
			engine.LeaveScope();
		}
	}
}