#region Copyright © 2019-2023 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

namespace Xtate.Core;

//TODO: uncomment
public partial class StateMachineInterpreter //: IInterpreterLoggerContext
{
	/*
#region Interface IInterpreterLoggerContext

	public string ConvertToText(DataModelValue value)
	{
		Infra.NotNull(_dataModelHandler);

		return _dataModelHandler.ConvertToText(value);
	}

	public DataModelValue GetDataModel()
	{
		if (_context == null!)
		{
			return default;
		}

		return new LazyValue<IStateMachineContext>(stateMachineContext => stateMachineContext.DataModel.AsConstant(), _context);
	}

	ImmutableArray<string> IInterpreterLoggerContext.GetActiveStates()
	{
		if (_context == null!)
		{
			return ImmutableArray<string>.Empty;
		}

		var configuration = _context.Configuration;

		var list = ImmutableArray.CreateBuilder<string>(configuration.Count);

		foreach (var node in configuration)
		{
			list.Add(node.Id.Value);
		}

		return list.MoveToImmutable();
	}

	IStateMachine IInterpreterLoggerContext.StateMachine => GetStateMachine();

	SessionId IInterpreterLoggerContext.SessionId => _sessionId;

#endregion

#region Interface ILoggerContext

	DataModelList ILoggerContext.GetProperties()
	{
		var properties = new DataModelList { { @"SessionId", _sessionId } };

		if (GetStateMachine().Name is { } stateMachineName)
		{
			properties.Add(key: @"StateMachineName", stateMachineName);
		}

		if (_context.Configuration.Count > 0)
		{
			var activeStates = new DataModelList();
			foreach (var node in _context.Configuration)
			{
				activeStates.Add(node.Id.Value);
			}

			activeStates.MakeDeepConstant();

			properties.Add(key: @"ActiveStates", activeStates);
		}

		properties.Add(key: @"DataModel", GetDataModel());

		properties.MakeDeepConstant();

		return properties;
	}

	string ILoggerContext.LoggerContextType => nameof(IInterpreterLoggerContext);

#endregion
	*/
	/*
	private IStateMachine GetStateMachine()
	{
		if (_model is not null)
		{
			return _model.Root;
		}

		Infra.NotNull(_stateMachine);

		return _stateMachine;
	}
	*/
}

public abstract class EntityParserBase<TEntity> : IEntityParserProvider, IEntityParserHandler
{
#region Interface IEntityParserHandler

	IEnumerable<LoggingParameter> IEntityParserHandler.EnumerateProperties<T>(T entity) => EnumerateProperties(ConvertHelper<T, TEntity>.Convert(entity));

#endregion

#region Interface IEntityParserProvider

	public virtual IEntityParserHandler TryGetEntityParserHandler<T>(T entity) => entity is TEntity ? this : default;

#endregion

	protected abstract IEnumerable<LoggingParameter> EnumerateProperties(TEntity entity);
}

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

public class InvokeIdEntityParser : EntityParserBase<InvokeId?>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(InvokeId? invokeId)
	{
		if (invokeId is not null)
		{
			yield return new LoggingParameter(name: @"InvokeId", invokeId);
		}
	}
}

public class DataModelValueEntityParser : EntityParserBase<DataModelValue>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(DataModelValue value)
	{
		if (!value.IsUndefined())
		{
			yield return new LoggingParameter(name: @"Parameter", value);
		}
	}
}

public class ExceptionEntityParser : EntityParserBase<Exception>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(Exception exception)
	{
		yield return new LoggingParameter(name: @"Exception", exception);
	}
}

public class TransitionEntityParser : EntityParserBase<ITransition>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(ITransition transition)
	{
		Infra.Requires(transition);

		yield return new LoggingParameter(name: @"TransitionType", transition.Type);

		if (!transition.EventDescriptors.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"EventDescriptors", EventDescriptor.ToString(transition.EventDescriptors));
		}

		if (!transition.Target.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"Target", Identifier.ToString(transition.Target));
		}
	}
}

public class InvokeDataEntityParser : EntityParserBase<InvokeData>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(InvokeData invokeData)
	{
		Infra.Requires(invokeData);

		if (invokeData.InvokeId is { } invokeId)
		{
			yield return new LoggingParameter(name: @"InvokeId", invokeId);
		}

		yield return new LoggingParameter(name: @"InvokeType", invokeData.Type);

		if (invokeData.Source is { } source)
		{
			yield return new LoggingParameter(name: @"InvokeSource", source);
		}
	}
}

public class EventEntityParser : EntityParserBase<IEvent>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(IEvent evt)
	{
		Infra.Requires(evt);

		if (!evt.NameParts.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"EventName", EventName.ToName(evt.NameParts));
		}

		yield return new LoggingParameter(name: @"EventType", evt.Type);

		if (evt.Origin is { } origin)
		{
			yield return new LoggingParameter(name: @"Origin", origin);
		}

		if (evt.OriginType is { } originType)
		{
			yield return new LoggingParameter(name: @"OriginType", originType);
		}

		if (evt.SendId is { } sendId)
		{
			yield return new LoggingParameter(name: @"SendId", sendId);
		}

		if (evt.InvokeId is { } invokeId)
		{
			yield return new LoggingParameter(name: @"InvokeId", invokeId);
		}
	}
}

public class OutgoingEventEntityParser : EntityParserBase<IOutgoingEvent>
{
	protected override IEnumerable<LoggingParameter> EnumerateProperties(IOutgoingEvent evt)
	{
		Infra.Requires(evt);

		if (!evt.NameParts.IsDefaultOrEmpty)
		{
			yield return new LoggingParameter(name: @"EventName", EventName.ToName(evt.NameParts));
		}

		if (evt.SendId is { } sendId)
		{
			yield return new LoggingParameter(name: @"SendId", sendId);
		}

		if (evt.Type is { } type)
		{
			yield return new LoggingParameter(name: @"Type", type);
		}

		if (evt.Target is { } target)
		{
			yield return new LoggingParameter(name: @"Target", target);
		}

		if (evt.DelayMs is var delayMs and > 0)
		{
			yield return new LoggingParameter(name: @"DelayMs", delayMs);
		}
	}
}