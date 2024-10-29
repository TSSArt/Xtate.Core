namespace Xtate.Core;

public interface IStateMachineIdlePeriod
{
	TimeSpan? IdlePeriod { get; }
}