// Copyright © 2019-2025 Sergii Artemenko
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

using Xtate.DataModel;
using Xtate.DataModel.Services;
using Xtate.Interpreter;
using Xtate.Interpreter.Internal;
using Xtate.Interpreter.Model;
using Xtate.Interpreter.Services;
using Xtate.IoC.Tools;
using Xtate.Logging;
using Xtate.StateMachine;

namespace Xtate.Core.Interpreter;

[TestClass]
public class InterpreterTest
{
	[TestMethod]
    public async Task StateMachineInterpreterEmptyRun()
    {
        // arrange
        var linkedList = new LinkedList<int>();
		var finalNode = new FinalNode(new DocumentIdNode(linkedList), new FinalEntity { Id = Identifier.New() });
        var target = ImmutableArray.Create<StateEntityNode>(finalNode);
		var transition = new TransitionNode(new DocumentIdNode(linkedList), new TransitionEntity { Target = [finalNode.Id] });
		var initial = new InitialNode(new DocumentIdNode(linkedList), new InitialEntity { Transition = transition });

		transition.TryMapTarget(new Dictionary<IIdentifier, StateEntityNode> { { finalNode.Id, finalNode } });

		var stateMachineEntity = new StateMachineEntity
                                 {
                                     Initial = initial,
                                     States = [finalNode]
                                 };
        var root = new StateMachineNode(new DocumentIdNode(linkedList), stateMachineEntity);

        var interpreterModelMock = new Mock<IInterpreterModel>();
        interpreterModelMock.Setup(m => m.Root).Returns(root);

        var eventQueueMock = new Mock<IEventReader>();
        var caseSensitivityMock = new Mock<ICaseSensitivity>();
        var invokeControllerMock = new Mock<IInvokeController>();

        var stateMachineContextMock = new Mock<IStateMachineContext>();
        stateMachineContextMock.Setup(ctx => ctx.Configuration).Returns([]);
        stateMachineContextMock.Setup(ctx => ctx.StatesToInvoke).Returns([]);
        stateMachineContextMock.Setup(ctx => ctx.InternalQueue).Returns(new EntityQueue<IIncomingEvent>());

        var loggerMock = new Mock<ILogger<StateMachineInterpreter>>();

        var stateMachineInterpreter = new StateMachineInterpreter
                                      {
                                          StateMachineContext = stateMachineContextMock.Object,
                                          DataConverter = new DataConverter(caseSensitivityMock.Object),
                                          CaseSensitivity = caseSensitivityMock.Object,
                                          EventReader = eventQueueMock.Object,
                                          Logger = loggerMock.Object,
                                          Model = interpreterModelMock.Object,
                                          NotifyStateChanged = [],
                                          UnhandledErrorBehaviour = null!,
                                          StateMachineArguments = null!,
                                          StateMachineRuntimeError = new StateMachineRuntimeError(new ScopeObject()),
                                          InvokeController = invokeControllerMock.Object,
										  DisposeToken = new DisposeToken()
                                      };

        // act
        await stateMachineInterpreter.Run();

        // assert
        Assert.IsFalse(false);
    }
}