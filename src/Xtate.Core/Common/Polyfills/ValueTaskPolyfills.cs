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

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks;

internal static class ValueTaskPolyfills
{
	extension(ValueTask)
	{
		/// <summary>Gets a task that has already completed successfully.</summary>
		public static ValueTask CompletedTask => new();

		/// <summary>Creates a <see cref="ValueTask{TResult}" /> that's completed successfully with the specified result.</summary>
		/// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
		/// <param name="result">The result to store into the completed task.</param>
		/// <returns>The successfully completed task.</returns>
		public static ValueTask<TResult> FromResult<TResult>(TResult result) => new(result);

		/// <summary>
		///     Creates a <see cref="ValueTask" /> that has completed due to cancellation with the specified cancellation
		///     token.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
		/// <returns>The canceled task.</returns>
		public static ValueTask FromCanceled(CancellationToken cancellationToken) => new(Task.FromCanceled(cancellationToken));

		/// <summary>
		///     Creates a <see cref="ValueTask{TResult}" /> that has completed due to cancellation with the specified
		///     cancellation token.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
		/// <returns>The canceled task.</returns>
		public static ValueTask<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) => new(Task.FromCanceled<TResult>(cancellationToken));

		/// <summary>Creates a <see cref="ValueTask" /> that has completed with the specified exception.</summary>
		/// <param name="exception">The exception with which to complete the task.</param>
		/// <returns>The faulted task.</returns>
		public static ValueTask FromException(Exception exception) => new(Task.FromException(exception));

		/// <summary>Creates a <see cref="ValueTask{TResult}" /> that has completed with the specified exception.</summary>
		/// <param name="exception">The exception with which to complete the task.</param>
		/// <returns>The faulted task.</returns>
		public static ValueTask<TResult> FromException<TResult>(Exception exception) => new(Task.FromException<TResult>(exception));
	}
}

#endif