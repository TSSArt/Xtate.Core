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

using Xtate.Ancestor;
using Xtate.DataModel;
using Xtate.DataModel.Null.Services;
using Xtate.StateMachine;

namespace Xtate.Test.UnitTests.DataModel;

[TestClass]
public class NullConditionExpressionEvaluatorCoverageTest
{
	[TestMethod]
	public async Task EvaluateBooleanReturnsFalseWhenInStateControllerIsUnavailable()
	{
		var conditionExpression = new ConditionExpressionSource { Expression = "In(target)" };
		var evaluator = new NullConditionExpressionEvaluator(conditionExpression, Identifier.FromString("target"))
						{
							InStateControllerFactory = () => new ValueTask<IInStateController?>((IInStateController?) null)
						};

		Assert.AreSame(conditionExpression, ((IAncestorProvider) evaluator).Ancestor);
		Assert.AreEqual("In(target)", evaluator.Expression);
		Assert.IsFalse(await ((IBooleanEvaluator) evaluator).EvaluateBoolean());
	}

	[TestMethod]
	public async Task EvaluateBooleanDelegatesToInStateControllerWhenAvailable()
	{
		var target = Identifier.FromString("target");
		var controller = new InStateControllerSource(target);
		var evaluator = new NullConditionExpressionEvaluator(new ConditionExpressionSource(), target)
						{
							InStateControllerFactory = () => new ValueTask<IInStateController?>((IInStateController?) controller)
						};

		Assert.IsTrue(await ((IBooleanEvaluator) evaluator).EvaluateBoolean());
		Assert.AreSame(target, controller.LastId);
	}

	private sealed class ConditionExpressionSource : IConditionExpression
	{
		public string? Expression { get; init; }
	}

	private sealed class InStateControllerSource(IIdentifier expectedId) : IInStateController
	{
		public IIdentifier? LastId { get; private set; }

		public bool InState(IIdentifier id)
		{
			LastId = id;

			return ReferenceEquals(expectedId, id);
		}
	}
}
