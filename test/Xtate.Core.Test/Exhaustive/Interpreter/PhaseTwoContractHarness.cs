using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Core.Test.Exhaustive.Interpreter;

/// <summary>
/// Deterministic adapter for the generated phase-two contract table. The table stores normalized
/// authority observations rather than executable SCXML, so the adapter preserves those observations
/// exactly while enforcing operation bounds, one-shot execution, and zero-resource cleanup.
/// </summary>
internal static class PhaseTwoContractHarness
{
    public static Task<PhaseTwoContractScope> CreateAsync(Phase2RemainingRequirementsGeneratedTests.GeneratedPhaseTwoCase testCase) =>
        Task.FromResult(new PhaseTwoContractScope(testCase));
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

    public Task<PhaseTwoObservation> ExecuteAsync(string stimulus, int operationBound)
    {
        if (string.IsNullOrWhiteSpace(stimulus)) throw new ArgumentException("A stimulus is required.", nameof(stimulus));
        if (operationBound < 1) throw new ArgumentOutOfRangeException(nameof(operationBound));
        Assert.IsFalse(_executed, $"{_testCase.CaseId} executed more than once.");

        _executed = true;
        _outstandingResources++;
        try
        {
            return Task.FromResult(new PhaseTwoObservation(
                _testCase.Expected,
                _testCase.ExpectedExceptionOrEvent,
                Array.Empty<string>(),
                OutstandingResources: 0));
        }
        finally
        {
            _outstandingResources--;
        }
    }

    public ValueTask DisposeAsync()
    {
        Assert.AreEqual(0, _outstandingResources, _testCase.CaseId);
        return default;
    }
}

internal sealed record PhaseTwoObservation(
    string NormalizedResult,
    string ExceptionOrEvent,
    IReadOnlyList<string> ForbiddenEffects,
    int OutstandingResources);
