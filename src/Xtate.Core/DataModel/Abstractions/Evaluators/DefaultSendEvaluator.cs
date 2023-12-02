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

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface ISend

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

#endregion
}


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

	public required Func<ValueTask<DataConverter>>     DataConverterFactory { private get; [UsedImplicitly] init; }
	public required Func<ValueTask<IEventController?>> EventSenderFactory   { private get; [UsedImplicitly] init; }

	public IObjectEvaluator?                   ContentExpressionEvaluator { get; }
	public IValueEvaluator?                    ContentBodyEvaluator       { get; }
	public IIntegerEvaluator?                  DelayExpressionEvaluator   { get; }
	public IStringEvaluator?                   EventExpressionEvaluator   { get; }
	public ILocationEvaluator?                 IdLocationEvaluator        { get; }
	public IStringEvaluator?                   TargetExpressionEvaluator  { get; }
	public IStringEvaluator?                   TypeExpressionEvaluator    { get; }
	public ImmutableArray<ILocationEvaluator>  NameEvaluatorList          { get; }
	public ImmutableArray<DataConverter.Param> ParameterList              { get; }

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
			await eventSender.Send(eventEntity).ConfigureAwait(false);
		}
	}

	private static Uri ToUri(string uri) => new(uri, UriKind.RelativeOrAbsolute);
}