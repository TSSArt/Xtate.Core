namespace Xtate.Core;

public class InterpreterVerboseLogEnricher<TSource> : ILogEnricher<TSource>
{
	public required IDataModelController DataModelController { private get; [UsedImplicitly] init; }

#region Interface ILogEnricher<TSource>

	public IEnumerable<LoggingParameter> EnumerateProperties()
	{
		yield return new LoggingParameter(name: @"DataModel", DataModelController.DataModel.AsConstant());
	}

	public string Namespace => @"ctx";

	public Level Level => Level.Verbose;

#endregion
}