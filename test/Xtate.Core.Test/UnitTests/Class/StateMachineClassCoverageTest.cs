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
using System.Text;
using Xtate.Class;
using Xtate.Interpreter;

namespace Xtate.Test.UnitTests.Class;

[TestClass]
public class StateMachineClassCoverageTest
{
	[TestMethod]
	public void ScxmlStreamStateMachineCreatesReaderOverProvidedStream()
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<scxml/>"));
		var stateMachine = new TestScxmlStreamStateMachine(stream);

		using var reader = stateMachine.GetTextReader();

		Assert.AreEqual(expected: "<scxml/>", reader.ReadToEnd());
	}

	[TestMethod]
	public void LocationStateMachineAcceptsAbsoluteStringAndUri()
	{
		var fromString = new LocationStateMachine("https://example.test/machine.scxml");
		var uri = new Uri("https://example.test/other.scxml");
		var fromUri = new LocationStateMachine(uri);

		Assert.AreEqual(new Uri("https://example.test/machine.scxml"), GetLocation(fromString));
		Assert.AreSame(uri, GetLocation(fromUri));
	}

	[TestMethod]
	public void LocationStateMachineCombinesBaseUriWithStringAndUri()
	{
		var baseUri = new Uri("https://example.test/folder/");

		var fromString = new LocationStateMachine(baseUri, relativeUri: "machine.scxml");
		var fromUri = new LocationStateMachine(baseUri, new Uri(uriString: "other.scxml", UriKind.Relative));

		Assert.AreEqual(new Uri("https://example.test/folder/machine.scxml"), GetLocation(fromString));
		Assert.AreEqual(new Uri("https://example.test/folder/other.scxml"), GetLocation(fromUri));
	}

	[TestMethod]
	public void LocationStateMachineWithoutBaseRequiresAbsoluteLocation()
	{
		var absolute = new LocationStateMachine(baseUri: null, relativeUri: "https://example.test/machine.scxml");

		Assert.AreEqual(new Uri("https://example.test/machine.scxml"), GetLocation(absolute));
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => _ = new LocationStateMachine(new Uri(uriString: "relative.scxml", UriKind.Relative)));
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => _ = new LocationStateMachine(baseUri: null, new Uri(uriString: "relative.scxml", UriKind.Relative)));
	}

	private static Uri? GetLocation(LocationStateMachine stateMachine) => ((IStateMachineLocation) stateMachine).Location;

	private sealed class TestScxmlStreamStateMachine(Stream stream) : ScxmlStreamStateMachine(stream)
	{
		public TextReader GetTextReader() => CreateTextReader();
	}
}