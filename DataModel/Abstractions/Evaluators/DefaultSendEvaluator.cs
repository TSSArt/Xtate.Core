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

namespace Xtate.DataModel;

public abstract class SendEvaluator(ISend send) : IExecEvaluator, ISend, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => send;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion

#region Interface ISend

	public virtual IContent? Content => send.Content;

	public virtual IValueExpression? DelayExpression => send.DelayExpression;

	public virtual int? DelayMs => send.DelayMs;

	public virtual string? EventName => send.EventName;

	public virtual IValueExpression? EventExpression => send.EventExpression;

	public virtual string? Id => send.Id;

	public virtual ILocationExpression? IdLocation => send.IdLocation;

	public virtual ImmutableArray<ILocationExpression> NameList => send.NameList;

	public virtual ImmutableArray<IParam> Parameters => send.Parameters;

	public virtual Uri? Target => send.Target;

	public virtual IValueExpression? TargetExpression => send.TargetExpression;

	public virtual Uri? Type => send.Type;

	public virtual IValueExpression? TypeExpression => send.TypeExpression;

#endregion
}

public class DefaultSendEvaluator(ISend send) : SendEvaluator(send)
{
	private readonly IValueEvaluator? _contentBodyEvaluator = send.Content?.Body?.As<IValueEvaluator>();

	private readonly IObjectEvaluator? _contentExpressionEvaluator = send.Content?.Expression?.As<IObjectEvaluator>();

	private readonly IIntegerEvaluator? _delayExpressionEvaluator = send.DelayExpression?.As<IIntegerEvaluator>();

	private readonly IStringEvaluator? _eventExpressionEvaluator = send.EventExpression?.As<IStringEvaluator>();

	private readonly ILocationEvaluator? _idLocationEvaluator = send.IdLocation?.As<ILocationEvaluator>();

	private readonly ImmutableArray<ILocationEvaluator> _nameEvaluatorList = send.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();

	private readonly ImmutableArray<DataConverter.Param> _parameterList = DataConverter.AsParamArray(send.Parameters);

	private readonly IStringEvaluator? _targetExpressionEvaluator = send.TargetExpression?.As<IStringEvaluator>();

	private readonly IStringEvaluator? _typeExpressionEvaluator = send.TypeExpression?.As<IStringEvaluator>();

	public required Func<ValueTask<DataConverter>> DataConverterFactory { private get; [UsedImplicitly] init; }

	public required Func<ValueTask<IEventController>> EventControllerFactory { private get; [UsedImplicitly] init; }

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

		var eventEntity = new EventEntity(name)
						  {
							  SendId = sendId,
							  Type = type,
							  Target = target,
							  DelayMs = delayMs,
							  Data = data,
							  RawData = rawContent
						  };

		var eventController = await EventControllerFactory().ConfigureAwait(false);
		await eventController.Send(eventEntity).ConfigureAwait(false);
	}

	private static Uri ToUri(string uri) => new(uri, UriKind.RelativeOrAbsolute);
}