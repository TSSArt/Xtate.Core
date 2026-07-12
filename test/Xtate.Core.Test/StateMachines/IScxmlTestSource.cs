namespace Xtate.Test.StateMachines;

/// <summary>
///     Implemented by every static machine-collection class to expose its test cases
///     to <see cref="ScxmlTestRegistry" />.
/// </summary>
public interface IScxmlTestSource
{
	IEnumerable<ScxmlTestCase> GetTestCases();
}