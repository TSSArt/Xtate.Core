#region Copyright © 2019-2021 Sergii Artemenko

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

namespace Xtate.Core
{
	/// <summary>
	///     Aggregates all errors and throw single exception with previously added errors
	/// </summary>
	public sealed class DetailedErrorProcessor : IErrorProcessor
	{
		private readonly StateMachineOrigin _origin;
		private readonly SessionId?         _sessionId;

		private ImmutableArray<ErrorItem>.Builder? _errors;

		public DetailedErrorProcessor(SessionId? sessionId, StateMachineOrigin origin)
		{
			_sessionId = sessionId;
			_origin = origin;
		}

	#region Interface IErrorProcessor

		public void ThrowIfErrors()
		{
			if (_errors is { } errors)
			{
				_errors = default;

				throw new StateMachineValidationException(errors.ToImmutable(), _sessionId, _origin);
			}
		}

		void IErrorProcessor.AddError(ErrorItem errorItem)
		{
			if (errorItem is null) throw new ArgumentNullException(nameof(errorItem));

			(_errors ??= ImmutableArray.CreateBuilder<ErrorItem>()).Add(errorItem);
		}

		bool IErrorProcessor.LineInfoRequired => true;

	#endregion
	}
}