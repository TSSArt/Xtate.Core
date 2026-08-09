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
using Xtate.DataTypes;
using Xtate.IoC;
using Xtate.Persistence.Extensions;
using Xtate.StateMachine;
using Xtate.StateMachineHost.Services;
using Xtate.TaskMonitor;

namespace Xtate.Persistence.Services;

[InstantiatedByIoC]
public class PersistedStateMachineScopeManager : StateMachineScopeManager, IAsyncInitialization
{
	private const int Operation = 0;

	private const int Add = 1;

	private const int Remove = 2;

	private const int SessionIdKey = 3;

	private readonly AsyncInit<PersistedStateMachineScopeManager> _asyncInit = new(m => m.Init());

	private readonly Bucket _bucket;

	private readonly SemaphoreSlim _lock = new(initialCount: 1, maxCount: 1);

	private int _record;

	public required ITransactionalStorage Storage
	{
		private get;
		[SetByIoC]
		init
		{
			_bucket = new Bucket(value);
			field = value;
		}
	}

	public required StorageManager StorageManager { private get; [SetByIoC] init; }

	public required ITaskMonitor PersistedTaskMonitor { private get; [SetByIoC] init; }

#region Interface IAsyncInitialization

	public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_asyncInit);

#endregion

	private async Task Resume(ResumedStateMachineClass stateMachineClass)
	{
		var stateMachineResult = await base.Run(stateMachineClass).ConfigureAwait(false);

		GetResultAndRemoveStorage(stateMachineClass.SessionId, stateMachineResult).Forget(PersistedTaskMonitor);
	}

	protected override async ValueTask<StateMachineResult> Run(StateMachineClass stateMachineClass)
	{
		await Register(stateMachineClass).ConfigureAwait(false);

		var stateMachineResult = await base.Run(stateMachineClass).ConfigureAwait(false);

		return new StateMachineResult(GetResultAndRemoveStorage(stateMachineClass.SessionId, stateMachineResult));
	}

	private async Task<DataModelValue> GetResultAndRemoveStorage(SessionId sessionId, StateMachineResult stateMachineResult)
	{
		var resultValue = await stateMachineResult.GetResult().ConfigureAwait(false);

		await Unregister(sessionId).ConfigureAwait(false);

		await StorageManager.RemoveStorage(sessionId).ConfigureAwait(false);

		return resultValue;
	}

	private async ValueTask Init()
	{
		var entries = new HashSet<SessionId>();

		var shrink = false;

		while (true)
		{
			var recordBucket = _bucket.Nested(_record);

			if (!recordBucket.TryGet(Operation, out int operation))
			{
				break;
			}

			var sessionId = _bucket.Nested(_record).GetSessionId(SessionIdKey);

			Infra.NotNull(sessionId);

			if (operation is Add)
			{
				entries.Add(sessionId);
			}

			if (operation is Remove)
			{
				shrink = true;

				entries.Remove(sessionId);
			}

			_record++;
		}

		if (shrink)
		{
			_bucket.RemoveSubtree(Bucket.RootKey);

			_record = 0;

			foreach (var entry in entries)
			{
				var bucket = _bucket.Nested(_record++);
				bucket.Add(Operation, Add);
				bucket.AddId(SessionIdKey, entry);
			}

			await Storage.CheckPoint(0).ConfigureAwait(false);

			await Storage.Shrink().ConfigureAwait(false);
		}

		if (entries.Count > 0)
		{
			var index = 0;
			var resumeTasks = new Task[entries.Count];

			foreach (var sessionId in entries)
			{
				resumeTasks[index++] = Resume(new ResumedStateMachineClass(sessionId));
			}

			await Task.WhenAll(resumeTasks).ConfigureAwait(false);
		}
	}

	private async ValueTask Register(StateMachineClass stateMachineClass)
	{
		await _lock.WaitAsync().ConfigureAwait(false);

		try
		{
			var bucket = _bucket.Nested(_record++);
			bucket.Add(Operation, Add);
			bucket.AddId(SessionIdKey, stateMachineClass.SessionId);

			await Storage.CheckPoint(0).ConfigureAwait(false);
		}
		finally
		{
			_lock.Release();
		}
	}

	private async ValueTask Unregister(SessionId sessionId)
	{
		await _lock.WaitAsync().ConfigureAwait(false);

		try
		{
			var bucket = _bucket.Nested(_record++);
			bucket.Add(Operation, Remove);
			bucket.AddId(SessionIdKey, sessionId);

			await Storage.CheckPoint(0).ConfigureAwait(false);
		}
		finally
		{
			_lock.Release();
		}
	}
}
