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
	public class DefaultRaiseEvaluator : IRaise, IExecEvaluator, IAncestorProvider
	{
		private readonly IRaise _raise;

		public DefaultRaiseEvaluator(IRaise raise)
		{
			if (raise is null) throw new ArgumentNullException(nameof(raise));

			Infra.NotNull(raise.OutgoingEvent);

			_raise = raise;
		}

	#region Interface IAncestorProvider

		object IAncestorProvider.Ancestor => _raise;

	#endregion

	#region Interface IExecEvaluator

		public virtual ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			return executionContext.Send(_raise.OutgoingEvent!, token);
		}

	#endregion

	#region Interface IRaise

		public IOutgoingEvent OutgoingEvent => _raise.OutgoingEvent!;

	#endregion
	}
}