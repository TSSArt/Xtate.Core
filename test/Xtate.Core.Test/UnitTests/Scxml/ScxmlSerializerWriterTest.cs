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
using Xtate.IoC;
using Xtate.Scxml.Services;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class ScxmlSerializerWriterTest
{
	private static IStateMachine Parse(string scxml)
	{
		var services = new ServiceCollection();
		var smc = new ScxmlStringStateMachine(scxml);
		smc.AddServices(services);
		var serviceProvider = services.BuildProvider();

		return serviceProvider.GetRequiredService<IStateMachine>().Result;
	}

	private static string Serialize(string scxml)
	{
		var stateMachine = Parse(scxml);
		using var stringWriter = new StringWriter();
		using var xmlWriter = XmlWriter.Create(
			stringWriter,
			new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				ConformanceLevel = ConformanceLevel.Fragment
			});

		new ScxmlSerializerWriter(xmlWriter).Serialize(stateMachine);
		xmlWriter.Flush();

		return stringWriter.ToString();
	}

	[TestMethod]
	public void SerializeWritesRootAttributesStatesTransitionsExecutableContentAndDoneData()
	{
		var xml = Serialize("""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="start">
			  <datamodel>
				<data id="value" expr="'initial'"/>
				<data id="inline"><root xmlns="">text</root></data>
			  </datamodel>
			  <script>1 + 1</script>
			  <state id="start">
				<onentry>
				  <log label="enter" expr="$value"/>
				  <raise event="go.next"/>
				  <assign location="value" expr="'changed'"/>
				</onentry>
				<transition event="go.next other.event" cond="true()" target="done" type="internal">
				  <log label="transition" />
				</transition>
			  </state>
			  <final id="done">
				<donedata>
				  <content expr="$value"/>
				  <param name="p" expr="$value"/>
				</donedata>
			  </final>
			</scxml>
			""");

		StringAssert.Contains(xml, "<scxml");
		StringAssert.Contains(xml, "version=\"1.0\"");
		StringAssert.Contains(xml, "datamodel=\"xpath\"");
		StringAssert.Contains(xml, "initial=\"start\"");
		StringAssert.Contains(xml, "<datamodel");
		StringAssert.Contains(xml, "<data id=\"value\" expr=\"'initial'\"");
		StringAssert.Contains(xml, "<script>1 + 1</script>");
		StringAssert.Contains(xml, "<state id=\"start\"");
		StringAssert.Contains(xml, "<onentry>");
		StringAssert.Contains(xml, "<log label=\"enter\" expr=\"$value\"");
		StringAssert.Contains(xml, "<raise event=\"go.next\"");
		StringAssert.Contains(xml, "<assign location=\"value\" expr=\"'changed'\"");
		StringAssert.Contains(xml, "<transition type=\"internal\" event=\"go.next other.event\" cond=\"true()\" target=\"done\"");
		StringAssert.Contains(xml, "<final id=\"done\"");
		StringAssert.Contains(xml, "<donedata>");
		StringAssert.Contains(xml, "<content expr=\"$value\"");
		StringAssert.Contains(xml, "<param name=\"p\" expr=\"$value\"");
	}

	[TestMethod]
	public void SerializeWritesParallelHistorySendCancelInvokeFinalizeAndInlineContent()
	{
		var xml = Serialize("""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="outer">
			  <state id="outer" initial="work">
				<history id="hist" type="deep">
				  <transition target="work"/>
				</history>
				<state id="work">
				  <onentry>
					<send event="notify" target="#_internal" type="scxml" id="send1" delay="10ms">
					  <param name="payload" expr="'value'"/>
					</send>
					<cancel sendid="send1"/>
					<invoke type="scxml" src="child.scxml" id="child" autoforward="true">
					  <finalize>
						<log label="finalize" expr="'done'"/>
					  </finalize>
					</invoke>
				  </onentry>
				  <transition target="parallel"/>
				</state>
			  </state>
			  <parallel id="parallel">
				<state id="regionA">
				  <initial><transition target="aDone"/></initial>
				  <final id="aDone"/>
				</state>
				<state id="regionB">
				  <initial><transition target="bDone"/></initial>
				  <final id="bDone"/>
				</state>
				<transition event="done.state.parallel" target="done"/>
			  </parallel>
			  <final id="done">
				<donedata>
				  <content><payload xmlns="">ok</payload></content>
				</donedata>
			  </final>
			</scxml>
			""");

		StringAssert.Contains(xml, "<history id=\"hist\" type=\"deep\"");
		StringAssert.Contains(xml, "<send event=\"notify\" target=\"#_internal\" type=\"scxml\" id=\"send1\" delay=\"10ms\"");
		StringAssert.Contains(xml, "<param name=\"payload\" expr=\"'value'\"");
		StringAssert.Contains(xml, "<cancel sendid=\"send1\"");
		StringAssert.Contains(xml, "<invoke type=\"scxml\" src=\"child.scxml\" id=\"child\" autoforward=\"true\"");
		StringAssert.Contains(xml, "<finalize>");
		StringAssert.Contains(xml, "<parallel id=\"parallel\"");
		StringAssert.Contains(xml, "<initial>");
		StringAssert.Contains(xml, "<content><payload xmlns=\"\">ok</payload></content>");
	}
}
