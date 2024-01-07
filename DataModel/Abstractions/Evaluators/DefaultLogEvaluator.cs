// Copyright © 2019-2024 Sergii Artemenko
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

public abstract class LogEvaluator(ILog log) : ILog, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => log;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface ILog

	public virtual IValueExpression? Expression => log.Expression;

	public virtual string? Label => log.Label;

#endregion
}

public class DefaultLogEvaluator : LogEvaluator
{
	private readonly IObjectEvaluator? _expressionEvaluator;

	public DefaultLogEvaluator(ILog log) : base(log) => _expressionEvaluator = base.Expression?.As<IObjectEvaluator>();

	public required Func<ValueTask<ILogger<ILog>>> LoggerFactory { private get; [UsedImplicitly] init; }

	public override async ValueTask Execute()
	{
		var data = default(DataModelValue);

		if (_expressionEvaluator is not null)
		{
			var obj = await _expressionEvaluator.EvaluateObject().ConfigureAwait(false);
			data = DataModelValue.FromObject(obj).AsConstant();
		}

		var logger = await LoggerFactory().ConfigureAwait(false);
		await logger.Write(Level.Info, base.Label, data).ConfigureAwait(false);
	}
}