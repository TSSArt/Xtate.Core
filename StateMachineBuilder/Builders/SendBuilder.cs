// Copyright © 2019-2024 Sergii Artemenko
// 
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

namespace Xtate.Builder;

public class SendBuilder : BuilderBase, ISendBuilder
{
	private IContent? _content;

	private IValueExpression? _delayExpression;

	private int? _delayMs;

	private string? _event;

	private IValueExpression? _eventExpression;

	private string? _id;

	private ILocationExpression? _idLocation;

	private ImmutableArray<ILocationExpression> _nameList;

	private ImmutableArray<IParam>.Builder? _parameters;

	private FullUri? _target;

	private IValueExpression? _targetExpression;

	private FullUri? _type;

	private IValueExpression? _typeExpression;

#region Interface ISendBuilder

	public ISend Build() =>
		new SendEntity
		{
			Ancestor = Ancestor, EventName = _event, EventExpression = _eventExpression, Target = _target, TargetExpression = _targetExpression,
			Type = _type, TypeExpression = _typeExpression, Id = _id, IdLocation = _idLocation, DelayMs = _delayMs,
			DelayExpression = _delayExpression, NameList = _nameList, Parameters = _parameters?.ToImmutable() ?? default, Content = _content
		};

	public void SetEvent(string evt)
	{
		Infra.RequiresNonEmptyString(evt);

		_event = evt;
	}

	public void SetEventExpression(IValueExpression eventExpression)
	{
		Infra.Requires(eventExpression);

		_eventExpression = eventExpression;
	}

	public void SetTarget(FullUri target)
	{
		Infra.Requires(target);

		_target = target;
	}

	public void SetTargetExpression(IValueExpression targetExpression)
	{
		Infra.Requires(targetExpression);

		_targetExpression = targetExpression;
	}

	public void SetType(FullUri type)
	{
		Infra.Requires(type);

		_type = type;
	}

	public void SetTypeExpression(IValueExpression typeExpression)
	{
		Infra.Requires(typeExpression);

		_typeExpression = typeExpression;
	}

	public void SetId(string id)
	{
		Infra.RequiresNonEmptyString(id);

		_id = id;
	}

	public void SetIdLocation(ILocationExpression idLocation)
	{
		Infra.Requires(idLocation);

		_idLocation = idLocation;
	}

	public void SetDelay(int delay)
	{
		Infra.RequiresNonNegative(delay);

		_delayMs = delay;
	}

	public void SetDelayExpression(IValueExpression delayExpression)
	{
		Infra.Requires(delayExpression);

		_delayExpression = delayExpression;
	}

	public void SetNameList(ImmutableArray<ILocationExpression> nameList)
	{
		Infra.RequiresNonEmptyCollection(nameList);

		_nameList = nameList;
	}

	public void AddParameter(IParam param)
	{
		Infra.Requires(param);

		(_parameters ??= ImmutableArray.CreateBuilder<IParam>()).Add(param);
	}

	public void SetContent(IContent content)
	{
		Infra.Requires(content);

		_content = content;
	}

#endregion
}