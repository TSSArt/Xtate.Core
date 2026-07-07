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
///     Represents a single SCXML state machine test case.
///     The SCXML content is stored as a raw string literal in the source file.
/// </summary>
/// <param name="Name">Unique display name shown in Test Explorer.</param>
/// <param name="Scxml">Full SCXML document as a string.</param>
/// <param name="ExpectedFinalData">
///     Optional value expected to be returned by the final state's done-data.
///     When <c>null</c> the test only verifies the machine runs to completion without throwing.
/// </param>
public record ScxmlTestCase(string Name, string Scxml, string? ExpectedFinalData = null)
{
	/// <summary>Produces a one-element <c>object[]</c> suitable for MSTest <c>[DynamicData]</c>.</summary>
	public object[] ToTestRow() => [this];

	/// <inheritdoc />
	public override string ToString() => Name;
}

/// <summary>
///     Implemented by every static machine-collection class to expose its test cases
///     to <see cref="ScxmlTestRegistry" />.
/// </summary>
public interface IScxmlTestSource
{
	IEnumerable<ScxmlTestCase> GetTestCases();
}
