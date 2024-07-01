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

<<<<<<< Updated upstream
#endregion

using System;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class LogEvaluator : ILog, IExecEvaluator, IAncestorProvider
{
	private readonly ILog _log;

	protected LogEvaluator(ILog log)
	{
		Infra.Requires(log);

		_log = log;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _log;
=======
namespace Xtate.DataModel;

public abstract class LogEvaluator(ILog log) : ILog, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => log;
>>>>>>> Stashed changes

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface ILog

<<<<<<< Updated upstream
	public virtual IValueExpression? Expression => _log.Expression;

	public virtual string? Label => _log.Label;
=======
	public virtual IValueExpression? Expression => log.Expression;

	public virtual string? Label => log.Label;
>>>>>>> Stashed changes

#endregion
}

public class DefaultLogEvaluator : LogEvaluator
{
<<<<<<< Updated upstream
	public DefaultLogEvaluator(ILog log) : base(log) => ExpressionEvaluator = log.Expression?.As<IObjectEvaluator>();

	public required Func<ValueTask<ILogger<ILog>>>  LoggerFactory { private get; init; }

	public virtual IObjectEvaluator? ExpressionEvaluator { get; }
=======
	private readonly IObjectEvaluator? _expressionEvaluator;

	public DefaultLogEvaluator(ILog log) : base(log) => _expressionEvaluator = base.Expression?.As<IObjectEvaluator>();

	public required Func<ValueTask<ILogger<ILog>>> LoggerFactory { private get; [UsedImplicitly] init; }
>>>>>>> Stashed changes

	public override async ValueTask Execute()
	{
		var data = default(DataModelValue);

<<<<<<< Updated upstream
		if (ExpressionEvaluator is not null)
		{
			var obj = await ExpressionEvaluator.EvaluateObject().ConfigureAwait(false);
=======
		if (_expressionEvaluator is not null)
		{
			var obj = await _expressionEvaluator.EvaluateObject().ConfigureAwait(false);
>>>>>>> Stashed changes
			data = DataModelValue.FromObject(obj).AsConstant();
		}

		var logger = await LoggerFactory().ConfigureAwait(false);
<<<<<<< Updated upstream

		await logger.Write(Level.Info, Label, data).ConfigureAwait(false);
=======
		await logger.Write(Level.Info, base.Label, data).ConfigureAwait(false);
>>>>>>> Stashed changes
	}
}