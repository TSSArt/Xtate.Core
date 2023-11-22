using System.Threading.Channels;

namespace Xtate.Core;

public class StateMachineInterpreterOptions : IStateMachineInterpreterOptions
{
	//private readonly IInterpreterModel  _interpreterModel;
	private readonly InterpreterOptions _interpreterOptions;

	public StateMachineInterpreterOptions(IStateMachineStartOptions stateMachineStartOptions, ServiceLocator serviceLocator)
	{
		//_interpreterModel = interpreterModel;
		SessionId = stateMachineStartOptions.SessionId;
		_interpreterOptions = new InterpreterOptions(serviceLocator) { };
	}

	public SessionId             SessionId    { get; }
	public ChannelReader<IEvent> eventChannel { get; }
	public InterpreterOptions    options      => _interpreterOptions;
	//public IInterpreterModel     model        => _interpreterModel;
}