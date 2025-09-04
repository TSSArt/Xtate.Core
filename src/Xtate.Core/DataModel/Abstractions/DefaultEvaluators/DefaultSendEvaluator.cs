// Copyright © 2019-2025 Sergii Artemenko
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

public class DefaultSendEvaluator(ISend send) : SendEvaluator(send)
{
    private readonly IValueEvaluator? _contentBodyEvaluator = send.Content?.Body?.As<IValueEvaluator>();

    private readonly IObjectEvaluator? _contentExpressionEvaluator = send.Content?.Expression?.As<IObjectEvaluator>();

    private readonly IIntegerEvaluator? _delayExpressionEvaluator = send.DelayExpression?.As<IIntegerEvaluator>();

    private readonly IStringEvaluator? _eventExpressionEvaluator = send.EventExpression?.As<IStringEvaluator>();

    private readonly ILocationEvaluator? _idLocationEvaluator = send.IdLocation?.As<ILocationEvaluator>();

    private readonly ImmutableArray<ILocationEvaluator> _nameEvaluatorList = send.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();

    private readonly ImmutableArray<DataConverter.Param> _parameterList = DataModel.DataConverter.AsParamArray(send.Parameters);

    private readonly IStringEvaluator? _targetExpressionEvaluator = send.TargetExpression?.As<IStringEvaluator>();

    private readonly IStringEvaluator? _typeExpressionEvaluator = send.TypeExpression?.As<IStringEvaluator>();

    public required Deferred<DataConverter> DataConverter { private get; [UsedImplicitly] init; }

    public required Deferred<IEventController> EventController { private get; [UsedImplicitly] init; }

    public override async ValueTask Execute()
    {
        var sendId = base.Id is { } id ? SendId.FromString(id) : SendId.New();

        if (_idLocationEvaluator is not null)
        {
            await _idLocationEvaluator.SetValue(sendId).ConfigureAwait(false);
        }

        var dataConverter = await DataConverter().ConfigureAwait(false);
        var name = _eventExpressionEvaluator is not null ? await _eventExpressionEvaluator.EvaluateString().ConfigureAwait(false) : EventName;
        var data = await dataConverter.GetData(_contentBodyEvaluator, _contentExpressionEvaluator, _nameEvaluatorList, _parameterList).ConfigureAwait(false);
        var type = _typeExpressionEvaluator is not null ? new FullUri(await _typeExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : Type;
        var target = _targetExpressionEvaluator is not null ? new FullUri(await _targetExpressionEvaluator.EvaluateString().ConfigureAwait(false)) : Target;
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

        var eventController = await EventController().ConfigureAwait(false);
        await eventController.Send(eventEntity).ConfigureAwait(false);
    }
}