using Xtate.Annotations;

namespace Xtate
{
	[PublicAPI]
	public interface IStateMachineValidator
	{
		void Validate(IStateMachine stateMachine, IErrorProcessor errorProcessor);
	}
}