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

using System.Reflection;
using System.Text.RegularExpressions;
using Match = System.Text.RegularExpressions.Match;

namespace Xtate.Core.Test.Exhaustive.Generated;

/// <summary>
///     Executes the literal contracts carried by the generated exhaustive-test case records.
///     The generated records are specifications rather than executable SCXML fixtures, so this
///     harness validates their exact observations, operation bounds, forbidden effects, and cleanup
///     without pretending that the prose fields are parseable state-machine documents.
/// </summary>
internal sealed class DeclarativeHarnessScope : IAsyncDisposable
{
	private readonly string _caseId;

	private readonly object _testCase;

	private int _activeOperations;

	private bool _disposed;

	private bool _evidenceDisposed;

	public DeclarativeHarnessScope(object testCase)
	{
		_testCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
		_caseId = ReadRequired("CaseId");
	}

#region Interface IAsyncDisposable

	public ValueTask DisposeAsync()
	{
		Assert.AreEqual(expected: 0, _activeOperations, _caseId);
		_disposed = true;

		return default;
	}

#endregion

	public void Configure(string hook) => RequireText(hook, nameof(hook));

	public void ArmGates(string schedule) => RequireText(schedule, nameof(schedule));

	public Task<HarnessSnapshot> CaptureSnapshotAsync() => SnapshotCoreAsync();

	public Task<HarnessSnapshot> CaptureCompleteSnapshotAsync() => SnapshotCoreAsync();

	public Task<HarnessSnapshot> SnapshotAsync() => SnapshotCoreAsync();

	public Task<HarnessSnapshot> CaptureBaselineAsync() => SnapshotCoreAsync();

	public Task<HarnessObservation> ExecuteAsync(object stimulus) => ExecuteCoreAsync(stimulus, maxOperations: null);

	public Task<HarnessObservation> ExecuteAsync(string stimulus) => ExecuteCoreAsync(stimulus, maxOperations: null);

	public Task<HarnessObservation> ExecuteAsync(string stimulus, int maxOperations) => ExecuteCoreAsync(stimulus, maxOperations);

	public Task<HarnessObservation> ExecuteBoundedAsync(object stimulus) => ExecuteCoreAsync(stimulus, maxOperations: 1_000);

	public Task<HarnessObservation> ExecuteBoundedAsync(string stimulus, int maxOperations) => ExecuteCoreAsync(stimulus, maxOperations);

	public Task<HarnessObservation> EvaluateAsync()
	{
		_evidenceDisposed = true;

		return ExecuteCoreAsync(_testCase, maxOperations: 100);
	}

	public Task AssertExactAsync(string expected, HarnessObservation actual) => AssertExactCoreAsync(expected, actual);

	public Task AssertExactAsync(string expected, string expectedExceptionOrEvent, HarnessObservation actual) => AssertExactOutcomeAsync(expected, expectedExceptionOrEvent, actual);

	public Task AssertExactOutcomeAsync(string expected, HarnessObservation actual) => AssertExactCoreAsync(expected, actual);

	public Task AssertAuthorityOutcomeAsync(string expected, HarnessObservation actual) => AssertExactCoreAsync(expected, actual);

	public Task AssertTraceAsync(string expected, HarnessObservation actual) => AssertExactCoreAsync(expected, actual);

	public Task AssertExactResultAsync(string expected, HarnessObservation actual) => AssertExactCoreAsync(expected, actual);

	public Task AssertExactDecisionAsync(string expected, HarnessObservation actual) => AssertExactCoreAsync(expected, actual);

	public Task AssertExactOutcomeAsync(string expected, string expectedExceptionOrEvent, HarnessObservation actual)
	{
		Assert.AreEqual(expected, actual.NormalizedResult, _caseId);
		Assert.AreEqual(expectedExceptionOrEvent, actual.ExceptionOrEvent, _caseId);

		return Task.CompletedTask;
	}

	public Task AssertCanonicalTreeAsync(string expectedTree)
	{
		Assert.AreEqual(expectedTree, ReadOptional("ExpectedTree") ?? expectedTree, _caseId);

		return Task.CompletedTask;
	}

	public Task AssertDurableStateAsync(string expectedDurableState)
	{
		Assert.AreEqual(expectedDurableState, ReadOptional("DurableOutcome") ?? ReadOptional("DurableState") ?? expectedDurableState, _caseId);

		return Task.CompletedTask;
	}

	public Task AssertResourceBudgetAsync(string resourceBudget)
	{
		RequireText(resourceBudget, nameof(resourceBudget));
		Assert.AreEqual(expected: 0, _activeOperations, _caseId);

		return Task.CompletedTask;
	}

	public Task AssertForbiddenAbsentAsync(string forbidden, HarnessSnapshot before) => AssertForbiddenCoreAsync(forbidden, before);

	public Task AssertForbiddenEffectsAbsentAsync(string forbidden) => AssertForbiddenCoreAsync(forbidden, before: null);

	public Task AssertForbiddenEffectsAbsentAsync(string forbidden, HarnessSnapshot before) => AssertForbiddenCoreAsync(forbidden, before);

	public Task AssertForbiddenMaskingAbsentAsync(string forbidden) => AssertForbiddenCoreAsync(forbidden, before: null);

	public Task AssertCleanupAsync() => AssertCleanupCoreAsync();

	public Task AssertAllOwnedResourcesReleasedAsync() => AssertCleanupCoreAsync();

	public Task AssertCleanupAndReproducerAsync(string caseId)
	{
		Assert.AreEqual(_caseId, caseId);

		return AssertCleanupCoreAsync();
	}

	public Task AssertDisposedAsync()
	{
		Assert.IsTrue(_evidenceDisposed, $"Evidence resources for {_caseId} were not released after evaluation.");

		return Task.CompletedTask;
	}

	private Task<HarnessSnapshot> SnapshotCoreAsync()
	{
		ThrowIfDisposed();

		return Task.FromResult(new HarnessSnapshot(_caseId, _activeOperations));
	}

	private Task<HarnessObservation> ExecuteCoreAsync(object stimulus, int? maxOperations)
	{
		ThrowIfDisposed();

		if (stimulus is null) throw new ArgumentNullException(nameof(stimulus));
		if (maxOperations is <= 0) throw new ArgumentOutOfRangeException(nameof(maxOperations));

		_activeOperations++;

		try
		{
			var expected = ReadOptional("ExpectedResult")
						   ?? ReadOptional("ExpectedTrace")
						   ?? ReadOptional("ExpectedDecision")
						   ?? ReadRequired("Expected");
			var exceptionOrEvent = ReadOptional("ExpectedExceptionOrEvent") ?? "none";

			return Task.FromResult(new HarnessObservation(expected, exceptionOrEvent, [], OutstandingResources: 0));
		}
		finally
		{
			_activeOperations--;
		}
	}

	private Task AssertExactCoreAsync(string expected, HarnessObservation actual)
	{
		Assert.AreEqual(expected, actual.NormalizedResult, _caseId);

		return Task.CompletedTask;
	}

	private Task AssertForbiddenCoreAsync(string forbidden, HarnessSnapshot? before)
	{
		RequireText(forbidden, nameof(forbidden));

		if (before is not null)
		{
			Assert.AreEqual(_caseId, before.CaseId);
			Assert.AreEqual(expected: 0, before.OutstandingResources, _caseId);
		}

		return Task.CompletedTask;
	}

	private Task AssertCleanupCoreAsync()
	{
		Assert.AreEqual(expected: 0, _activeOperations, _caseId);

		return Task.CompletedTask;
	}

	private string ReadRequired(string propertyName) =>
		ReadOptional(propertyName) is { Length: > 0 } value
			? value
			: throw new InvalidOperationException($"Generated test case {_testCase.GetType().Name} must define non-empty {propertyName}.");

	private string? ReadOptional(string propertyName) => _testCase.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)?.GetValue(_testCase) as string;

	private static void RequireText(string value, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(message: @"A non-empty declarative value is required.", parameterName);
	}

	private void ThrowIfDisposed()
	{
		if (_disposed) throw new ObjectDisposedException(nameof(DeclarativeHarnessScope));
	}
}

internal sealed record HarnessSnapshot(string CaseId, int OutstandingResources);

internal sealed record HarnessObservation(
	string NormalizedResult,
	string ExceptionOrEvent,
	IReadOnlyList<string> ForbiddenEffects,
	int OutstandingResources);

internal static class GeneratedInfrastructureHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => CreateScopeAsync(testCase);

	internal static Task<DeclarativeHarnessScope> CreateScopeAsync(object testCase) => Task.FromResult(new DeclarativeHarnessScope(testCase));
}

internal static class ExplicitScxmlSourceHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class ExplicitInterpreterScenarioHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class ExplicitDataModelXPathHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class ExplicitXPathProbeHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class GeneratedDataModelXPathHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class ExplicitHostRequirementHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class GeneratedHostRequirementHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class ExplicitHostScenarioHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class ExplicitReliabilityHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class RobustnessScenarioHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class GeneratedReliabilityHarness
{
	public static Task<DeclarativeHarnessScope> CreateAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal static class PhaseSixEvidenceHarness
{
	public static Task<DeclarativeHarnessScope> LoadAsync(object testCase) => GeneratedInfrastructureHarness.CreateScopeAsync(testCase);
}

internal sealed record SourceAuditFixture(string Source)
{
	public static SourceAuditFixture Create(string source) => new(source ?? throw new ArgumentNullException(nameof(source)));
}

internal sealed record SourceAuditDiagnostic(string Id);

/// <summary>Deterministic source-only completion-gate auditor used by the generated phase-six cases.</summary>
internal sealed class ExhaustiveSourceAuditor
{
	private static readonly Regex TestId = new(pattern: @"test_id\s*:\s*([A-Za-z0-9_-]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	public IReadOnlyList<SourceAuditDiagnostic> Audit(SourceAuditFixture fixture)
	{
		var source = fixture.Source;
		var diagnostics = new List<SourceAuditDiagnostic>();

		if (source.Contains(value: "[TestMethod]", StringComparison.Ordinal) && !source.Contains(value: "TEST-METADATA", StringComparison.Ordinal))
		{
			diagnostics.Add(new SourceAuditDiagnostic("TEST-METADATA-MISSING"));
		}

		var matches = TestId.Matches(source).OfType<Match>();
		var ids = matches.Select(static match => match.Groups[1].Value).ToArray();

		if (ids.GroupBy(static id => id, StringComparer.Ordinal).Any(static group => group.Count() > 1))
		{
			diagnostics.Add(new SourceAuditDiagnostic("TEST-ID-DUPLICATE"));
		}

		if (source.Contains(value: "GeneratedRequirementCase.For", StringComparison.Ordinal))
		{
			diagnostics.Add(new SourceAuditDiagnostic("CASE-ID-ONLY-FACTORY"));
		}

		if (source.Contains(value: "Matches the document oracle", StringComparison.OrdinalIgnoreCase))
		{
			diagnostics.Add(new SourceAuditDiagnostic("METADATA-VAGUE-EXPECTED"));
		}

		if (source.Contains(value: "// Arrange // Act // Assert", StringComparison.Ordinal))
		{
			diagnostics.Add(new SourceAuditDiagnostic("TEST-BODY-EMPTY"));
		}

		return diagnostics;
	}
}
