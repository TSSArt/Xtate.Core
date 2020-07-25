using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xtate
{
	public sealed partial class StateMachineHost : IHost
	{
	#region Interface IHost

		async ValueTask IHost.StartStateMachineAsync(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, CancellationToken token) =>
				await StartStateMachine(sessionId, origin, parameters, token).ConfigureAwait(false);

		ValueTask<DataModelValue> IHost.ExecuteStateMachineAsync(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, CancellationToken token) =>
				ExecuteStateMachine(sessionId, origin, parameters, token);

		void IHost.DestroyStateMachine(SessionId sessionId) => GetCurrentContext().TriggerDestroySignal(sessionId);

	#endregion

		private async ValueTask<StateMachineController> StartStateMachine(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, CancellationToken token = default)
		{
			if (sessionId == null) throw new ArgumentNullException(nameof(sessionId));
			if (origin.Type == StateMachineOriginType.None) throw new ArgumentException(Resources.Exception_StateMachine_origin_missed, nameof(origin));

			var context = GetCurrentContext();
			var errorProcessor = CreateErrorProcessor(sessionId, origin);

			var controller = await context.CreateAndAddStateMachine(sessionId, origin, parameters, errorProcessor, token).ConfigureAwait(false);

			await controller.StartAsync(token).ConfigureAwait(false);

			CompleteStateMachine(context, controller).Forget();

			return controller;
		}

		private static async ValueTask CompleteStateMachine(StateMachineHostContext context, StateMachineController controller)
		{
			try
			{
				await controller.Result.ConfigureAwait(false);
			}
			finally
			{
				await context.RemoveStateMachine(controller.SessionId).ConfigureAwait(false);
			}
		}

		private async ValueTask<DataModelValue> ExecuteStateMachine(SessionId sessionId, StateMachineOrigin origin, DataModelValue parameters, CancellationToken token = default)
		{
			var context = GetCurrentContext();
			var errorProcessor = CreateErrorProcessor(sessionId, origin);

			var controller = await context.CreateAndAddStateMachine(sessionId, origin, parameters, errorProcessor, token).ConfigureAwait(false);

			try
			{
				return await controller.ExecuteAsync().ConfigureAwait(false);
			}
			finally
			{
				await context.RemoveStateMachine(sessionId).ConfigureAwait(false);
			}
		}
	}
}