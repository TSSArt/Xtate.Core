<<<<<<< Updated upstream
﻿#region Copyright © 2019-2023 Sergii Artemenko

=======
﻿// Copyright © 2019-2023 Sergii Artemenko
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
using System.Threading.Tasks;
using Xtate.Core;

namespace Xtate.DataModel;

public abstract class CancelEvaluator : ICancel, IExecEvaluator, IAncestorProvider
{
	private readonly ICancel _cancel;

	protected CancelEvaluator(ICancel cancel)
	{
		Infra.Requires(cancel);

		_cancel = cancel;
	}

#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => _cancel;
=======
namespace Xtate.DataModel;

public abstract class CancelEvaluator(ICancel cancel) : ICancel, IExecEvaluator, IAncestorProvider
{
#region Interface IAncestorProvider

	object IAncestorProvider.Ancestor => cancel;
>>>>>>> Stashed changes

#endregion

#region Interface ICancel

<<<<<<< Updated upstream
	public virtual string?           SendId           => _cancel.SendId;
	public virtual IValueExpression? SendIdExpression => _cancel.SendIdExpression;
=======
	public virtual string?           SendId           => cancel.SendId;
	public virtual IValueExpression? SendIdExpression => cancel.SendIdExpression;
>>>>>>> Stashed changes

#endregion

#region Interface IExecEvaluator

	public abstract ValueTask Execute();

#endregion
}

public class DefaultCancelEvaluator : CancelEvaluator
{
<<<<<<< Updated upstream
	public DefaultCancelEvaluator(ICancel cancel) : base(cancel) => SendIdExpressionEvaluator = cancel.SendIdExpression?.As<IStringEvaluator>();

	public required Func<ValueTask<IEventController?>> EventSenderFactory { private get; init; }

	public IStringEvaluator? SendIdExpressionEvaluator { get; }

	public override async ValueTask Execute()
	{
		var sendId = SendIdExpressionEvaluator is not null ? await SendIdExpressionEvaluator.EvaluateString().ConfigureAwait(false) : SendId;
=======
	private readonly IStringEvaluator? _sendIdExpressionEvaluator;

	public DefaultCancelEvaluator(ICancel cancel) : base(cancel) => _sendIdExpressionEvaluator = base.SendIdExpression?.As<IStringEvaluator>();

	public required Func<ValueTask<IEventController?>> EventSenderFactory { private get; [UsedImplicitly] init; }

	public override async ValueTask Execute()
	{
		var sendId = _sendIdExpressionEvaluator is not null ? await _sendIdExpressionEvaluator.EvaluateString().ConfigureAwait(false) : base.SendId;
>>>>>>> Stashed changes

		if (string.IsNullOrEmpty(sendId))
		{
			throw new ExecutionException(Resources.Exception_SendIdIsEmpty);
		}

		if (await EventSenderFactory().ConfigureAwait(false) is { } eventSender)
		{
			await eventSender.Cancel(Xtate.SendId.FromString(sendId)).ConfigureAwait(false);
		}
	}
}