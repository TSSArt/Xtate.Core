using System;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.Service
{
	[PublicAPI]
	public abstract class SimpleServiceBase : IService, IAsyncDisposable, IDisposable
	{
		private readonly TaskCompletionSource<DataModelValue> _completedTcs = new TaskCompletionSource<DataModelValue>();
		private readonly CancellationTokenSource              _tokenSource  = new CancellationTokenSource();

		private bool        _disposed;
		private InvokeData? _invokeData;

		protected SimpleServiceBase()
		{
			_invokeData = null!;
			ServiceCommunication = null!;
		}

		protected Uri?                  BaseUri              { get; private set; }
		protected IServiceCommunication ServiceCommunication { get; private set; }

		protected Uri?           Source     => _invokeData?.Source;
		protected string?        RawContent => _invokeData?.RawContent;
		protected DataModelValue Content    => _invokeData?.Content ?? default;
		protected DataModelValue Parameters => _invokeData?.Parameters ?? default;

		protected CancellationToken StopToken => _tokenSource.Token;

	#region Interface IAsyncDisposable

		public virtual ValueTask DisposeAsync()
		{
			try
			{
				Dispose();

				return default;
			}
			catch (Exception ex)
			{
				return new ValueTask(Task.FromException(ex));
			}
		}

	#endregion

	#region Interface IDisposable

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

	#endregion

	#region Interface IService

		ValueTask IService.Send(IEvent evt, CancellationToken token) => default;

		ValueTask IService.Destroy(CancellationToken token)
		{
			_tokenSource.Cancel();
			_completedTcs.TrySetCanceled();

			return default;
		}

		Task<DataModelValue> IService.Result => _completedTcs.Task;

	#endregion

		internal void Start(Uri? baseUri, InvokeData invokeData, IServiceCommunication serviceCommunication)
		{
			BaseUri = baseUri;
			_invokeData = invokeData;
			ServiceCommunication = serviceCommunication;

			RunAsync().Forget();

			async ValueTask RunAsync()
			{
				try
				{
					_completedTcs.TrySetResult(await Execute().ConfigureAwait(false));
				}
				catch (OperationCanceledException ex)
				{
					_completedTcs.TrySetCanceled(ex.CancellationToken);
				}
				catch (Exception ex)
				{
					_completedTcs.TrySetException(ex);
				}
			}
		}

		protected abstract ValueTask<DataModelValue> Execute();

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				_tokenSource.Dispose();
			}

			_disposed = true;
		}
	}
}