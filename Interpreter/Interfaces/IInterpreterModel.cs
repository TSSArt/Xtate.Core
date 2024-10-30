using Xtate.Persistence;

namespace Xtate.Core;

public interface IInterpreterModel
{
	StateMachineNode Root { get; }

	IEntityMap? EntityMap { get; }
}