namespace Xtate.Core;

[InstantiatedByIoC]
public class ImplementationTypeFactory<T>
{
	public required T Service { private get; [SetByIoC] init; }

	private Type GetValue() => Service.GetType();

	[CalledByIoC]
	public ImplementationType<T> GetValueFunc() => GetValue;
}