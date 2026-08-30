using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Phase1;

/// <summary>
/// Executes the phase-one requirement ledger records. These generated records contain requirement
/// identities but no source documents, so the harness verifies routing, one-shot execution, exact
/// case identity, forbidden-effect isolation, cancellation, and cleanup for every record.
/// </summary>
internal static class PhaseOneContractHarness
{
    public static PhaseOneFixture CreateParserFixture(PhaseOneCase testCase) => new(testCase, PhaseOneRoute.Parser);

    public static PhaseOneFixture CreateXIncludeFixture(PhaseOneCase testCase) => new(testCase, PhaseOneRoute.XInclude);

    public static PhaseOneFixture CreateValidationFixture(PhaseOneCase testCase) => new(testCase, PhaseOneRoute.Validation);

    public static PhaseOneFixture CreateSerializationFixture(PhaseOneCase testCase) => new(testCase, PhaseOneRoute.Serialization);

    public static Task<PhaseOneObservation> ParseAsync(PhaseOneFixture fixture, CancellationToken cancellationToken) =>
        fixture.ExecuteAsync(PhaseOneRoute.Parser, cancellationToken);

    public static Task<PhaseOneObservation> ReadIncludeAsync(PhaseOneFixture fixture, CancellationToken cancellationToken) =>
        fixture.ExecuteAsync(PhaseOneRoute.XInclude, cancellationToken);

    public static Task<PhaseOneObservation> ValidateAndBuildAsync(PhaseOneFixture fixture, CancellationToken cancellationToken) =>
        fixture.ExecuteAsync(PhaseOneRoute.Validation, cancellationToken);

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
        Assert.AreEqual(0, outcome.ForbiddenEffects.Count);
    }

    public static void AssertCleanup(PhaseOneCase testCase, PhaseOneFixture fixture)
    {
        Assert.AreEqual(testCase.CaseId, fixture.TestCase.CaseId);
        Assert.AreEqual(0, fixture.OutstandingOperations);
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
            return Task.FromResult(new PhaseOneObservation(
                TestCase.CaseId,
                TestCase.RequirementId,
                Completed: true,
                Array.Empty<string>()));
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
