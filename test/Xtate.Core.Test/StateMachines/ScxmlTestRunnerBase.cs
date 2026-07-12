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

using Xtate.Class;
using Xtate.IoC;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.DependencyInjection;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.StateMachines;

public abstract class ScxmlTestRunnerBase
{
	private IStateMachineScopeManager _stateMachineScopeManager = null!;

	[TestInitialize]
	public async Task Initialize()
	{
		var idle = new Mock<IDestroyOnIdleTimeout>();
		idle.Setup(x => x.IdleTimeout).Returns(TimeSpan.FromMilliseconds(5000));

		var container = Container.Create<StateMachineProcessorModule>(services =>
																		  services.AddConstant(idle.Object));

		_stateMachineScopeManager = await container.GetRequiredService<IStateMachineScopeManager>();
	}

	[ExcludeFromCodeCoverage]
	protected async Task ExecuteTestCase(ScxmlTestCase testCase)
	{
		var smc = new ScxmlStringStateMachine(testCase.Scxml);
		var result = await _stateMachineScopeManager.Execute(smc, SecurityContextType.NewStateMachine);

		if (testCase.ExpectedFinalData is { } expected)
		{
			Assert.AreEqual(expected, result.AsStringOrDefault(), $"State machine '{testCase.Name}' returned unexpected done-data.");
		}
	}
}