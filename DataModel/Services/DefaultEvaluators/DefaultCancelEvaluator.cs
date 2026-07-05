// Copyright © 2019-2026 Sergii Artemenko
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

using Xtate.Ancestor.Extensions;
using Xtate.IoC.Tools;
using Xtate.StateMachine;

namespace Xtate.DataModel.Services;

[InstantiatedByIoC]
public class DefaultCancelEvaluator : CancelEvaluator
{
	private readonly IStringEvaluator? _sendIdExpressionEvaluator;

	public DefaultCancelEvaluator(ICancel cancel) : base(cancel) => _sendIdExpressionEvaluator = base.SendIdExpression?.UseAncestor.As<IStringEvaluator>();

	public required Deferred<IEventController> EventController { private get; [SetByIoC] init; }

	public override async ValueTask Execute()
	{
		var sendId = _sendIdExpressionEvaluator is not null ? await _sendIdExpressionEvaluator.EvaluateString().ConfigureAwait(false) : base.SendId;

		if (string.IsNullOrEmpty(sendId))
		{
			throw new ExecutionException(Resources.Exception_SendIdIsEmpty);
		}

		var eventController = await EventController().ConfigureAwait(false);

		await eventController.Cancel(Xtate.StateMachine.SendId.FromString(sendId)).ConfigureAwait(false);
	}
}