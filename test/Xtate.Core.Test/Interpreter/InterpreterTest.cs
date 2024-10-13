// Copyright © 2019-2024 Sergii Artemenko
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

using System.ComponentModel.Design;
using Xtate.DataModel;

namespace Xtate.Core.Interpreter;

[TestClass]
public class InterpreterTest
{
	[TestMethod]
	public async Task StateMachineInterpreterEmptyRun()
	{
		// arrange
		var linkedList = new LinkedList<int>();
		var finalNode = new FinalNode(new DocumentIdNode(linkedList), new FinalEntity());
		var target = ImmutableArray.Create<StateEntityNode>(finalNode);
		var transition = new EmptyTransitionNode(new DocumentIdNode(linkedList), target);
		var stateMachineEntity = new StateMachineEntity
								 {
									 Initial = new EmptyInitialNode(new DocumentIdNode(linkedList), transition),
									 States = [finalNode]
								 };
		var root = new StateMachineNode(new DocumentIdNode(linkedList), stateMachineEntity);

		var interpreterModelMock = new Mock<IInterpreterModel>();
		interpreterModelMock.Setup(m => m.Root).Returns(root);

		var eventQueueMock = new Mock<IEventQueueReader>();
		var caseSensitivityMock = new Mock<ICaseSensitivity>();

		var stateMachineContextMock = new Mock<IStateMachineContext>();
		stateMachineContextMock.Setup(ctx => ctx.Configuration).Returns([]);
		stateMachineContextMock.Setup(ctx => ctx.StatesToInvoke).Returns([]);
		stateMachineContextMock.Setup(ctx => ctx.InternalQueue).Returns(new EntityQueue<IEvent>());

		var loggerMock = new Mock<ILogger<IStateMachineInterpreter>>();

		var stateMachineInterpreter = new StateMachineInterpreter
									  {
										  StateMachineContext = stateMachineContextMock.Object,
										  DataConverter = new DataConverter(caseSensitivityMock.Object),
										  CaseSensitivity = caseSensitivityMock.Object,
										  EventQueueReader = eventQueueMock.Object,
										  Logger = loggerMock.Object,
										  Model = interpreterModelMock.Object,
										  NotifyStateChanged = null,
										  UnhandledErrorBehaviour = null,
										  StateMachineArguments = null,
										  StateMachineRuntimeError = new StateMachineRuntimeError()
									  };

		// act
		await stateMachineInterpreter.RunAsync();

		// assert
		Assert.IsFalse(false);
	}
}