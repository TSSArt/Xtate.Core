using System.Collections.Immutable;
using Xtate.CustomAction;

namespace Xtate
{
	internal sealed class PreDataModelProcessor : StateMachineVisitor
	{
		private readonly ImmutableArray<ICustomActionFactory> _customActionProviders;
		private readonly IErrorProcessor                      _errorProcessor;

		public PreDataModelProcessor(IErrorProcessor errorProcessor, ImmutableArray<ICustomActionFactory> customActionProviders)
		{
			_errorProcessor = errorProcessor;
			_customActionProviders = customActionProviders;
		}

		public void Process(ref IExecutableEntity executableEntity)
		{
			Visit(ref executableEntity);
		}

		protected override void Build(ref ICustomAction customAction, ref CustomActionEntity customActionProperties)
		{
			base.Build(ref customAction, ref customActionProperties);

			var customActionDispatcher = new CustomActionDispatcher(_customActionProviders, _errorProcessor, customActionProperties);
			customActionDispatcher.SetupExecutor();

			customAction = customActionDispatcher;
		}
	}
}