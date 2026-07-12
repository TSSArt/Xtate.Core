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

using Xtate.Test.StateMachines.Basic;
using Xtate.Test.StateMachines.DataModel;
using Xtate.Test.StateMachines.History;
using Xtate.Test.StateMachines.Metadata;
using Xtate.Test.StateMachines.Parallel;
using Xtate.Test.StateMachines.Transitions;
using Xtate.Test.StateMachines.XPath;

namespace Xtate.Test.StateMachines;

/// <summary>
///     Central registry that aggregates every <see cref="IScxmlTestSource" /> and provides
///     lookup by stable test-case name for <see cref="ScxmlTestRunner" />.
/// </summary>
/// <remarks>
///     To add a new collection of machines:
///     <list type="number">
///         <item>Create a new <c>*Machines.cs</c> file anywhere under <c>StateMachines/</c>.</item>
///         <item>Make its class implement <see cref="IScxmlTestSource" />.</item>
///         <item>Add a <c>new YourClass()</c> entry to <see cref="Sources" /> below.</item>
///     </list>
/// </remarks>
public static class ScxmlTestRegistry
{
	private static readonly Lazy<IReadOnlyDictionary<string, ScxmlTestCase>> TestCasesByName = new(() => Sources!
																										 .SelectMany(src => src.GetTestCases())
																										 .ToDictionary(testCase => testCase.Name, StringComparer.Ordinal));

	/// <summary>
	///     Explicit list of all registered test sources.
	///     Add one entry per <c>*Machines</c> class when you create new machine files.
	/// </summary>
	private static readonly IScxmlTestSource[] Sources =
	[
		new BasicMachines(),
		new InitialMachines(),
		new DocumentConfigurationMachines(),
		new CompoundMachines(),
		new StateElementMachines(),
		new OnEntryOnExitMachines(),
		new EventlessTransitionMachines(),
		new ParallelMachines(),
		new ShallowHistoryMachines(),
		new DeepHistoryMachines(),
		new NullAndXPathMachines(),
		new MetadataMachines(),
		new XPathMachines(),
		new ExecutableContentMachines(),
		new EventDescriptorMachines(),
		new TransitionSelectionMachines(),
		new TransitionMachines(),
		new DataModelMachines()
	];

	public static IEnumerable<ScxmlTestCase> GetTestCases() => TestCasesByName.Value.Values;
}