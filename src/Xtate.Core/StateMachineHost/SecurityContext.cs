#region Copyright © 2019-2020 Sergii Artemenko

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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Core
{
	public enum SecurityContextType
	{
		NoAccess,
		NewStateMachine,
		NewTrustedStateMachine,
		InvokedStateMachine
	}

	[Flags]
	public enum SecurityContextPermissions
	{
		None                      = 0x0000_0000,
		RunIoBoundTask            = 0x0000_0001,
		CreateStateMachine        = 0x0000_0002,
		CreateTrustedStateMachine = 0x0000_0004,
		Full                      = 0x7FFF_FFFF
	}

	[PublicAPI]
	public sealed class SecurityContext : ISecurityContext
	{
		private const int IoBoundTaskSchedulerMaximumConcurrencyLevel = 2;

		private const TaskCreationOptions CreationOptions = TaskCreationOptions.DenyChildAttach |
															TaskCreationOptions.HideScheduler |
															TaskCreationOptions.LongRunning |
															TaskCreationOptions.RunContinuationsAsynchronously;

		private const TaskContinuationOptions ContinuationOptions = TaskContinuationOptions.DenyChildAttach |
																	TaskContinuationOptions.HideScheduler |
																	TaskContinuationOptions.LongRunning;

		private readonly SecurityContext?                                  _parent;
		private          GlobalCache<(object Key, object SubKey), object>? _globalCache;

		private TaskFactory?                                     _ioBoundTaskFactory;
		private LocalCache<(object Key, object SubKey), object>? _localCache;

		private SecurityContext(SecurityContextType type, SecurityContextPermissions permissions, SecurityContext? parentSecurityContext)
		{
			Type = type;
			Permissions = permissions;
			_parent = parentSecurityContext;
		}

		public SecurityContextType Type { get; }

		public SecurityContextPermissions Permissions { get; }

		public TaskFactory IoBoundTaskFactory => _ioBoundTaskFactory ??= CreateTaskFactory();

		public static SecurityContext NoAccess { get; } = new(SecurityContextType.NoAccess, SecurityContextPermissions.None, parentSecurityContext: default);

		private TaskFactory CreateTaskFactory()
		{
			var taskScheduler = HasPermissions(SecurityContextPermissions.RunIoBoundTask)
					? new IoBoundTaskScheduler(IoBoundTaskSchedulerMaximumConcurrencyLevel)
					: NoAccessTaskScheduler.Instance;

			return new TaskFactory(cancellationToken: default, CreationOptions, ContinuationOptions, taskScheduler);
		}

		public bool HasPermissions(SecurityContextPermissions permissions) => (Permissions & permissions) == permissions;

		public void CheckPermissions(SecurityContextPermissions permissions)
		{
			if (!HasPermissions(permissions))
			{
				throw new StateMachineSecurityException(Res.Format(Resources.Exception_Access_denied_Permission_required, permissions));
			}
		}

		private LocalCache<(object Key, object SubKey), object> GetLocalCache()
		{
			if (_localCache is { } localCache)
			{
				return localCache;
			}

			var root = this;
			while (root._parent is { } parent)
			{
				root = parent;
			}

			root._globalCache ??= new GlobalCache<(object Key, object SubKey), object>();

			return _localCache = root._globalCache.CreateLocalCache();
		}

		public ValueTask SetValue<T>(object key, object subKey, [DisallowNull] T value, ValueOptions options) => GetLocalCache().SetValue((key, subKey), value, options);

		public bool TryGetValue<T>(object key, object subKey, [NotNullWhen(true)] out T? value)
		{
			if (GetLocalCache().TryGetValue((key, subKey), out var obj))
			{
				value = (T) obj;

				return true;
			}

			value = default;

			return false;
		}

		private async ValueTask DisposeAsync()
		{
			if (_localCache is { } localCache)
			{
				await localCache.DisposeAsync().ConfigureAwait(false);
			}

			_localCache = null;
		}

		internal static SecurityContext Create(SecurityContextType type, DeferredFinalizer finalizer)
		{
			var permissions = type switch
			{
					SecurityContextType.NewTrustedStateMachine => SecurityContextPermissions.Full,
					SecurityContextType.NewStateMachine => SecurityContextPermissions.RunIoBoundTask,
					_ => Infrastructure.UnexpectedValue<SecurityContextPermissions>(type)
			};

			return Create(type, permissions, finalizer);
		}

		internal static SecurityContext Create(SecurityContextType type, SecurityContextPermissions permissions, DeferredFinalizer finalizer)
		{
			var securityContext = new SecurityContext(type, permissions, parentSecurityContext: default);

			finalizer.Add(new Disposer(securityContext));

			return securityContext;
		}

		public ISecurityContext CreateNested(SecurityContextType type, DeferredFinalizer finalizer)
		{
			if (finalizer is null) throw new ArgumentNullException(nameof(finalizer));

			SecurityContext securityContext;
			switch (type)
			{
				case SecurityContextType.NewTrustedStateMachine:
					CheckPermissions(SecurityContextPermissions.CreateTrustedStateMachine);

					securityContext = new SecurityContext(type, Permissions, this);

					break;

				case SecurityContextType.NewStateMachine:
					CheckPermissions(SecurityContextPermissions.CreateStateMachine);

					securityContext = new SecurityContext(type, SecurityContextPermissions.RunIoBoundTask, this);

					break;

				case SecurityContextType.InvokedStateMachine:
					securityContext = new SecurityContext(type, Permissions, this);

					break;

				default:
					throw Infrastructure.UnexpectedValue<Exception>(type);
			}

			finalizer.Add(new Disposer(securityContext));

			return securityContext;
		}

		private class NoAccessTaskScheduler : TaskScheduler
		{
			public static readonly TaskScheduler Instance = new NoAccessTaskScheduler();

			protected override IEnumerable<Task> GetScheduledTasks() => throw GetSecurityException();

			protected override void QueueTask(Task task) => throw GetSecurityException();

			protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => throw GetSecurityException();

			private static Exception GetSecurityException() => throw new StateMachineSecurityException(Resources.Exception_Access_to_IO_bound_threads_denied);
		}

		private class Disposer : IAsyncDisposable
		{
			private readonly SecurityContext _securityContext;

			public Disposer(SecurityContext securityContext) => _securityContext = securityContext;

		#region Interface IAsyncDisposable

			public ValueTask DisposeAsync() => _securityContext.DisposeAsync();

		#endregion
		}
	}
}