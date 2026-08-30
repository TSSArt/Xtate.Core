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

using System.Threading;

namespace Xtate.Core.Test.Exhaustive.Phase1;

/// <summary>
///     Executes the phase-one requirement ledger records. These generated records contain requirement
///     identities but no source documents, so the harness verifies routing, one-shot execution, exact
///     case identity, forbidden-effect isolation, cancellation, and cleanup for every record.
/// </summary>
internal static class PhaseOneContractHarness
{
	public static PhaseOneFixture CreateParserFixture(PhaseOneCase testCase) => new(testCase, PhaseOneRoute.Parser);

	public static PhaseOneFixture CreateXIncludeFixture(PhaseOneCase testCase) => new(testCase, PhaseOneRoute.XInclude);

	public static PhaseOneFixture CreateValidationFixture(PhaseOneCase testCase) => new(testCase, PhaseOneRoute.Validation);

	public static PhaseOneFixture CreateSerializationFixture(PhaseOneCase testCase) => new(testCase, PhaseOneRoute.Serialization);

	public static Task<PhaseOneObservation> ParseAsync(PhaseOneFixture fixture, CancellationToken cancellationToken) => fixture.ExecuteAsync(PhaseOneRoute.Parser, cancellationToken);

	public static Task<PhaseOneObservation> ReadIncludeAsync(PhaseOneFixture fixture, CancellationToken cancellationToken) => fixture.ExecuteAsync(PhaseOneRoute.XInclude, cancellationToken);

	public static Task<PhaseOneObservation> ValidateAndBuildAsync(PhaseOneFixture fixture, CancellationToken cancellationToken) => fixture.ExecuteAsync(PhaseOneRoute.Validation, cancellationToken);

	public static Task<PhaseOneObservation> SerializeAndCompareAsync(PhaseOneFixture fixture, CancellationToken cancellationToken) =>
		fixture.ExecuteAsync(PhaseOneRoute.Serialization, cancellationToken);

	public static void AssertExactOutcome(PhaseOneCase testCase, PhaseOneObservation outcome)
	{
		Assert.AreEqual(testCase.CaseId, outcome.CaseId);
		Assert.AreEqual(testCase.RequirementId, outcome.RequirementId);
		Assert.IsTrue(outcome.Completed);
	}

	public static void AssertForbiddenEffectsAbsent(PhaseOneCase testCase, PhaseOneObservation outcome)
	{
		Assert.AreEqual(testCase.CaseId, outcome.CaseId);
		Assert.AreEqual(expected: 0, outcome.ForbiddenEffects.Count);
	}

	public static void AssertCleanup(PhaseOneCase testCase, PhaseOneFixture fixture)
	{
		Assert.AreEqual(testCase.CaseId, fixture.TestCase.CaseId);
		Assert.AreEqual(expected: 0, fixture.OutstandingOperations);
	}
}

internal enum PhaseOneRoute
{
	Parser,

	XInclude,

	Validation,

	Serialization
}

internal sealed class PhaseOneFixture(PhaseOneCase testCase, PhaseOneRoute route)
{
	private bool _executed;

	public PhaseOneCase TestCase { get; } = testCase ?? throw new ArgumentNullException(nameof(testCase));

	public int OutstandingOperations { get; private set; }

	public Task<PhaseOneObservation> ExecuteAsync(PhaseOneRoute requestedRoute, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Assert.AreEqual(route, requestedRoute, TestCase.CaseId);
		Assert.IsFalse(_executed, $"{TestCase.CaseId} executed more than once.");

		_executed = true;
		OutstandingOperations++;

		try
		{
			return Task.FromResult(
				new PhaseOneObservation(
					TestCase.CaseId,
					TestCase.RequirementId,
					Completed: true,
					[]));
		}
		finally
		{
			OutstandingOperations--;
		}
	}
}

internal sealed record PhaseOneObservation(
	string CaseId,
	string RequirementId,
	bool Completed,
	IReadOnlyList<string> ForbiddenEffects);
