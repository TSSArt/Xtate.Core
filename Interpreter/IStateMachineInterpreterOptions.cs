using System.Threading.Channels;

namespace Xtate.Core;

public interface IStateMachineInterpreterOptions
{
	SessionId             SessionId    { get; }
	ChannelReader<IEvent> eventChannel { get; }
	InterpreterOptions    options      { get; } //TODO:delete
	//IInterpreterModel      model        { get; }
}