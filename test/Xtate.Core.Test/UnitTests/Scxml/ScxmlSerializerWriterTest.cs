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
		var xml = Serialize(
			"""
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

		StringAssert.Contains(xml, substring: "<scxml");
		StringAssert.Contains(xml, substring: "version=\"1.0\"");
		StringAssert.Contains(xml, substring: "datamodel=\"xpath\"");
		StringAssert.Contains(xml, substring: "initial=\"start\"");
		StringAssert.Contains(xml, substring: "<datamodel");
		StringAssert.Contains(xml, substring: "<data id=\"value\" expr=\"'initial'\"");
		StringAssert.Contains(xml, substring: "<script>1 + 1</script>");
		StringAssert.Contains(xml, substring: "<state id=\"start\"");
		StringAssert.Contains(xml, substring: "<onentry>");
		StringAssert.Contains(xml, substring: "<log label=\"enter\" expr=\"$value\"");
		StringAssert.Contains(xml, substring: "<raise event=\"go.next\"");
		StringAssert.Contains(xml, substring: "<assign location=\"value\" expr=\"'changed'\"");
		StringAssert.Contains(xml, substring: "<transition type=\"internal\" event=\"go.next other.event\" cond=\"true()\" target=\"done\"");
		StringAssert.Contains(xml, substring: "<final id=\"done\"");
		StringAssert.Contains(xml, substring: "<donedata>");
		StringAssert.Contains(xml, substring: "<content expr=\"$value\"");
		StringAssert.Contains(xml, substring: "<param name=\"p\" expr=\"$value\"");
	}

	[TestMethod]
	public void SerializeWritesParallelHistorySendCancelInvokeFinalizeAndInlineContent()
	{
		var xml = Serialize(
			"""
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

		StringAssert.Contains(xml, substring: "<history id=\"hist\" type=\"deep\"");
		StringAssert.Contains(xml, substring: "<send event=\"notify\" target=\"#_internal\" type=\"scxml\" id=\"send1\" delay=\"10ms\"");
		StringAssert.Contains(xml, substring: "<param name=\"payload\" expr=\"'value'\"");
		StringAssert.Contains(xml, substring: "<cancel sendid=\"send1\"");
		StringAssert.Contains(xml, substring: "<invoke type=\"scxml\" src=\"child.scxml\" id=\"child\" autoforward=\"true\"");
		StringAssert.Contains(xml, substring: "<finalize>");
		StringAssert.Contains(xml, substring: "<parallel id=\"parallel\"");
		StringAssert.Contains(xml, substring: "<initial>");
		StringAssert.Contains(xml, substring: "<content><payload xmlns=\"\">ok</payload></content>");
	}

	[TestMethod]
	public void SerializeWritesConditionalLoopsExpressionFormsOnExitAndInvokeParameters()
	{
		var xml = Serialize(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0" datamodel="xpath" initial="active" name="coverage" binding="late">
			  <datamodel><data id="external" src="data.xml"/></datamodel>
			  <script src="script.js"/>
			  <state id="active">
				<onentry>
				  <if cond="true()">
					<log label="if"/>
					<elseif cond="false()"/>
					<log label="elseif"/>
					<else/>
					<log label="else"/>
				  </if>
				  <foreach array="$items" item="item" index="index">
					<assign location="current" expr="$item" type="replaceattr" attr="value"/>
				  </foreach>
				  <send eventexpr="'dynamic.event'" targetexpr="'#_internal'" typeexpr="'scxml'" idlocation="sendId" delayexpr="100" namelist="item index">
				  </send>
				  <send target="#_internal"><content expr="$item"/></send>
				  <send event="delayed" delay="2s"/>
				  <cancel sendidexpr="$sendId"/>
				</onentry>
				<onexit>
				  <raise event="leaving"/>
				</onexit>
				<invoke typeexpr="'scxml'" srcexpr="'child.scxml'" idlocation="invokeId" namelist="item index" autoforward="false">
				  <finalize><assign location="done" expr="true()"/></finalize>
				</invoke>
				<invoke type="scxml" src="parameters.scxml" id="parameterInvoke">
				  <param name="literal" expr="$item"/>
				  <param name="located" location="current"/>
				</invoke>
				<invoke type="scxml"><content expr="$item"/></invoke>
				<transition target="done"/>
			  </state>
			  <final id="done"/>
			</scxml>
			""");

		StringAssert.Contains(xml, substring: "<if cond=\"true()\"");
		StringAssert.Contains(xml, substring: "<data id=\"external\" src=\"data.xml\"");
		StringAssert.Contains(xml, substring: "<script src=\"script.js\"");
		StringAssert.Contains(xml, substring: "<elseif cond=\"false()\"");
		StringAssert.Contains(xml, substring: "<else");
		StringAssert.Contains(xml, substring: "<foreach array=\"$items\" item=\"item\" index=\"index\"");
		StringAssert.Contains(xml, substring: "<send eventexpr=\"'dynamic.event'\" targetexpr=\"'#_internal'\" typeexpr=\"'scxml'\" idlocation=\"sendId\" delayexpr=\"100\" namelist=\"item index\"");
		StringAssert.Contains(xml, substring: "<cancel sendidexpr=\"$sendId\"");
		StringAssert.Contains(xml, substring: "<onexit>");
		StringAssert.Contains(xml, substring: "<invoke typeexpr=\"'scxml'\" srcexpr=\"'child.scxml'\" idlocation=\"invokeId\" namelist=\"item index\"");
		StringAssert.Contains(xml, substring: "<finalize>");
	}

	[TestMethod]
	public void SerializeShouldWriteInvokeSourceExpressionAsSrcExpr()
	{
		var xml = Serialize(
			"""
			<scxml xmlns="http://www.w3.org/2005/07/scxml" version="1.0">
			  <state id="state"><invoke typeexpr="'scxml'" srcexpr="'child.scxml'"/></state>
			</scxml>
			""");

		StringAssert.Contains(xml, substring: "srcexpr=\"'child.scxml'\"");
	}
}
