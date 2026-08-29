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

using Xtate.DataModel;
using Xtate.IoC;
using Xtate.IoC.Tools;
using Xtate.Persistence.Extensions;
using Xtate.StateMachineHost;
using Xtate.StateMachineHost.Services;

namespace Xtate.Persistence.Services;

[InstantiatedByIoC]
public class PersistedExternalServiceScopeManager : ExternalServiceScopeManager, IAsyncInitialization
{
	private const int Operation = 0;

	private const int Add = 1;

	private const int Remove = 2;

	private const int Invoke = 3;

	private const int RefId = 4;

	private readonly Bucket _bucket;

	private readonly AsyncInit<PersistedExternalServiceScopeManager> _init = new(manager => manager.Init());

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

	public required Func<InvokeData, ValueTask<ResumeExternalServiceClass>> ResumeExternalServiceClassFactory { private get; [SetByIoC] init; }

	public required DisposeToken DisposeToken { private get; [SetByIoC] init; }

#region Interface IAsyncInitialization

	public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

#endregion

	private async ValueTask Init()
	{
		var refIds = new HashSet<int>();
		var shrink = false;

		while (true)
		{
			var recordBucket = _bucket.Nested(_record);

			if (!recordBucket.TryGet(Operation, out int operation))
			{
				break;
			}

			switch (operation)
			{
				case Add:
					refIds.Add(_record);

					break;

				case Remove:
					shrink = true;
					refIds.Remove(recordBucket.GetInt32(RefId));

					break;

				default:
					throw new PersistenceException(Resources.Exception_IncorrectDataFormat);
			}

			_record++;
		}

		var invokes = new List<PersistedInvokeData>(refIds.Count);

		foreach (var refId in refIds)
		{
			invokes.Add(new PersistedInvokeData(_bucket.Nested(refId).Nested(Invoke)) { RefId = refId });
		}

		if (shrink)
		{
			_bucket.RemoveSubtree(Bucket.RootKey);
			_record = 0;

			foreach (var invokeData in invokes)
			{
				invokeData.RefId = _record++;
				var recordBucket = _bucket.Nested(invokeData.RefId);
				recordBucket.Add(Operation, Add);
				invokeData.Store(recordBucket.Nested(Invoke));
			}

			await Storage.CheckPoint(level: 0).ConfigureAwait(false);
			await Storage.Shrink().ConfigureAwait(false);
		}

		if (invokes.Count > 0)
		{
			var index = 0;
			var resumeTasks = new Task[invokes.Count];

			foreach (var invokeData in invokes)
			{
				resumeTasks[index++] = Resume(invokeData);
			}

			await Task.WhenAll(resumeTasks).ConfigureAwait(false);
		}
	}

	private async Task Resume(PersistedInvokeData invokeData)
	{
		var externalServiceClass = await ResumeExternalServiceClassFactory(invokeData).ConfigureAwait(false);

		await StartInternal(invokeData, externalServiceClass, DisposeToken).ConfigureAwait(false);
	}

	public override async ValueTask<ExternalServiceResult> Start(ExternalServiceClass externalServiceClass, CancellationToken token)
	{
		var invokeData = CreatePersistedInvokeData(externalServiceClass);

		await AddInvoke(invokeData, token).ConfigureAwait(false);

		return await StartInternal(invokeData, externalServiceClass, token).ConfigureAwait(false);
	}

	private async ValueTask<ExternalServiceResult> StartInternal(PersistedInvokeData invokeData, ExternalServiceClass externalServiceClass, CancellationToken token)
	{
		try
		{
			var result = await base.Start(externalServiceClass, token).ConfigureAwait(false);

			return new ExternalServiceResult(WaitForCompletionAndRemoveStorage(invokeData, result));
		}
		catch (Exception ex)
		{
			if (!IsSuspendException(ex))
			{
				await RemoveInvoke(invokeData).ConfigureAwait(false);
			}

			return new ExternalServiceResult(Task.FromException(ex));
		}
	}

	private async Task WaitForCompletionAndRemoveStorage(PersistedInvokeData invokeData, ExternalServiceResult externalServiceResult)
	{
		try
		{
			await externalServiceResult.WaitForCompletion().ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			if (!IsSuspendException(ex))
			{
				await RemoveInvoke(invokeData).ConfigureAwait(false);
			}

			throw;
		}

		await RemoveInvoke(invokeData).ConfigureAwait(false);
	}

	private static bool IsSuspendException(Exception exception) => exception is ExternalServiceSuspendedException;

	private static PersistedInvokeData CreatePersistedInvokeData(ExternalServiceClass externalServiceClass)
	{
		IExternalServiceInvokeId invokeId = externalServiceClass;
		IExternalServiceType type = externalServiceClass;
		IExternalServiceSource source = externalServiceClass;
		IExternalServiceParameters parameters = externalServiceClass;

		return new PersistedInvokeData(invokeId.InvokeId, type.Type, source.Source, source.RawContent, source.Content, parameters.Parameters);
	}

	private async ValueTask AddInvoke(PersistedInvokeData invokeData, CancellationToken token)
	{
		await _lock.WaitAsync(token).ConfigureAwait(false);

		try
		{
			invokeData.RefId = _record++;
			var recordBucket = _bucket.Nested(invokeData.RefId);
			recordBucket.Add(Operation, Add);
			invokeData.Store(recordBucket.Nested(Invoke));

			try
			{
				await Storage.CheckPoint(level: 0).ConfigureAwait(false);
			}
			catch
			{
				_bucket.RemoveSubtree(--_record);

				throw;
			}
		}
		finally
		{
			_lock.Release();
		}
	}

	private async ValueTask RemoveInvoke(PersistedInvokeData invokeData)
	{
		await _lock.WaitAsync().ConfigureAwait(false);

		try
		{
			var recordBucket = _bucket.Nested(_record++);
			recordBucket.Add(Operation, Remove);
			recordBucket.Add(RefId, invokeData.RefId);

			try
			{
				await Storage.CheckPoint(level: 0).ConfigureAwait(false);
			}
			catch
			{
				_bucket.RemoveSubtree(--_record);

				throw;
			}
		}
		finally
		{
			_lock.Release();
		}
	}
}
