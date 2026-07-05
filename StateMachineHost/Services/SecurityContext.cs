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

using Xtate.IoBoundTask;
using Xtate.IoBoundTask.Services;
using Xtate.StateMachineHost.Exceptions;

namespace Xtate.StateMachineHost.Services;

public sealed class SecurityContext : IIoBoundTask
{
	private const int IoBoundTaskSchedulerMaximumConcurrencyLevel = 2;

	private const TaskCreationOptions CreationOptions = TaskCreationOptions.DenyChildAttach |
														TaskCreationOptions.HideScheduler |
														TaskCreationOptions.LongRunning |
														TaskCreationOptions.RunContinuationsAsynchronously;

	private const TaskContinuationOptions ContinuationOptions = TaskContinuationOptions.DenyChildAttach |
																TaskContinuationOptions.HideScheduler |
																TaskContinuationOptions.LongRunning;

	private SecurityContext(SecurityContextType type, SecurityContextPermissions permissions)
	{
		Type = type;
		Permissions = permissions;
	}

	public SecurityContextType Type { get; }

	public SecurityContextPermissions Permissions { get; }

	public static SecurityContext NoAccess { get; } = new(SecurityContextType.NoAccess, SecurityContextPermissions.None);

	internal static SecurityContext FullAccess { get; } = new(SecurityContextType.NewTrustedStateMachine, SecurityContextPermissions.Full);

#region Interface IIoBoundTask

	public TaskFactory Factory => field ??= CreateTaskFactory();

#endregion

	internal SecurityContext CreateNested(SecurityContextType type)
	{
		SecurityContext securityContext;

		switch (type)
		{
			case SecurityContextType.NewTrustedStateMachine:
				CheckPermissions(SecurityContextPermissions.CreateTrustedStateMachine);

				securityContext = new SecurityContext(type, Permissions);

				break;

			case SecurityContextType.NewStateMachine:
				CheckPermissions(SecurityContextPermissions.CreateStateMachine);

				securityContext = new SecurityContext(type, SecurityContextPermissions.RunIoBoundTask);

				break;

			case SecurityContextType.InvokedService:
				securityContext = new SecurityContext(type, Permissions);

				break;

			default:
				throw Infra.Unmatched(type);
		}

		return securityContext;
	}
	
	private TaskFactory CreateTaskFactory()
	{
		var taskScheduler = HasPermissions(SecurityContextPermissions.RunIoBoundTask)
			? new IoBoundTaskScheduler(IoBoundTaskSchedulerMaximumConcurrencyLevel)
			: NoAccessTaskScheduler.Instance;

		return new TaskFactory(cancellationToken: CancellationToken.None, CreationOptions, ContinuationOptions, taskScheduler);
	}

	public bool HasPermissions(SecurityContextPermissions permissions) => (Permissions & permissions) == permissions;

	public void CheckPermissions(SecurityContextPermissions permissions)
	{
		if (!HasPermissions(permissions))
		{
			throw new StateMachineSecurityException(Res.Format(Resources.Exception_AccessDeniedPermissionRequired, permissions));
		}
	}

	internal static SecurityContext Create(SecurityContextType type, SecurityContextPermissions permissions) => new(type, permissions);

	private class NoAccessTaskScheduler : TaskScheduler
	{
		public static readonly TaskScheduler Instance = new NoAccessTaskScheduler();

		protected override IEnumerable<Task> GetScheduledTasks() => throw GetSecurityException();

		protected override void QueueTask(Task task) => throw GetSecurityException();

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => throw GetSecurityException();

		private static Exception GetSecurityException() => throw new StateMachineSecurityException(Resources.Exception_AccessToIOBoundThreadsDenied);
	}
}