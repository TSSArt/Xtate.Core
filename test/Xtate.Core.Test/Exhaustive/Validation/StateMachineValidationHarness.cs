using Xtate.StateMachine;
using Xtate.StateMachine.Validator;
using Xtate.StateMachine.Validator.Services;

namespace Xtate.Core.Test.Exhaustive.Validation;

/// <summary>Captures validation diagnostics from the public-model validation layer without test-only dependencies.</summary>
internal static class StateMachineValidationHarness
{
	public static IReadOnlyList<string> Validate(IStateMachine model)
	{
		var errors = new CollectingErrors();
		new StateMachineValidator { ErrorProcessorService = errors }.Validate(model);
		return errors.Messages;
	}

	private sealed class CollectingErrors : IErrorProcessorService<StateMachineValidator>
	{
		private readonly List<string> _messages = [];
		public IReadOnlyList<string> Messages => _messages;
		public void AddError(object? entity, string message, Exception? exception = null) => _messages.Add(message);
	}
}
