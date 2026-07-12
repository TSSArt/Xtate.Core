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
using Xtate.StateMachine.Validator.Services;

namespace Xtate.Test.UnitTests.StateMachine.Validator;

[TestClass]
public class DetailedErrorProcessorCoverageTest
{
	[TestMethod]
	public void ThrowIfErrorsDoesNothingWhenNoErrorsWereAdded()
	{
		var processor = new DetailedErrorProcessor(SessionId.FromString("session"));

		processor.ThrowIfErrors();
	}

	[TestMethod]
	public void ThrowIfErrorsAggregatesAddedErrorsAndClearsInternalBuilder()
	{
		var sessionId = SessionId.FromString("session");
		var first = new ErrorItem(typeof(DetailedErrorProcessorCoverageTest), "first", exception: null);
		var second = new ErrorItem(typeof(string), "second", exception: null);
		var processor = new DetailedErrorProcessor(sessionId);
		var errorProcessor = (IErrorProcessor) processor;

		errorProcessor.AddError(first);
		errorProcessor.AddError(second);

		var exception = Assert.ThrowsExactly<StateMachineValidationException>(processor.ThrowIfErrors);

		Assert.AreSame(sessionId, exception.SessionId);
		Assert.AreEqual(2, exception.ValidationMessages.Length);
		Assert.AreSame(first, exception.ValidationMessages[0]);
		Assert.AreSame(second, exception.ValidationMessages[1]);

		processor.ThrowIfErrors();
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage] () => errorProcessor.AddError(null!));
	}
}
