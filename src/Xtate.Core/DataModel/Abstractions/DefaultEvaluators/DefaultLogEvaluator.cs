#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultLogEvaluator : ILog, IExecEvaluator, IAncestorProvider
	{
		private readonly LogEntity _log;

		public DefaultLogEvaluator(in LogEntity log)
		{
			_log = log;
			ExpressionEvaluator = log.Expression?.As<IObjectEvaluator>();
		}

		public IObjectEvaluator? ExpressionEvaluator { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _log.Ancestor;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var data = default(DataModelValue);

			if (ExpressionEvaluator != null)
			{
				var obj = await ExpressionEvaluator.EvaluateObject(executionContext, token).ConfigureAwait(false);
				data = DataModelValue.FromObject(obj.ToObject()).AsConstant();
			}

			await executionContext.Log(_log.Label, data, token).ConfigureAwait(false);
		}

	#endregion

	#region Interface ILog

		public IValueExpression? Expression => _log.Expression;

		public string? Label => _log.Label;

	#endregion
	}
}