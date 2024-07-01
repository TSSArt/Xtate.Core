<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2024 Sergii Artemenko
// 
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
#endregion

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class SendEvaluator : IExecEvaluator, ISend, IAncestorProvider
{
	private readonly ISend _send;

	protected SendEvaluator(ISend send)
	{
		Infra.Requires(send);

		_send = send;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _send;
=======
namespace Xtate.DataModel;

public abstract class SendEvaluator(ISend send) : IExecEvaluator, ISend, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => send;
>>>>>>> Stashed changes

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface ISend

<<<<<<< Updated upstream
	public virtual IContent?                           Content          => _send.Content;
	public virtual IValueExpression?                   DelayExpression  => _send.DelayExpression;
	public virtual int?                                DelayMs          => _send.DelayMs;
	public virtual string?                             EventName        => _send.EventName;
	public virtual IValueExpression?                   EventExpression  => _send.EventExpression;
	public virtual string?                             Id               => _send.Id;
	public virtual ILocationExpression?                IdLocation       => _send.IdLocation;
	public virtual ImmutableArray<ILocationExpression> NameList         => _send.NameList;
	public virtual ImmutableArray<IParam>              Parameters       => _send.Parameters;
	public virtual Uri?                                Target           => _send.Target;
	public virtual IValueExpression?                   TargetExpression => _send.TargetExpression;
	public virtual Uri?                                Type             => _send.Type;
	public virtual IValueExpression?                   TypeExpression   => _send.TypeExpression;
=======
	public virtual IContent?                           Content          => send.Content;
	public virtual IValueExpression?                   DelayExpression  => send.DelayExpression;
	public virtual int?                                DelayMs          => send.DelayMs;
	public virtual string?                             EventName        => send.EventName;
	public virtual IValueExpression?                   EventExpression  => send.EventExpression;
	public virtual string?                             Id               => send.Id;
	public virtual ILocationExpression?                IdLocation       => send.IdLocation;
	public virtual ImmutableArray<ILocationExpression> NameList         => send.NameList;
	public virtual ImmutableArray<IParam>              Parameters       => send.Parameters;
	public virtual Uri?                                Target           => send.Target;
	public virtual IValueExpression?                   TargetExpression => send.TargetExpression;
	public virtual Uri?                                Type             => send.Type;
	public virtual IValueExpression?                   TypeExpression   => send.TypeExpression;
>>>>>>> Stashed changes

#endregion
}

<<<<<<< Updated upstream
[PublicAPI]
public class DefaultSendEvaluator : SendEvaluator
{
	public DefaultSendEvaluator(ISend send) : base(send)
	{
		EventExpressionEvaluator = send.EventExpression?.As<IStringEvaluator>();
		TypeExpressionEvaluator = send.TypeExpression?.As<IStringEvaluator>();
		TargetExpressionEvaluator = send.TargetExpression?.As<IStringEvaluator>();
		DelayExpressionEvaluator = send.DelayExpression?.As<IIntegerEvaluator>();
		ContentExpressionEvaluator = send.Content?.Expression?.As<IObjectEvaluator>();
		ContentBodyEvaluator = send.Content?.Body?.As<IValueEvaluator>();
		IdLocationEvaluator = send.IdLocation?.As<ILocationEvaluator>();
		NameEvaluatorList = send.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();
		ParameterList = DataConverter.AsParamArray(send.Parameters);
	}

	public required Func<ValueTask<DataConverter>>     DataConverterFactory { private get; init; }
	public required Func<ValueTask<IEventController?>> EventSenderFactory   { private get; init; }

	public IObjectEvaluator?                  ContentExpressionEvaluator { get; }
	public IValueEvaluator?                   ContentBodyEvaluator       { get; }
	public IIntegerEvaluator?                 DelayExpressionEvaluator   { get; }
	public IStringEvaluator?                  EventExpressionEvaluator   { get; }
	public ILocationEvaluator?                IdLocationEvaluator        { get; }
	public IStringEvaluator?                  TargetExpressionEvaluator  { get; }
	public IStringEvaluator?                  TypeExpressionEvaluator    { get; }
	public ImmutableArray<ILocationEvaluator> NameEvaluatorList          { get; }
	public ImmutableArray<DataConverter.Param>       ParameterList              { get; }

	public override async ValueTask Execute()
	{
		var sendId = Id is not null ? SendId.FromString(Id) : SendId.New();

		if (IdLocationEvaluator is not null)
		{
			await IdLocationEvaluator.SetValue(sendId).ConfigureAwait(false);
		}

		var dataConverter = await DataConverterFactory().ConfigureAwait(false);
		var name = EventExpressionEvaluator is not null ? await EventExpressionEvaluator.EvaluateString().ConfigureAwait(false) : EventName;
		var data = await dataConverter.GetData(ContentBodyEvaluator, ContentExpressionEvaluator, NameEvaluatorList, ParameterList).ConfigureAwait(false);
		var type = TypeExpressionEvaluator is not null ? ToUri(await TypeExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : Type;
		var target = TargetExpressionEvaluator is not null ? ToUri(await TargetExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : Target;
		var delayMs = DelayExpressionEvaluator is not null ? await DelayExpressionEvaluator.EvaluateInteger().ConfigureAwait(false) : DelayMs ?? 0;

		var eventEntity = new EventEntity(name)
						  {
							  SendId = sendId,
							  Type = type,
							  Target = target,
							  DelayMs = delayMs,
							  Data = data
						  };

		if (ContentBodyEvaluator is IStringEvaluator rawContentEvaluator)
		{
			eventEntity.RawData = await rawContentEvaluator.EvaluateString().ConfigureAwait(false);
		}

		if (await EventSenderFactory().ConfigureAwait(false) is { } eventSender)
		{
=======
public class DefaultSendEvaluator(ISend send) : SendEvaluator(send)
{
	private readonly IValueEvaluator? _contentBodyEvaluator = send.Content?.Body?.As<IValueEvaluator>();

	private readonly IObjectEvaluator?                   _contentExpressionEvaluator = send.Content?.Expression?.As<IObjectEvaluator>();
	private readonly IIntegerEvaluator?                  _delayExpressionEvaluator   = send.DelayExpression?.As<IIntegerEvaluator>();
	private readonly IStringEvaluator?                   _eventExpressionEvaluator   = send.EventExpression?.As<IStringEvaluator>();
	private readonly ILocationEvaluator?                 _idLocationEvaluator        = send.IdLocation?.As<ILocationEvaluator>();
	private readonly ImmutableArray<ILocationEvaluator>  _nameEvaluatorList          = send.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();
	private readonly ImmutableArray<DataConverter.Param> _parameterList              = DataConverter.AsParamArray(send.Parameters);
	private readonly IStringEvaluator?                   _targetExpressionEvaluator  = send.TargetExpression?.As<IStringEvaluator>();
	private readonly IStringEvaluator?                   _typeExpressionEvaluator    = send.TypeExpression?.As<IStringEvaluator>();
	public required  Func<ValueTask<DataConverter>>      DataConverterFactory { private get; [UsedImplicitly] init; }
	public required  Func<ValueTask<IEventController?>>  EventSenderFactory   { private get; [UsedImplicitly] init; }

	public override async ValueTask Execute()
	{
		var sendId = base.Id is { } id ? SendId.FromString(id) : SendId.New();

		if (_idLocationEvaluator is not null)
		{
			await _idLocationEvaluator.SetValue(sendId).ConfigureAwait(false);
		}

		var dataConverter = await DataConverterFactory().ConfigureAwait(false);
		var name = _eventExpressionEvaluator is not null ? await _eventExpressionEvaluator.EvaluateString().ConfigureAwait(false) : EventName;
		var data = await dataConverter.GetData(_contentBodyEvaluator, _contentExpressionEvaluator, _nameEvaluatorList, _parameterList).ConfigureAwait(false);
		var type = _typeExpressionEvaluator is not null ? ToUri(await _typeExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : Type;
		var target = _targetExpressionEvaluator is not null ? ToUri(await _targetExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : Target;
		var delayMs = _delayExpressionEvaluator is not null ? await _delayExpressionEvaluator.EvaluateInteger().ConfigureAwait(false) : DelayMs ?? 0;
		var rawContent = _contentBodyEvaluator is IStringEvaluator rawContentEvaluator ? await rawContentEvaluator.EvaluateString().ConfigureAwait(false) : null;

		if (await EventSenderFactory().ConfigureAwait(false) is { } eventSender)
		{
			var eventEntity = new EventEntity(name)
							  {
								  SendId = sendId,
								  Type = type,
								  Target = target,
								  DelayMs = delayMs,
								  Data = data,
								  RawData = rawContent
							  };

>>>>>>> Stashed changes
			await eventSender.Send(eventEntity).ConfigureAwait(false);
		}
	}

	private static Uri ToUri(string uri) => new(uri, UriKind.RelativeOrAbsolute);
}