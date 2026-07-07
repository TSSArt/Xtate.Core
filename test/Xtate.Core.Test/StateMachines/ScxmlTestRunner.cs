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

using System.IO;
using System.Xml;
using Xtate.Class;
using Xtate.DataTypes;
using Xtate.Interpreter;
using Xtate.Interpreter.DependencyInjection;
using Xtate.IoC;
using Xtate.Scxml;
using Xtate.Scxml.DependencyInjection;
using Xtate.StateMachine;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.DependencyInjection;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.StateMachines;

/// <summary>
///     MSTest runner for the SCXML state machine collection.
///
///     How it works
///     ────────────
///     • <see cref="ScxmlTestRegistry.GetAllTestRows" /> supplies every registered
///       <see cref="ScxmlTestCase" /> to the two <c>[DynamicData]</c>-driven test methods.
///     • <see cref="RunSequential" /> executes the machines one at a time (still
///       honours the class-level sequential ordering in Test Explorer).
///     • <see cref="RunParallel" /> is identical in logic but declared separately so
///       it appears as a distinct test group; the assembly-level
///       <c>[Parallelize(MethodLevel)]</c> already in <c>ParallelizeAttribute.cs</c>
///       causes MSTest to run all methods concurrently.
///     • Both methods delegate to <see cref="ExecuteTestCase" /> which builds a
///       lightweight DI scope, parses the SCXML, runs the interpreter, and optionally
///       asserts the returned done-data value.
///
///     Adding more machines
///     ────────────────────
///     1. Create a new <c>*Machines.cs</c> file anywhere under <c>StateMachines/</c>.
///     2. Implement <see cref="IScxmlTestSource" /> on its class.
///     3. Add an instance to <see cref="ScxmlTestRegistry" />.<c>Sources</c>.
///     All new cases are automatically picked up by both test methods.
/// </summary>
[TestClass]
public class ScxmlTestRunner
{
	// ─────────────────────────────────────────────
	// DynamicData source
	// ─────────────────────────────────────────────

	/// <summary>
	///     MSTest data source — returns one <c>object[]</c> per registered test case.
	///     Must be <c>public static</c> and return <c>IEnumerable&lt;object[]&gt;</c>.
	/// </summary>
	public static IEnumerable<object[]> AllTestCases => ScxmlTestRegistry.GetAllTestRows();

	// ─────────────────────────────────────────────
	// Test methods
	// ─────────────────────────────────────────────

	/// <summary>
	///     Executes every registered SCXML machine sequentially (in the order they
	///     are yielded by their <see cref="IScxmlTestSource" /> implementations).
	///     Parallel execution across methods is still possible because of the
	///     assembly-level <c>[Parallelize(MethodLevel)]</c>.
	/// </summary>
	[TestMethod]
	[DynamicData(nameof(AllTestCases))]
	public Task RunSequential(ScxmlTestCase testCase) => ExecuteTestCase(testCase);

	/// <summary>
	///     Executes every registered SCXML machine.  This separate method gives a
	///     distinct group in Test Explorer labelled "Parallel", making it easy to
	///     trigger only the parallel run from the UI.
	///     Because the assembly attribute sets <c>MethodLevel</c> parallelism, all
	///     rows of this method can run concurrently with each other and with
	///     <see cref="RunSequential" />.
	/// </summary>
	[TestMethod]
	[DynamicData(nameof(AllTestCases))]
	[Ignore]
	public Task RunParallel(ScxmlTestCase testCase) => ExecuteTestCase(testCase);

	// ─────────────────────────────────────────────
	// Core execution
	// ─────────────────────────────────────────────

	/// <summary>
	///     Parses <paramref name="testCase" />.<see cref="ScxmlTestCase.Scxml" /> via
	///     <c>ScxmlModule</c>, runs the resulting <see cref="IStateMachine" /> through
	///     <c>StateMachineInterpreterModule</c>, and — when
	///     <see cref="ScxmlTestCase.ExpectedFinalData" /> is non-null — asserts that
	///     the returned <see cref="DataModelValue" /> matches.
	/// </summary>
	private static async Task ExecuteTestCase(ScxmlTestCase testCase)
	{
		var smc = new ScxmlStringStateMachine(testCase.Scxml);

		var idle = new Mock<IDestroyOnIdleTimeout>();
		idle.Setup(x => x.IdleTimeout).Returns(TimeSpan.FromMilliseconds(5000));

		var container = Container.Create<StateMachineProcessorModule>(
			services =>
				services.AddConstant(idle.Object));

		var stateMachineScopeManager = await container.GetRequiredService<IStateMachineScopeManager>();

		var result = await stateMachineScopeManager.Execute(smc, SecurityContextType.NewStateMachine);

		if (testCase.ExpectedFinalData is { } expected)
		{
			Assert.AreEqual(expected, result.AsStringOrDefault(), $"State machine '{testCase.Name}' returned unexpected done-data.");
		}
	}
}
