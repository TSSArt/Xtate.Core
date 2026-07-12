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

namespace Xtate.Test.StateMachines.Transitions;

public class EventDescriptorMachines : IScxmlTestSource
{
	public static readonly string ExactRaisedEventMatchesTransition = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
		  <state id="start">
			<onentry>
			  <raise event="go"/>
			</onentry>
			<transition event="go" target="done"/>
			<transition event="other" target="failed"/>
		  </state>
		  <final id="failed">
			<donedata><content>failed</content></donedata>
		  </final>
		  <final id="done">
			<donedata><content>exact</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string PrefixEventDescriptorMatchesChildEventName = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
		  <state id="start">
			<onentry>
			  <raise event="order.created"/>
			</onentry>
			<transition event="order" target="done"/>
			<transition event="fallback" target="failed"/>
		  </state>
		  <final id="failed">
			<donedata><content>failed</content></donedata>
		  </final>
		  <final id="done">
			<donedata><content>prefix</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string PrefixEventDescriptorDoesNotMatchLexicalPrefix = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
		  <state id="start">
			<onentry>
			  <raise event="foobar"/>
			</onentry>
			<transition event="foo" target="failed"/>
			<transition event="foobar" target="done"/>
		  </state>
		  <final id="failed">
			<donedata><content>failed</content></donedata>
		  </final>
		  <final id="done">
			<donedata><content>not-lexical-prefix</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string MultipleEventDescriptorsMatchAnyToken = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
		  <state id="start">
			<onentry>
			  <raise event="beta"/>
			</onentry>
			<transition event="alpha beta gamma" target="done"/>
			<transition event="fallback" target="failed"/>
		  </state>
		  <final id="failed">
			<donedata><content>failed</content></donedata>
		  </final>
		  <final id="done">
			<donedata><content>multi</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string WildcardEventDescriptorMatchesRaisedEvent = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
		  <state id="start">
			<onentry>
			  <raise event="anything.here"/>
			</onentry>
			<transition event="*" target="done"/>
		  </state>
		  <final id="done">
			<donedata><content>wildcard</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string EventMatchingIsCaseSensitive = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
		  <state id="start">
			<onentry>
			  <raise event="Go"/>
			</onentry>
			<transition event="go" target="failed"/>
			<transition event="Go" target="done"/>
		  </state>
		  <final id="failed">
			<donedata><content>failed</content></donedata>
		  </final>
		  <final id="done">
			<donedata><content>case-sensitive</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string EventlessTransitionRunsBeforeRaisedInternalEvent = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
		  <state id="start">
			<onentry>
			  <raise event="go"/>
			</onentry>
			<transition target="done"/>
			<transition event="go" target="failed"/>
		  </state>
		  <final id="failed">
			<donedata><content>failed</content></donedata>
		  </final>
		  <final id="done">
			<donedata><content>eventless-first</content></donedata>
		  </final>
		</scxml>
		""";

	public static readonly string RaisedInternalEventsAreProcessedFifo = """
		<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" initial="start">
		  <state id="start">
			<onentry>
			  <raise event="first"/>
			  <raise event="second"/>
			</onentry>
			<transition event="second" target="failed"/>
			<transition event="first" target="sawFirst"/>
		  </state>
		  <state id="sawFirst">
			<transition event="second" target="done"/>
		  </state>
		  <final id="failed">
			<donedata><content>failed</content></donedata>
		  </final>
		  <final id="done">
			<donedata><content>fifo</content></donedata>
		  </final>
		</scxml>
		""";

	public IEnumerable<ScxmlTestCase> GetTestCases()
	{
		yield return new ScxmlTestCase("Transitions/EventDescriptors/ExactRaisedEventMatchesTransition", ExactRaisedEventMatchesTransition, ExpectedFinalData: "exact");
		yield return new ScxmlTestCase("Transitions/EventDescriptors/PrefixEventDescriptorMatchesChildEventName", PrefixEventDescriptorMatchesChildEventName, ExpectedFinalData: "prefix");
		yield return new ScxmlTestCase("Transitions/EventDescriptors/PrefixEventDescriptorDoesNotMatchLexicalPrefix", PrefixEventDescriptorDoesNotMatchLexicalPrefix, ExpectedFinalData: "not-lexical-prefix");
		yield return new ScxmlTestCase("Transitions/EventDescriptors/MultipleEventDescriptorsMatchAnyToken", MultipleEventDescriptorsMatchAnyToken, ExpectedFinalData: "multi");
		yield return new ScxmlTestCase("Transitions/EventDescriptors/WildcardEventDescriptorMatchesRaisedEvent", WildcardEventDescriptorMatchesRaisedEvent, ExpectedFinalData: "wildcard");
		yield return new ScxmlTestCase("Transitions/EventDescriptors/EventMatchingIsCaseSensitive", EventMatchingIsCaseSensitive, ExpectedFinalData: "case-sensitive");
		yield return new ScxmlTestCase("Transitions/EventDescriptors/EventlessTransitionRunsBeforeRaisedInternalEvent", EventlessTransitionRunsBeforeRaisedInternalEvent, ExpectedFinalData: "eventless-first");
		yield return new ScxmlTestCase("Transitions/EventDescriptors/RaisedInternalEventsAreProcessedFifo", RaisedInternalEventsAreProcessedFifo, ExpectedFinalData: "fifo");
	}
}
