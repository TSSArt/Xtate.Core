// Copyright © 2019-2026 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>.
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

namespace Xtate.Test.StateMachines.Metadata;

/// <summary>
///     SCXML machines focused on root-level metadata and attributes that should
///     not affect execution semantics but are important for parser coverage.
/// </summary>
public class MetadataMachines : IScxmlTestSource
{
	public static readonly string NamedStateMachine = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" name="NamedStateMachine" initial="done">
		  <final id="done"/>
		</scxml>
		""";

	public static readonly string BindingEarlyStateMachine = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" binding="early" initial="done">
		  <final id="done"/>
		</scxml>
		""";

	public static readonly string NullDataModelNamedStateMachine = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" name="NullDataModel" datamodel="null" initial="done">
		  <final id="done"/>
		</scxml>
		""";

	public static readonly string OnEntryOnExitMetadataStateMachine = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" name="OnEntryOnExitMetadata" initial="start">
		  <state id="start">
			<onentry>
			  <log label="enter"/>
			</onentry>
			<onexit>
			  <log label="exit"/>
			</onexit>
			<transition target="done"/>
		  </state>
		  <final id="done"/>
		</scxml>
		""";

	public static readonly string CompoundWithExplicitInitial = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="outer">
		  <state id="outer" initial="inner">
			<state id="inner">
			  <transition target="done"/>
			</state>
		  </state>
		  <final id="done"/>
		</scxml>
		""";

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase("Metadata/NamedStateMachine",             NamedStateMachine);
		yield return new ScxmlTestCase("Metadata/BindingEarlyStateMachine",      BindingEarlyStateMachine);
		yield return new ScxmlTestCase("Metadata/NullDataModelNamedStateMachine", NullDataModelNamedStateMachine);
		yield return new ScxmlTestCase("Metadata/OnEntryOnExitMetadataStateMachine", OnEntryOnExitMetadataStateMachine);
		yield return new ScxmlTestCase("Metadata/CompoundWithExplicitInitial",    CompoundWithExplicitInitial);
	}
}
