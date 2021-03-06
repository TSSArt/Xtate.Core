﻿#region Copyright © 2019-2021 Sergii Artemenko

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
using System.Threading;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultLogEvaluator : ILog, IExecEvaluator, IAncestorProvider
	{
		private readonly ILog _log;

		public DefaultLogEvaluator(ILog log)
		{
			_log = log ?? throw new ArgumentNullException(nameof(log));
			ExpressionEvaluator = log.Expression?.As<IObjectEvaluator>();
		}

		public IObjectEvaluator? ExpressionEvaluator { get; }

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _log;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			var data = default(DataModelValue);

			if (ExpressionEvaluator is not null)
			{
				var obj = await ExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);
				data = DataModelValue.FromObject(obj).AsConstant();
			}

			await executionContext.Log(LogLevel.Info, _log.Label, data, token: token).ConfigureAwait(false);
		}

	#endregion

	#region Interface ILog

		public IValueExpression? Expression => _log.Expression;

		public string? Label => _log.Label;

	#endregion
	}
}