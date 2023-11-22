#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
									 States = ImmutableArray.Create<IStateEntity>(finalNode)
								 };
		var root = new StateMachineNode(new DocumentIdNode(linkedList), stateMachineEntity);

		var interpreterModelMock = new Mock<IInterpreterModel>();
		interpreterModelMock.Setup(m => m.Root).Returns(root);

		var eventQueueMock = new Mock<IEventQueueReader>();
		var dataModelHandlerMock = new Mock<IDataModelHandler>();

		var stateMachineContextMock = new Mock<IStateMachineContext>();
		stateMachineContextMock.Setup(ctx => ctx.Configuration).Returns(new OrderedSet<StateEntityNode>());
		stateMachineContextMock.Setup(ctx => ctx.StatesToInvoke).Returns(new OrderedSet<StateEntityNode>());
		stateMachineContextMock.Setup(ctx => ctx.InternalQueue).Returns(new EntityQueue<IEvent>());

		var entityParserHandlerMock = new Mock<IEntityParserHandler>();
		var loggerMock = new Mock<ILogger<IStateMachineInterpreter>>();

		var stateMachineInterpreter = new StateMachineInterpreter()
									  {
										  ContextFactory = () => new ValueTask<IStateMachineContext>(stateMachineContextMock.Object),
										  _dataConverter = new DataConverter(dataModelHandlerMock.Object),
										  _dataModelHandler = dataModelHandlerMock.Object,
										  _eventQueueReader = eventQueueMock.Object,
										  _externalCommunication = null,
										  _logger = loggerMock.Object,
										  _model = interpreterModelMock.Object,
										  _notifyStateChanged = null,
										  _resourceLoader = null,
										  _stateMachineLocation = null,
										  _unhandledErrorBehaviour = null

									  };

		// act
		await stateMachineInterpreter.RunAsync();

		// assert
		Assert.IsFalse(false);
	}
}