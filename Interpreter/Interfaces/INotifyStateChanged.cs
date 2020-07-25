using System.Threading.Tasks;

namespace Xtate
{
	public interface INotifyStateChanged
	{
		ValueTask OnChanged(StateMachineInterpreterState state);
	}
}