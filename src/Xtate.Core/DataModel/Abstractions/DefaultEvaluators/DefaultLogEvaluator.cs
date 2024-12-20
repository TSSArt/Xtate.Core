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

namespace Xtate.DataModel;

public class DefaultLogEvaluator : LogEvaluator
{
	private readonly IObjectEvaluator? _expressionEvaluator;

	public DefaultLogEvaluator(ILog log) : base(log) => _expressionEvaluator = base.Expression?.As<IObjectEvaluator>();

	public required Deferred<ILogController> LogController { private get; [UsedImplicitly] init; }

	public override async ValueTask Execute()
	{
		var logController = await LogController().ConfigureAwait(false);

		if (logController.IsEnabled)
		{
			var data = default(DataModelValue);

			if (_expressionEvaluator is not null)
			{
				var obj = await _expressionEvaluator.EvaluateObject().ConfigureAwait(false);
				data = DataModelValue.FromObject(obj).AsConstant();
			}

			await logController.Log(base.Label, data).ConfigureAwait(false);
		}
	}
}