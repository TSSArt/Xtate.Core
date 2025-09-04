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

using Xtate.IoC;

namespace Xtate.Core.Test.DI;

[TestClass]
public class StateMachineInterpreterDiTest
{
    [TestMethod]
    public async Task EmptyRun()
    {
        var services = new ServiceCollection();
        services.AddTransient<IStateMachine>(_ => new StateMachineEntity { States = [new FinalEntity()] });
        services.AddModule<StateMachineInterpreterModule>();

        var serviceProvider = services.BuildProvider();

        var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();

        await stateMachineInterpreter.RunAsync();
    }

    [TestMethod]
    public async Task XpathDataModelRun()
    {
        var services = new ServiceCollection();
        var stateMachineEntity = new StateMachineEntity
                                 {
                                     DataModelType = "xpath",
                                     States =
                                     [
                                         new FinalEntity
                                         {
                                             DoneData = new DoneDataEntity
                                                        {
                                                            Content = new ContentEntity
                                                                      {
                                                                          Body = new ContentBody { Value = "qwerty" }
                                                                      }
                                                        }
                                         }
                                     ]
                                 };

        services.AddTransient<IStateMachine>(_ => stateMachineEntity);
        services.AddModule<StateMachineInterpreterModule>();

        var serviceProvider = services.BuildProvider();

        var stateMachineInterpreter = await serviceProvider.GetRequiredService<IStateMachineInterpreter>();

        var dataModelValue = await stateMachineInterpreter.RunAsync();

        Assert.AreEqual(expected: "qwerty", dataModelValue);
    }
}