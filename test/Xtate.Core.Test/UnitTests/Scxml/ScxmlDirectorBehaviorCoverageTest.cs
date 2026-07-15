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
using System.Reflection;
using System.Xml;
using Xtate.Ancestor;
using Xtate.Scxml;
using Xtate.Scxml.Services;
using Xtate.StateMachine.Validator;

namespace Xtate.Test.UnitTests.Scxml;

[TestClass]
public class ScxmlDirectorBehaviorCoverageTest
{
	[TestMethod]
	public void LineInfoAndErrorForwardingPreserveReaderPositionAndAncestor()
	{
		using var reader = XmlReader.Create(new StringReader("<scxml/>"));
		Assert.IsTrue(reader.Read());
		var director = (ScxmlDirector) Activator.CreateInstance(typeof(ScxmlDirector), reader)!;
		var errorProcessor = new Mock<IErrorProcessorService<ScxmlDirector>>();
		var ancestor = new object();
		var lineInfoRequired = new Mock<ILineInfoRequired>();
		lineInfoRequired.SetupGet(static value => value.LineInfoRequired).Returns(true);
		SetProperty(director, nameof(ScxmlDirector.ErrorProcessorService), errorProcessor.Object);
		SetProperty(director, nameof(ScxmlDirector.LineInfoRequired), lineInfoRequired.Object);

		var createLineInfo = typeof(ScxmlDirector).GetMethod("CreateXmlLineInfo", BindingFlags.Instance | BindingFlags.NonPublic)!;
		var lineInfo = createLineInfo.Invoke(director, [ancestor]);

		Assert.IsInstanceOfType<IXmlLineInfo>(lineInfo);
		Assert.AreSame(ancestor, ((IAncestorProvider) lineInfo).Ancestor);
		Assert.IsGreaterThan(0, ((IXmlLineInfo) lineInfo).LineNumber);

		var exception = new InvalidOperationException("invalid SCXML");
		var onError = typeof(ScxmlDirector).GetMethod("OnError", BindingFlags.Instance | BindingFlags.NonPublic)!;
		onError.Invoke(director, ["message", exception]);
		errorProcessor.Verify(
			processor => processor.AddError(It.Is<IXmlLineInfo>(value => value.LineNumber > 0), "message", exception),
			Times.Once);

		lineInfoRequired.SetupGet(static value => value.LineInfoRequired).Returns(false);
		Assert.AreSame(ancestor, createLineInfo.Invoke(director, [ancestor]));
		SetProperty(director, nameof(ScxmlDirector.LineInfoRequired), value: null);
		Assert.AreSame(ancestor, createLineInfo.Invoke(director, [ancestor]));
	}

	private static void SetProperty(ScxmlDirector director, string name, object? value) =>
		typeof(ScxmlDirector).GetProperty(name)!.SetValue(director, value);
}
