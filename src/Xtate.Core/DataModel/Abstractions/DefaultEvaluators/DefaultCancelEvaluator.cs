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
	public class DefaultCancelEvaluator : ICancel, IExecEvaluator, IAncestorProvider
	{
		private readonly CancelEntity _cancel;

		public DefaultCancelEvaluator(in CancelEntity cancel)
		{
			_cancel = cancel;
			SendIdExpressionEvaluator = cancel.SendIdExpression?.As<IStringEvaluator>();
		}

		public IStringEvaluator? SendIdExpressionEvaluator { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _cancel.Ancestor;

	#endregion

	#region Interface ICancel

		public string?           SendId           => _cancel.SendId;
		public IValueExpression? SendIdExpression => _cancel.SendIdExpression;

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var sendId = SendIdExpressionEvaluator != null ? await SendIdExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false) : SendId;

			if (string.IsNullOrEmpty(sendId))
			{
				throw new ExecutionException(Resources.Exception_SendIdIsEmpty);
			}

			await executionContext.Cancel(Xtate.SendId.FromString(sendId), token).ConfigureAwait(false);
		}

	#endregion
	}
}