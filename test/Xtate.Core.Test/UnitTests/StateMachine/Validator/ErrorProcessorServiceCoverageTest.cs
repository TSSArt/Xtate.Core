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

using System.Xml;
using Xtate.StateMachine.Validator;
using Xtate.StateMachine.Validator.Services;

namespace Xtate.Test.UnitTests.StateMachine.Validator;

[TestClass]
public class ErrorProcessorServiceCoverageTest
{
	[TestMethod]
	public void AddErrorUsesEntityLineInfoThenXmlExceptionThenLocationlessFallback()
	{
		var processor = new CollectingErrorProcessor();
		var service = new ErrorProcessorService<ErrorProcessorServiceCoverageTest> { ErrorProcessor = processor };
		var entityException = new InvalidOperationException("entity");

		service.AddError(new LineInfo(hasLineInfo: true, lineNumber: 11, linePosition: 12), "entity line", entityException);
		service.AddError(new LineInfo(hasLineInfo: false, lineNumber: 21, linePosition: 22), "xml line", new XmlException("xml", innerException: null, lineNumber: 31, linePosition: 32));
		service.AddError(entity: null, "plain", new XmlException("no line"));

		Assert.HasCount(expected: 3, processor.Items);
		Assert.AreEqual(typeof(ErrorProcessorServiceCoverageTest), processor.Items[0].Source);
		Assert.AreEqual("entity line", processor.Items[0].Message);
		Assert.AreSame(entityException, processor.Items[0].Exception);
		Assert.AreEqual(expected: 11, processor.Items[0].LineNumber);
		Assert.AreEqual(expected: 12, processor.Items[0].LinePosition);
		Assert.AreEqual(expected: 31, processor.Items[1].LineNumber);
		Assert.AreEqual(expected: 32, processor.Items[1].LinePosition);
		Assert.AreEqual(expected: 0, processor.Items[2].LineNumber);
		Assert.AreEqual(expected: 0, processor.Items[2].LinePosition);
	}

	private sealed class CollectingErrorProcessor : IErrorProcessor
	{
		public List<ErrorItem> Items { get; } = [];

		public void AddError(ErrorItem errorItem) => Items.Add(errorItem);

		public void ThrowIfErrors() { }
	}

	private sealed class LineInfo(bool hasLineInfo, int lineNumber, int linePosition) : IXmlLineInfo
	{
		public bool HasLineInfo() => hasLineInfo;

		public int LineNumber => lineNumber;

		public int LinePosition => linePosition;
	}
}
