#region Copyright © 2019-2020 Sergii Artemenko

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

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Xtate.Annotations;

namespace Xtate.DataModel
{
	[PublicAPI]
	public class DefaultSendEvaluator : IExecEvaluator, ISend, IAncestorProvider, IDebugEntityId
	{
		private readonly SendEntity _send;

		public DefaultSendEvaluator(in SendEntity send)
		{
			_send = send;

			EventExpressionEvaluator = send.EventExpression?.As<IStringEvaluator>();
			TypeExpressionEvaluator = send.TypeExpression?.As<IStringEvaluator>();
			TargetExpressionEvaluator = send.TargetExpression?.As<IStringEvaluator>();
			DelayExpressionEvaluator = send.DelayExpression?.As<IIntegerEvaluator>();
			ContentExpressionEvaluator = send.Content?.Expression?.As<IObjectEvaluator>();
			ContentBodyEvaluator = send.Content?.Body?.As<IValueEvaluator>();
			IdLocationEvaluator = send.IdLocation?.As<ILocationEvaluator>();
			NameEvaluatorList = send.NameList.AsArrayOf<ILocationExpression, ILocationEvaluator>();
			ParameterList = send.Parameters.AsArrayOf<IParam, DefaultParam>();
		}

		public IObjectEvaluator?                  ContentExpressionEvaluator { get; }
		public IValueEvaluator?                   ContentBodyEvaluator       { get; }
		public IIntegerEvaluator?                 DelayExpressionEvaluator   { get; }
		public IStringEvaluator?                  EventExpressionEvaluator   { get; }
		public ILocationEvaluator?                IdLocationEvaluator        { get; }
		public IStringEvaluator?                  TargetExpressionEvaluator  { get; }
		public IStringEvaluator?                  TypeExpressionEvaluator    { get; }
		public ImmutableArray<ILocationEvaluator> NameEvaluatorList          { get; }
		public ImmutableArray<DefaultParam>       ParameterList              { get; }

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _send.Ancestor;

	#endregion

	#region Interface IDebugEntityId

		public FormattableString EntityId => @$"{Id}";

	#endregion

	#region Interface IExecEvaluator

		public virtual async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext is null) throw new ArgumentNullException(nameof(executionContext));

			var sendId = _send.Id is not null ? SendId.FromString(_send.Id) : SendId.New();

			if (IdLocationEvaluator is not null)
			{
				await IdLocationEvaluator.SetValue(sendId, customData: null, executionContext, token).ConfigureAwait(false);
			}

			var name = EventExpressionEvaluator is not null ? await EventExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false) : _send.EventName;
			var data = await DataConverter.GetData(ContentBodyEvaluator, ContentExpressionEvaluator, NameEvaluatorList, ParameterList, executionContext, token).ConfigureAwait(false);
			var type = TypeExpressionEvaluator is not null ? ToUri(await TypeExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false)) : _send.Type;
			var target = TargetExpressionEvaluator is not null ? ToUri(await TargetExpressionEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false)) : _send.Target;
			var delayMs = DelayExpressionEvaluator is not null ? await DelayExpressionEvaluator.EvaluateInteger(executionContext, token).ConfigureAwait(false) : _send.DelayMs ?? 0;

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
				eventEntity.RawData = await rawContentEvaluator.EvaluateString(executionContext, token).ConfigureAwait(false);
			}

			await executionContext.Send(eventEntity, token).ConfigureAwait(false);
		}

	#endregion

	#region Interface ISend

		public IContent?                           Content          => _send.Content;
		public IValueExpression?                   DelayExpression  => _send.DelayExpression;
		public int?                                DelayMs          => _send.DelayMs;
		public string?                             EventName        => _send.EventName;
		public IValueExpression?                   EventExpression  => _send.EventExpression;
		public string?                             Id               => _send.Id;
		public ILocationExpression?                IdLocation       => _send.IdLocation;
		public ImmutableArray<ILocationExpression> NameList         => _send.NameList;
		public ImmutableArray<IParam>              Parameters       => _send.Parameters;
		public Uri?                                Target           => _send.Target;
		public IValueExpression?                   TargetExpression => _send.TargetExpression;
		public Uri?                                Type             => _send.Type;
		public IValueExpression?                   TypeExpression   => _send.TypeExpression;

	#endregion

		private static Uri ToUri(string uri) => new(uri, UriKind.RelativeOrAbsolute);
	}
}