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

using System.Reflection;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Exceptions;
using Xtate.StateMachineHost.Services;

namespace Xtate.Test.UnitTests.StateMachineHost;

[TestClass]
public class SecurityContextCoverageTest
{
	[TestMethod]
	public async Task FactoryCreatesNestedContextsAndRestoresParentsOnDisposal()
	{
		var factory = new SecurityContextFactory();
		var fullAccess = (SecurityContext) factory.GetIIoBoundTask();

		Assert.AreEqual(SecurityContextType.NewTrustedStateMachine, fullAccess.Type);
		Assert.AreEqual(SecurityContextPermissions.Full, fullAccess.Permissions);
		Assert.IsTrue(fullAccess.HasPermissions(SecurityContextPermissions.CreateStateMachine));
		var firstTaskFactory = fullAccess.Factory;
		var secondTaskFactory = fullAccess.Factory;
		Assert.AreSame(firstTaskFactory, secondTaskFactory);

		await using (factory.GetRegistration(SecurityContextType.NewStateMachine))
		{
			var stateMachine = (SecurityContext) factory.GetIIoBoundTask();
			Assert.AreEqual(SecurityContextType.NewStateMachine, stateMachine.Type);
			Assert.AreEqual(SecurityContextPermissions.RunIoBoundTask, stateMachine.Permissions);
			stateMachine.CheckPermissions(SecurityContextPermissions.RunIoBoundTask);

			await using (factory.GetRegistration(SecurityContextType.InvokedService))
			{
				var invokedService = (SecurityContext) factory.GetIIoBoundTask();
				Assert.AreEqual(SecurityContextType.InvokedService, invokedService.Type);
				Assert.AreEqual(stateMachine.Permissions, invokedService.Permissions);
			}

			Assert.AreSame(stateMachine, factory.GetIIoBoundTask());
		}

		Assert.AreSame(fullAccess, factory.GetIIoBoundTask());
	}

	[TestMethod]
	public void NoAccessContextReportsAndEnforcesMissingPermissions()
	{
		var context = SecurityContext.NoAccess;
		var created = SecurityContext.Create(SecurityContextType.InvokedService, SecurityContextPermissions.RunIoBoundTask);

		Assert.AreEqual(SecurityContextType.NoAccess, context.Type);
		Assert.AreEqual(SecurityContextPermissions.None, context.Permissions);
		Assert.IsTrue(context.HasPermissions(SecurityContextPermissions.None));
		Assert.IsFalse(context.HasPermissions(SecurityContextPermissions.RunIoBoundTask));
		Assert.AreEqual(SecurityContextType.InvokedService, created.Type);
		Assert.AreEqual(SecurityContextPermissions.RunIoBoundTask, created.Permissions);
		Assert.ThrowsExactly<StateMachineSecurityException>(
			[ExcludeFromCodeCoverage] () => context.CheckPermissions(SecurityContextPermissions.RunIoBoundTask));
	}

	[TestMethod]
	public void NoAccessTaskSchedulerRejectsQueuedWorkWithSecurityException()
	{
		var taskFactory = SecurityContext.NoAccess.Factory;
		Exception? captured = null;

		try
		{
			_ = taskFactory.StartNew(static () => { });
		}
		catch (Exception ex)
		{
			captured = ex;
		}

		Assert.IsNotNull(captured);
		Assert.IsTrue(
			captured is StateMachineSecurityException || captured.InnerException is StateMachineSecurityException,
			$"Expected a security exception directly or as the inner exception, but received {captured.GetType().FullName}.");
	}

	[TestMethod]
	public void NoAccessTaskSchedulerRejectsInspectionQueueingAndInlineExecution()
	{
		var scheduler = SecurityContext.NoAccess.Factory.Scheduler!;
		var flags = BindingFlags.Instance | BindingFlags.NonPublic;

		AssertSchedulerCallThrows(scheduler.GetType().GetMethod("GetScheduledTasks", flags)!, scheduler, parameters: null);
		AssertSchedulerCallThrows(scheduler.GetType().GetMethod("QueueTask", flags)!, scheduler, [Task.CompletedTask]);
		AssertSchedulerCallThrows(scheduler.GetType().GetMethod("TryExecuteTaskInline", flags)!, scheduler, [Task.CompletedTask, false]);
	}

	private static void AssertSchedulerCallThrows(MethodInfo method, TaskScheduler scheduler, object?[]? parameters)
	{
		try
		{
			_ = method.Invoke(scheduler, parameters);
			Assert.Fail($"{method.Name} did not deny access.");
		}
		catch (TargetInvocationException exception)
		{
			Assert.AreEqual(typeof(StateMachineSecurityException), exception.InnerException?.GetType());
		}
	}
}
