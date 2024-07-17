namespace Xtate.Core;

public class StateEntityParser : EntityParserBase<IStateEntity>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(IStateEntity stateEntity)
	{
		Infra.Requires(stateEntity);

		if (stateEntity.Id is { } stateId)
		{
			yield return new LoggingParameter(name: @"StateId", stateId);
		}
	}
}