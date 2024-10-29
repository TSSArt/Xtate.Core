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

public abstract class CancelEvaluator(ICancel cancel) : ICancel, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => cancel;

#endregion

#region Interface ICancel

	public virtual string? SendId => cancel.SendId;

	public virtual IValueExpression? SendIdExpression => cancel.SendIdExpression;

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion
}

public class DefaultCancelEvaluator : CancelEvaluator
{
	private readonly IStringEvaluator? _sendIdExpressionEvaluator;

	public DefaultCancelEvaluator(ICancel cancel) : base(cancel) => _sendIdExpressionEvaluator = base.SendIdExpression?.As<IStringEvaluator>();

	public required Deferred<IEventController?> EventController { private get; [UsedImplicitly] init; }

	public override async ValueTask Execute()
	{
		var sendId = _sendIdExpressionEvaluator is not null ? await _sendIdExpressionEvaluator.EvaluateString().ConfigureAwait(false) : base.SendId;

		if (string.IsNullOrEmpty(sendId))
		{
			throw new ExecutionException(Resources.Exception_SendIdIsEmpty);
		}

		if (await EventController().ConfigureAwait(false) is { } eventController)
		{
			await eventController.Cancel(Xtate.SendId.FromString(sendId)).ConfigureAwait(false);
		}
	}
}