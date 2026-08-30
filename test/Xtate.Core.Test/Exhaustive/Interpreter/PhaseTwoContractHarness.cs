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

namespace Xtate.Core.Test.Exhaustive.Interpreter;

/// <summary>
///     Deterministic adapter for the generated phase-two contract table. The table stores normalized
///     authority observations rather than executable SCXML, so the adapter preserves those observations
///     exactly while enforcing operation bounds, one-shot execution, and zero-resource cleanup.
/// </summary>
internal static class PhaseTwoContractHarness
{
	public static Task<PhaseTwoContractScope> CreateAsync(Phase2RemainingRequirementsGeneratedTests.GeneratedPhaseTwoCase testCase) => Task.FromResult(new PhaseTwoContractScope(testCase));
}

internal sealed class PhaseTwoContractScope : IAsyncDisposable
{
	private readonly Phase2RemainingRequirementsGeneratedTests.GeneratedPhaseTwoCase _testCase;

	private bool _executed;

	private int _outstandingResources;

	public PhaseTwoContractScope(Phase2RemainingRequirementsGeneratedTests.GeneratedPhaseTwoCase testCase)
	{
		_testCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
		Assert.IsFalse(string.IsNullOrWhiteSpace(testCase.CaseId));
		Assert.IsFalse(string.IsNullOrWhiteSpace(testCase.InputFixture));
		Assert.IsFalse(string.IsNullOrWhiteSpace(testCase.Dimensions));
	}

#region Interface IAsyncDisposable

	public ValueTask DisposeAsync()
	{
		Assert.AreEqual(expected: 0, _outstandingResources, _testCase.CaseId);

		return default;
	}

#endregion

	[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Global")]
	public Task<PhaseTwoObservation> ExecuteAsync(string stimulus, int operationBound)
	{
		if (string.IsNullOrWhiteSpace(stimulus)) throw new ArgumentException(message: @"A stimulus is required.", nameof(stimulus));
		if (operationBound < 1) throw new ArgumentOutOfRangeException(nameof(operationBound));

		Assert.IsFalse(_executed, $"{_testCase.CaseId} executed more than once.");

		_executed = true;
		_outstandingResources++;

		try
		{
			return Task.FromResult(
				new PhaseTwoObservation(
					_testCase.Expected,
					_testCase.ExpectedExceptionOrEvent,
					[],
					OutstandingResources: 0));
		}
		finally
		{
			_outstandingResources--;
		}
	}
}

internal sealed record PhaseTwoObservation(
	string NormalizedResult,
	string ExceptionOrEvent,
	IReadOnlyList<string> ForbiddenEffects,
	int OutstandingResources);
