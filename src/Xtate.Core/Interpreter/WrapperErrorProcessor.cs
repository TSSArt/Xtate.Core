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
using System.Collections.Immutable;

namespace Xtate.Core
{
	/// <summary>
	///     Makes sure error is thrown by <see cref="ThrowIfErrors()" /> in case of underlying error processor does not throw
	///     exception in it.
	/// </summary>
	internal sealed class WrapperErrorProcessor : IErrorProcessor
	{
		private readonly IErrorProcessor _errorProcessor;

		private ErrorItem? _error;

		public WrapperErrorProcessor(IErrorProcessor errorProcessor) => _errorProcessor = errorProcessor;

	#region Interface IErrorProcessor

		public void AddError(ErrorItem errorItem)
		{
			_error ??= errorItem ?? throw new ArgumentNullException(nameof(errorItem));

			_errorProcessor.AddError(errorItem);
		}

		public void ThrowIfErrors()
		{
			_errorProcessor.ThrowIfErrors();

			if (_error is { } error)
			{
				throw new StateMachineValidationException(ImmutableArray.Create(error));
			}
		}

		public bool LineInfoRequired => _errorProcessor.LineInfoRequired;

	#endregion
	}
}