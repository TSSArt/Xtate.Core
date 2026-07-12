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

using Xtate.StateMachine;
using Xtate.StateMachine.Validator;

namespace Xtate.Test.UnitTests.StateMachine.Validator;

[TestClass]
public class StateMachineValidationExceptionCoverageTest
{
	[TestMethod]
	public void EmptyValidationMessagesProduceNullMessageAndPreserveSessionId()
	{
		var sessionId = SessionId.FromString("session-1");
		var exception = new StateMachineValidationException(ImmutableArray<ErrorItem>.Empty, sessionId);

		Assert.AreSame(sessionId, exception.SessionId);
		Assert.AreEqual(expected: 0, exception.ValidationMessages.Length);
		StringAssert.Contains(exception.Message, nameof(StateMachineValidationException));
	}

	[TestMethod]
	public void SingleValidationMessageUsesErrorItemText()
	{
		var error = new ErrorItem(typeof(StateMachineValidationExceptionCoverageTest), message: "single error", new InvalidOperationException("boom"), lineNumber: 3, linePosition: 7);
		var exception = new StateMachineValidationException([error]);

		Assert.IsNull(exception.SessionId);
		Assert.AreSame(error, exception.ValidationMessages[0]);
		StringAssert.Contains(exception.Message, substring: "Error");
		StringAssert.Contains(exception.Message, nameof(StateMachineValidationExceptionCoverageTest));
		StringAssert.Contains(exception.Message, substring: "Ln: 3");
		StringAssert.Contains(exception.Message, substring: "Col: 7");
		StringAssert.Contains(exception.Message, substring: "single error");
		StringAssert.Contains(exception.Message, substring: "Exception ==>");
	}

	[TestMethod]
	public void MultipleValidationMessagesAreNumberedAndSeparatedByLines()
	{
		var first = new ErrorItem(typeof(string), message: "first error", exception: null);
		var second = new ErrorItem(typeof(int), message: "second error", exception: null, lineNumber: 5, linePosition: 9);

		var exception = new StateMachineValidationException([first, second]);

		Assert.AreEqual(expected: 2, exception.ValidationMessages.Length);
		StringAssert.Contains(exception.Message, substring: "1");
		StringAssert.Contains(exception.Message, substring: "2");
		StringAssert.Contains(exception.Message, substring: "first error");
		StringAssert.Contains(exception.Message, substring: "second error");
		StringAssert.Contains(exception.Message, Environment.NewLine);
	}
}