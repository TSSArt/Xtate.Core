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

using System.Threading.Channels;
using Xtate.Builder;
using Xtate.Core;
using Xtate.IoC;

// ReSharper disable AccessToModifiedClosure

namespace Xtate.Test;

[TestClass]
public class StateMachineFluentBuilderTest
{
    public static ValueTask<StateMachineFluentBuilder> GetStateMachineFluentBuilder()
    {
        var services = new ServiceCollection();
        services.AddModule<StateMachineFluentBuilderModule>();
        var sp = services.BuildProvider();

        return sp.GetRequiredService<StateMachineFluentBuilder>();
    }

    [TestMethod]
    public async Task EmptyStateMachineTest()
    {
        var builder = await GetStateMachineFluentBuilder();

        var stateMachine = builder.Build();

        Assert.IsNotNull(stateMachine);
        Assert.IsTrue(stateMachine.States.IsDefault);
    }

    [TestMethod]
    public async Task InitialAttributeStateMachineTest()
    {
        var builder = await GetStateMachineFluentBuilder();

        builder
            .SetInitial((Identifier)"S1")
            .BeginState((Identifier)"S1")
            .EndState();

        var stateMachine = builder.Build();

        Assert.IsNotNull(stateMachine);
        Assert.AreEqual(expected: 1, stateMachine.States.Length);
        Assert.IsInstanceOfType(stateMachine.Initial, typeof(IInitial));
        Assert.IsInstanceOfType(stateMachine.States[0], typeof(IState));
        Assert.AreEqual((Identifier)"S1", ((IState)stateMachine.States[0]).Id);
    }

    [TestMethod]
    public async Task RootStatesStateMachineTest()
    {
        var builder = await GetStateMachineFluentBuilder();

        builder
            .BeginState((Identifier)"S1")
            .EndState()
            .BeginParallel((Identifier)"P1")
            .EndParallel()
            .BeginFinal((Identifier)"F1")
            .EndFinal();

        var stateMachine = builder.Build();

        Assert.IsNotNull(stateMachine);
        Assert.AreEqual(expected: 3, stateMachine.States.Length);
        Assert.IsInstanceOfType(stateMachine.States[0], typeof(IState));
        Assert.IsInstanceOfType(stateMachine.States[1], typeof(IParallel));
        Assert.IsInstanceOfType(stateMachine.States[2], typeof(IFinal));
        Assert.AreEqual((Identifier)"S1", ((IState)stateMachine.States[0]).Id);
        Assert.AreEqual((Identifier)"P1", ((IParallel)stateMachine.States[1]).Id);
        Assert.AreEqual((Identifier)"F1", ((IFinal)stateMachine.States[2]).Id);
    }

    [TestMethod]
    public async Task DataModelTest()
    {
        IStateMachine stateMachine = default!;

        var services = new ServiceCollection();
        services.AddForwarding(_ => stateMachine);
        services.AddModule<StateMachineInterpreterModule>();
        services.AddModule<StateMachineFluentBuilderModule>();
        var serviceProvider = services.BuildProvider();

        var fluentBuilder = await serviceProvider.GetRequiredService<StateMachineFluentBuilder>();

        stateMachine = fluentBuilder
                       .BeginState((Identifier)"S1")
                       .AddOnEntry(() => Runtime.DataModel["Hello"] = new DataModelValue("World"))
                       .EndState()
                       .Build();

        var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();
        var eventQueueWriter = await serviceProvider.GetRequiredService<IEventQueueWriter>();
        eventQueueWriter.Complete();

        var channel = Channel.CreateUnbounded<IIncomingEvent>();
        channel.Writer.Complete();

        try
        {
            await stateMachineInterpreter.Run();

            Assert.Fail("StateMachineQueueClosedException should be raised");
        }
        catch (StateMachineQueueClosedException)
        {
            // ignore
        }
    }

    [TestMethod]
    public async Task LiveLockErrorConditionTest()
    {
        IStateMachine stateMachine = default!;

        var services = new ServiceCollection();
        services.AddForwarding(_ => stateMachine);
        services.AddModule<StateMachineInterpreterModule>();
        services.AddModule<StateMachineFluentBuilderModule>();
        var serviceProvider = services.BuildProvider();

        var fluentBuilder = await serviceProvider.GetRequiredService<StateMachineFluentBuilder>();

        //var builder = FluentBuilderFactory.Create();

        stateMachine = fluentBuilder
                       .BeginState((Identifier)"S1")
                       .BeginTransition()
                       .SetCondition(new Func<bool>(() => throw new InvalidOperationException("some exception")))
                       .EndTransition()
                       .EndState()
                       .Build();

        var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();
        var eventQueueWriter = await serviceProvider.GetRequiredService<IEventQueueWriter>();
        eventQueueWriter.Complete();

        var channel = Channel.CreateUnbounded<IIncomingEvent>();
        channel.Writer.Complete();

        await Assert.ThrowsExactlyAsync<StateMachineDestroyedException>(async () => await stateMachineInterpreter.Run());
    }

    [TestMethod]
    public async Task LiveLockPingPongTest()
    {
        IStateMachine stateMachine = default!;

        var services = new ServiceCollection();
        services.AddForwarding(_ => stateMachine);
        services.AddModule<StateMachineInterpreterModule>();
        services.AddModule<StateMachineFluentBuilderModule>();
        var serviceProvider = services.BuildProvider();

        var fluentBuilder = await serviceProvider.GetRequiredService<StateMachineFluentBuilder>();

        stateMachine = fluentBuilder
                       .BeginState((Identifier)"S1")
                       .BeginTransition()
                       .SetTarget((Identifier)"S2")
                       .EndTransition()
                       .EndState()
                       .BeginState((Identifier)"S2")
                       .BeginTransition()
                       .SetTarget((Identifier)"S1")
                       .EndTransition()
                       .EndState()
                       .Build();

        var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();
        var eventQueueWriter = await serviceProvider.GetRequiredService<IEventQueueWriter>();
        eventQueueWriter.Complete();

        var channel = Channel.CreateUnbounded<IIncomingEvent>();
        channel.Writer.Complete();

        await Assert.ThrowsExactlyAsync<StateMachineDestroyedException>(async () => await stateMachineInterpreter.Run());
    }
}