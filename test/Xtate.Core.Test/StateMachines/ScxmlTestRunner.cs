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

namespace Xtate.Test.StateMachines;

/// <summary>
///     MSTest runner for the SCXML state machine test collection.
///     How it works
///     ────────────
///     • The <see cref="Run" /> method executes all registered <see cref="ScxmlTestCase" />
///     instances using <c>[DynamicData]</c>, appearing as a single parameterized test in Test Explorer.
///     • Each registered test case also has a dedicated <c>[TestMethod]</c> below,
///     allowing individual test execution and better visibility in Test Explorer.
///     • Assembly-level <c>[Parallelize(MethodLevel)]</c> in <c>ParallelizeAttribute.cs</c>
///     enables concurrent execution of all test methods.
///     • All test methods delegate to <see cref="ExecuteTestCase" />, which builds a
///     lightweight DI scope, parses the SCXML, runs the interpreter, and asserts
///     the returned done-data value when expected.
///     Adding more test cases
///     ──────────────────────
///     1. Create a new <c>*Machines.cs</c> file anywhere under <c>StateMachines/</c>.
///     2. Implement <see cref="IScxmlTestSource" /> on its class.
///     3. Add an instance to <see cref="ScxmlTestRegistry" />.<c>Sources</c>.
///     New test cases are automatically picked up by the <see cref="Run" /> method and
///     should have corresponding dedicated test methods added below for individual execution.
/// </summary>
[TestClass]
public class ScxmlTestRunner : ScxmlTestRunnerBase
{
	[TestMethod]
	[DynamicData(nameof(AllTestCases))]
	public Task Run(ScxmlTestCase testCase) => ExecuteTestCase(testCase);

	[TestMethod]
	public Task RunParallel()
	{
		var tasks = ScxmlTestRegistry.GetTestCases().Select(AsyncScxmlTestCase);

		return Task.WhenAll(tasks);
	}

	private async Task AsyncScxmlTestCase(ScxmlTestCase testCase)
	{
		await Task.Yield();

		await ExecuteTestCase(testCase);
	}

	public static IEnumerable<ScxmlTestCase> AllTestCases() => ScxmlTestRegistry.GetTestCases();
}