// Copyright © 2019-2026 Sergii Artemenko
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

#if !NET5_0_OR_GREATER

namespace System.Threading.Tasks;

internal static class TaskCompletionSourcePolyfills
{
	extension<TResult>(TaskCompletionSource<TResult> tcs)
	{
		/// <summary>
		///     Transitions the underlying <see cref="Task{TResult}" /> into the <see cref="TaskStatus.Canceled" /> state
		///     using the specified token.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token with which to cancel the <see cref="Task{TResult}" />.</param>
		/// <exception cref="InvalidOperationException">
		///     The underlying <see cref="Task{TResult}" /> is already in one of the three final states:
		///     <see cref="TaskStatus.RanToCompletion" />,
		///     <see cref="TaskStatus.Faulted" />, or
		///     <see cref="TaskStatus.Canceled" />.
		/// </exception>
		public void SetCanceled(CancellationToken cancellationToken)
		{
			if (!tcs.TrySetCanceled(cancellationToken))
			{
				throw new InvalidOperationException(Xtate.Resources.Exception_TheUnderlyingTaskIsAlreadyInFinalState);
			}
		}
	}
}

#endif
